using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI.Controls
{
    /// <summary>A navigation entry of the <see cref="SidebarPanel" />.</summary>
    public sealed class SidebarItem
    {
        public SidebarItem(string text, Image image, object tag)
        {
            Text = text;
            Image = image;
            Tag = tag;
        }

        public string Text { get; }
        public Image Image { get; }
        public object Tag { get; }
    }

    /// <summary>
    /// Fluent navigation pane: rounded hover and selection highlights with an accent pill,
    /// a section caption, mouse wheel scrolling and an icon-only compact mode.
    /// </summary>
    public sealed class SidebarPanel : Control, IThemedControl
    {
        private const int ItemLogicalHeight = 36;
        private const int CaptionLogicalHeight = 34;

        private readonly List<SidebarItem> _items = new();
        private int _selectedIndex = -1;
        private int _hoverIndex = -1;
        private int _scrollOffset;
        private bool _compact;

        public SidebarPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            Caption = "Devices";
        }

        public event EventHandler SelectionChanged;

        public string Caption { get; set; }

        public IReadOnlyList<SidebarItem> Items => _items;

        public SidebarItem SelectedItem => _selectedIndex >= 0 && _selectedIndex < _items.Count ? _items[_selectedIndex] : null;

        public bool Compact
        {
            get { return _compact; }
            set
            {
                if (_compact == value)
                    return;

                _compact = value;
                Invalidate();
            }
        }

        /// <summary>Replaces the items, keeping the selection on the item with the same tag when possible.</summary>
        public void SetItems(IEnumerable<SidebarItem> items, object selectedTag)
        {
            _items.Clear();
            _items.AddRange(items);
            _selectedIndex = _items.FindIndex(i => Equals(i.Tag, selectedTag));
            if (_selectedIndex < 0 && _items.Count > 0)
                _selectedIndex = 0;

            _hoverIndex = -1;
            ClampScroll();
            Invalidate();
        }

        /// <summary>Selects the item carrying <paramref name="tag" /> and raises <see cref="SelectionChanged" /> when it changes.</summary>
        public void SelectTag(object tag)
        {
            int index = _items.FindIndex(i => Equals(i.Tag, tag));
            if (index < 0 || index == _selectedIndex)
                return;

            _selectedIndex = index;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyTheme(Theme theme)
        {
            BackColor = theme.BackgroundColor;
            ForeColor = theme.ForegroundColor;
            Invalidate();
        }

        private int ItemHeight => LogicalToDeviceUnits(ItemLogicalHeight);

        private int ItemsTop => _compact ? LogicalToDeviceUnits(8) : LogicalToDeviceUnits(CaptionLogicalHeight);

        private Rectangle ItemBounds(int index)
        {
            int margin = LogicalToDeviceUnits(6);
            return new Rectangle(margin, ItemsTop + (index * ItemHeight) - _scrollOffset, Width - (margin * 2), ItemHeight - LogicalToDeviceUnits(2));
        }

        private int HitTest(Point point)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (ItemBounds(i).Contains(point))
                    return i;
            }

            return -1;
        }

        private void ClampScroll()
        {
            int content = ItemsTop + (_items.Count * ItemHeight) + LogicalToDeviceUnits(8);
            _scrollOffset = Math.Max(0, Math.Min(_scrollOffset, content - Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme theme = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(theme.BackgroundColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (!_compact && !string.IsNullOrEmpty(Caption))
            {
                Rectangle caption = new(LogicalToDeviceUnits(16), 0, Width - LogicalToDeviceUnits(24), ItemsTop);
                TextRenderer.DrawText(g, Caption, FluentStyle.UiFontSemibold, caption, theme.MutedForegroundColor,
                                      TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
            }

            for (int i = 0; i < _items.Count; i++)
            {
                Rectangle bounds = ItemBounds(i);
                if (bounds.Bottom < 0 || bounds.Top > Height)
                    continue;

                DrawItem(g, theme, _items[i], bounds, i == _selectedIndex, i == _hoverIndex);
            }
        }

        private void DrawItem(Graphics g, Theme theme, SidebarItem item, Rectangle bounds, bool selected, bool hover)
        {
            float radius = LogicalToDeviceUnits(4);
            if (selected || hover)
            {
                Color fill = FluentStyle.Blend(theme.BackgroundColor, theme.ForegroundColor, selected ? (theme.IsDark ? 0.10 : 0.07) : (theme.IsDark ? 0.05 : 0.035));
                FluentStyle.FillRounded(g, fill, bounds, radius);
            }

            if (selected)
            {
                int pillHeight = LogicalToDeviceUnits(16);
                RectangleF pill = new(bounds.Left, bounds.Top + ((bounds.Height - pillHeight) / 2f), LogicalToDeviceUnits(3), pillHeight);
                FluentStyle.FillRounded(g, theme.AccentColor, pill, pill.Width / 2f);
            }

            int iconSize = LogicalToDeviceUnits(16);
            int iconLeft = _compact ? bounds.Left + ((bounds.Width - iconSize) / 2) : bounds.Left + LogicalToDeviceUnits(12);
            if (item.Image != null)
                g.DrawImage(item.Image, new Rectangle(iconLeft, bounds.Top + ((bounds.Height - iconSize) / 2), iconSize, iconSize));

            if (_compact)
                return;

            int textLeft = iconLeft + iconSize + LogicalToDeviceUnits(12);
            Rectangle text = new(textLeft, bounds.Top, bounds.Right - textLeft - LogicalToDeviceUnits(6), bounds.Height);
            TextRenderer.DrawText(g, item.Text, selected ? FluentStyle.UiFontSemibold : FluentStyle.UiFont, text, theme.ForegroundColor,
                                  TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            int index = HitTest(e.Location);
            if (index == _hoverIndex)
                return;

            _hoverIndex = index;
            Cursor = index >= 0 ? Cursors.Hand : Cursors.Default;
            UpdateToolTip(index);
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
            if (e.Button != MouseButtons.Left)
                return;

            int index = HitTest(e.Location);
            if (index < 0 || index == _selectedIndex)
                return;

            _selectedIndex = index;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            _scrollOffset -= e.Delta / 120 * ItemHeight;
            ClampScroll();
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            ClampScroll();
        }

        private ToolTip _toolTip;

        private void UpdateToolTip(int index)
        {
            _toolTip ??= new ToolTip { ShowAlways = true };
            _toolTip.SetToolTip(this, index >= 0 ? _items[index].Text : null);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _toolTip?.Dispose();

            base.Dispose(disposing);
        }
    }
}
