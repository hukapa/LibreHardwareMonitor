using System.Drawing;
using System.Linq;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    /// <summary>True black (OLED) variant of the Fluent dark theme.</summary>
    public class BlackTheme : LightTheme
    {
        private readonly Color[] _plotColorPalette;
        public override bool IsDark => true;
        public override Color ForegroundColor => Color.FromArgb(228, 228, 228);
        public override Color BackgroundColor => Color.FromArgb(0, 0, 0);
        public override Color SurfaceColor => Color.FromArgb(14, 14, 14);
        public override Color HyperlinkColor => AccentColor;
        public override Color SelectedForegroundColor => ForegroundColor;
        public override Color SelectedBackgroundColor => Color.FromArgb(30, 30, 30);
        public override Color LineColor => Color.FromArgb(24, 24, 24);
        public override Color StrongLineColor => Color.FromArgb(36, 36, 36);
        public override Color[] PlotColorPalette => _plotColorPalette;
        public override Color PlotGridMajorColor => Color.FromArgb(52, 52, 52);
        public override Color PlotGridMinorColor => Color.FromArgb(28, 28, 28);
        public override bool WindowTitlebarFallbackToImmersiveDarkMode => true;

        public BlackTheme() : base("black", "Black")
        {
            string[] colors = {
                "#4CC2FF",
                "#FF6B7A",
                "#5EE36A",
                "#FFD43B",
                "#B98CFF",
                "#33E0D0",
                "#FF9F45",
                "#FF7AD9",
                "#7C9BFF",
                "#9BE564"
            };

            _plotColorPalette = colors.Select(color => ColorTranslator.FromHtml(color)).ToArray();
        }
    }
}
