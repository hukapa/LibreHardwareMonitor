using System.Drawing;
using System.Linq;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    /// <summary>Fluent dark: Mica-like base layer with slightly raised content surfaces.</summary>
    public class DarkTheme : LightTheme
    {
        private readonly Color[] _plotColorPalette;
        public override bool IsDark => true;
        public override Color ForegroundColor => Color.FromArgb(240, 240, 240);
        public override Color BackgroundColor => Color.FromArgb(32, 32, 32);
        public override Color SurfaceColor => Color.FromArgb(43, 43, 43);
        public override Color HyperlinkColor => AccentColor;
        public override Color SelectedForegroundColor => ForegroundColor;
        public override Color SelectedBackgroundColor => Color.FromArgb(55, 55, 55);
        public override Color LineColor => Color.FromArgb(50, 50, 50);
        public override Color StrongLineColor => Color.FromArgb(29, 29, 29);
        public override Color SurfaceStrokeColor => Color.FromArgb(58, 58, 58);
        public override Color[] PlotColorPalette => _plotColorPalette;
        public override Color PlotGridMajorColor => Color.FromArgb(70, 70, 70);
        public override Color PlotGridMinorColor => Color.FromArgb(52, 52, 52);
        public override bool WindowTitlebarFallbackToImmersiveDarkMode => true;

        public DarkTheme() : base("dark", "Dark")
        {
            string[] colors = {
                "#60CDFF",
                "#FF99A4",
                "#6CCB5F",
                "#C5A3F5",
                "#FFB86B",
                "#4DD0C8",
                "#FCE100",
                "#F7A8DA",
                "#8EA6FF",
                "#A6E07A",
                "#E89CF2",
                "#7FD4E8"
            };

            _plotColorPalette = colors.Select(color => ColorTranslator.FromHtml(color)).ToArray();
        }
    }
}
