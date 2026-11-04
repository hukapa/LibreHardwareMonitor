// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// Copyright (C) LibreHardwareMonitor and Contributors.
// Partial Copyright (C) Michael Möller <mmoeller@openhardwaremonitor.org> and Contributors.
// All Rights Reserved.

using System.Drawing;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Windows.Forms.UI.Icons;

namespace LibreHardwareMonitor.Windows.Forms.UI;

/// <summary>
/// Hardware type icon lookup, backed by the resolution independent <see cref="FluentIcons" /> set.
/// </summary>
public class HardwareTypeImage
{
    private HardwareTypeImage() { }

    public static HardwareTypeImage Instance { get; } = new HardwareTypeImage();

    public Image GetImage(HardwareType hardwareType)
    {
        return FluentIcons.ForHardware(hardwareType);
    }
}
