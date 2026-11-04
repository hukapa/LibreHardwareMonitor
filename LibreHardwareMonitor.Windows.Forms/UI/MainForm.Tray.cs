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
    private void ExitClick(object sender, EventArgs e)
    {
        CloseApplication();
    }


    private void CloseApplication()
    {
        FormClosed -= MainForm_FormClosed;

        Visible = false;
        _systemTray.IsMainIconEnabled = false;
        timer.Enabled = false;
        _computer.Close();
        SaveConfiguration();
        if (_runWebServer.Value)
            Server.Quit();

        _systemTray.Dispose();
        timer.Dispose();
        backgroundUpdater.Dispose();

        Application.Exit();
    }

    private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
    {
        CloseApplication();
    }


    private void SysTrayHideShow()
    {
        Visible = !Visible;
        if (Visible)
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;
            Activate();
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && _minimizeOnClose.Value)
        {
            e.Cancel = true;
            Visible = false;
            if (!_systemTray.IsMainIconEnabled)
                _systemTray.IsMainIconEnabled = true;
            return;
        }

        base.OnFormClosing(e);
    }

    protected override void WndProc(ref Message m)
    {
        const int WM_SYSCOMMAND = 0x112;
        const int WM_WININICHANGE = 0x001A;
        const int SC_MINIMIZE = 0xF020;
        const int SC_CLOSE = 0xF060;

        int sysCmd = (int)(m.WParam.ToInt64() & 0xFFF0);
        if (_minimizeToTray.Value && m.Msg == WM_SYSCOMMAND && sysCmd == SC_MINIMIZE)
        {
            SysTrayHideShow();
        }
        else if (m.Msg == WM_WININICHANGE && Marshal.PtrToStringUni(m.LParam) == "ImmersiveColorSet" && _autoThemeMenuItem?.Checked == true)
        {
            Theme.SetAutoTheme();
        }
        else if (_minimizeOnClose.Value && m.Msg == WM_SYSCOMMAND && sysCmd == SC_CLOSE)
        {
            SysTrayHideShow();
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    private void HideShowClick(object sender, EventArgs e)
    {
        SysTrayHideShow();
    }


    private void ResetClick(object sender, EventArgs e)
    {
        // disable the fallback MainIcon during reset, otherwise icon visibility
        // might be lost
        _systemTray.IsMainIconEnabled = false;
        _computer.Reset();
        // restore the MainIcon setting
        _systemTray.IsMainIconEnabled = _minimizeToTray.Value;
    }

}
