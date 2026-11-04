using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI.Controls
{
    /// <summary>
    /// Container drawn as a Fluent card: a rounded surface with a hairline stroke on top of the
    /// window background. Its padding keeps square child controls inside the rounded corners.
    /// </summary>
    public sealed class CardPanel : Panel, IThemedControl
    {
        public CardPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Padding = new Padding(LogicalToDeviceUnits(4));
        }

        public void ApplyTheme(Theme theme)
        {
            BackColor = theme.SurfaceColor;
            foreach (Control child in Controls)
                theme.Apply(child);

            Invalidate();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            Theme theme = Theme.Current;
            e.Graphics.Clear(theme.BackgroundColor);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float radius = LogicalToDeviceUnits(theme.CornerRadius + 2);
            RectangleF bounds = new(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            FluentStyle.FillRounded(e.Graphics, theme.SurfaceColor, bounds, radius);
            FluentStyle.DrawRounded(e.Graphics, theme.SurfaceStrokeColor, bounds, radius);
        }
    }
}
