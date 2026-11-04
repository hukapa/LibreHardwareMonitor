using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using LibreHardwareMonitor.Hardware;
using LibreHardwareMonitor.Windows.Forms.UI.Icons;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI.Controls
{
    /// <summary>
    /// Strip of Fluent KPI cards (title, headline value, detail line and progress bar) that
    /// adapts the number of visible cards to the available width.
    /// </summary>
    public sealed class SummaryPanel : Control, IThemedControl
    {
        private const int CardLogicalHeight = 104;
        private const int CardLogicalMinWidth = 150;
        private const int GapLogical = 12;

        private readonly List<SummaryMetric> _metrics = new();
        private readonly List<Rectangle> _cardBounds = new();
        private Font _valueFont;
        private Font _detailFont;
        private int _hoverIndex = -1;

        public SummaryPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Height = LogicalToDeviceUnits(CardLogicalHeight + GapLogical);
        }

        /// <summary>Raised with the card's hardware when a card is clicked.</summary>
        public event EventHandler<IHardware> CardClicked;

        /// <summary>Returns true when temperatures must be shown in Fahrenheit.</summary>
        public Func<bool> UseFahrenheit { get; set; } = () => false;

        public bool HasMetrics => _metrics.Count > 0;

        public void SetMetrics(IEnumerable<SummaryMetric> metrics)
        {
            _metrics.Clear();
            _metrics.AddRange(metrics);
            Invalidate();
        }

        public void ApplyTheme(Theme theme)
        {
            BackColor = theme.BackgroundColor;
            Invalidate();
        }

        private Font ValueFont => _valueFont ??= FluentStyle.CreateDisplayFont(17f);

        private Font DetailFont => _detailFont ??= FluentStyle.CreateUiFont(FontStyle.Regular, 8.25f);

        private void LayoutCards()
        {
            _cardBounds.Clear();
            if (_metrics.Count == 0)
                return;

            int gap = LogicalToDeviceUnits(GapLogical);
            int height = LogicalToDeviceUnits(CardLogicalHeight);
            int minWidth = LogicalToDeviceUnits(CardLogicalMinWidth);
            int count = Math.Max(1, Math.Min(_metrics.Count, (Width + gap) / (minWidth + gap)));
            int width = (Width - (gap * (count - 1))) / count;

            for (int i = 0; i < count; i++)
                _cardBounds.Add(new Rectangle(i * (width + gap), 0, width, height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme theme = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(theme.BackgroundColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            LayoutCards();
            for (int i = 0; i < _cardBounds.Count; i++)
                DrawCard(g, theme, _metrics[i], _cardBounds[i], i == _hoverIndex);
        }

        private void DrawCard(Graphics g, Theme theme, SummaryMetric metric, Rectangle bounds, bool hover)
        {
            float radius = LogicalToDeviceUnits(theme.CornerRadius + 2);
            RectangleF card = new(bounds.X + 0.5f, bounds.Y + 0.5f, bounds.Width - 1.5f, bounds.Height - 1.5f);
            Color surface = hover ? theme.HoverBackgroundColor : theme.SurfaceColor;
            FluentStyle.FillRounded(g, surface, card, radius);
            FluentStyle.DrawRounded(g, theme.SurfaceStrokeColor, card, radius);

            int pad = LogicalToDeviceUnits(14);
            int icon = LogicalToDeviceUnits(16);
            int x = bounds.X + pad;
            int innerWidth = bounds.Width - (pad * 2);
            int y = bounds.Y + pad;

            g.DrawImage(IconFor(metric), new Rectangle(x, y, icon, icon));
            Rectangle title = new(x + icon + LogicalToDeviceUnits(8), y - LogicalToDeviceUnits(2), innerWidth - icon - LogicalToDeviceUnits(8), icon + LogicalToDeviceUnits(4));
            TextRenderer.DrawText(g, metric.Title, FluentStyle.UiFont, title, theme.MutedForegroundColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            float? value = metric.Sensor.Value;
            SeverityLevel level = SensorSeverity.Classify(metric.Sensor);
            Color valueColor = SensorSeverity.ColorFor(level, theme, theme.ForegroundColor);
            y += icon + LogicalToDeviceUnits(6);
            Rectangle valueRect = new(x - LogicalToDeviceUnits(1), y, innerWidth, ValueFont.Height + LogicalToDeviceUnits(2));
            TextRenderer.DrawText(g, FormatValue(metric.Sensor.SensorType, value), ValueFont, valueRect, valueColor, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis);

            y = valueRect.Bottom + LogicalToDeviceUnits(2);
            Rectangle detail = new(x, y, innerWidth, DetailFont.Height + LogicalToDeviceUnits(2));
            TextRenderer.DrawText(g, metric.Detail(), DetailFont, detail, theme.MutedForegroundColor, TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            float? ratio = SensorSeverity.Ratio(metric.Sensor.SensorType, value);
            if (ratio == null)
                return;

            float barHeight = Math.Max(3f, LogicalToDeviceUnits(4));
            RectangleF track = new(x, bounds.Bottom - pad - barHeight + LogicalToDeviceUnits(2), innerWidth, barHeight);
            FluentStyle.FillRounded(g, FluentStyle.Blend(surface, theme.ForegroundColor, 0.1), track, barHeight / 2);
            if (ratio.Value > 0)
            {
                RectangleF fill = new(track.X, track.Y, Math.Max(barHeight, track.Width * ratio.Value), barHeight);
                FluentStyle.FillRounded(g, SensorSeverity.ColorFor(level, theme, theme.AccentColor), fill, barHeight / 2);
            }
        }

        private static Image IconFor(SummaryMetric metric)
        {
            return metric.Sensor.SensorType is SensorType.Temperature or SensorType.Power
                ? FluentIcons.ForSensorType(metric.Sensor.SensorType)
                : FluentIcons.ForHardware(metric.Hardware.HardwareType);
        }

        private string FormatValue(SensorType type, float? value)
        {
            if (!value.HasValue || float.IsNaN(value.Value))
                return "-";

            CultureInfo culture = CultureInfo.CurrentCulture;
            float v = value.Value;
            return type switch
            {
                SensorType.Load => string.Format(culture, "{0:F0} %", v),
                SensorType.Temperature when UseFahrenheit() => string.Format(culture, "{0:F0} °F", (v * 1.8f) + 32f),
                SensorType.Temperature => string.Format(culture, "{0:F0} °C", v),
                SensorType.Power => string.Format(culture, "{0:F1} W", v),
                _ => string.Format(culture, "{0:F1}", v)
            };
        }

        private int HitTest(Point point)
        {
            return _cardBounds.FindIndex(r => r.Contains(point));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int index = HitTest(e.Location);
            if (index == _hoverIndex)
                return;

            _hoverIndex = index;
            Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hoverIndex = -1;
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = HitTest(e.Location);
            if (e.Button == MouseButtons.Left && index >= 0 && index < _metrics.Count)
                CardClicked?.Invoke(this, _metrics[index].Hardware);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _valueFont?.Dispose();
                _detailFont?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
