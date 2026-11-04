using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Aga.Controls.Tree;
using Microsoft.Win32;

namespace LibreHardwareMonitor.Windows.Forms.UI.Themes
{
    public abstract class Theme
    {
        private static Theme _current = new LightTheme();
        public static Theme Current
        {
            get { return _current; }
            set
            {
                _current = value;
                foreach (Form form in Application.OpenForms)
                {
                    _current.Apply(form);
                }

                Init();
            }
        }

        private static void Init()
        {
            ThemedTreeRenderer.Install(_current);
        }

        private static List<Theme> _all;
        public static List<Theme> All
        {
            get
            {
                if (_all == null)
                {
                    _all = new List<Theme>();
                    foreach (Type type in typeof(Theme).Assembly.GetTypes())
                    {
                        if (type != typeof(Theme) && typeof(Theme).IsAssignableFrom(type))
                        {
                            _all.Add((Theme)type.GetConstructor(new Type[] { }).Invoke(new object[] { }));
                        }
                    }
                }
                return _all.OrderBy(x => x.DisplayName).ToList();
            }
        }

        public static bool SupportsAutoThemeSwitching()
        {
            if (Software.OperatingSystem.IsUnix)
            {
                return false;
            }

            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", -1) is int useLightTheme)
            {
                return useLightTheme != -1;
            }
            return false;
        }

        public static void SetAutoTheme()
        {
            if (Software.OperatingSystem.IsUnix)
            {
                return;
            }

            if (Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 1) is int useLightTheme)
            {
                if (useLightTheme > 0)
                {
                    Current = new LightTheme();
                }
                else
                {
                    Current = new DarkTheme();
                }
            }
            else
            {
                // Fallback incase registry fails
                Current = new LightTheme();
            }
        }

