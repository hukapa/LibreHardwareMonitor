using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using LibreHardwareMonitor.Hardware;

namespace LibreHardwareMonitor.Windows.Forms.UI.Icons
{
    /// <summary>
    /// Resolution independent icon set drawn with GDI+ on a 16 x 16 logical grid.
    /// Bitmaps are rendered at the screen DPI and carry their logical size in
    /// <see cref="Image.Tag" />, so DPI aware consumers draw them 1:1 without blur.
    /// </summary>
    public static class FluentIcons
    {
        public const int LogicalSize = 16;

        private static readonly Dictionary<string, Image> Cache = new();
        private static float? _screenScale;

        // Category tones chosen to keep contrast on both light and dark surfaces.
        private static readonly Color Blue = Color.FromArgb(0, 120, 212);
        private static readonly Color Teal = Color.FromArgb(0, 153, 153);
        private static readonly Color Green = Color.FromArgb(76, 160, 32);
        private static readonly Color Purple = Color.FromArgb(135, 100, 184);
        private static readonly Color Orange = Color.FromArgb(232, 108, 26);
        private static readonly Color Red = Color.FromArgb(220, 62, 48);
        private static readonly Color Amber = Color.FromArgb(222, 160, 0);
        private static readonly Color Slate = Color.FromArgb(110, 124, 140);
        private static readonly Color Cyan = Color.FromArgb(0, 164, 220);
        private static readonly Color Pink = Color.FromArgb(206, 72, 150);
        private static readonly Color Indigo = Color.FromArgb(92, 107, 210);

        public static float ScreenScale
        {
            get
            {
                if (_screenScale == null)
                {
                    try
                    {
                        using Graphics g = Graphics.FromHwnd(IntPtr.Zero);
                        _screenScale = Math.Max(1f, g.DpiX / 96f);
                    }
                    catch (Exception)
                    {
                        _screenScale = 1f;
                    }
                }

                return _screenScale.Value;
            }
        }

        public static Image Computer => Get("computer", Slate, FluentIconShapes.Computer);

        public static Image ForHardware(HardwareType type)
        {
            return type switch
            {
                HardwareType.Cpu => Get("cpu", Blue, FluentIconShapes.Chip),
                HardwareType.GpuNvidia => Get("gpu-nvidia", Green, FluentIconShapes.GraphicsCard),
                HardwareType.GpuAmd => Get("gpu-amd", Red, FluentIconShapes.GraphicsCard),
                HardwareType.GpuIntel => Get("gpu-intel", Cyan, FluentIconShapes.GraphicsCard),
                HardwareType.Storage => Get("storage", Slate, FluentIconShapes.Drive),
                HardwareType.Motherboard => Get("mainboard", Teal, FluentIconShapes.Board),
                HardwareType.SuperIO or HardwareType.EmbeddedController => Get("superio", Slate, FluentIconShapes.SmallChip),
                HardwareType.Memory => Get("memory", Purple, FluentIconShapes.Memory),
                HardwareType.Network => Get("network", Indigo, FluentIconShapes.Network),
                HardwareType.Cooler => Get("cooler", Teal, FluentIconShapes.Fan),
                HardwareType.Psu => Get("psu", Amber, FluentIconShapes.PowerSupply),
                HardwareType.Battery => Get("battery", Green, FluentIconShapes.Battery),
                HardwareType.PowerMonitor => Get("powermonitor", Amber, FluentIconShapes.Gauge),
                _ => Get("chip-generic", Slate, FluentIconShapes.SmallChip)
            };
        }

        public static Image ForSensorType(SensorType type)
        {
            return type switch
            {
                SensorType.Voltage => Get("voltage", Amber, FluentIconShapes.Bolt),
                SensorType.Current => Get("current", Orange, FluentIconShapes.Bolt),
                SensorType.Conductivity => Get("conductivity", Cyan, FluentIconShapes.Bolt),
                SensorType.Energy => Get("energy", Green, FluentIconShapes.Battery),
                SensorType.Clock => Get("clock", Purple, FluentIconShapes.Gauge),
                SensorType.Frequency => Get("frequency", Purple, FluentIconShapes.Wave),
                SensorType.Load => Get("load", Blue, FluentIconShapes.BarChart),
                SensorType.Temperature => Get("temperature", Red, FluentIconShapes.Thermometer),
                SensorType.Fan => Get("fan", Teal, FluentIconShapes.Fan),
                SensorType.Flow => Get("flow", Cyan, FluentIconShapes.Droplet),
                SensorType.Control => Get("control", Indigo, FluentIconShapes.Sliders),
                SensorType.Level => Get("level", Green, FluentIconShapes.Level),
                SensorType.Power => Get("power", Orange, FluentIconShapes.Plug),
                SensorType.Data => Get("data", Slate, FluentIconShapes.Database),
                SensorType.Factor => Get("factor", Pink, FluentIconShapes.Percent),
                SensorType.Throughput => Get("throughput", Cyan, FluentIconShapes.Transfer),
                SensorType.TimeSpan or SensorType.Timing => Get("time", Indigo, FluentIconShapes.Clock),
                SensorType.Noise => Get("noise", Slate, FluentIconShapes.Speaker),
                SensorType.Humidity => Get("humidity", Cyan, FluentIconShapes.Droplet),
                _ => Get("sensor-generic", Slate, FluentIconShapes.BarChart)
            };
        }

        /// <summary>Renders (or returns a cached copy of) an icon at the screen DPI.</summary>
        public static Image Get(string name, Color color, Action<Graphics, Pen, Brush> shape)
        {
            return Render(name, color, shape, ScreenScale);
        }

        public static Image Render(string name, Color color, Action<Graphics, Pen, Brush> shape, float scale)
        {
            int pixels = (int)Math.Round(LogicalSize * scale);
            string key = name + "|" + color.ToArgb() + "|" + pixels;
            lock (Cache)
            {
                if (Cache.TryGetValue(key, out Image cached))
                    return cached;

                Bitmap bitmap = new(pixels, pixels, PixelFormat.Format32bppPArgb);
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.ScaleTransform(pixels / (float)LogicalSize, pixels / (float)LogicalSize);

                    using Pen pen = new(color, 1.25f) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                    using Brush fill = new SolidBrush(Color.FromArgb(46, color));
                    shape(g, pen, fill);
                }

                bitmap.Tag = new Size(LogicalSize, LogicalSize);
                Cache[key] = bitmap;
                return bitmap;
            }
        }
    }
}
