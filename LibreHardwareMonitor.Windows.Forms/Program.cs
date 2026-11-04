// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System;
using System.IO;
using System.Reflection;
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

    private static bool IsAssemblyAvailable(string assemblyName)
    {
        try
        {
            Assembly[] loaded = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < loaded.Length; i++)
            {
                if (string.Equals(loaded[i].GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            Assembly asm = Assembly.Load(new AssemblyName(assemblyName));
            if (asm != null)
                return true;
        }
        catch
        {
        }

        string path = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar;
        if (File.Exists(path + assemblyName + ".dll"))
            return true;

        MessageBox.Show("The following component could not be found: " + assemblyName +
                        "\nPlease extract all files from the archive.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
        return false;
    }

    private static bool AllRequiredFilesAvailable()
    {
        if (!IsAssemblyAvailable("Aga.Controls"))
            return false;

        if (!IsAssemblyAvailable("LibreHardwareMonitorLib"))
            return false;

        if (!IsAssemblyAvailable("OxyPlot"))
            return false;

        if (!IsAssemblyAvailable("OxyPlot.WindowsForms"))
            return false;

        return true;
    }
}
