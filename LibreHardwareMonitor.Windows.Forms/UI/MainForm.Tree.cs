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
    private void StopFileHardwareMenuFromClosing(object sender, ToolStripDropDownClosingEventArgs e)
    {
        if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
        {
            e.Cancel = true;
        }
    }


    private void NodeTextBoxText_DrawText(object sender, DrawEventArgs e)
    {
        if (e.Node.Tag is Node node)
        {
            if (node.IsVisible)
            {
                if (plotMenuItem.Checked && node is SensorNode sensorNode && _sensorPlotColors.TryGetValue(sensorNode.Sensor, out Color color))
                    e.TextColor = color;
            }
            else
                e.TextColor = Color.DarkGray;
        }
    }


    private void NodeTextBoxText_EditorShowing(object sender, CancelEventArgs e)
    {
        e.Cancel = !(treeView.CurrentNode != null && (treeView.CurrentNode.Tag is SensorNode || treeView.CurrentNode.Tag is HardwareNode));
    }

    private void NodeCheckBox_IsVisibleValueNeeded(object sender, NodeControlValueEventArgs e)
    {
        e.Value = e.Node.Tag is SensorNode && plotMenuItem.Checked;
    }


    private void TreeView_Click(object sender, EventArgs e)
    {
        if (!(e is MouseEventArgs m) || (m.Button != MouseButtons.Left && m.Button != MouseButtons.Right))
            return;

        NodeControlInfo info = treeView.GetNodeControlInfoAt(new Point(m.X, m.Y));
        if (m.Button == MouseButtons.Left && info.Node != null)
        {
            if (info.Node.Tag is IExpandPersistNode expandPersistNode)
            {
                expandPersistNode.Expanded = info.Node.IsExpanded;
            }
            return;
        }

        treeView.SelectedNode = info.Node;
        if (info.Node != null)
        {
            if (info.Node.Tag is SensorNode node && node.Sensor != null)
            {
                treeContextMenu.Items.Clear();
                if (node.Sensor.Parameters.Count > 0)
                {
                    ToolStripItem item = new ToolStripMenuItem("Parameters...");
                    item.Click += delegate { ShowParameterForm(node.Sensor); };
                    treeContextMenu.Items.Add(item);
                }

                if (nodeTextBoxText.EditEnabled)
                {
                    ToolStripItem item = new ToolStripMenuItem("Rename");
                    item.Click += delegate { nodeTextBoxText.BeginEdit(); };
                    treeContextMenu.Items.Add(item);
                }

                if (node.IsVisible)
                {
                    ToolStripItem item = new ToolStripMenuItem("Hide");
                    item.Click += delegate { node.IsVisible = false; };
                    treeContextMenu.Items.Add(item);
                }
                else
                {
                    ToolStripItem item = new ToolStripMenuItem("Unhide");
                    item.Click += delegate { node.IsVisible = true; };
                    treeContextMenu.Items.Add(item);
                }

                treeContextMenu.Items.Add(new ToolStripSeparator());
                {
                    ToolStripItem item = new ToolStripMenuItem("Pen Color...");
                    item.Click += delegate
                    {
                        ColorDialog dialog = new() { Color = node.PenColor.GetValueOrDefault() };
                        if (dialog.ShowDialog() == DialogResult.OK)
                            node.PenColor = dialog.Color;
                    };

                    treeContextMenu.Items.Add(item);
                }

                {
                    ToolStripItem item = new ToolStripMenuItem("Reset Pen Color");
                    item.Click += delegate { node.PenColor = null; };
                    treeContextMenu.Items.Add(item);
                }

                treeContextMenu.Items.Add(new ToolStripSeparator());
                {
                    ToolStripMenuItem item = new("Show in Tray") { Checked = _systemTray.Contains(node.Sensor) };
                    item.Click += delegate
                    {
                        if (item.Checked)
                            _systemTray.Remove(node.Sensor);
                        else
                            _systemTray.Add(node.Sensor, true);
                    };

                    treeContextMenu.Items.Add(item);
                }

                if (_gadget != null)
                {
                    ToolStripMenuItem item = new("Show in Gadget") { Checked = _gadget.Contains(node.Sensor) };
                    item.Click += delegate
                    {
                        if (item.Checked)
                        {
                            _gadget.Remove(node.Sensor);
                        }
                        else
                        {
                            _gadget.Add(node.Sensor);
                        }
                    };

                    treeContextMenu.Items.Add(item);
                }

                if (node.Sensor.Control != null)
                {
                    treeContextMenu.Items.Add(new ToolStripSeparator());
                    IControl control = node.Sensor.Control;
                    ToolStripMenuItem controlItem = new("Control");
                    ToolStripItem defaultItem = new ToolStripMenuItem("Default") { Checked = control.ControlMode == ControlMode.Default };
                    controlItem.DropDownItems.Add(defaultItem);
                    defaultItem.Click += delegate { control.SetDefault(); };
                    ToolStripMenuItem manualItem = new("Manual");
                    controlItem.DropDownItems.Add(manualItem);
                    manualItem.Checked = control.ControlMode == ControlMode.Software;
                    for (int i = 0; i <= 100; i += 5)
                    {
                        if (i <= control.MaxSoftwareValue &&
                            i >= control.MinSoftwareValue)
                        {
                            ToolStripMenuItem item = new ToolStripRadioButtonMenuItem(i + " %");
                            manualItem.DropDownItems.Add(item);
                            item.Checked = control.ControlMode == ControlMode.Software && Math.Round(control.SoftwareValue) == i;
                            int softwareValue = i;
                            item.Click += delegate { control.SetSoftware(softwareValue); };
                        }
                    }

                    treeContextMenu.Items.Add(controlItem);
                }

                treeContextMenu.Show(treeView, new Point(m.X, m.Y));
            }

            if (info.Node.Tag is HardwareNode hardwareNode && hardwareNode.Hardware != null)
            {
                treeContextMenu.Items.Clear();

                if (nodeTextBoxText.EditEnabled)
                {
                    ToolStripItem item = new ToolStripMenuItem("Rename");
                    item.Click += delegate { nodeTextBoxText.BeginEdit(); };
                    treeContextMenu.Items.Add(item);
                }

                treeContextMenu.Show(treeView, new Point(m.X, m.Y));
            }
        }
    }


    private void ShowParameterForm(ISensor sensorForm)
    {
        ParameterForm form = new() { Parameters = sensorForm.Parameters, captionLabel = { Text = sensorForm.Name } };
        form.ShowDialog();
    }

    private void TreeView_NodeMouseDoubleClick(object sender, TreeNodeAdvMouseEventArgs e)
    {
        if (e.Node.Tag is SensorNode node && node.Sensor != null && node.Sensor.Parameters.Count > 0)
            ShowParameterForm(node.Sensor);
    }


    private void ExpandAllNodes_Click(object sender, EventArgs e)
    {
        treeView.ExpandAll();

        foreach (var node in treeView.AllNodes)
        {
            if (node.Tag is IExpandPersistNode expandPersistNode)
            {
                expandPersistNode.Expanded = true;
            }
        }
    }

    private void CollapseAllNodes_Click(object sender, EventArgs e)
    {
        treeView.CollapseAll();

        foreach (var node in treeView.AllNodes)
        {
            if (node.Tag is IExpandPersistNode expandPersistNode)
            {
                expandPersistNode.Expanded = false;
            }
        }
    }


    private void TreeView_MouseMove(object sender, MouseEventArgs e)
    {
        _selectionDragging &= (e.Button & (MouseButtons.Left | MouseButtons.Right)) > 0;
        if (_selectionDragging)
            treeView.SelectedNode = treeView.GetNodeAt(e.Location);
    }

    private void TreeView_MouseDown(object sender, MouseEventArgs e)
    {
        _selectionDragging = true;
    }

    private void TreeView_MouseUp(object sender, MouseEventArgs e)
    {
        _selectionDragging = false;
    }

    private void TreeView_SizeChanged(object sender, EventArgs e)
    {
        int newWidth = treeView.Width;
        for (int i = 1; i < treeView.Columns.Count; i++)
        {
            if (treeView.Columns[i].IsVisible)
                newWidth -= treeView.Columns[i].Width;
        }
        treeView.Columns[0].Width = newWidth;
    }

    private void TreeView_KeyDown(object sender, KeyEventArgs e)
    {
        if (treeView.SelectedNode != null)
        {
            switch (e.KeyCode)
            {
                case Keys.Right:
                    if (treeView.SelectedNode.Tag is IExpandPersistNode expandPersistNodeR)
                    {
                        expandPersistNodeR.Expanded = true;
                    }
                    return;
                case Keys.Left:
                    if (treeView.SelectedNode.Tag is IExpandPersistNode expandPersistNodeL)
                    {
                        expandPersistNodeL.Expanded = false;
                    }
                    return;
            }
        }
    }

    private void TreeView_ColumnWidthChanged(TreeColumn column)
    {
        int index = treeView.Columns.IndexOf(column);
        int columnsWidth = 0;
        foreach (TreeColumn treeColumn in treeView.Columns)
        {
            if (treeColumn.IsVisible)
                columnsWidth += treeColumn.Width;
        }

        int nextColumnIndex = index + 1;
        while (nextColumnIndex < treeView.Columns.Count && treeView.Columns[nextColumnIndex].IsVisible == false)
            nextColumnIndex++;

        if (nextColumnIndex < treeView.Columns.Count) {
            int diff = treeView.Width - columnsWidth;
            treeView.Columns[nextColumnIndex].Width = Math.Max(20, treeView.Columns[nextColumnIndex].Width + diff);
        }
    }

}
