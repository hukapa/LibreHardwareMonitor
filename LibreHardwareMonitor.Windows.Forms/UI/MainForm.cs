// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

public sealed partial class MainForm : Form
{
    private ToolStripMenuItem _autoThemeMenuItem;
    private readonly UserOption _autoStart;
    private readonly Computer _computer;
    private readonly SensorGadget _gadget;
    private readonly Logger _logger;
    private readonly UserRadioGroup _loggingInterval;
    private readonly UserRadioGroup _smartUpdateCycle;
    private readonly UserRadioGroup _updateInterval;
    private readonly UserOption _logSensors;
    private readonly UserOption _excludeUsbDevices;
    private readonly UserOption _forceDriveWakeup;
    private readonly UserOption _minimizeOnClose;
    private readonly UserOption _minimizeToTray;
    private readonly PlotPanel _plotPanel;
    private readonly UserOption _readBatterySensors;
    private readonly UserOption _readCpuSensors;
    private readonly UserOption _readFanControllersSensors;
    private readonly UserOption _readGpuSensors;
    private readonly UserOption _readPowerMonitorSensors;
    private readonly UserOption _readHddSensors;
    private readonly UserOption _readMainboardSensors;
    private readonly UserOption _readNicSensors;
    private readonly UserOption _readPsuSensors;
    private readonly UserOption _readRamSensors;
    private readonly Node _root;
    private readonly UserOption _runWebServer;
    private readonly UserRadioGroup _sensorValuesTimeWindow;
    private readonly PersistentSettings _settings;
    private readonly UserOption _showGadget;
    private readonly StartupManager _startupManager = new();
    private readonly SystemTray _systemTray;
    private readonly UnitManager _unitManager;
    private readonly UpdateVisitor _updateVisitor = new();

    private int _delayCount;
    private Form _plotForm;
    private UserRadioGroup _plotLocation;
    private UserRadioGroup _splitPanelScalingSetting;
    private bool _selectionDragging;
    private IDictionary<ISensor, Color> _sensorPlotColors = new Dictionary<ISensor, Color>();
    private UserOption _showPlot;
    private UserRadioGroup _strokeThickness;
    private double _plotStrokeThickness = 2;

