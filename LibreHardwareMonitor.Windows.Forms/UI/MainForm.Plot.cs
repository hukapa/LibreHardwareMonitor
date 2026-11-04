// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Aga.Controls.Tree.NodeControls;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Hardware.Storage;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;
using LibreHardwareMonitor.Windows.Forms.Utilities;

namespace LibreHardwareMonitor.Windows.Forms.UI;

public sealed partial class MainForm
{
    private void InitializeSplitter()
    {
        splitContainer.SplitterDistance = _settings.GetValue("splitContainer.SplitterDistance", 400);
        splitContainer.SplitterMoved += delegate { _settings.SetValue("splitContainer.SplitterDistance", splitContainer.SplitterDistance); };
    }

    private void InitializePlotForm()
    {
        _plotForm = new Form { FormBorderStyle = FormBorderStyle.SizableToolWindow, ShowInTaskbar = false, StartPosition = FormStartPosition.Manual };
        AddOwnedForm(_plotForm);
        _plotForm.Bounds = new Rectangle
        {
            X = _settings.GetValue("plotForm.Location.X", -100000),
            Y = _settings.GetValue("plotForm.Location.Y", 100),
            Width = _settings.GetValue("plotForm.Width", 600),
            Height = _settings.GetValue("plotForm.Height", 400)
        };

        _showPlot = new UserOption("plotMenuItem", false, plotMenuItem, _settings);
        _plotLocation = new UserRadioGroup("plotLocation", 0, new[] { plotWindowMenuItem, plotBottomMenuItem, plotRightMenuItem }, _settings);
        _splitPanelScalingSetting = new UserRadioGroup("splitPanelScalingSetting", 0, new[] { splitPanelPercentageScalingMenuItem, splitPanelFixedPlotScalingMenuItem, splitPanelFixedSensorScalingMenuItem }, _settings);

        _showPlot.Changed += delegate
        {
            if (_plotLocation.Value == 0)
            {
                if (_showPlot.Value && Visible)
                {
                    Theme.Current.Apply(_plotForm);
                    _plotForm.Show();
                }
                else
                    _plotForm.Hide();
            }
            else
            {
                splitContainer.Panel2Collapsed = !_showPlot.Value;
            }

            treeView.Invalidate();
        };

        _strokeThickness = new UserRadioGroup("plotStroke", 1, new[] { strokeThickness1ptMenuItem, strokeThickness2ptMenuItem, strokeThickness3ptMenuItem, strokeThickness4ptMenuItem }, _settings);

        _strokeThickness.Changed += (sender, e) =>
        {
            _plotStrokeThickness = (_strokeThickness.Value >= 0 && _strokeThickness.Value <= 3)
                                                   ? _strokeThickness.Value + 1
                                                   : 4;
            _plotPanel.UpdateStrokeThickness(_plotStrokeThickness);
        };

        _plotLocation.Changed += delegate
        {
            switch (_plotLocation.Value)
            {
                case 0:
                    splitContainer.Panel2.Controls.Clear();
                    splitContainer.Panel2Collapsed = true;
                    _plotForm.Controls.Add(_plotPanel);
                    if (_showPlot.Value && Visible)
                        _plotForm.Show();
                    break;
                case 1:
                    _plotForm.Controls.Clear();
                    _plotForm.Hide();
                    splitContainer.Orientation = Orientation.Horizontal;
                    splitContainer.Panel2.Controls.Add(_plotPanel);
                    splitContainer.Panel2Collapsed = !_showPlot.Value;
                    break;
                case 2:
                    _plotForm.Controls.Clear();
                    _plotForm.Hide();
                    splitContainer.Orientation = Orientation.Vertical;
                    splitContainer.Panel2.Controls.Add(_plotPanel);
                    splitContainer.Panel2Collapsed = !_showPlot.Value;
                    break;
            }
        };

        _splitPanelScalingSetting.Changed += delegate
        {
            switch (_splitPanelScalingSetting.Value)
            {
                case 0:
                    splitContainer.FixedPanel = FixedPanel.None;
                    break;
                case 1:
                    splitContainer.FixedPanel = FixedPanel.Panel2;
                    break;
                case 2:
                    splitContainer.FixedPanel = FixedPanel.Panel1;
                    break;
            }
        };

        _plotForm.FormClosing += delegate(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // just switch off the plotting when the user closes the form
                if (_plotLocation.Value == 0)
                {
                    _showPlot.Value = false;
                }

                e.Cancel = true;
            }
        };

