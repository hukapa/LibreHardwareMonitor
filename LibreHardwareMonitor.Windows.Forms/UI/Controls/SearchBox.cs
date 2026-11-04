using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using LibreHardwareMonitor.Windows.Forms.UI.Themes;

namespace LibreHardwareMonitor.Windows.Forms.UI.Controls
{
    /// <summary>
    /// Fluent search field: rounded surface, magnifier glyph, placeholder text, an accent
    /// underline while focused and a debounced <see cref="SearchChanged" /> event.
    /// </summary>
    public sealed class SearchBox : Control, IThemedControl
    {
        private const int EM_SETCUEBANNER = 0x1501;
        private const int HeightLogical = 32;

        private readonly TextBox _textBox;
        private readonly Timer _debounce;
        private string _placeholder = "Search sensors";

        public SearchBox()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);

            _textBox = new TextBox { BorderStyle = BorderStyle.None, Font = FluentStyle.UiFont };
            _textBox.TextChanged += delegate { _debounce.Stop(); _debounce.Start(); Invalidate(); };
            _textBox.GotFocus += delegate { Invalidate(); };
            _textBox.LostFocus += delegate { Invalidate(); };
            _textBox.KeyDown += TextBox_KeyDown;
            _textBox.HandleCreated += delegate { ApplyPlaceholder(); };
            Controls.Add(_textBox);

            _debounce = new Timer { Interval = 180 };
            _debounce.Tick += delegate
            {
                _debounce.Stop();
                SearchChanged?.Invoke(this, EventArgs.Empty);
            };

            Height = LogicalToDeviceUnits(HeightLogical);
            Cursor = Cursors.IBeam;
        }

        public event EventHandler SearchChanged;

        public string Query => _textBox.Text.Trim();

        public string Placeholder
        {
            get { return _placeholder; }
            set
            {
                _placeholder = value;
                ApplyPlaceholder();
            }
        }

        public void FocusSearch()
        {
            _textBox.Focus();
            _textBox.SelectAll();
        }

        public void Clear()
        {
            _textBox.Clear();
        }

        public void ApplyTheme(Theme theme)
        {
            BackColor = theme.BackgroundColor;
            _textBox.BackColor = theme.SurfaceColor;
            _textBox.ForeColor = theme.ForegroundColor;
            Invalidate();
        }

        private void ApplyPlaceholder()
        {
            if (_textBox.IsHandleCreated && !Software.OperatingSystem.IsUnix)
                SendMessage(_textBox.Handle, EM_SETCUEBANNER, (IntPtr)1, _placeholder);
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                _textBox.Clear();
                e.Handled = e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter)
            {
                _debounce.Stop();
                SearchChanged?.Invoke(this, EventArgs.Empty);
                e.Handled = e.SuppressKeyPress = true;
            }
        }

        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            int left = LogicalToDeviceUnits(34);
            int right = LogicalToDeviceUnits(10);
            _textBox.SetBounds(left, (Height - _textBox.PreferredHeight) / 2 + 1, Math.Max(10, Width - left - right), _textBox.PreferredHeight);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _textBox.Focus();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Theme theme = Theme.Current;
            Graphics g = e.Graphics;
            g.Clear(theme.BackgroundColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            float radius = LogicalToDeviceUnits(theme.CornerRadius - 1);
            RectangleF box = new(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            FluentStyle.FillRounded(g, theme.SurfaceColor, box, radius);
            FluentStyle.DrawRounded(g, theme.SurfaceStrokeColor, box, radius);

            if (_textBox.Focused)
            {
                // Fluent text fields mark focus with an accent bottom edge.
                using GraphicsPath clip = FluentStyle.RoundedRectangle(box, radius);
                g.SetClip(clip);
                float thickness = LogicalToDeviceUnits(2);
                using (Brush accent = new SolidBrush(theme.AccentColor))
                    g.FillRectangle(accent, 0, Height - thickness - 0.5f, Width, thickness + 0.5f);

                g.ResetClip();
            }

            DrawMagnifier(g, theme.MutedForegroundColor);
        }

        private void DrawMagnifier(Graphics g, Color color)
        {
            float scale = DeviceDpi / 96f;
            float size = 11f * scale;
            float x = LogicalToDeviceUnits(12);
            float y = (Height - size) / 2f;
            using Pen pen = new(color, 1.3f * scale) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            float circle = size * 0.72f;
            g.DrawEllipse(pen, x, y, circle, circle);
            g.DrawLine(pen, x + (circle * 0.86f), y + (circle * 0.86f), x + size, y + size);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _debounce.Dispose();

            base.Dispose(disposing);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
    }
}