    public MainForm()
    {
        InitializeComponent();

        _settings = new PersistentSettings();
        _settings.Load(Path.ChangeExtension(Application.ExecutablePath, ".config"));

        _unitManager = new UnitManager(_settings);

        // make sure the buffers used for double buffering are not disposed
        // after each draw call
        BufferedGraphicsManager.Current.MaximumBuffer = Screen.PrimaryScreen.Bounds.Size;

        // set the DockStyle here, to avoid conflicts with the MainMenu
        splitContainer.Dock = DockStyle.Fill;

        Font = SystemFonts.MessageBoxFont;
        treeView.Font = SystemFonts.MessageBoxFont;

        // Set the bounds immediately, so that our child components can be
        // properly placed.
        Bounds = new Rectangle
        {
            X = _settings.GetValue("mainForm.Location.X", Location.X),
            Y = _settings.GetValue("mainForm.Location.Y", Location.Y),
            Width = _settings.GetValue("mainForm.Width", 470),
            Height = _settings.GetValue("mainForm.Height", 640)
        };

        Theme setTheme = Theme.All.FirstOrDefault(theme => _settings.GetValue("theme", "auto") == theme.Id);
        if (setTheme != null)
        {
            Theme.Current = setTheme;
        }
        else
        {
            Theme.SetAutoTheme();
        }

        _plotPanel = new PlotPanel(_settings, _unitManager) { Font = SystemFonts.MessageBoxFont, Dock = DockStyle.Fill };

        nodeCheckBox.IsVisibleValueNeeded += NodeCheckBox_IsVisibleValueNeeded;
        nodeTextBoxText.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxValue.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxMin.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxMax.DrawText += NodeTextBoxText_DrawText;
        nodeTextBoxText.EditorShowing += NodeTextBoxText_EditorShowing;

        for (int i = 1; i < treeView.Columns.Count; i++)
        {
            TreeColumn column = treeView.Columns[i];
            column.Width = Math.Max(20, Math.Min(400, _settings.GetValue("treeView.Columns." + column.Header + ".Width", column.Width)));
        }

        TreeModel treeModel = new();
        _root = new Node(Environment.MachineName) { Image = EmbeddedResources.GetImage("computer.png") };

        treeModel.Nodes.Add(_root);
        treeView.Model = treeModel;

        _computer = new Computer(_settings);

        _systemTray = new SystemTray(_computer, _settings, _unitManager);
        _systemTray.HideShowCommand += HideShowClick;
        _systemTray.ExitCommand += ExitClick;

        if (Software.OperatingSystem.IsUnix)
        {
            // Unix
            treeView.RowHeight = Math.Max(treeView.RowHeight, 18);
            splitContainer.BorderStyle = BorderStyle.None;
            splitContainer.SplitterWidth = 4;
            treeView.BorderStyle = BorderStyle.Fixed3D;
            _plotPanel.BorderStyle = BorderStyle.Fixed3D;
            gadgetMenuItem.Visible = false;
            minCloseMenuItem.Visible = false;
            minTrayMenuItem.Visible = false;
            startMinMenuItem.Visible = false;
        }
        else
        {
            // Windows
            treeView.RowHeight = Math.Max(treeView.Font.Height + 1, 18);
            _gadget = new SensorGadget(_computer, _settings, _unitManager);
            _gadget.HideShowCommand += HideShowClick;
        }

        treeView.ShowNodeToolTips = true;
        NodeToolTipProvider tooltipProvider = new();
        nodeTextBoxText.ToolTipProvider = tooltipProvider;
        nodeTextBoxValue.ToolTipProvider = tooltipProvider;
        _logger = new Logger(_computer);
        var saved = _settings.GetValue("logger.fileRotation", 0); // 0 = PerSession, 1 = Daily.
        _logger.FileRotationMethod = (LoggerFileRotation)Math.Max(0, Math.Min(saved, 1));
        perSessionFileRotationMenuItem.Checked = _logger.FileRotationMethod == LoggerFileRotation.PerSession;
        dailyFileRotationMenuItem.Checked = _logger.FileRotationMethod == LoggerFileRotation.Daily;

        _computer.HardwareAdded += HardwareAdded;
        _computer.HardwareRemoved += HardwareRemoved;

        if (PawnIo.PawnIo.IsInstalled)
        {
            if (PawnIo.PawnIo.Version < new Version(2, 0, 0, 0))
            {
                DialogResult result = MessageBox.Show("PawnIO is outdated, do you want to update it?", nameof(LibreHardwareMonitor), MessageBoxButtons.OKCancel);
                if (result == DialogResult.OK)
                    InstallPawnIO();
            }
        }
        else
        {
            DialogResult result = MessageBox.Show("PawnIO is not installed, do you want to install it?", nameof(LibreHardwareMonitor), MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
                InstallPawnIO();
        }

        _computer.Open();

        static void InstallPawnIO()
        {
            string path = ExtractPawnIO();
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                // The installer requires elevation; ShellExecute with "runas" triggers UAC
                // instead of failing with ERROR_ELEVATION_REQUIRED (740) when not elevated.
                var startInfo = new ProcessStartInfo(path, "-install") { UseShellExecute = true, Verb = "runas" };
                using Process process = Process.Start(startInfo);
                process?.WaitForExit();
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                // ERROR_CANCELLED: the user declined the UAC prompt; continue without PawnIO.
            }
            catch (Win32Exception ex)
            {
                MessageBox.Show("PawnIO installation failed: " + ex.Message, nameof(LibreHardwareMonitor), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                try
                {
                    File.Delete(path);
                }
                catch (IOException)
                { }
                catch (UnauthorizedAccessException)
                { }
            }
        }

        static string ExtractPawnIO()
        {
            string destination = Path.Combine(Directory.GetCurrentDirectory(), "PawnIO_setup.exe");

            try
            {

                using Stream resourceStream = typeof(MainForm).Assembly.GetManifestResourceStream("LibreHardwareMonitor.Windows.Forms.Resources.PawnIO_setup.exe");
                using FileStream fileStream = new(destination, FileMode.Create, FileAccess.Write);
                resourceStream.CopyTo(fileStream);

                return destination;
            }
            catch
            {
                return null;
            }
        }

        backgroundUpdater.DoWork += BackgroundUpdater_DoWork;
        timer.Enabled = true;

        UserOption showHiddenSensors = new("hiddenMenuItem", false, hiddenMenuItem, _settings);
        showHiddenSensors.Changed += delegate { treeModel.ForceVisible = showHiddenSensors.Value; };

        UserOption showValue = new("valueMenuItem", true, valueMenuItem, _settings);
        showValue.Changed += delegate { treeView.Columns[1].IsVisible = showValue.Value; };

        UserOption showMin = new("minMenuItem", false, minMenuItem, _settings);
        showMin.Changed += delegate { treeView.Columns[2].IsVisible = showMin.Value; };

        UserOption showMax = new("maxMenuItem", true, maxMenuItem, _settings);
        showMax.Changed += delegate { treeView.Columns[3].IsVisible = showMax.Value; };

        _ = new UserOption("startMinMenuItem", false, startMinMenuItem, _settings);
        _minimizeToTray = new UserOption("minTrayMenuItem", true, minTrayMenuItem, _settings);
        _minimizeToTray.Changed += delegate { _systemTray.IsMainIconEnabled = _minimizeToTray.Value; };

        _minimizeOnClose = new UserOption("minCloseMenuItem", true, minCloseMenuItem, _settings);

        _autoStart = new UserOption(null, _startupManager.Startup, startupMenuItem, _settings);
        _autoStart.Changed += delegate
        {
            try
            {
                _startupManager.Startup = _autoStart.Value;
            }
            catch (InvalidOperationException)
            {
                MessageBox.Show("Updating the auto-startup option failed.",
                                "Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);

                _autoStart.Value = _startupManager.Startup;
            }
        };

        _readMainboardSensors = new UserOption("mainboardMenuItem", true, mainboardMenuItem, _settings);
        _readMainboardSensors.Changed += delegate { _computer.IsMotherboardEnabled = _readMainboardSensors.Value; };

        _readCpuSensors = new UserOption("cpuMenuItem", true, cpuMenuItem, _settings);
        _readCpuSensors.Changed += delegate { _computer.IsCpuEnabled = _readCpuSensors.Value; };

        _readRamSensors = new UserOption("ramMenuItem", true, ramMenuItem, _settings);
        _readRamSensors.Changed += delegate { _computer.IsMemoryEnabled = _readRamSensors.Value; };

        _readGpuSensors = new UserOption("gpuMenuItem", true, gpuMenuItem, _settings);
        _readGpuSensors.Changed += delegate { _computer.IsGpuEnabled = _readGpuSensors.Value; };

        _readPowerMonitorSensors = new UserOption("powerMonitorMenuItem", true, powerMonitorMenuItem, _settings);
        _readPowerMonitorSensors.Changed += delegate { _computer.IsPowerMonitorEnabled = _readPowerMonitorSensors.Value; };

        _readFanControllersSensors = new UserOption("fanControllerMenuItem", true, fanControllerMenuItem, _settings);
        _readFanControllersSensors.Changed += delegate { _computer.IsControllerEnabled = _readFanControllersSensors.Value; };

        _excludeUsbDevices = new UserOption("excludeUsbDevicesItem", false, excludeUsbDevicesItem, _settings);
        _excludeUsbDevices.Changed += delegate { StorageDevice.ExcludeUsbDevices = _excludeUsbDevices.Value; };

        _readHddSensors = new UserOption("hddMenuItem", true, hddMenuItem, _settings);
        _readHddSensors.Changed += delegate { _computer.IsStorageEnabled = _readHddSensors.Value; };

        _readNicSensors = new UserOption("nicMenuItem", true, nicMenuItem, _settings);
        _readNicSensors.Changed += delegate { _computer.IsNetworkEnabled = _readNicSensors.Value; };

        _readPsuSensors = new UserOption("psuMenuItem", true, psuMenuItem, _settings);
        _readPsuSensors.Changed += delegate { _computer.IsPsuEnabled = _readPsuSensors.Value; };

        _readBatterySensors = new UserOption("batteryMenuItem", true, batteryMenuItem, _settings);
        _readBatterySensors.Changed += delegate { _computer.IsBatteryEnabled = _readBatterySensors.Value; };

        _showGadget = new UserOption("gadgetMenuItem", false, gadgetMenuItem, _settings);

        _forceDriveWakeup = new UserOption("forceDriveWakeupItem", false, forceDriveWakeupItem, _settings);
        _forceDriveWakeup.Changed += delegate
        {
            _computer.Hardware
                .OfType<StorageDevice>()
                .ToList()
                .ForEach(sd =>
            {
                sd.ForceWakeup = _forceDriveWakeup.Value;
            });
        };

        // Prevent Menu From Closing When UnClicking Hardware Items
        menuItemFileHardware.DropDown.Closing += StopFileHardwareMenuFromClosing;

        _showGadget.Changed += delegate
        {
            if (_gadget != null)
                _gadget.Visible = _showGadget.Value;
        };

        celsiusMenuItem.Checked = _unitManager.TemperatureUnit == TemperatureUnit.Celsius;
        fahrenheitMenuItem.Checked = !celsiusMenuItem.Checked;

        Server = new HttpServer(_root,
                                _computer,
                                _settings.GetValue("listenerIp", "?"),
                                _settings.GetValue("listenerPort", 8085),
                                _settings.GetValue("authenticationEnabled", false),
                                _settings.GetValue("authenticationUserName", ""),
                                _settings.GetValue("authenticationPassword", ""));

        if (Server.PlatformNotSupported)
        {
            webMenuItemSeparator.Visible = false;
            webMenuItem.Visible = false;
        }

        _runWebServer = new UserOption("runWebServerMenuItem", false, runWebServerMenuItem, _settings);
        _runWebServer.Changed += delegate
        {
            if (_runWebServer.Value)
                Server.StartHttpListener();
            else
                Server.StopHttpListener();
        };

        authWebServerMenuItem.Checked = _settings.GetValue("authenticationEnabled", false);

        _logSensors = new UserOption("logSensorsMenuItem", false, logSensorsMenuItem, _settings);

        _loggingInterval = new UserRadioGroup("loggingInterval",
                                              0,
                                              new[]
                                              {
                                                  log1sMenuItem,
                                                  log2sMenuItem,
                                                  log5sMenuItem,
                                                  log10sMenuItem,
                                                  log30sMenuItem,
                                                  log1minMenuItem,
                                                  log2minMenuItem,
                                                  log5minMenuItem,
                                                  log10minMenuItem,
                                                  log30minMenuItem,
                                                  log1hMenuItem,
                                                  log2hMenuItem,
                                                  log6hMenuItem
                                              },
                                              _settings);

        _loggingInterval.Changed += (sender, e) =>
        {
            switch (_loggingInterval.Value)
            {
                case 0:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 1);
                    break;
                case 1:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 2);
                    break;
                case 2:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 5);
                    break;
                case 3:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 10);
                    break;
                case 4:
                    _logger.LoggingInterval = new TimeSpan(0, 0, 30);
                    break;
                case 5:
                    _logger.LoggingInterval = new TimeSpan(0, 1, 0);
                    break;
                case 6:
                    _logger.LoggingInterval = new TimeSpan(0, 2, 0);
                    break;
                case 7:
                    _logger.LoggingInterval = new TimeSpan(0, 5, 0);
                    break;
                case 8:
                    _logger.LoggingInterval = new TimeSpan(0, 10, 0);
                    break;
                case 9:
                    _logger.LoggingInterval = new TimeSpan(0, 30, 0);
                    break;
                case 10:
                    _logger.LoggingInterval = new TimeSpan(1, 0, 0);
                    break;
                case 11:
                    _logger.LoggingInterval = new TimeSpan(2, 0, 0);
                    break;
                case 12:
                    _logger.LoggingInterval = new TimeSpan(6, 0, 0);
                    break;
            }
        };

        _updateInterval = new UserRadioGroup("updateIntervalMenuItem",
                                             2,
                                             new[]
                                             {
                                                 updateInterval250msMenuItem,
                                                 updateInterval500msMenuItem,
                                                 updateInterval1sMenuItem,
                                                 updateInterval2sMenuItem,
                                                 updateInterval5sMenuItem,
                                                 updateInterval10sMenuItem
                                             },
                                             _settings);

        _updateInterval.Changed += (sender, e) =>
        {
            switch (_updateInterval.Value)
            {
                case 0:
                    timer.Interval = 250;
                    break;
                case 1:
                    timer.Interval = 500;
                    break;
                case 2:
                    timer.Interval = 1000;
                    break;
                case 3:
                    timer.Interval = 2000;
                    break;
                case 4:
                    timer.Interval = 5000;
                    break;
                case 5:
                    timer.Interval = 10000;
                    break;
            }
        };

        _smartUpdateCycle = new UserRadioGroup("smartUpdateCycle",
                                               0,
                                               new[]
                                               {
                                                   smartUpdateFollowUpdateIntervalMenuItem,
                                                   smartUpdate10CyclesMenuItem,
                                                   smartUpdate25CyclesMenuItem,
                                                   smartUpdate50CyclesMenuItem,
                                                   smartUpdate100CyclesMenuItem
                                               },
                                               _settings);

        _smartUpdateCycle.Changed += (sender, e) =>
        {
            StorageDevice.SmartUpdateCycleCount = _smartUpdateCycle.Value switch
            {
                1 => 10,
                2 => 25,
                3 => 50,
                4 => 100,
                _ => 1
            };
        };

        _sensorValuesTimeWindow = new UserRadioGroup("sensorValuesTimeWindow",
                                                     10,
                                                     new[]
                                                     {
                                                         timeWindow30sMenuItem,
                                                         timeWindow1minMenuItem,
                                                         timeWindow2minMenuItem,
                                                         timeWindow5minMenuItem,
                                                         timeWindow10minMenuItem,
                                                         timeWindow30minMenuItem,
                                                         timeWindow1hMenuItem,
                                                         timeWindow2hMenuItem,
                                                         timeWindow6hMenuItem,
                                                         timeWindow12hMenuItem,
                                                         timeWindow24hMenuItem
                                                     },
                                                     _settings);

        perSessionFileRotationMenuItem.Checked = _logger.FileRotationMethod == LoggerFileRotation.PerSession;
        dailyFileRotationMenuItem.Checked = _logger.FileRotationMethod == LoggerFileRotation.Daily;

        _sensorValuesTimeWindow.Changed += (sender, e) =>
        {
            TimeSpan timeWindow = TimeSpan.Zero;
            switch (_sensorValuesTimeWindow.Value)
            {
                case 0:
                    timeWindow = new TimeSpan(0, 0, 30);
                    break;
                case 1:
                    timeWindow = new TimeSpan(0, 1, 0);
                    break;
                case 2:
                    timeWindow = new TimeSpan(0, 2, 0);
                    break;
                case 3:
                    timeWindow = new TimeSpan(0, 5, 0);
                    break;
                case 4:
                    timeWindow = new TimeSpan(0, 10, 0);
                    break;
                case 5:
                    timeWindow = new TimeSpan(0, 30, 0);
                    break;
                case 6:
                    timeWindow = new TimeSpan(1, 0, 0);
                    break;
                case 7:
                    timeWindow = new TimeSpan(2, 0, 0);
                    break;
                case 8:
                    timeWindow = new TimeSpan(6, 0, 0);
                    break;
                case 9:
                    timeWindow = new TimeSpan(12, 0, 0);
                    break;
                case 10:
                    timeWindow = new TimeSpan(24, 0, 0);
                    break;
            }

            _computer.Accept(new SensorVisitor(delegate(ISensor sensor) { sensor.ValuesTimeWindow = timeWindow; }));
        };

        InitializeTheme();
        InitializePlotForm();
        InitializeSplitter();

        startupMenuItem.Visible = _startupManager.IsAvailable;

        if (startMinMenuItem.Checked)
        {
            if (!minTrayMenuItem.Checked)
            {
                WindowState = FormWindowState.Minimized;
                Show();
            }
        }
        else
        {
            Show();
        }

        // Create a handle, otherwise calling Close() does not fire FormClosed

        // Make sure the settings are saved when the user logs off
        Microsoft.Win32.SystemEvents.SessionEnded += delegate
        {
            _computer.Close();
            SaveConfiguration();
            if (_runWebServer.Value)
                Server.Quit();
        };

        Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;
    }


    private void BackgroundUpdater_DoWork(object sender, DoWorkEventArgs e)
    {
        _computer.Accept(_updateVisitor);

        if (_logSensors != null && _logSensors.Value && _delayCount >= 4)
            _logger.Log();

        if (_delayCount < 4)
            _delayCount++;

        _plotPanel.InvalidatePlot();
    }

    private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs eventArgs)
    {
        if (eventArgs.Mode == Microsoft.Win32.PowerModes.Resume)
        {
            _computer.Reset();
        }
    }

    private void InitializeTheme()
    {
        mainMenu.Renderer = new ThemedToolStripRenderer();
        treeContextMenu.Renderer = new ThemedToolStripRenderer();
        ThemedVScrollIndicator.AddToControl(treeView);
        ThemedHScrollIndicator.AddToControl(treeView);

        string themeSetting = _settings.GetValue("theme", "auto");
        bool themeSelected = false;

        void ClearThemeMenu()
        {
            foreach (ToolStripItem x in themeMenuItem.DropDownItems)
            {
                if (x is ToolStripMenuItem tmi)
                {
                    tmi.Checked = false;
                }
            }
        }

        if (Theme.SupportsAutoThemeSwitching())
        {
            _autoThemeMenuItem = new ToolStripMenuItem();
            _autoThemeMenuItem.Text = "Auto";
            _autoThemeMenuItem.Click += (o, e) =>
            {
                ClearThemeMenu();
                _autoThemeMenuItem.Checked = true;
                Theme.SetAutoTheme();
                _settings.SetValue("theme", "auto");
                PlotSelectionChanged(o, e);
            };
            themeMenuItem.DropDownItems.Add(_autoThemeMenuItem);
        }

        foreach (Theme theme in Theme.All)
        {
            ToolStripMenuItem item = new ToolStripMenuItem();
            item.Text = theme.DisplayName;
            item.Click += (o, e) =>
            {
                ClearThemeMenu();
                item.Checked = true;
                Theme.Current = theme;
                _settings.SetValue("theme", theme.Id);
                PlotSelectionChanged(o, e);
            };
            themeMenuItem.DropDownItems.Add(item);

            if (themeSetting == theme.Id)
            {
                item.PerformClick();
                themeSelected = true;
            }
        }

        if (!themeSelected)
        {
            themeMenuItem.DropDownItems[0].PerformClick();
        }

        Theme.Current.Apply(this);
    }


    private void InsertSorted(IList<Node> nodes, HardwareNode node)
    {
        int i = 0;
        while (i < nodes.Count && nodes[i] is HardwareNode && ((HardwareNode)nodes[i]).Hardware.HardwareType <= node.Hardware.HardwareType)
            i++;

        nodes.Insert(i, node);
    }

    private void SubHardwareAdded(IHardware hardware, Node node)
    {
        HardwareNode hardwareNode = new(hardware, _settings, _unitManager);
        hardwareNode.PlotSelectionChanged += PlotSelectionChanged;
        InsertSorted(node.Nodes, hardwareNode);
        foreach (IHardware subHardware in hardware.SubHardware)
            SubHardwareAdded(subHardware, hardwareNode);
    }

    private void HardwareAdded(IHardware hardware)
    {
        SubHardwareAdded(hardware, _root);
        PlotSelectionChanged(this, null);
    }

    private void HardwareRemoved(IHardware hardware)
    {
        List<HardwareNode> nodesToRemove = new();
        foreach (Node node in _root.Nodes)
        {
            if (node is HardwareNode hardwareNode && hardwareNode.Hardware == hardware)
                nodesToRemove.Add(hardwareNode);
        }

        foreach (HardwareNode hardwareNode in nodesToRemove)
        {
            _root.Nodes.Remove(hardwareNode);
            hardwareNode.PlotSelectionChanged -= PlotSelectionChanged;
        }

        PlotSelectionChanged(this, null);
    }


    private void Timer_Tick(object sender, EventArgs e)
    {
        treeView.Invalidate();
        _systemTray.Redraw();
        _gadget?.Redraw();

        if (!backgroundUpdater.IsBusy)
            backgroundUpdater.RunWorkerAsync();
    }

    private void SaveConfiguration()
    {
        if (_plotPanel == null || _settings == null)
            return;

        _plotPanel.SetCurrentSettings();

        foreach (TreeColumn column in treeView.Columns)
            _settings.SetValue("treeView.Columns." + column.Header + ".Width", column.Width);

        _settings.SetValue("listenerIp", Server.ListenerIp);
        _settings.SetValue("listenerPort", Server.ListenerPort);
        _settings.SetValue("authenticationEnabled", Server.AuthEnabled);
        _settings.SetValue("authenticationUserName", Server.UserName);
        _settings.SetValue("authenticationPassword", Server.PasswordSHA256);

        string fileName = Path.ChangeExtension(Application.ExecutablePath, ".config");

        try
        {
            _settings.Save(fileName);
        }
        catch (UnauthorizedAccessException)
        {
            MessageBox.Show("Access to the path '" +
                            fileName +
                            "' is denied. " +
                            "The current settings could not be saved.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }
        catch (IOException)
        {
            MessageBox.Show("The path '" +
                            fileName +
                            "' is not writeable. " +
                            "The current settings could not be saved.",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
        }
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        Rectangle newBounds = new()
        {
            X = _settings.GetValue("mainForm.Location.X", Location.X),
            Y = _settings.GetValue("mainForm.Location.Y", Location.Y),
            Width = _settings.GetValue("mainForm.Width", 470),
            Height = _settings.GetValue("mainForm.Height", 640)
        };

        Rectangle fullWorkingArea = new(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);

        foreach (Screen screen in Screen.AllScreens)
            fullWorkingArea = Rectangle.Union(fullWorkingArea, screen.Bounds);

        Rectangle intersection = Rectangle.Intersect(fullWorkingArea, newBounds);
        if (intersection.Width < 20 || intersection.Height < 20 || !_settings.Contains("mainForm.Location.X"))
        {
            newBounds.X = (Screen.PrimaryScreen.WorkingArea.Width / 2) - (newBounds.Width / 2);
            newBounds.Y = (Screen.PrimaryScreen.WorkingArea.Height / 2) - (newBounds.Height / 2);
        }

        Bounds = newBounds;

        RestoreCollapsedNodeState(treeView);

        FormClosed += MainForm_FormClosed;
    }

    private void RestoreCollapsedNodeState(TreeViewAdv treeViewAdv)
    {
        var collapsedHwNodes = treeViewAdv.AllNodes
                                          .Where(n => n.IsExpanded && n.Tag is IExpandPersistNode expandPersistNode && !expandPersistNode.Expanded)
                                          .OrderByDescending(n => n.Level)
                                          .ToList();

        foreach (TreeNodeAdv node in collapsedHwNodes)
        {
            node.IsExpanded = false;
        }
    }


    private void AboutMenuItem_Click(object sender, EventArgs e)
    {
        _ = new AboutBox().ShowDialog();
    }


    private void SaveReportMenuItem_Click(object sender, EventArgs e)
    {
        string report = _computer.GetReport();
        if (saveFileDialog.ShowDialog() == DialogResult.OK)
        {
            using (TextWriter w = new StreamWriter(saveFileDialog.FileName))
            {
                w.Write(report);
            }
        }
    }


    private void CelsiusMenuItem_Click(object sender, EventArgs e)
    {
        celsiusMenuItem.Checked = true;
        fahrenheitMenuItem.Checked = false;
        _unitManager.TemperatureUnit = TemperatureUnit.Celsius;
    }

    private void FahrenheitMenuItem_Click(object sender, EventArgs e)
    {
        celsiusMenuItem.Checked = false;
        fahrenheitMenuItem.Checked = true;
        _unitManager.TemperatureUnit = TemperatureUnit.Fahrenheit;
    }

    private void ResetMinMaxMenuItem_Click(object sender, EventArgs e)
    {
        _computer.Accept(new SensorVisitor(delegate(ISensor sensorClick)
        {
            sensorClick.ResetMin();
            sensorClick.ResetMax();
        }));
    }


}