        void MoveOrResizePlotForm(object sender, EventArgs e)
        {
            if (_plotForm.WindowState != FormWindowState.Minimized)
            {
                _settings.SetValue("plotForm.Location.X", _plotForm.Bounds.X);
                _settings.SetValue("plotForm.Location.Y", _plotForm.Bounds.Y);
                _settings.SetValue("plotForm.Width", _plotForm.Bounds.Width);
                _settings.SetValue("plotForm.Height", _plotForm.Bounds.Height);
            }
        }

        _plotForm.Move += MoveOrResizePlotForm;
        _plotForm.Resize += MoveOrResizePlotForm;

        _plotForm.VisibleChanged += delegate
        {
            Rectangle bounds = new(_plotForm.Location, _plotForm.Size);
            Screen screen = Screen.FromRectangle(bounds);
            Rectangle intersection = Rectangle.Intersect(screen.WorkingArea, bounds);
            if (intersection.Width < Math.Min(16, bounds.Width) ||
                intersection.Height < Math.Min(16, bounds.Height))
            {
                _plotForm.Location = new Point(screen.WorkingArea.Width / 2 - bounds.Width / 2,
                                               screen.WorkingArea.Height / 2 - bounds.Height / 2);
            }
        };

        VisibleChanged += delegate
        {
            if (Visible && _showPlot.Value && _plotLocation.Value == 0)
                _plotForm.Show();
            else
                _plotForm.Hide();
        };

        Theme.Current.Apply(_plotForm);
    }


    private void PlotSelectionChanged(object sender, EventArgs e)
    {
        List<ISensor> selected = new();
        IDictionary<ISensor, Color> colors = new Dictionary<ISensor, Color>();
        int colorIndex = 0;

        foreach (TreeNodeAdv node in treeView.AllNodes)
        {
            if (node.Tag is SensorNode sensorNode)
            {
                if (sensorNode.Plot)
                {
                    if (!sensorNode.PenColor.HasValue)
                    {
                        colors.Add(sensorNode.Sensor,
                                   Theme.Current.PlotColorPalette[colorIndex % Theme.Current.PlotColorPalette.Length]);
                    }

                    selected.Add(sensorNode.Sensor);
                }

                colorIndex++;
            }
        }

        // if a sensor is assigned a color that's already being used by another
        // sensor, try to assign it a new color. This is done only after the
        // previous loop sets an unchanging default color for all sensors, so that
        // colors jump around as little as possible as sensors get added/removed
        // from the plot
        var usedColors = new List<Color>();
        foreach (ISensor curSelectedSensor in selected)
        {
            if (!colors.ContainsKey(curSelectedSensor))
                continue;

            Color curColor = colors[curSelectedSensor];
            if (usedColors.Contains(curColor))
            {
                foreach (Color potentialNewColor in Theme.Current.PlotColorPalette)
                {
                    if (!colors.Values.Contains(potentialNewColor))
                    {
                        colors[curSelectedSensor] = potentialNewColor;
                        usedColors.Add(potentialNewColor);
                        break;
                    }
                }
            }
            else
            {
                usedColors.Add(curColor);
            }
        }

        foreach (TreeNodeAdv node in treeView.AllNodes)
        {
            if (node.Tag is SensorNode sensorNode && sensorNode.Plot && sensorNode.PenColor.HasValue)
                colors.Add(sensorNode.Sensor, sensorNode.PenColor.Value);
        }

        _sensorPlotColors = colors;
        _plotPanel.SetSensors(selected, colors, _plotStrokeThickness);
    }


    private void resetPlotMenuItem_Click(object sender, EventArgs e)
    {
        _computer.Accept(new SensorVisitor(delegate (ISensor sensorClick)
        {
            sensorClick.ClearValues();
        }));
    }

    private void MainForm_MoveOrResize(object sender, EventArgs e)
    {
        if (WindowState != FormWindowState.Minimized)
        {
            _settings.SetValue("mainForm.Location.X", Bounds.X);
            _settings.SetValue("mainForm.Location.Y", Bounds.Y);
            _settings.SetValue("mainForm.Width", Bounds.Width);
            _settings.SetValue("mainForm.Height", Bounds.Height);
        }
    }


    private void perSessionFileRotationMenuItem_Click(object sender, EventArgs e)
    {
        dailyFileRotationMenuItem.Checked = false;
        perSessionFileRotationMenuItem.Checked = true;
        _logger.FileRotationMethod = LoggerFileRotation.PerSession;
        _settings.SetValue("logger.fileRotation", (int)LoggerFileRotation.PerSession);
    }

    private void dailyFileRotationMenuItem_Click(object sender, EventArgs e)
    {
        dailyFileRotationMenuItem.Checked = true;
        perSessionFileRotationMenuItem.Checked = false;
        _logger.FileRotationMethod = LoggerFileRotation.Daily;
        _settings.SetValue("logger.fileRotation", (int)LoggerFileRotation.Daily);
    }
}
