using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Aga.Controls.Tree;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    /// <summary>
    /// Fluent rendering hooks for <see cref="TreeViewAdv" />: chevron expanders, rounded
    /// check boxes, flat column headers and accent-tinted selection.
    /// </summary>
    internal static class ThemedTreeRenderer
    {
        private static Pen _rowSeparatorPen;
        private static Brush _selectedRowBrush;

        public static void Install(Theme theme)
        {
            TreeViewAdv.CustomPlusMinusRenderFunc = DrawChevron;
            TreeViewAdv.CustomCheckRenderFunc = DrawCheckBox;
            TreeViewAdv.CustomColumnBackgroundRenderFunc = DrawColumnBackground;
            TreeViewAdv.CustomColumnTextRenderFunc = DrawColumnText;

            _rowSeparatorPen?.Dispose();
            _selectedRowBrush?.Dispose();
            _rowSeparatorPen = new Pen(theme.TreeRowSepearatorColor);
            _selectedRowBrush = new SolidBrush(theme.TreeSelectedBackgroundColor);

            TreeViewAdv.CustomHorizontalLinePen = _rowSeparatorPen;
            TreeViewAdv.CustomSelectedRowBrush = _selectedRowBrush;
            TreeViewAdv.CustomSelectedTextColor = theme.TreeSelectedTextColor;
        }

        private static float Scale(Graphics g) => g.DpiX / 96f;

        private static void DrawChevron(Graphics g, Rectangle rect, bool isExpanded)
        {
            Theme theme = Theme.Current;
            float scale = Scale(g);
            float half = 3.5f * scale;
            float cx = rect.Left + (rect.Width / 2f) - (1f * scale);
            float cy = rect.Top + (rect.Height / 2f);

            using (Brush brush = new SolidBrush(theme.TreeBackgroundColor))
                g.FillRectangle(brush, rect);

            PointF[] points = isExpanded
                ? new[] { new PointF(cx - half, cy - (half / 2)), new PointF(cx, cy + (half / 2)), new PointF(cx + half, cy - (half / 2)) }
                : new[] { new PointF(cx - (half / 2), cy - half), new PointF(cx + (half / 2), cy), new PointF(cx - (half / 2), cy + half) };

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new(theme.MutedForegroundColor, 1.4f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
                g.DrawLines(pen, points);

            g.SmoothingMode = previous;
        }

        private static void DrawCheckBox(Graphics g, Rectangle rect, bool isChecked)
        {
            Theme theme = Theme.Current;
            float scale = Scale(g);
            float size = Math.Min(rect.Width, 13 * scale) - 1;
            RectangleF box = new(rect.Left + 0.5f, rect.Top + ((rect.Height - size) / 2f), size, size);
            float radius = 3f * scale;

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (isChecked)
            {
                FluentStyle.FillRounded(g, theme.AccentColor, box, radius);
                using Pen pen = new(theme.AccentForegroundColor, 1.5f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round };
                g.DrawLines(pen, new[]
                {
                    new PointF(box.Left + (box.Width * 0.24f), box.Top + (box.Height * 0.52f)),
                    new PointF(box.Left + (box.Width * 0.43f), box.Top + (box.Height * 0.70f)),
                    new PointF(box.Left + (box.Width * 0.77f), box.Top + (box.Height * 0.32f))
                });
            }
            else
            {
                FluentStyle.FillRounded(g, theme.TreeBackgroundColor, box, radius);
                FluentStyle.DrawRounded(g, theme.MutedForegroundColor, box, radius, Math.Max(1f, scale));
            }

            g.SmoothingMode = previous;
        }

        private static void DrawColumnBackground(Graphics g, Rectangle rect, bool isPressed, bool isHot)
        {
            Theme theme = Theme.Current;
            Color fill = isPressed ? theme.SelectedBackgroundColor : isHot ? theme.HoverBackgroundColor : theme.TreeBackgroundColor;
            using (Brush brush = new SolidBrush(fill))
                g.FillRectangle(brush, rect.Left, rect.Top, rect.Width, rect.Height + 1);

            using Pen pen = new(theme.StrongLineColor);
            g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
        }

        private static void DrawColumnText(Graphics g, Rectangle rect, Font font, string text)
        {
            int padding = (int)Math.Round(8 * Scale(g));
            Rectangle textRect = new(rect.Left + padding, rect.Top, Math.Max(0, rect.Width - (padding * 2)), rect.Height);
            TextRenderer.DrawText(g, text, FluentStyle.UiFontSemibold, textRect, Theme.Current.MutedForegroundColor,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
