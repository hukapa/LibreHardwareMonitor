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
    public bool AuthWebServerMenuItemChecked
    {
        get { return authWebServerMenuItem.Checked; }
        set { authWebServerMenuItem.Checked = value; }
    }

    public HttpServer Server { get; }


    private void ServerInterfacePortMenuItem_Click(object sender, EventArgs e)
    {
        new InterfacePortForm(this).ShowDialog();
    }

    private void AuthWebServerMenuItem_Click(object sender, EventArgs e)
    {
        new AuthForm(this).ShowDialog();
    }

}
