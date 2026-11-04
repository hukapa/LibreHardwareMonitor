using System.Drawing;
using System.Drawing.Drawing2D;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI.Icons
{
    /// <summary>
    /// Outline glyphs on a 16 x 16 logical grid. Each shape receives the stroke pen and a
    /// translucent fill brush of the same hue, in the Fluent "regular" icon style.
    /// </summary>
    internal static class FluentIconShapes
    {
        private static void Box(Graphics g, Pen pen, Brush fill, RectangleF r, float radius)
        {
            using GraphicsPath path = FluentStyle.RoundedRectangle(r, radius);
            g.FillPath(fill, path);
            g.DrawPath(pen, path);
        }

        public static void Computer(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(1.5f, 2.5f, 13f, 9f), 1.5f);
            g.DrawLine(pen, 8f, 11.5f, 8f, 13.5f);
            g.DrawLine(pen, 5f, 13.5f, 11f, 13.5f);
        }

        public static void Chip(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(3.5f, 3.5f, 9f, 9f), 1.5f);
            g.DrawRectangle(pen, 6.5f, 6.5f, 3f, 3f);
            foreach (float p in new[] { 6f, 8f, 10f })
            {
                g.DrawLine(pen, p, 1.5f, p, 3.5f);
                g.DrawLine(pen, p, 12.5f, p, 14.5f);
                g.DrawLine(pen, 1.5f, p, 3.5f, p);
                g.DrawLine(pen, 12.5f, p, 14.5f, p);
            }
        }

        public static void SmallChip(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(4f, 4f, 8f, 8f), 1.5f);
            foreach (float p in new[] { 6.5f, 9.5f })
            {
                g.DrawLine(pen, p, 2f, p, 4f);
                g.DrawLine(pen, p, 12f, p, 14f);
                g.DrawLine(pen, 2f, p, 4f, p);
                g.DrawLine(pen, 12f, p, 14f, p);
            }
        }

        public static void GraphicsCard(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(2.5f, 3.5f, 12f, 8f), 1.5f);
            g.DrawEllipse(pen, 6f, 5f, 5f, 5f);
            g.DrawLine(pen, 1f, 2f, 2.5f, 3.5f);
            g.DrawLine(pen, 1f, 2f, 1f, 14f);
            g.DrawLine(pen, 4.5f, 11.5f, 4.5f, 13.5f);
            g.DrawLine(pen, 4.5f, 13.5f, 9.5f, 13.5f);
        }

        public static void Board(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(1.5f, 1.5f, 13f, 13f), 2f);
            g.DrawRectangle(pen, 4f, 4f, 4f, 4f);
            g.DrawLine(pen, 10.5f, 4f, 10.5f, 8f);
            g.DrawLine(pen, 12.5f, 4f, 12.5f, 8f);
            g.DrawLine(pen, 4f, 11f, 12.5f, 11f);
        }

        public static void Memory(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(1.5f, 4.5f, 13f, 6f), 1f);
            g.DrawRectangle(pen, 3.5f, 6.5f, 2f, 2f);
            g.DrawRectangle(pen, 7f, 6.5f, 2f, 2f);
            g.DrawRectangle(pen, 10.5f, 6.5f, 2f, 2f);
            foreach (float x in new[] { 3f, 5f, 7f, 9f, 11f, 13f })
                g.DrawLine(pen, x, 10.5f, x, 12.5f);
        }

        public static void Drive(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(1.5f, 3.5f, 13f, 9f), 2f);
            g.DrawLine(pen, 1.5f, 9f, 14.5f, 9f);
            g.FillEllipse(pen.Brush, 11f, 10f, 1.6f, 1.6f);
            g.DrawLine(pen, 4f, 10.8f, 7.5f, 10.8f);
        }

        public static void Network(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(5.5f, 1.5f, 5f, 4f), 1f);
            Box(g, pen, fill, new RectangleF(1.5f, 10.5f, 5f, 4f), 1f);
            Box(g, pen, fill, new RectangleF(9.5f, 10.5f, 5f, 4f), 1f);
            g.DrawLine(pen, 8f, 5.5f, 8f, 8f);
            g.DrawLine(pen, 4f, 8f, 12f, 8f);
            g.DrawLine(pen, 4f, 8f, 4f, 10.5f);
            g.DrawLine(pen, 12f, 8f, 12f, 10.5f);
        }

        public static void Fan(Graphics g, Pen pen, Brush fill)
        {
            g.FillEllipse(fill, 1.5f, 1.5f, 13f, 13f);
            g.DrawEllipse(pen, 1.5f, 1.5f, 13f, 13f);
            for (int i = 0; i < 3; i++)
            {
                GraphicsState state = g.Save();
                g.TranslateTransform(8f, 8f);
                g.RotateTransform(i * 120f);
                g.DrawBezier(pen, 0f, -1.4f, 0.5f, -4.5f, 3.5f, -5f, 3.6f, -2.6f);
                g.Restore(state);
            }

            g.DrawEllipse(pen, 6.8f, 6.8f, 2.4f, 2.4f);
        }

        public static void PowerSupply(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(1.5f, 3.5f, 13f, 9f), 1.5f);
            g.DrawLines(pen, new[] { new PointF(8.8f, 5f), new PointF(6.5f, 8.3f), new PointF(9.2f, 8.3f), new PointF(7f, 11.2f) });
        }

        public static void Battery(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(1.5f, 4.5f, 11.5f, 7f), 1.5f);
            g.DrawLine(pen, 14.5f, 6.8f, 14.5f, 9.2f);
            g.FillRectangle(pen.Brush, 3.5f, 6.5f, 4.5f, 3f);
        }

        public static void Gauge(Graphics g, Pen pen, Brush fill)
        {
            using GraphicsPath dial = new();
            dial.AddArc(1.5f, 2.5f, 13f, 13f, 150, 240);
            g.DrawPath(pen, dial);
            g.DrawLine(pen, 8f, 9f, 11f, 5.8f);
            g.FillEllipse(pen.Brush, 6.8f, 7.8f, 2.4f, 2.4f);
        }

        public static void Bolt(Graphics g, Pen pen, Brush fill)
        {
            PointF[] bolt = { new(9.5f, 1.5f), new(3.5f, 9f), new(7.8f, 9f), new(6.5f, 14.5f), new(12.5f, 6.8f), new(8.2f, 6.8f) };
            g.FillPolygon(fill, bolt);
            g.DrawPolygon(pen, bolt);
        }

        public static void Wave(Graphics g, Pen pen, Brush fill)
        {
            g.DrawBezier(pen, 1.5f, 8f, 3.5f, 1.5f, 5.5f, 1.5f, 8f, 8f);
            g.DrawBezier(pen, 8f, 8f, 10.5f, 14.5f, 12.5f, 14.5f, 14.5f, 8f);
        }

        public static void BarChart(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(2f, 8.5f, 3f, 5.5f), 0.8f);
            Box(g, pen, fill, new RectangleF(6.5f, 5f, 3f, 9f), 0.8f);
            Box(g, pen, fill, new RectangleF(11f, 2f, 3f, 12f), 0.8f);
        }

        public static void Thermometer(Graphics g, Pen pen, Brush fill)
        {
            using GraphicsPath body = new();
            body.AddArc(6f, 1.5f, 4f, 4f, 180, 180);
            body.AddLine(10f, 3.5f, 10f, 9.2f);
            body.AddArc(4.5f, 8.2f, 7f, 7f, -55, 290);
            body.AddLine(6f, 9.2f, 6f, 3.5f);
            body.CloseFigure();
            g.FillPath(fill, body);
            g.DrawPath(pen, body);
            g.FillEllipse(pen.Brush, 6.4f, 10.1f, 3.2f, 3.2f);
            g.DrawLine(pen, 8f, 5f, 8f, 10.5f);
        }

        public static void Droplet(Graphics g, Pen pen, Brush fill)
        {
            using GraphicsPath drop = new();
            drop.AddBezier(8f, 1.5f, 6f, 4.5f, 3f, 7.5f, 3f, 10f);
            drop.AddArc(3f, 5f, 10f, 9.5f, 180, -180);
            drop.AddBezier(13f, 10f, 13f, 7.5f, 10f, 4.5f, 8f, 1.5f);
            drop.CloseFigure();
            g.FillPath(fill, drop);
            g.DrawPath(pen, drop);
        }

        public static void Sliders(Graphics g, Pen pen, Brush fill)
        {
            foreach (float y in new[] { 4f, 8f, 12f })
                g.DrawLine(pen, 1.5f, y, 14.5f, y);

            foreach (PointF knob in new[] { new PointF(10f, 4f), new PointF(5f, 8f), new PointF(11.5f, 12f) })
            {
                g.FillEllipse(fill, knob.X - 1.8f, knob.Y - 1.8f, 3.6f, 3.6f);
                g.DrawEllipse(pen, knob.X - 1.8f, knob.Y - 1.8f, 3.6f, 3.6f);
            }
        }

        public static void Level(Graphics g, Pen pen, Brush fill)
        {
            Box(g, pen, fill, new RectangleF(3.5f, 1.5f, 9f, 13f), 2f);
            g.FillRectangle(pen.Brush, 5.5f, 8.5f, 5f, 4f);
            g.DrawLine(pen, 5.5f, 6.5f, 10.5f, 6.5f);
        }

        public static void Plug(Graphics g, Pen pen, Brush fill)
        {
            g.DrawLine(pen, 5.5f, 1.5f, 5.5f, 4.5f);
            g.DrawLine(pen, 10.5f, 1.5f, 10.5f, 4.5f);
            using GraphicsPath head = new();
            head.AddLine(3f, 4.5f, 13f, 4.5f);
            head.AddLine(13f, 4.5f, 13f, 7.5f);
            head.AddArc(3f, 3f, 10f, 9f, 0, 180);
            head.CloseFigure();
            g.FillPath(fill, head);
            g.DrawPath(pen, head);
            g.DrawLine(pen, 8f, 12f, 8f, 14.5f);
        }

        public static void Database(Graphics g, Pen pen, Brush fill)
        {
            g.FillRectangle(fill, 2.5f, 3.5f, 11f, 9f);
            g.FillEllipse(fill, 2.5f, 1.5f, 11f, 4f);
            g.FillEllipse(fill, 2.5f, 10.5f, 11f, 4f);
            g.DrawEllipse(pen, 2.5f, 1.5f, 11f, 4f);
            g.DrawArc(pen, 2.5f, 6f, 11f, 4f, 0, 180);
            g.DrawArc(pen, 2.5f, 10.5f, 11f, 4f, 0, 180);
            g.DrawLine(pen, 2.5f, 3.5f, 2.5f, 12.5f);
            g.DrawLine(pen, 13.5f, 3.5f, 13.5f, 12.5f);
        }

        public static void Percent(Graphics g, Pen pen, Brush fill)
        {
            g.DrawLine(pen, 12.5f, 2.5f, 3.5f, 13.5f);
            g.FillEllipse(fill, 2f, 2f, 4.5f, 4.5f);
            g.DrawEllipse(pen, 2f, 2f, 4.5f, 4.5f);
            g.FillEllipse(fill, 9.5f, 9.5f, 4.5f, 4.5f);
            g.DrawEllipse(pen, 9.5f, 9.5f, 4.5f, 4.5f);
        }

        public static void Transfer(Graphics g, Pen pen, Brush fill)
        {
            g.DrawLine(pen, 5f, 13.5f, 5f, 2.5f);
            g.DrawLines(pen, new[] { new PointF(2f, 5.5f), new PointF(5f, 2.5f), new PointF(8f, 5.5f) });
            g.DrawLine(pen, 11f, 2.5f, 11f, 13.5f);
            g.DrawLines(pen, new[] { new PointF(8f, 10.5f), new PointF(11f, 13.5f), new PointF(14f, 10.5f) });
        }

        public static void Clock(Graphics g, Pen pen, Brush fill)
        {
            g.FillEllipse(fill, 1.5f, 1.5f, 13f, 13f);
            g.DrawEllipse(pen, 1.5f, 1.5f, 13f, 13f);
            g.DrawLines(pen, new[] { new PointF(8f, 4.5f), new PointF(8f, 8f), new PointF(10.5f, 9.5f) });
        }

        public static void Speaker(Graphics g, Pen pen, Brush fill)
        {
            PointF[] cone = { new(1.5f, 6f), new(4.5f, 6f), new(8.5f, 2.5f), new(8.5f, 13.5f), new(4.5f, 10f), new(1.5f, 10f) };
            g.FillPolygon(fill, cone);
            g.DrawPolygon(pen, cone);
            g.DrawArc(pen, 7f, 5f, 4.5f, 6f, -60, 120);
            g.DrawArc(pen, 7f, 2.5f, 7.5f, 11f, -60, 120);
        }
    }
}
