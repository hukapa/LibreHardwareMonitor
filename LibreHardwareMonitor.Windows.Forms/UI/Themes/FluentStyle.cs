using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    /// <summary>
    /// Shared Fluent (Windows 11) design primitives: typography, system accent color,
    /// color math, rounded geometry and DWM window effects.
    /// </summary>
    public static class FluentStyle
    {
        private static readonly Color DefaultAccent = Color.FromArgb(0, 103, 192);
        private static Font _uiFont;
        private static Font _uiFontSemibold;
        private static Color? _systemAccent;

        /// <summary>Default UI font: Segoe UI Variable Text when installed, otherwise the system message font.</summary>
        public static Font UiFont => _uiFont ??= CreateUiFont(FontStyle.Regular);

        /// <summary>Semibold variant used for headings and group titles.</summary>
        public static Font UiFontSemibold => _uiFontSemibold ??= CreateUiFont(FontStyle.Bold);

        /// <summary>The user's Windows accent color, read once from the DWM registry key.</summary>
        public static Color SystemAccentColor => _systemAccent ??= ReadSystemAccent();

        public static Font CreateUiFont(FontStyle style, float size = 9f)
        {
            string family = style == FontStyle.Bold ? "Segoe UI Variable Text Semibold" : "Segoe UI Variable Text";
            if (HasFamily(family))
                return new Font(family, size, FontStyle.Regular, GraphicsUnit.Point);

            if (style == FontStyle.Bold && HasFamily("Segoe UI Semibold"))
                return new Font("Segoe UI Semibold", size, FontStyle.Regular, GraphicsUnit.Point);

            Font system = SystemFonts.MessageBoxFont;
            return new Font(system.FontFamily, size, style, GraphicsUnit.Point);
        }

        /// <summary>Large display font for headline numbers (KPI values), semibold where available.</summary>
        public static Font CreateDisplayFont(float size)
        {
            if (HasFamily("Segoe UI Variable Display Semib"))
                return new Font("Segoe UI Variable Display Semib", size, FontStyle.Regular, GraphicsUnit.Point);

            return CreateUiFont(FontStyle.Bold, size);
        }

        /// <summary>Linear blend of <paramref name="top" /> over <paramref name="bottom" /> by <paramref name="amount" /> (0..1).</summary>
        public static Color Blend(Color bottom, Color top, double amount)
        {
            amount = Math.Max(0, Math.Min(1, amount));
            return Color.FromArgb(
                (int)Math.Round(bottom.R + ((top.R - bottom.R) * amount)),
                (int)Math.Round(bottom.G + ((top.G - bottom.G) * amount)),
                (int)Math.Round(bottom.B + ((top.B - bottom.B) * amount)));
        }

        /// <summary>Returns the accent adjusted so it keeps contrast against light or dark surfaces.</summary>
        public static Color AccentFor(bool darkSurface)
        {
            Color accent = SystemAccentColor;
            double luminance = RelativeLuminance(accent);
            if (darkSurface && luminance < 0.25)
                return Blend(accent, Color.White, 0.45);

            if (!darkSurface && luminance > 0.45)
                return Blend(accent, Color.Black, 0.35);

            return accent;
        }

        public static double RelativeLuminance(Color color)
        {
            static double Channel(int c)
            {
                double v = c / 255.0;
                return v <= 0.03928 ? v / 12.92 : Math.Pow((v + 0.055) / 1.055, 2.4);
            }

            return (0.2126 * Channel(color.R)) + (0.7152 * Channel(color.G)) + (0.0722 * Channel(color.B));
        }

        public static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            GraphicsPath path = new();
            float diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
            if (diameter <= 0.5f)
            {
                path.AddRectangle(bounds);
                return path;
            }

            RectangleF arc = new(bounds.Location, new SizeF(diameter, diameter));
            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        public static void FillRounded(Graphics g, Color color, RectangleF bounds, float radius)
        {
            using GraphicsPath path = RoundedRectangle(bounds, radius);
            using SolidBrush brush = new(color);
            g.FillPath(brush, path);
        }

        public static void DrawRounded(Graphics g, Color color, RectangleF bounds, float radius, float width = 1f)
        {
            using GraphicsPath path = RoundedRectangle(bounds, radius);
            using Pen pen = new(color, width);
            g.DrawPath(pen, path);
        }

        /// <summary>Asks DWM (Windows 11 build 22000+) to round the window corners and tint its border.</summary>
        public static void ApplyWindowChrome(IntPtr handle, Color borderColor, bool smallCorners = false)
        {
            if (handle == IntPtr.Zero || !IsWindows11())
                return;

            try
            {
                int corner = smallCorners ? DWMWCP_ROUNDSMALL : DWMWCP_ROUND;
                DwmSetWindowAttribute(handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref corner, sizeof(int));

                int border = ColorTranslator.ToWin32(borderColor);
                DwmSetWindowAttribute(handle, DWMWA_BORDER_COLOR, ref border, sizeof(int));
            }
            catch (DllNotFoundException)
            { }
            catch (EntryPointNotFoundException)
            { }
        }

        public static bool IsWindows11()
        {
            return !Software.OperatingSystem.IsUnix && Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= 22000;
        }

        private static bool HasFamily(string name)
        {
            return FontFamily.Families.Any(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private static Color ReadSystemAccent()
        {
            if (Software.OperatingSystem.IsUnix)
                return DefaultAccent;

            try
            {
                // AccentColor is stored as 0xAABBGGRR.
                if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\DWM", "AccentColor", null) is int abgr)
                    return Color.FromArgb(abgr & 0xFF, (abgr >> 8) & 0xFF, (abgr >> 16) & 0xFF);
            }
            catch (System.Security.SecurityException)
            { }

            return DefaultAccent;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWCP_ROUND = 2;
        private const int DWMWCP_ROUNDSMALL = 3;
    }
}
