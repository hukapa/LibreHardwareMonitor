// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.IO;
using System.Windows.Forms;
using LibreHardwareMonitor.Windows.Forms.UI;

namespace LibreHardwareMonitor.Windows.Forms;

public static class Program
{
    [STAThread]
    public static void Main()
    {
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            if (e.ExceptionObject is Exception ex)
            {
                try
                {
                    File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), $"[{DateTime.Now}] Unhandled Exception: {ex}\n");
                    MessageBox.Show($"Ocorreu um erro ao iniciar o LibreHardwareMonitor:\n\n{ex.Message}\n\nConsulte crash.log para mais detalhes.", "Erro LibreHardwareMonitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch { }
            }
        };

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (s, e) =>
        {
            try
            {
                File.AppendAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log"), $"[{DateTime.Now}] Thread Exception: {e.Exception}\n");
                MessageBox.Show($"Ocorreu um erro no LibreHardwareMonitor:\n\n{e.Exception.Message}\n\nConsulte crash.log para mais detalhes.", "Erro LibreHardwareMonitor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch { }
        };

        if (!AllRequiredFilesAvailable())
            Environment.Exit(0);

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        using (MainForm form = new MainForm())
        {
            form.FormClosed += delegate
            {
                Application.Exit();
            };
            Application.Run();
        }
    }

    private static bool IsFileAvailable(string fileName)
    {
        string path = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar;
        if (!File.Exists(path + fileName))
        {
            MessageBox.Show("The following file could not be found: " + fileName +
                            "\nPlease extract all files from the archive.", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        return true;
    }

    private static bool AllRequiredFilesAvailable()
    {
        if (!IsFileAvailable("Aga.Controls.dll"))
            return false;

        if (!IsFileAvailable("LibreHardwareMonitorLib.dll"))
            return false;

        if (!IsFileAvailable("OxyPlot.dll"))
            return false;

        if (!IsFileAvailable("OxyPlot.WindowsForms.dll"))
            return false;

        return true;
    }
}