        public Theme(string id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public abstract Color BackgroundColor { get; }
        public abstract Color ForegroundColor { get; }
        public abstract Color HyperlinkColor { get; }
        public abstract Color LineColor { get; }
        public abstract Color StrongLineColor { get; }
        public abstract Color SelectedBackgroundColor { get; }
        public abstract Color SelectedForegroundColor { get; }

        // fluent design tokens
        public virtual bool IsDark => false;
        public virtual Color AccentColor => FluentStyle.AccentFor(IsDark);
        public virtual Color AccentForegroundColor => FluentStyle.RelativeLuminance(AccentColor) > 0.4 ? Color.Black : Color.White;
        public virtual Color SurfaceColor => BackgroundColor;
        public virtual Color SurfaceStrokeColor => StrongLineColor;
        public virtual Color MutedForegroundColor => FluentStyle.Blend(ForegroundColor, SurfaceColor, 0.42);
        public virtual Color HoverBackgroundColor => FluentStyle.Blend(SurfaceColor, ForegroundColor, IsDark ? 0.06 : 0.04);
        public virtual Color StateCoolColor => IsDark ? Color.FromArgb(108, 203, 95) : Color.FromArgb(15, 123, 15);
        public virtual Color StateWarmColor => IsDark ? Color.FromArgb(252, 225, 0) : Color.FromArgb(157, 93, 0);
        public virtual Color StateHotColor => IsDark ? Color.FromArgb(255, 153, 164) : Color.FromArgb(196, 43, 28);
        public virtual int CornerRadius => 6;

        // button
        public virtual Color ButtonBackgroundColor => BackgroundColor;
        public virtual Color ButtonBorderColor => ForegroundColor;
        public virtual Color ButtonHoverBackgroundColor => SelectedBackgroundColor;
        public virtual Color ButtonPressedBackgroundColor => LineColor;
        public virtual Color ButtonTextColor => ForegroundColor;

        // menu
        public virtual Color MenuBackgroundColor => SurfaceColor;
        public virtual Color MenuBorderColor => StrongLineColor;
        public virtual Color MenuForegroundColor => ForegroundColor;
        public virtual Color MenuSelectedBackgroundColor => HoverBackgroundColor;
        public virtual Color MenuSelectedForegroundColor => SelectedForegroundColor;

        // plot
        public virtual Color PlotBackgroundColor => SurfaceColor;
        public virtual Color PlotBorderColor => StrongLineColor;
        public abstract Color[] PlotColorPalette { get; }
        public abstract Color PlotGridMajorColor { get; }
        public abstract Color PlotGridMinorColor { get; }
        public virtual Color PlotTextColor => MutedForegroundColor;

        // scrollbar
        public virtual Color ScrollbarBackground => SurfaceColor;
        public virtual Color ScrollbarTrack => FluentStyle.Blend(SurfaceColor, ForegroundColor, 0.28);

        // splitter
        public virtual Color SplitterColor => BackgroundColor;
        public virtual Color SplitterHoverColor => SelectedBackgroundColor;

        // tree
        public virtual Color TreeBackgroundColor => SurfaceColor;
        public virtual Color TreeOutlineColor => MutedForegroundColor;
        public virtual Color TreeSelectedBackgroundColor => FluentStyle.Blend(SurfaceColor, AccentColor, IsDark ? 0.24 : 0.14);
        public virtual Color TreeTextColor => ForegroundColor;
        public virtual Color TreeSelectedTextColor => SelectedForegroundColor;
        public virtual Color TreeRowSepearatorColor => LineColor;

        // window
        public virtual Color WindowTitlebarBackgroundColor => BackgroundColor;
        public abstract bool WindowTitlebarFallbackToImmersiveDarkMode { get; }
        public virtual Color WindowTitlebarForegroundColor => ForegroundColor;

        public void Apply(Form form)
        {
            if (IsWindows10OrGreater(22000))
            {
                // Windows 11, Set the titlebar color based on theme
                int color = ColorTranslator.ToWin32(WindowTitlebarBackgroundColor);
                DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref color, sizeof(int));
                color = ColorTranslator.ToWin32(WindowTitlebarForegroundColor);
                DwmSetWindowAttribute(form.Handle, DWMWA_TEXT_COLOR, ref color, sizeof(int));
                FluentStyle.ApplyWindowChrome(form.Handle, SurfaceStrokeColor);
            }
            else if (IsWindows10OrGreater(17763))
            {
                // Windows 10, fallback to using "Immersive Dark Mode" instead
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                {
                    // Windows 10 20H1 or later
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                }
                int useImmersiveDarkMode = WindowTitlebarFallbackToImmersiveDarkMode ? 1 : 0;
                DwmSetWindowAttribute(form.Handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int));
            }
            form.BackColor = BackgroundColor;
            foreach (Control control in form.Controls)
            {
                Apply(control);
            }
        }

        public void Apply(Control control)
        {
            if (control is IThemedControl themedControl)
            {
                themedControl.ApplyTheme(this);
            }
            else if (control is Button button)
            {
                button.ForeColor = ButtonTextColor;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderColor = ButtonBorderColor;
                button.FlatAppearance.MouseOverBackColor = ButtonHoverBackgroundColor;
                button.FlatAppearance.MouseDownBackColor = ButtonPressedBackgroundColor;
            }
            else if (control is LinkLabel linkLabel)
            {
                linkLabel.LinkColor = HyperlinkColor;
            }
            else if (control is PlotPanel plotPanel)
            {
                plotPanel.ApplyTheme();
            }
            else if (control is TreeViewAdv treeView)
            {
                treeView.BackColor = TreeBackgroundColor;
                treeView.ForeColor = TreeTextColor;
                treeView.LineColor = TreeOutlineColor;
            }
            else
            {
                control.BackColor = BackgroundColor;
                control.ForeColor = ForegroundColor;
            }

            foreach (Control child in control.Controls)
            {
                Apply(child);
            }
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWA_TEXT_COLOR = 36;

        private static bool IsWindows10OrGreater(int build = -1)
        {
            return !Software.OperatingSystem.IsUnix && Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
        }
    }

    /// <summary>Controls that paint themselves from the active <see cref="Theme" /> tokens.</summary>
    public interface IThemedControl
    {
        void ApplyTheme(Theme theme);
    }
}
