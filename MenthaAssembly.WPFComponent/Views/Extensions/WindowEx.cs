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
    public static class WindowEx
    {
        #region FixSize
        public static readonly DependencyProperty FixSizeProperty =
            DependencyProperty.RegisterAttached("FixSize", typeof(bool), typeof(WindowEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is Window This)
                    {
                        WindowInteropHelper InteropHelper = new(This);
                        if (InteropHelper.Handle == IntPtr.Zero)
                            InteropHelper.EnsureHandle();

                        if (e.NewValue is true)
                        {
                            HwndSource.FromHwnd(InteropHelper.Handle).AddHook(FixSizeWindowProc);
                            AttachWindowState(This);
                        }
                        else
                        {
                            HwndSource.FromHwnd(InteropHelper.Handle).RemoveHook(FixSizeWindowProc);
                            DetachWindowState(This);
                        }

                        //if (e.NewValue is true)
                        //{
                        //    if (!AttachFixSize(This))
                        //        This.SourceInitialized += OnAttachFixSizeSourceInitialized;
                        //}
                        //else
                        //{
                        //    if (!DetachFixSize(This))
                        //        This.SourceInitialized += OnDetachFixSizeSourceInitialized;
                        //}
                    }
                }));
        public static bool GetFixSize(Window obj)
            => (bool)obj.GetValue(FixSizeProperty);
        public static void SetFixSize(Window obj, bool value)
            => obj.SetValue(FixSizeProperty, value);

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
            DependencyProperty.RegisterAttached("DisableMaximize", typeof(bool), typeof(WindowEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is Window This)
                    {
                        WindowInteropHelper InteropHelper = new(This);
                        if (InteropHelper.Handle == IntPtr.Zero)
                            InteropHelper.EnsureHandle();

                        if (e.NewValue is true)
                        {
                            long Style = Desktop.GetWindowLong(InteropHelper.Handle, WindowLongType.Style);
                            Desktop.SetWindowLong(InteropHelper.Handle, WindowLongType.Style, Style & ~(long)WindowStyles.MaximizeBox);
                            Desktop.SetWindowPos(InteropHelper.Handle, IntPtr.Zero, 0, 0, 0, 0,
                                                 WindowPosFlags.NoMove | WindowPosFlags.NoSize | WindowPosFlags.NoZOrder | WindowPosFlags.NoActivate | WindowPosFlags.FrameChanged);
                            HwndSource.FromHwnd(InteropHelper.Handle).AddHook(DisableMaximizeWindowProc);
                        }
                        else
                        {
                            HwndSource.FromHwnd(InteropHelper.Handle).RemoveHook(DisableMaximizeWindowProc);
                        }
                    }
                }));
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
            DependencyProperty.RegisterAttached("DisableMinimize", typeof(bool), typeof(WindowEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is Window This)
                    {
                        WindowInteropHelper InteropHelper = new(This);
                        if (InteropHelper.Handle == IntPtr.Zero)
                            InteropHelper.EnsureHandle();

                        if (e.NewValue is true)
                        {
                            long Style = Desktop.GetWindowLong(InteropHelper.Handle, WindowLongType.Style);
                            Desktop.SetWindowLong(InteropHelper.Handle, WindowLongType.Style, Style & ~(long)WindowStyles.MinimizeBox);
                            Desktop.SetWindowPos(InteropHelper.Handle, IntPtr.Zero, 0, 0, 0, 0,
                                                 WindowPosFlags.NoMove | WindowPosFlags.NoSize | WindowPosFlags.NoZOrder | WindowPosFlags.NoActivate | WindowPosFlags.FrameChanged);
                            HwndSource.FromHwnd(InteropHelper.Handle).AddHook(DisableMinimizeWindowProc);
                        }
                        else
                        {
                            HwndSource.FromHwnd(InteropHelper.Handle).RemoveHook(DisableMinimizeWindowProc);
                        }
                    }
                }));
        public static bool GetDisableMinimize(Window obj)
            => (bool)obj.GetValue(DisableMinimizeProperty);
        public static void SetDisableMinimize(Window obj, bool value)
            => obj.SetValue(DisableMinimizeProperty, value);

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

        private static void OnEnableWindows11RoundedCornersChanged(DependencyObject Object, DependencyPropertyChangedEventArgs e)
        {
            if (Object is not Window Window)
                return;

            Window.SourceInitialized -= OnEnableWindows11RoundedCornersSourceInitialized;
            if (!(bool)e.NewValue)
                return;

            if (new WindowInteropHelper(Window).Handle == IntPtr.Zero)
            {
                Window.SourceInitialized += OnEnableWindows11RoundedCornersSourceInitialized;
                return;
            }

            EnableWindows11RoundedCorners(Window);
        }

        private static void OnEnableWindows11RoundedCornersSourceInitialized(object sender, EventArgs e)
        {
            if (sender is not Window Window)
                return;

            Window.SourceInitialized -= OnEnableWindows11RoundedCornersSourceInitialized;
            EnableWindows11RoundedCorners(Window);
        }

        private static unsafe void EnableWindows11RoundedCorners(Window Window)
        {
            if (!TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", out object BuildNumberValue) ||
                !int.TryParse(BuildNumberValue?.ToString(), out int BuildNumber) ||
                BuildNumber < 22000)
                return;

            IntPtr Handle = new WindowInteropHelper(Window).Handle;
            DwmWindowCornerPreference Preference = DwmWindowCornerPreference.Round;
            _ = Desktop.DwmSetWindowAttribute(Handle, DwmWindowAttribute.WindowCornerPreference, &Preference, sizeof(DwmWindowCornerPreference));
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
            DependencyProperty.RegisterAttached("AcrylicBlur", typeof(bool), typeof(WindowEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is Window This)
                    {
                        WindowInteropHelper InteropHelper = new(This);
                        if (InteropHelper.Handle == IntPtr.Zero)
                            InteropHelper.EnsureHandle();

                        AccentPolicy Accent = new();
                        if (e.NewValue is true)
                        {
                            if (TryGetRegistryKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseID", out object StrBuildNumber) &&
                                int.TryParse(StrBuildNumber?.ToString(), out int BuildNumber) &&
                                BuildNumber >= 1803)
                            {
                                Accent.AccentState = AccentState.Enable_AcrylicBlurBehind;
                                Color BackgroundColor = This.Background is SolidColorBrush Brush ? Brush.Color :
                                                                                                   Color.FromArgb(0x40, byte.MaxValue, byte.MaxValue, byte.MaxValue);
                                This.Background = null;
                                Accent.GradientColor = BackgroundColor.A << 24 |
                                                       BackgroundColor.R << 16 |
                                                       BackgroundColor.G << 8 |
                                                       BackgroundColor.B;

                                if (WindowChrome.GetWindowChrome(This) is null)
                                    WindowChrome.SetWindowChrome(This, new WindowChrome { GlassFrameThickness = new Thickness(1, 30, 1, 1) });
                            }
                            else
                            {
                                Accent.AccentState = AccentState.Enable_BlurBehind;
                            }
                        }
                        else
                        {
                            Accent.AccentState = AccentState.Disabled;
                        }

                        unsafe
                        {
                            WindowCompositionAttributeData Data = new()
                            {
                                Attribute = WindowCompositionAttribute.Accent_Policy,
                                SizeOfData = sizeof(WindowCompositionAttributeData),
                                Data = (IntPtr)(&Accent)
                            };
                            SetWindowCompositionAttribute(InteropHelper.Handle, ref Data);
                        }
                    }
                }));
        public static void SetAcrylicBlur(Window obj, bool value)
            => obj.SetValue(AcrylicBlurProperty, value);
        public static bool GetAcrylicBlur(Window obj)
            => (bool)obj.GetValue(AcrylicBlurProperty);

        #endregion

        #region TitleBar ContextMenu
        public static readonly DependencyProperty DisableTitleBarContextMenuProperty =
            DependencyProperty.RegisterAttached("DisableTitleBarContextMenu", typeof(bool), typeof(WindowEx), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is Window This)
                    {
                        WindowInteropHelper InteropHelper = new(This);
                        if (InteropHelper.Handle == IntPtr.Zero)
                            InteropHelper.EnsureHandle();

                        IntPtr OnTitleBarContextMenuWindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
                        {
                            switch ((Win32Messages)msg)
                            {
                                case Win32Messages.WM_InitMenuPopup:
                                    {
                                        Win32.System.SendMessage(hwnd, Win32Messages.WM_CancelMode, IntPtr.Zero, IntPtr.Zero);
                                        if (This.ContextMenu is ContextMenu Menu)
                                            Menu.IsOpen = true;

                                        handled = true;
                                        break;
                                    }
                                    //case Win32Messages.WM_NCRButtonDown:
                                    //    {
                                    //        switch ((WindowHitTests)wParam.ToInt32())
                                    //        {
                                    //            case WindowHitTests.Caption:
                                    //            case WindowHitTests.Size:
                                    //            case WindowHitTests.MinButton:
                                    //            case WindowHitTests.MaxButton:
                                    //            case WindowHitTests.Close:
                                    //            case WindowHitTests.Help:
                                    //                handled = true;
                                    //                return (IntPtr)1;
                                    //        }
                                    //        break;
                                    //    }
                            }

                            return IntPtr.Zero;
                        }

                        if (e.NewValue is true)
                            HwndSource.FromHwnd(InteropHelper.Handle).AddHook(OnTitleBarContextMenuWindowProc);
                        else
                            HwndSource.FromHwnd(InteropHelper.Handle).RemoveHook(OnTitleBarContextMenuWindowProc);
                    }
                }));
        public static bool GetDisableTitleBarContextMenu(Window obj)
            => (bool)obj.GetValue(DisableTitleBarContextMenuProperty);
        public static void SetDisableTitleBarContextMenu(Window obj, bool value)
            => obj.SetValue(DisableTitleBarContextMenuProperty, value);

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
}
