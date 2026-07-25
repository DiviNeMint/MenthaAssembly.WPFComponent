using MenthaAssembly.Devices;
using MenthaAssembly.Win32;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace MenthaAssembly.MarkupExtensions
{
    public static partial class WindowEx
    {
        private static readonly DependencyProperty WindowRegistrationProperty =
            DependencyProperty.RegisterAttached("WindowRegistration", typeof(WindowRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static WindowRegistrationLease AcquireWindowRegistration(Window Window)
        {
            if (Window.GetValue(WindowRegistrationProperty) is WindowRegistration Registration)
                return Registration.Acquire();

            Registration = new WindowRegistration(Window);
            Window.SetValue(WindowRegistrationProperty, Registration);
            return Registration.Acquire();
        }

        #region FixSize
        public static readonly DependencyProperty FixSizeProperty =
            DependencyProperty.RegisterAttached("FixSize", typeof(bool), typeof(WindowEx), new PropertyMetadata(false, OnFixSizeChanged));
        public static bool GetFixSize(Window obj)
            => (bool)obj.GetValue(FixSizeProperty);
        public static void SetFixSize(Window obj, bool value)
            => obj.SetValue(FixSizeProperty, value);

        private static readonly DependencyProperty FixSizeRegistrationProperty =
            DependencyProperty.RegisterAttached("FixSizeRegistration", typeof(FixSizeRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static void OnFixSizeChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Window Window)
                return;

            if (Window.GetValue(FixSizeRegistrationProperty) is FixSizeRegistration Registration)
                Registration.Dispose();

            Window.ClearValue(FixSizeRegistrationProperty);
            if ((bool)e.NewValue)
                Window.SetValue(FixSizeRegistrationProperty, new FixSizeRegistration(Window));
        }

        private static unsafe IntPtr FixSizeWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Win32Messages)msg)
            {
                case Win32Messages.WM_GetMinMaxInfo:
                    {
                        ScreenInfo Info = Screen.GetScreenByWindow(hwnd) ?? Screen.Current;
                        if (Info is not null)
                        {
                            int Width = Info.WorkArea.Right - Info.WorkArea.Left,
                                Height = Info.WorkArea.Bottom - Info.WorkArea.Top;

                            WindowMinMaxInfo* pInfo = (WindowMinMaxInfo*)lParam;
                            pInfo->ptMaxPosition = new Point<int>(Info.WorkArea.Left - Info.Bound.Left,
                                                                  Info.WorkArea.Top - Info.Bound.Top);

                            pInfo->ptMaxSize = new Size<int>(Width, Height);
                            pInfo->ptMaxTrackSize = pInfo->ptMaxSize;
                        }
                        break;
                    }
            }

            return IntPtr.Zero;
        }

        public static void FixSize(this Window This)
            => SetFixSize(This, true);

        #endregion

        #region DisableMaximize
        private const int SC_Maximize = 0xF030;

        public static readonly DependencyProperty DisableMaximizeProperty =
            DependencyProperty.RegisterAttached("DisableMaximize", typeof(bool), typeof(WindowEx), new PropertyMetadata(false, OnWindowStyleFeatureChanged));
        public static bool GetDisableMaximize(Window obj)
            => (bool)obj.GetValue(DisableMaximizeProperty);
        public static void SetDisableMaximize(Window obj, bool value)
            => obj.SetValue(DisableMaximizeProperty, value);

        private static IntPtr DisableMaximizeWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Win32Messages)msg)
            {
                case Win32Messages.WM_SysCommand:
                    {
                        if ((wParam.ToInt32() & 0xFFF0) == SC_Maximize)
                        {
                            handled = true;
                            return IntPtr.Zero;
                        }
                        break;
                    }
                case Win32Messages.WM_NCLButtonDoubldClick:
                    {
                        if ((WindowHitTests)wParam.ToInt32() == WindowHitTests.Caption)
                        {
                            handled = true;
                            return IntPtr.Zero;
                        }
                        break;
                    }
            }

            return IntPtr.Zero;
        }

        #endregion

        #region DisableMinimize
        private const int SC_Minimize = 0xF020;

        public static readonly DependencyProperty DisableMinimizeProperty =
            DependencyProperty.RegisterAttached("DisableMinimize", typeof(bool), typeof(WindowEx), new PropertyMetadata(false, OnWindowStyleFeatureChanged));
        public static bool GetDisableMinimize(Window obj)
            => (bool)obj.GetValue(DisableMinimizeProperty);
        public static void SetDisableMinimize(Window obj, bool value)
            => obj.SetValue(DisableMinimizeProperty, value);

        private static readonly DependencyProperty WindowStyleRegistrationProperty =
            DependencyProperty.RegisterAttached("WindowStyleRegistration", typeof(WindowStyleRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static void OnWindowStyleFeatureChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Window Window)
                return;

            if (Window.GetValue(WindowStyleRegistrationProperty) is WindowStyleRegistration Registration)
            {
                Registration.Update();
                if (!GetDisableMaximize(Window) &&
                    !GetDisableMinimize(Window))
                {
                    Registration.Dispose();
                    Window.ClearValue(WindowStyleRegistrationProperty);
                }
                return;
            }

            if (GetDisableMaximize(Window) ||
                GetDisableMinimize(Window))
                Window.SetValue(WindowStyleRegistrationProperty, new WindowStyleRegistration(Window));
        }

        private static IntPtr DisableMinimizeWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch ((Win32Messages)msg)
            {
                case Win32Messages.WM_SysCommand:
                    {
                        if ((wParam.ToInt32() & 0xFFF0) == SC_Minimize)
                        {
                            handled = true;
                            return IntPtr.Zero;
                        }
                        break;
                    }
            }

            return IntPtr.Zero;
        }

        #endregion

        #region Windows 11 Rounded Corners
        public static readonly DependencyProperty EnableWindows11RoundedCornersProperty =
            DependencyProperty.RegisterAttached("EnableWindows11RoundedCorners", typeof(bool), typeof(WindowEx),
                new PropertyMetadata(false, OnEnableWindows11RoundedCornersChanged));
        public static bool GetEnableWindows11RoundedCorners(Window Window)
            => (bool)Window.GetValue(EnableWindows11RoundedCornersProperty);
        public static void SetEnableWindows11RoundedCorners(Window Window, bool Value)
            => Window.SetValue(EnableWindows11RoundedCornersProperty, Value);

        private static readonly DependencyProperty Windows11RoundedCornersRegistrationProperty =
            DependencyProperty.RegisterAttached("Windows11RoundedCornersRegistration", typeof(Windows11RoundedCornersRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static void OnEnableWindows11RoundedCornersChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Window Window)
                return;

            if (Window.GetValue(Windows11RoundedCornersRegistrationProperty) is Windows11RoundedCornersRegistration Registration)
                Registration.Dispose();

            Window.ClearValue(Windows11RoundedCornersRegistrationProperty);
            if ((bool)e.NewValue &&
                IsWindows11RoundedCornersSupported())
                Window.SetValue(Windows11RoundedCornersRegistrationProperty, new Windows11RoundedCornersRegistration(Window));
        }

        private static unsafe bool TrySetWindows11CornerPreference(Window Window, DwmWindowCornerPreference Preference)
        {
            if (!IsWindows11RoundedCornersSupported())
                return false;

            IntPtr Handle = new WindowInteropHelper(Window).Handle;
            return Handle != IntPtr.Zero &&
                   Desktop.DwmSetWindowAttribute(Handle, DwmWindowAttribute.WindowCornerPreference, &Preference, sizeof(DwmWindowCornerPreference)) == 0;
        }

        private static bool IsWindows11RoundedCornersSupported()
            => TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", out object BuildNumberValue) &&
               int.TryParse(BuildNumberValue?.ToString(), out int BuildNumber) &&
               BuildNumber >= 22000;

        #endregion

        #region Windows 11 Snap Layouts
        public static readonly DependencyProperty EnableWindows11SnapLayoutsProperty =
            DependencyProperty.RegisterAttached("EnableWindows11SnapLayouts", typeof(bool), typeof(WindowEx),
                new PropertyMetadata(false, OnEnableWindows11SnapLayoutsChanged));
        public static bool GetEnableWindows11SnapLayouts(Button Button)
            => (bool)Button.GetValue(EnableWindows11SnapLayoutsProperty);
        public static void SetEnableWindows11SnapLayouts(Button Button, bool Value)
            => Button.SetValue(EnableWindows11SnapLayoutsProperty, Value);

        private static readonly DependencyProperty SnapLayoutRegistrationProperty =
            DependencyProperty.RegisterAttached("SnapLayoutRegistration", typeof(SnapLayoutRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static void OnEnableWindows11SnapLayoutsChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Button Button)
                return;

            Button.Loaded -= OnSnapLayoutButtonLoaded;
            Button.Unloaded -= OnSnapLayoutButtonUnloaded;
            DetachSnapLayout(Button);
            if (!(bool)e.NewValue)
                return;

            Button.Loaded += OnSnapLayoutButtonLoaded;
            Button.Unloaded += OnSnapLayoutButtonUnloaded;
            if (Button.IsLoaded)
                AttachSnapLayout(Button);
        }

        private static void OnSnapLayoutButtonLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button Button)
                AttachSnapLayout(Button);
        }

        private static void OnSnapLayoutButtonUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button Button)
                DetachSnapLayout(Button);
        }

        private static void AttachSnapLayout(Button Button)
        {
            DetachSnapLayout(Button);
            if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", out object BuildNumberValue) ||
                !int.TryParse(BuildNumberValue?.ToString(), out int BuildNumber) ||
                BuildNumber < 22000 ||
                Window.GetWindow(Button) is not Window Owner)
                return;

            Button.SetValue(SnapLayoutRegistrationProperty, new SnapLayoutRegistration(Button, Owner));
        }

        private static void DetachSnapLayout(Button Button)
        {
            if (Button.GetValue(SnapLayoutRegistrationProperty) is SnapLayoutRegistration Registration)
                Registration.Dispose();

            Button.ClearValue(SnapLayoutRegistrationProperty);
        }

        #endregion

        #region WindowState
        /// <summary>
        /// Because of the binding delay bug at WindowState, we create this property.
        /// </summary>
        public static readonly DependencyPropertyKey WindowStatePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("WindowState", typeof(WindowState), typeof(WindowEx), new PropertyMetadata(WindowState.Normal));

        public static WindowState GetWindowState(Window obj)
            => (WindowState)obj.GetValue(WindowStatePropertyKey.DependencyProperty);

        private static void AttachWindowState(Window Window)
        {
            Window.StateChanged += OnWindowStateChanged;
            Window.SetValue(WindowStatePropertyKey, Window.WindowState);
        }
        private static void DetachWindowState(Window Window)
            => Window.StateChanged -= OnWindowStateChanged;

        private static void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (sender is Window This)
                This.SetValue(WindowStatePropertyKey, This.WindowState);
        }

        #endregion

        #region AcrylicBlur

        #region Windows API
        [DllImport("user32")]
        private static extern bool SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private enum WindowCompositionAttribute
        {
            Undefined = 0,
            NCRendering_Enabled = 1,
            NCRendering_Policy = 2,
            Ttansitions_Forcedisabled = 3,
            Allow_NCPaint = 4,
            Caption_Button_Bounds = 5,
            Nonclient_RTL_Layout = 6,
            Force_Iconic_Representation = 7,
            Extended_Frame_Bounds = 8,
            Has_Iconic_Bitmap = 9,
            Theme_Attributes = 10,
            NCRendering_Exiled = 11,
            NCAdornmentInfo = 12,
            Excluded_From_Livepreview = 13,
            Video_Overlay_Active = 14,
            Force_ActiveWindow_Appearance = 15,
            Disallow_Peek = 16,
            Cloak = 17,
            Cloaded = 18,
            Accent_Policy = 19,
            Freeze_Representation = 20,
            Ever_Uncloaked = 21,
            Visual_Owner = 22,
            Holographic = 23,
            Excluded_From_DDA = 24,
            PassiveUpdateMode = 25,
            UseDarkModeColors = 26,
            Last = 27
        }

        [Flags]
        private enum AccentFlags : uint
        {
            None = 0,
            DrawLeftBorder = 32,
            DrawTopBorder = 64,
            DrawRightBorder = 128,
            DrawBottomBorder = 256,
            DrawAllBorders = DrawLeftBorder | DrawTopBorder | DrawRightBorder | DrawBottomBorder,
        }

        private enum AccentState
        {
            Disabled = 0,
            Enable_Gradient = 1,
            Enable_TransparentGradient = 2,
            Enable_BlurBehind = 3,
            Enable_AcrylicBlurBehind = 4,   // RS4 1803
            Enable_HostBackDrop = 5,        // RS5 1809
            Invalid_State = 6,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState { set; get; }
            public AccentFlags AccentFlags { set; get; }
            public int GradientColor { set; get; }
            public int AnimationId { set; get; }
        }

        #endregion

        public static readonly DependencyProperty AcrylicBlurProperty =
            DependencyProperty.RegisterAttached("AcrylicBlur", typeof(bool), typeof(WindowEx), new PropertyMetadata(false, OnAcrylicBlurChanged));
        public static void SetAcrylicBlur(Window obj, bool value)
            => obj.SetValue(AcrylicBlurProperty, value);
        public static bool GetAcrylicBlur(Window obj)
            => (bool)obj.GetValue(AcrylicBlurProperty);

        private static readonly DependencyProperty AcrylicBlurRegistrationProperty =
            DependencyProperty.RegisterAttached("AcrylicBlurRegistration", typeof(AcrylicBlurRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static void OnAcrylicBlurChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Window Window)
                return;

            if (Window.GetValue(AcrylicBlurRegistrationProperty) is AcrylicBlurRegistration Registration)
                Registration.Dispose();

            Window.ClearValue(AcrylicBlurRegistrationProperty);
            if ((bool)e.NewValue)
                Window.SetValue(AcrylicBlurRegistrationProperty, new AcrylicBlurRegistration(Window));
        }

        #endregion

        #region TitleBar ContextMenu
        public static readonly DependencyProperty DisableTitleBarContextMenuProperty =
            DependencyProperty.RegisterAttached("DisableTitleBarContextMenu", typeof(bool), typeof(WindowEx), new PropertyMetadata(false, OnDisableTitleBarContextMenuChanged));
        public static bool GetDisableTitleBarContextMenu(Window obj)
            => (bool)obj.GetValue(DisableTitleBarContextMenuProperty);
        public static void SetDisableTitleBarContextMenu(Window obj, bool value)
            => obj.SetValue(DisableTitleBarContextMenuProperty, value);

        private static readonly DependencyProperty TitleBarContextMenuRegistrationProperty =
            DependencyProperty.RegisterAttached("TitleBarContextMenuRegistration", typeof(TitleBarContextMenuRegistration), typeof(WindowEx), new PropertyMetadata(null));
        private static void OnDisableTitleBarContextMenuChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Window Window)
                return;

            if (Window.GetValue(TitleBarContextMenuRegistrationProperty) is TitleBarContextMenuRegistration Registration)
                Registration.Dispose();

            Window.ClearValue(TitleBarContextMenuRegistrationProperty);
            if ((bool)e.NewValue)
                Window.SetValue(TitleBarContextMenuRegistrationProperty, new TitleBarContextMenuRegistration(Window));
        }

        #endregion

        private static bool TryGetRegistryKey(string Path, string Key, out object Value)
        {
            Value = null;
            try
            {
                using RegistryKey rk = Registry.LocalMachine.OpenSubKey(Path);
                if (rk == null)
                    return false;

                Value = rk.GetValue(Key);
                return Value != null;
            }
            catch
            {
                return false;
            }
        }

    }

    public static partial class WindowEx
    {
        private sealed class WindowMessageEventArgs : EventArgs
        {
            public IntPtr Hwnd { get; }

            public int Message { get; }

            public IntPtr WParam { get; }

            public IntPtr LParam { get; }

            public bool Handled { get; set; }

            public IntPtr Result { get; set; }

            public WindowMessageEventArgs(IntPtr Hwnd, int Message, IntPtr WParam, IntPtr LParam)
            {
                this.Hwnd = Hwnd;
                this.Message = Message;
                this.WParam = WParam;
                this.LParam = LParam;
            }

        }

        private sealed class WindowRegistration : IDisposable
        {
            public event EventHandler SourceAvailable;

            public event EventHandler<WindowMessageEventArgs> WindowMessage
            {
                add
                {
                    WindowMessageHandlers += value;
                    PromoteHook();
                }
                remove
                {
                    WindowMessageHandlers -= value;
                }
            }

            private Window Window { get; }

            public HwndSource Source { get; private set; }

            private HwndSourceHook Hook { get; }

            private EventHandler<WindowMessageEventArgs> WindowMessageHandlers { get; set; }

            private int ReferenceCount { get; set; }

            private bool IsDisposed { get; set; }

            public WindowRegistration(Window Window)
            {
                this.Window = Window;
                Hook = WindowProc;
                Window.SourceInitialized += OnSourceInitialized;
                Window.Closed += OnClosed;
                AttachSource();
            }

            public WindowRegistrationLease Acquire()
            {
                ReferenceCount++;
                return new WindowRegistrationLease(this);
            }

            public bool TryConfigureBeforeSource(Action<Window> Configure)
            {
                if (Source is not null ||
                    new WindowInteropHelper(Window).Handle != IntPtr.Zero)
                    return false;

                Configure(Window);
                return true;
            }

            public void Dispose()
            {
                if (IsDisposed)
                    return;

                IsDisposed = true;
                ReferenceCount = 0;
                Window.SourceInitialized -= OnSourceInitialized;
                Window.Closed -= OnClosed;
                Source?.RemoveHook(Hook);
                Source = null;
                SourceAvailable = null;
                WindowMessageHandlers = null;
                if (ReferenceEquals(Window.GetValue(WindowRegistrationProperty), this))
                    Window.ClearValue(WindowRegistrationProperty);
            }

            public void Release()
            {
                if (IsDisposed ||
                    ReferenceCount <= 0)
                    return;

                ReferenceCount--;
                if (ReferenceCount == 0)
                    Dispose();
            }

            private void OnSourceInitialized(object sender, EventArgs e)
            {
                if (AttachSource())
                    SourceAvailable?.Invoke(this, EventArgs.Empty);
            }

            private void OnClosed(object sender, EventArgs e)
                => Dispose();

            private bool AttachSource()
            {
                if (Source is not null)
                    return false;

                IntPtr Handle = new WindowInteropHelper(Window).Handle;
                if (Handle == IntPtr.Zero ||
                    HwndSource.FromHwnd(Handle) is not HwndSource WindowSource)
                    return false;

                Source = WindowSource;
                Source.AddHook(Hook);
                return true;
            }

            private void PromoteHook()
            {
                if (Source is null)
                    return;

                Source.RemoveHook(Hook);
                Source.AddHook(Hook);
            }

            private IntPtr WindowProc(IntPtr Hwnd, int Message, IntPtr WParam, IntPtr LParam, ref bool Handled)
            {
                WindowMessageEventArgs e = new(Hwnd, Message, WParam, LParam);
                if (WindowMessageHandlers is EventHandler<WindowMessageEventArgs> Handlers)
                {
                    foreach (EventHandler<WindowMessageEventArgs> Handler in Handlers.GetInvocationList())
                    {
                        Handler(this, e);
                        if (e.Handled)
                            break;
                    }
                }

                Handled = e.Handled;
                return e.Result;
            }

        }

        private sealed class WindowRegistrationLease(WindowRegistration Registration) : IDisposable
        {
            public WindowRegistration Registration { get; private set; } = Registration;

            public void Dispose()
            {
                if (Registration is null)
                    return;

                WindowRegistration Target = Registration;
                Registration = null;
                Target.Release();
            }

        }

        private sealed class FixSizeRegistration : IDisposable
        {
            private Window Window { get; }

            private WindowRegistrationLease Lease { get; }

            public FixSizeRegistration(Window Window)
            {
                this.Window = Window;
                Lease = AcquireWindowRegistration(Window);
                Lease.Registration.WindowMessage += OnWindowMessage;
                AttachWindowState(Window);
            }

            public void Dispose()
            {
                Lease.Registration.WindowMessage -= OnWindowMessage;
                DetachWindowState(Window);
                Lease.Dispose();
            }

            private void OnWindowMessage(object sender, WindowMessageEventArgs e)
            {
                if ((Win32Messages)e.Message != Win32Messages.WM_GetMinMaxInfo)
                    return;

                bool Handled = e.Handled;
                e.Result = FixSizeWindowProc(e.Hwnd, e.Message, e.WParam, e.LParam, ref Handled);
                e.Handled = Handled;
            }

        }

        private sealed class WindowStyleRegistration : IDisposable
        {
            private Window Window { get; }

            private WindowRegistrationLease Lease { get; }

            private long OriginalStyle { get; set; }

            private bool HasOriginalStyle { get; set; }

            public WindowStyleRegistration(Window Window)
            {
                this.Window = Window;
                Lease = AcquireWindowRegistration(Window);
                Lease.Registration.SourceAvailable += OnSourceAvailable;
                Lease.Registration.WindowMessage += OnWindowMessage;
                if (Lease.Registration.Source is not null)
                    InitializeSource();
            }

            public void Update()
            {
                if (!HasOriginalStyle ||
                    Lease.Registration.Source is null)
                    return;

                long Style = Desktop.GetWindowLong(Lease.Registration.Source.Handle, WindowLongType.Style),
                     NewStyle = UpdateWindowStyle(Style, OriginalStyle, WindowStyles.MaximizeBox, GetDisableMaximize(Window));
                NewStyle = UpdateWindowStyle(NewStyle, OriginalStyle, WindowStyles.MinimizeBox, GetDisableMinimize(Window));
                if (NewStyle == Style)
                    return;

                Desktop.SetWindowLong(Lease.Registration.Source.Handle, WindowLongType.Style, NewStyle);
                Desktop.SetWindowPos(Lease.Registration.Source.Handle, IntPtr.Zero, 0, 0, 0, 0,
                                     WindowPosFlags.NoMove | WindowPosFlags.NoSize | WindowPosFlags.NoZOrder | WindowPosFlags.NoActivate | WindowPosFlags.FrameChanged);
            }

            public void Dispose()
            {
                Lease.Registration.SourceAvailable -= OnSourceAvailable;
                Lease.Registration.WindowMessage -= OnWindowMessage;
                Lease.Dispose();
            }

            private void OnSourceAvailable(object sender, EventArgs e)
                => InitializeSource();

            private void InitializeSource()
            {
                if (!HasOriginalStyle)
                {
                    OriginalStyle = Desktop.GetWindowLong(Lease.Registration.Source.Handle, WindowLongType.Style);
                    HasOriginalStyle = true;
                }

                Update();
            }

            private long UpdateWindowStyle(long Style, long OriginalStyle, WindowStyles TargetStyle, bool Disable)
            {
                long Mask = (long)TargetStyle;
                if (Disable || (OriginalStyle & Mask) == 0)
                    return Style & ~Mask;

                return Style | Mask;
            }

            private void OnWindowMessage(object sender, WindowMessageEventArgs e)
            {
                switch ((Win32Messages)e.Message)
                {
                    case Win32Messages.WM_SysCommand:
                        {
                            if (GetDisableMaximize(Window) &&
                                TryHandleMessage(DisableMaximizeWindowProc, e))
                                return;

                            if (GetDisableMinimize(Window))
                                TryHandleMessage(DisableMinimizeWindowProc, e);

                            break;
                        }
                    case Win32Messages.WM_NCLButtonDoubldClick:
                        {
                            if (GetDisableMaximize(Window))
                                TryHandleMessage(DisableMaximizeWindowProc, e);

                            break;
                        }
                }
            }

            private bool TryHandleMessage(WindowMessageHandler Handler, WindowMessageEventArgs e)
            {
                bool Handled = e.Handled;
                IntPtr Result = Handler(e.Hwnd, e.Message, e.WParam, e.LParam, ref Handled);
                if (!Handled)
                    return false;

                e.Handled = true;
                e.Result = Result;
                return true;
            }

            private delegate IntPtr WindowMessageHandler(IntPtr Hwnd, int Message, IntPtr WParam, IntPtr LParam, ref bool Handled);

        }

        private sealed class AcrylicBlurRegistration : IDisposable
        {
            private WindowRegistrationLease Lease { get; }

            private AccentState State { get; set; }

            private int GradientColor { get; set; }

            public AcrylicBlurRegistration(Window Window)
            {
                Lease = AcquireWindowRegistration(Window);
                Prepare(Window);
                Lease.Registration.SourceAvailable += OnSourceAvailable;
                if (Lease.Registration.Source is not null)
                    Apply(true);
            }

            public void Dispose()
            {
                if (Lease.Registration.Source is not null)
                    Apply(false);

                Lease.Registration.SourceAvailable -= OnSourceAvailable;
                Lease.Dispose();
            }

            private void Prepare(Window Window)
            {
                if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseID", out object BuildNumberValue) &&
                    int.TryParse(BuildNumberValue?.ToString(), out int BuildNumber) &&
                    BuildNumber >= 1803)
                {
                    State = AccentState.Enable_AcrylicBlurBehind;
                    Color BackgroundColor = Window.Background is SolidColorBrush Brush ? Brush.Color :
                                                                                        Color.FromArgb(0x40, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                    Window.Background = null;
                    GradientColor = BackgroundColor.A << 24 |
                                    BackgroundColor.R << 16 |
                                    BackgroundColor.G << 8 |
                                    BackgroundColor.B;

                    if (WindowChrome.GetWindowChrome(Window) is null)
                        WindowChrome.SetWindowChrome(Window, new WindowChrome { GlassFrameThickness = new Thickness(1, 30, 1, 1) });
                }
                else
                {
                    State = AccentState.Enable_BlurBehind;
                }
            }

            private void OnSourceAvailable(object sender, EventArgs e)
                => Apply(true);

            private unsafe void Apply(bool Enable)
            {
                AccentPolicy Accent = new()
                {
                    AccentState = Enable ? State : AccentState.Disabled,
                    GradientColor = Enable ? GradientColor : 0
                };
                WindowCompositionAttributeData Data = new()
                {
                    Attribute = WindowCompositionAttribute.Accent_Policy,
                    SizeOfData = sizeof(WindowCompositionAttributeData),
                    Data = (IntPtr)(&Accent)
                };
                SetWindowCompositionAttribute(Lease.Registration.Source.Handle, ref Data);
            }

        }

        private sealed class Windows11RoundedCornersRegistration : IDisposable
        {
            private Window Window { get; }

            private WindowRegistrationLease Lease { get; }

            private RoundedCornerRegistration BorderRegistration { get; set; }

            private bool IsApplied { get; set; }

            public Windows11RoundedCornersRegistration(Window Window)
            {
                this.Window = Window;
                Lease = AcquireWindowRegistration(Window);
                Lease.Registration.TryConfigureBeforeSource(
                    Target => Target.SetCurrentValue(Window.AllowsTransparencyProperty, false));
                Lease.Registration.SourceAvailable += OnSourceAvailable;
                Window.Loaded += OnLoaded;
                Window.Unloaded += OnUnloaded;
                if (Lease.Registration.Source is not null)
                    Apply();
            }

            public void Dispose()
            {
                Lease.Registration.SourceAvailable -= OnSourceAvailable;
                Window.Loaded -= OnLoaded;
                Window.Unloaded -= OnUnloaded;
                if (IsApplied &&
                    Lease.Registration.Source is not null)
                    TrySetWindows11CornerPreference(Window, DwmWindowCornerPreference.Default);

                DetachBorder();
                Lease.Dispose();
            }

            private void OnSourceAvailable(object sender, EventArgs e)
                => Apply();

            private void OnLoaded(object sender, RoutedEventArgs e)
            {
                if (IsApplied)
                    AttachBorder();
            }

            private void OnUnloaded(object sender, RoutedEventArgs e)
                => DetachBorder();

            private void Apply()
            {
                if (Window.AllowsTransparency ||
                    !TrySetWindows11CornerPreference(Window, DwmWindowCornerPreference.Round))
                    return;

                IsApplied = true;
                if (Window.IsLoaded)
                    AttachBorder();
            }

            private void AttachBorder()
            {
                DetachBorder();
                Window.ApplyTemplate();

                Border Border = Window.Content as Border;
                if (VisualTreeHelper.GetChildrenCount(Window) > 0 &&
                    VisualTreeHelper.GetChild(Window, 0) is Border TemplateRoot)
                    Border = TemplateRoot;

                if (Border is not null)
                    BorderRegistration = new RoundedCornerRegistration(Border);
            }

            private void DetachBorder()
            {
                BorderRegistration?.Dispose();
                BorderRegistration = null;
            }

            private sealed class RoundedCornerRegistration : IDisposable
            {
                private Border Target { get; }

                private CornerRadius CornerRadius { get; }

                public RoundedCornerRegistration(Border Target)
                {
                    this.Target = Target;
                    CornerRadius = Target.CornerRadius;
                    Target.SetCurrentValue(Border.CornerRadiusProperty, new CornerRadius());
                }

                public void Dispose()
                    => Target.SetCurrentValue(Border.CornerRadiusProperty, CornerRadius);

            }
        }

        private sealed class TitleBarContextMenuRegistration : IDisposable
        {
            private Window Window { get; }

            private WindowRegistrationLease Lease { get; }

            public TitleBarContextMenuRegistration(Window Window)
            {
                this.Window = Window;
                Lease = AcquireWindowRegistration(Window);
                Lease.Registration.WindowMessage += OnWindowMessage;
            }

            public void Dispose()
            {
                Lease.Registration.WindowMessage -= OnWindowMessage;
                Lease.Dispose();
            }

            private void OnWindowMessage(object sender, WindowMessageEventArgs e)
            {
                if ((Win32Messages)e.Message != Win32Messages.WM_InitMenuPopup)
                    return;

                Win32.System.SendMessage(e.Hwnd, Win32Messages.WM_CancelMode, IntPtr.Zero, IntPtr.Zero);
                if (Window.ContextMenu is ContextMenu Menu)
                    Menu.IsOpen = true;

                e.Handled = true;
            }

        }

        private sealed class SnapLayoutRegistration : IDisposable
        {
            private Button Button { get; }

            private WindowRegistrationLease Lease { get; }

            private bool IsDispatchingClientMessage { get; set; }

            private bool IsClientMouseOverButton { get; set; }

            private bool IsMaxButtonHitTest { get; set; }

            public SnapLayoutRegistration(Button Button, Window Owner)
            {
                this.Button = Button;
                Lease = AcquireWindowRegistration(Owner);
                Lease.Registration.WindowMessage += OnWindowMessage;
            }

            public void Dispose()
            {
                Lease.Registration.WindowMessage -= OnWindowMessage;
                Lease.Dispose();
            }

            private unsafe void OnWindowMessage(object sender, WindowMessageEventArgs e)
            {
                if (IsDispatchingClientMessage)
                    return;

                switch ((Win32Messages)e.Message)
                {
                    case Win32Messages.WM_NCHitTest:
                        {
                            IsMaxButtonHitTest = IsPointOverButton(e.LParam);
                            if (!IsMaxButtonHitTest)
                                break;

                            e.Handled = true;
                            e.Result = new IntPtr((int)WindowHitTests.MaxButton);
                            break;
                        }
                    case Win32Messages.WM_NCMouseMove:
                        {
                            if (IsMaxButtonHitTest)
                            {
                                IsClientMouseOverButton = true;
                                TryDispatchClientMouseMessage(Win32Messages.WM_MouseMove, IntPtr.Zero, e.LParam);
                            }
                            else if (IsClientMouseOverButton)
                            {
                                IsClientMouseOverButton = false;
                                DispatchClientMouseMessage(Win32Messages.WM_MouseLeave, IntPtr.Zero, IntPtr.Zero);
                            }

                            break;
                        }
                    case Win32Messages.WM_NCMouseLeave:
                        {
                            IsMaxButtonHitTest = false;
                            if (!IsClientMouseOverButton)
                                break;

                            IsClientMouseOverButton = false;
                            DispatchClientMouseMessage(Win32Messages.WM_MouseLeave, IntPtr.Zero, IntPtr.Zero);
                            break;
                        }
                    case Win32Messages.WM_MouseLeave:
                        {
                            if (!IsPointOverButton(GlobalMouse.Position))
                                break;

                            e.Handled = true;
                            break;
                        }
                    case Win32Messages.WM_NCLButtonDown:
                        {
                            if ((WindowHitTests)e.WParam.ToInt32() != WindowHitTests.MaxButton ||
                                !IsPointOverButton(e.LParam) ||
                                !TryDispatchClientMouseMessage(Win32Messages.WM_LButtonDown, new IntPtr(1), e.LParam))
                                break;

                            e.Handled = true;
                            break;
                        }
                    case Win32Messages.WM_NCLButtonUp:
                        {
                            if ((WindowHitTests)e.WParam.ToInt32() != WindowHitTests.MaxButton ||
                                !TryDispatchClientMouseMessage(Win32Messages.WM_LButtonUp, IntPtr.Zero, e.LParam))
                                break;

                            e.Handled = true;
                            break;
                        }
                }
            }

            private unsafe bool TryDispatchClientMouseMessage(Win32Messages Message, IntPtr WParam, IntPtr ScreenPosition)
            {
                long Position = ScreenPosition.ToInt64();
                Point<int> Point = new(unchecked((short)(Position & 0xFFFF)),
                                       unchecked((short)((Position >> 16) & 0xFFFF)));
                if (Lease.Registration.Source is null ||
                    !Desktop.ScreenToClient(Lease.Registration.Source.Handle, &Point))
                    return false;

                int ClientPosition = (Point.Y & 0xFFFF) << 16 |
                                     Point.X & 0xFFFF;
                DispatchClientMouseMessage(Message, WParam, new IntPtr(ClientPosition));
                return true;
            }

            private void DispatchClientMouseMessage(Win32Messages Message, IntPtr WParam, IntPtr LParam)
            {
                try
                {
                    IsDispatchingClientMessage = true;
                    Win32.System.SendMessage(Lease.Registration.Source.Handle, Message, WParam, LParam);
                }
                finally
                {
                    IsDispatchingClientMessage = false;
                }
            }

            private bool IsPointOverButton(IntPtr LParam)
            {
                long Position = LParam.ToInt64();
                Point<int> Point = new(unchecked((short)(Position & 0xFFFF)),
                                       unchecked((short)((Position >> 16) & 0xFFFF)));
                return IsPointOverButton(Point);
            }

            private bool IsPointOverButton(Point<int> Point)
            {
                if (!Button.IsVisible ||
                    !Button.IsEnabled ||
                    PresentationSource.FromVisual(Button) is null)
                    return false;

                Point ButtonPoint = Button.PointFromScreen(new Point(Point.X, Point.Y));
                return new Rect(0, 0, Button.ActualWidth, Button.ActualHeight).Contains(ButtonPoint);
            }

        }

    }

}
