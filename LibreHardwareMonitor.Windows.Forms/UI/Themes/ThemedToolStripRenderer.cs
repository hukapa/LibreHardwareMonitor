using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    /// <summary>
    /// Fluent menu renderer: inset rounded highlights, accent check marks, hairline separators
    /// and rounded drop-down windows on Windows 11.
    /// </summary>
    public class ThemedToolStripRenderer : ToolStripRenderer
    {
        protected override void Initialize(ToolStrip toolStrip)
        {
            base.Initialize(toolStrip);
            if (toolStrip is ToolStripDropDown dropDown)
            {
                dropDown.HandleCreated -= DropDown_HandleCreated;
                dropDown.HandleCreated += DropDown_HandleCreated;
                if (dropDown.IsHandleCreated)
                    FluentStyle.ApplyWindowChrome(dropDown.Handle, Theme.Current.SurfaceStrokeColor);
            }
        }

        private static void DropDown_HandleCreated(object sender, EventArgs e)
        {
            if (sender is ToolStripDropDown dropDown)
                FluentStyle.ApplyWindowChrome(dropDown.Handle, Theme.Current.SurfaceStrokeColor);
        }

        private static bool IsTopLevel(ToolStrip toolStrip) => toolStrip is not ToolStripDropDown;

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle bounds = new(Point.Empty, e.Item.Size);
            using (Brush brush = new SolidBrush(Theme.Current.MenuBackgroundColor))
                e.Graphics.FillRectangle(brush, bounds);

            int y = bounds.Height / 2;
            using Pen pen = new(Theme.Current.SurfaceStrokeColor);
            e.Graphics.DrawLine(pen, bounds.Left + 4, y, bounds.Right - 4, y);
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            Color color = e.Item != null && e.Item.Selected ? Theme.Current.MenuSelectedForegroundColor : Theme.Current.MutedForegroundColor;
            float scale = e.Graphics.DpiX / 96f;
            float cx = e.ArrowRectangle.Left + (e.ArrowRectangle.Width / 2f);
            float cy = e.ArrowRectangle.Top + (e.ArrowRectangle.Height / 2f);
            float half = 3f * scale;

            SmoothingMode previous = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new(color, 1.3f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                e.Graphics.DrawLines(pen, new[]
                {
                    new PointF(cx - (half / 2), cy - half),
                    new PointF(cx + (half / 2), cy),
                    new PointF(cx - (half / 2), cy + half)
                });
            }

            e.Graphics.SmoothingMode = previous;
        }

        protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
        {
            float scale = e.Graphics.DpiX / 96f;
            Rectangle r = e.ImageRectangle;
            float size = 10f * scale;
            float x = r.Left + ((r.Width - size) / 2f);
            float y = r.Top + ((r.Height - size) / 2f);

            SmoothingMode previous = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new(Theme.Current.AccentColor, 1.6f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round })
            {
                e.Graphics.DrawLines(pen, new[]
                {
                    new PointF(x, y + (size * 0.55f)),
                    new PointF(x + (size * 0.36f), y + (size * 0.88f)),
                    new PointF(x + size, y + (size * 0.15f))
                });
            }

            e.Graphics.SmoothingMode = previous;
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            bool enabled = e.Item.Enabled;
            e.TextColor = !enabled
                ? Theme.Current.MutedForegroundColor
                : e.Item.Selected ? Theme.Current.MenuSelectedForegroundColor : Theme.Current.MenuForegroundColor;

            base.OnRenderItemText(e);
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            if (IsTopLevel(e.ToolStrip))
                return;

            // Windows 11 draws the rounded DWM border itself; older systems get a flat hairline.
            if (FluentStyle.IsWindows11())
                return;

            Rectangle bounds = new(Point.Empty, new Size(e.ToolStrip.Width - 1, e.ToolStrip.Height - 1));
            using Pen pen = new(Theme.Current.MenuBorderColor);
            e.Graphics.DrawRectangle(pen, bounds);
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            Color color = IsTopLevel(e.ToolStrip) ? Theme.Current.BackgroundColor : Theme.Current.MenuBackgroundColor;
            Rectangle bounds = new(Point.Empty, e.ToolStrip.Size);
            using Brush brush = new SolidBrush(color);
            e.Graphics.FillRectangle(brush, bounds);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using Brush brush = new SolidBrush(Theme.Current.MenuBackgroundColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStrip owner = e.ToolStrip;
            Color background = owner != null && IsTopLevel(owner) ? Theme.Current.BackgroundColor : Theme.Current.MenuBackgroundColor;
            Rectangle bounds = new(Point.Empty, e.Item.Size);
            using (Brush brush = new SolidBrush(background))
                e.Graphics.FillRectangle(brush, bounds);

            bool highlighted = e.Item.Selected || (e.Item is ToolStripMenuItem menuItem && menuItem.DropDown.Visible && IsTopLevel(owner));
            if (!highlighted || !e.Item.Enabled)
                return;

            bool topLevel = owner != null && IsTopLevel(owner);
            float scale = e.Graphics.DpiX / 96f;
            float inset = (topLevel ? 1f : 4f) * scale;
            RectangleF highlight = new(inset, 1f * scale, bounds.Width - (inset * 2), bounds.Height - (2f * scale));
            Color fill = topLevel
                ? FluentStyle.Blend(Theme.Current.BackgroundColor, Theme.Current.ForegroundColor, Theme.Current.IsDark ? 0.09 : 0.06)
                : Theme.Current.MenuSelectedBackgroundColor;

            SmoothingMode previous = e.Graphics.SmoothingMode;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            FluentStyle.FillRounded(e.Graphics, fill, highlight, 4f * scale);
            e.Graphics.SmoothingMode = previous;
        }
    }
}
