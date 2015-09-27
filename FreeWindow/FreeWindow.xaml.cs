using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Draw = System.Drawing;

// ReSharper disable UnusedMember.Local

namespace FreeWindow
{
    /// <summary>
    /// Interaction logic for FreeWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class TheFreeWindow : Window
    {
        public TheFreeWindow()
        {
            InitializeComponent();

            StartMaximize = false;
            IsResizable = true;
            Title = "MENU";

            AddMenuItem("Exit", s => { Close(); });

            InitializeFreeWindow();
        }

        #region FreeWindow

        protected void InitializeFreeWindow()
        {
            IsMaximized = false;
            _nBorderSize = BorderSize;
            Loaded += InitializeWindowSource;
        }

        #region DependencyProperties
        public static readonly DependencyProperty BorderSizeProperty = DependencyProperty.Register(
            "BorderSize", typeof (Thickness), typeof (TheFreeWindow), new PropertyMetadata(new Thickness(8)));

        public static readonly DependencyProperty BorderShadowSizeProperty = DependencyProperty.Register(
            "BorderShadowSize", typeof (Thickness), typeof (TheFreeWindow), new PropertyMetadata(new Thickness(8)));

        public static readonly DependencyProperty TitleBarHeightProperty = DependencyProperty.Register(
            "TitleBarHeight", typeof (double), typeof (TheFreeWindow), new PropertyMetadata(30.0));

        public static readonly DependencyProperty TitleBarBackgroundProperty = DependencyProperty.Register(
            "TitleBarBackground", typeof (Brush), typeof (TheFreeWindow), new PropertyMetadata(Brushes.White));

        public static readonly DependencyProperty TitleBarForegroundProperty = DependencyProperty.Register(
            "TitleBarForeground", typeof (Brush), typeof (TheFreeWindow), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty BorderShadowBrushProperty = DependencyProperty.Register(
            "BorderShadowBrush", typeof (SolidColorBrush), typeof (TheFreeWindow), new PropertyMetadata(Brushes.LightBlue));

        public static readonly DependencyProperty HasMenuProperty = DependencyProperty.Register(
            "HasMenu", typeof (bool), typeof (TheFreeWindow), new PropertyMetadata(true));

        public bool HasMenu
        {
            get { return (bool) GetValue(HasMenuProperty); }
            set { SetValue(HasMenuProperty, value); }
        }
        public Brush TitleBarForeground
        {
            get { return (Brush) GetValue(TitleBarForegroundProperty); }
            set { SetValue(TitleBarForegroundProperty, value); }
        }
        public Brush TitleBarBackground
        {
            get { return (Brush) GetValue(TitleBarBackgroundProperty); }
            set { SetValue(TitleBarBackgroundProperty, value); }
        }
        public double TitleBarHeight
        {
            get { return (double) GetValue(TitleBarHeightProperty); }
            set { SetValue(TitleBarHeightProperty, value); }
        }
        public Thickness BorderSize
        {
            get { return (Thickness) GetValue(BorderSizeProperty); }
            set { SetValue(BorderSizeProperty, value); }
        }
        public Thickness BorderShadowSize
        {
            get { return (Thickness)GetValue(BorderShadowSizeProperty); }
            set { SetValue(BorderShadowSizeProperty, value); }
        }
        public SolidColorBrush BorderShadowBrush
        {
            get { return (SolidColorBrush)GetValue(BorderShadowBrushProperty); }
            set { SetValue(BorderShadowBrushProperty, value); }
        }
        #endregion

        #region Variables

        /// <summary>
        /// Stores the border mesures
        /// </summary>
        private Thickness _nBorderSize;
        /// <summary>
        /// Stores the window position 0 = Left, 1 = Top, 2 = With, 3 = Height
        /// </summary>
        private double[] _nWindowState = { 10.0, 10.0, 400.0, 300.0 };

        /// <summary>
        /// Window mensage code
        /// </summary>
        // ReSharper disable once InconsistentNaming
        protected const int WM_SYSCOMMAND = 0x112;
        protected HwndSource HwndSource;

        protected Dictionary<ResizeDirection, Cursor> Cursors = new Dictionary<ResizeDirection, Cursor>
        {
            {ResizeDirection.Top, System.Windows.Input.Cursors.SizeNS},
            {ResizeDirection.Bottom, System.Windows.Input.Cursors.SizeNS},
            {ResizeDirection.Left, System.Windows.Input.Cursors.SizeWE},
            {ResizeDirection.Right, System.Windows.Input.Cursors.SizeWE},
            {ResizeDirection.TopLeft, System.Windows.Input.Cursors.SizeNWSE},
            {ResizeDirection.BottomRight, System.Windows.Input.Cursors.SizeNWSE},
            {ResizeDirection.TopRight, System.Windows.Input.Cursors.SizeNESW},
            {ResizeDirection.BottomLeft, System.Windows.Input.Cursors.SizeNESW},
            {ResizeDirection.None, System.Windows.Input.Cursors.Arrow }
        };

        protected enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
            None = 9
        }
        #endregion

        #region Propertys

        /// <summary>
        /// Gets a value indicating whether this window is maximized.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is maximized; otherwise, <c>false</c>.
        /// </value>
        public bool IsMaximized { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this window is resisable.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is resisable; otherwise, <c>false</c>.
        /// </value>
        public bool IsResizable { get; set; }

        public bool StartMaximize { get; set; }
        #endregion

        #region TaskBar
        public enum TaskbarPosition
        {
            Unknown = -1,
            Left,
            Top,
            Right,
            Bottom
        }
        private enum Abm : uint
        {
            New = 0x00000000,
            Remove = 0x00000001,
            QueryPos = 0x00000002,
            SetPos = 0x00000003,
            GetState = 0x00000004,
            GetTaskbarPos = 0x00000005,
            Activate = 0x00000006,
            GetAutoHideBar = 0x00000007,
            SetAutoHideBar = 0x00000008,
            WindowPosChanged = 0x00000009,
            SetState = 0x0000000A
        }
        private enum Abe : uint
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }
        private static class Abs
        {
            public const int Autohide = 0x0000001;
            public const int AlwaysOnTop = 0x0000002;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct AppBarData
        {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public Abe uEdge;
            public Rect rc;
            public int lParam;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        private static class Shell32
        {
            [DllImport("shell32.dll", SetLastError = true)]
            public static extern IntPtr SHAppBarMessage(Abm dwMessage, [In] ref AppBarData pData);
        }
        private static class User32
        {
            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        }
        public sealed class Taskbar
        {
            /// <summary>
            /// The name id  of task bar
            /// </summary>
            private const string ClassName = "Shell_TrayWnd";

            public Draw.Rectangle Bounds { get; }
            public TaskbarPosition Position { get; }
            public Draw.Point Location => Bounds.Location;
            /// <summary>
            /// 0 = Width
            /// 1 = Heigth
            /// </summary>
            public double[] Size { get; }

            //Always returns false under Windows 7
            public bool AlwaysOnTop
            {
                get;
            }
            public bool AutoHide
            {
                get;
            }

            public Taskbar()
            {
                var taskbarHandle = User32.FindWindow(ClassName, null);

                var data = new AppBarData
                {
                    cbSize = (uint) Marshal.SizeOf(typeof (AppBarData)),
                    hWnd = taskbarHandle
                };
                var result = Shell32.SHAppBarMessage(Abm.GetTaskbarPos, ref data);
                if (result == IntPtr.Zero)
                    throw new InvalidOperationException();

                //Get DPI SCALE VALUE
                var source = PresentationSource.FromVisual(Application.Current.MainWindow)?.CompositionTarget?.TransformToDevice;
                var dpiScale = (source?.M11 ?? 1.0) - 1;

                Position = (TaskbarPosition)data.uEdge;
                Bounds = Draw.Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

                Size = new double[2];
                Size[0] = Bounds.Width - Bounds.Width*dpiScale;
                Size[1] = Bounds.Height - Bounds.Height*dpiScale;

                data.cbSize = (uint)Marshal.SizeOf(typeof(AppBarData));
                result = Shell32.SHAppBarMessage(Abm.GetState, ref data);
                var state = result.ToInt32();
                AlwaysOnTop = (state & Abs.AlwaysOnTop) == Abs.AlwaysOnTop;
                AutoHide = (state & Abs.Autohide) == Abs.Autohide;
            }
        }
        #endregion

        #region Tools
        public static class Tools
        {
            /// <summary>
            /// Gets the height of the window.
            /// </summary>
            /// <returns></returns>
            public static double GetWindowHeight()
            {
                var height = SystemParameters.PrimaryScreenHeight;

                var bar = new Taskbar();
                if (bar.Position == TaskbarPosition.Top || bar.Position == TaskbarPosition.Bottom)
                    return height - bar.Size[1];

                return height;
            }
            /// <summary>
            /// Gets the width of the window.
            /// </summary>
            /// <returns></returns>
            public static double GetWindowWidth()
            {
                var with = SystemParameters.PrimaryScreenWidth;

                var bar = new Taskbar();
                if (bar.Position == TaskbarPosition.Left || bar.Position == TaskbarPosition.Right)
                    return with - bar.Size[0];

                return with;
            }
            /// <summary>
            /// Gets the window left postition.
            /// </summary>
            /// <returns></returns>
            public static double GetWindowLeftPostition()
            {
                double pos = 0;

                var bar = new Taskbar();
                if (bar.Position == TaskbarPosition.Left)
                    pos += bar.Size[0];

                return pos;
            }
            /// <summary>
            /// Gets the window top postition.
            /// </summary>
            /// <returns></returns>
            public static double GetWindowTopPostition()
            {
                double pos = 0;

                var bar = new Taskbar();
                if (bar.Position == TaskbarPosition.Top)
                    pos += bar.Size[1];

                return pos;
            }
        }
        #endregion

        #region ResizeMethods
        /// <summary>
        /// Sends system message.
        /// Method imported from user32.dll
        /// </summary>
        /// <param name="hWnd">The h WND.</param>
        /// <param name="msg">The message</param>
        /// <param name="wParam">The w parameter.</param>
        /// <param name="lParam">The l parameter.</param>
        /// <returns></returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        protected static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Initializes the HwndSource of this window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void InitializeWindowSource(object sender, EventArgs e)
        {
            HwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            SizeChanged += OnSizeChanged;
            if(StartMaximize) MaximizeWindow(null, null);
        }

        /// <summary>
        /// Drags the window if is not maximized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void DragWindow(object sender, MouseButtonEventArgs e)
        {
            if (IsMaximized) return;
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        /// <summary>
        /// Resizes if is resizable and if is not maximized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        protected void ResizeIfPressed(object sender, MouseEventArgs e)
        {
            if (IsMaximized || !IsResizable)
                return;

            var element = sender as FrameworkElement;
            if(element == null) return;
            var direction = GetDirectionFromName(element.Name);

            Cursor = Cursors[direction];

            if (e.LeftButton == MouseButtonState.Pressed)
                ResizeWindow(direction);
        }

        /// <summary>
        /// Gets the name of the direction from the border element name
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected static ResizeDirection GetDirectionFromName(string name)
        {
            var enumName = name.Replace("Border", "");
            try
            {
                return (ResizeDirection) Enum.Parse(typeof (ResizeDirection), enumName);
            }
            catch
            {
                return ResizeDirection.None;
            }
        }

        /// <summary>
        /// Resets the cursor if is not over a Resize border element
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        protected void ResetCursor(object sender, MouseEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed) return;

            Cursor = Cursors[ResizeDirection.None];
        }

        /// <summary>
        /// Send the resize command using user32.dll method
        /// </summary>
        /// <param name="direction">The direction.</param>
        protected void ResizeWindow(ResizeDirection direction)
        {
            if (IsMaximized || !IsResizable)
                return;

            SendMessage(HwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
        }
        /// <summary>
        /// Maximizes the window respecting the toolbar
        /// </summary>
        private bool Maximize()
        {
            _nWindowState = new [] {Left, Top, ActualWidth, ActualHeight};
            Left = Tools.GetWindowLeftPostition();
            Top = Tools.GetWindowTopPostition();
            Width = Tools.GetWindowWidth();
            Height = Tools.GetWindowHeight();
            return true;
        }
        /// <summary>
        /// Restores the window to previous state
        /// </summary>
        private bool Restore()
        {
            Left = _nWindowState[0];
            Top = _nWindowState[1];
            Width = _nWindowState[2];
            Height = _nWindowState[3];
            return false;
        }
        #endregion

        #region Actions
        /// <summary>
        /// Closes the window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected void CloseWindow(object sender, RoutedEventArgs e)
        {
            Close();
        }
        /// <summary>
        /// Maximizes the window.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected void MaximizeWindow(object sender, RoutedEventArgs e)
        {
            _nBorderSize = IsMaximized ? _nBorderSize : BorderSize;
            BorderSize = IsMaximized ? _nBorderSize : new Thickness(0);

            IsMaximized = IsMaximized ? Restore() : Maximize();
        }

        /// <summary>
        /// Minimizes Window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void MinimezeWindow(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        /// <summary>
        /// Called when [size changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }
        /// <summary>
        /// Call when show/hide menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ShowMenu(object sender, RoutedEventArgs e)
        {
            FwMenuPopup.IsOpen = !FwMenuPopup.IsOpen;
        }
        #endregion
        
        #region Menu
        protected void AddMenuItem(string name, Action<string> action)
        {
            var btn = new Button
            {
                Content = name,
                Style = Resources["FwBtnStyleBase"] as Style
            };
            btn.Click += (obj, e)=> { action(name); };
            FwStackMenu.Children.Add(btn);
        }
        #endregion

        #endregion
    }
}
