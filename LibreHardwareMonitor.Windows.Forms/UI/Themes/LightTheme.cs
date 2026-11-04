using System.Drawing;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    /// <summary>Fluent light: layered neutral window background with white content surfaces.</summary>
    public class LightTheme : Theme
    {
        private readonly Color[] _plotColorPalette = new Color[] {
            ColorTranslator.FromHtml("#0067C0"),
            ColorTranslator.FromHtml("#C42B1C"),
            ColorTranslator.FromHtml("#0F7B0F"),
            ColorTranslator.FromHtml("#8764B8"),
            ColorTranslator.FromHtml("#CA5010"),
            ColorTranslator.FromHtml("#038387"),
            ColorTranslator.FromHtml("#9D5D00"),
            ColorTranslator.FromHtml("#E3008C"),
            ColorTranslator.FromHtml("#4F6BED"),
            ColorTranslator.FromHtml("#498205"),
            ColorTranslator.FromHtml("#B146C2"),
            ColorTranslator.FromHtml("#005B70"),
            ColorTranslator.FromHtml("#A4262C")
        };

        public LightTheme(string id, string displayName) : base(id, displayName) { }
        public override Color ForegroundColor => Color.FromArgb(27, 27, 27);
        public override Color BackgroundColor => Color.FromArgb(243, 243, 243);
        public override Color SurfaceColor => Color.FromArgb(255, 255, 255);
        public override Color HyperlinkColor => AccentColor;
        public override Color SelectedForegroundColor => ForegroundColor;
        public override Color SelectedBackgroundColor => Color.FromArgb(234, 234, 234);
        public override Color LineColor => Color.FromArgb(240, 240, 240);
        public override Color StrongLineColor => Color.FromArgb(229, 229, 229);
        public override Color[] PlotColorPalette => _plotColorPalette;
        public override Color PlotGridMajorColor => Color.FromArgb(222, 222, 222);
        public override Color PlotGridMinorColor => Color.FromArgb(240, 240, 240);
        public override bool WindowTitlebarFallbackToImmersiveDarkMode => false;

        public LightTheme() : base("light", "Light") { }
    }
}
