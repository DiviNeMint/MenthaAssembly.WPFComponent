using MenthaAssembly.Devices;
using MenthaAssembly.Utils;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using Bitmap = System.Drawing.Bitmap;

namespace MenthaAssembly.Views
{
    [ContentProperty(nameof(Balloon))]
    public class NotifyIcon : UIElement, IDisposable
    {
        private static readonly UidPool Uids = new UidPool();
        private static readonly int TaskbarRestartMessageId = RegisterWindowMessage("TaskbarCreated");
        private static readonly int CallbackMessageId = 0x400;

        public event EventHandler<HandledEventArgs> PreviewMouseDoubleClick;
        public event EventHandler<HandledEventArgs> MouseDoubleClick;
        public event EventHandler Click;

        #region Windows API

        /// <summary>
        /// Gets the maximum number of milliseconds that can elapse between a
        /// first click and a second click for the OS to consider the
        /// mouse action a double-click.
        /// </summary>
        /// <returns>The maximum amount of time, in milliseconds, that can
        /// elapse between a first click and a second click for the OS to
        /// consider the mouse action a double-click.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern int GetDoubleClickTime();

        [DllImport("shell32.dll")]
        private static extern bool Shell_NotifyIcon(NotifyCommand Command, ref NotifyIconData Data);

        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW")]
        private static extern int RegisterWindowMessage([MarshalAs(UnmanagedType.LPWStr)] string lpString);

        //[DllImport("shell32.dll", SetLastError = true)]
        //private static extern int Shell_NotifyIconGetRect([In] ref NotifyIconIdentifier identifier, [Out] out Int32Bound iconLocation);
        //private Int32Bound NotifyIconRect;
        //private Int32Bound GetNotifyIconRect()
        //{
        //    NotifyIconIdentifier Identifier = Data.Identifier;
        //    int Result = Shell_NotifyIconGetRect(ref Identifier, out Int32Bound Bound);

        //    if (Result != 0)
        //        throw new Win32Exception(Result);

        //    return Bound;
        //}

        internal enum NotifyCommand
        {
            /// <summary>
            /// The taskbar icon is being created.
            /// </summary>
            Add = 0x00,

            /// <summary>
            /// The settings of the taskbar icon are being updated.
            /// </summary>
            Modify = 0x01,

            /// <summary>
            /// The taskbar icon is deleted.
            /// </summary>
            Delete = 0x02,

            /// <summary>
            /// Focus is returned to the taskbar icon. Currently not in use.
            /// </summary>
            SetFocus = 0x03,

            /// <summary>
            /// Shell32.dll version 5.0 and later only. Instructs the taskbar
            /// to behave according to the version number specified in the 
            /// uVersion member of the structure pointed to by lpdata.
            /// This message allows you to specify whether you want the version
            /// 5.0 behavior found on Microsoft Windows 2000 systems, or the
            /// behavior found on earlier Shell versions. The default value for
            /// uVersion is zero, indicating that the original Windows 95 notify
            /// icon behavior should be used.
            /// </summary>
            SetVersion = 0x04
        }

        [Flags]
        internal enum NotifyIconFlags
        {
            /// <summary>
            /// The message ID is set.
            /// </summary>
            Message = 0x01,

            /// <summary>
            /// The notification icon is set.
            /// </summary>
            Icon = 0x02,

            /// <summary>
            /// The tooltip is set.
            /// </summary>
            Tip = 0x04,

            /// <summary>
            /// State information (<see cref="IconState"/>) is set. This
            /// applies to both <see cref="NotifyIconData.IconState"/> and
            /// <see cref="NotifyIconData.StateMask"/>.
            /// </summary>
            State = 0x08,

            /// <summary>
            /// The balloon ToolTip is set. Accordingly, the following
            /// members are set: <see cref="NotifyIconData.BalloonText"/>,
            /// <see cref="NotifyIconData.BalloonTitle"/>, <see cref="NotifyIconData.BalloonFlags"/>,
            /// and <see cref="NotifyIconData.VersionOrTimeout"/>.
            /// </summary>
            Info = 0x10,

            // Internal identifier is set. Reserved, thus commented out.
            //Guid = 0x20,

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. If the ToolTip
            /// cannot be displayed immediately, discard it.<br/>
            /// Use this flag for ToolTips that represent real-time information which
            /// would be meaningless or misleading if displayed at a later time.
            /// For example, a message that states "Your telephone is ringing."<br/>
            /// This modifies and must be combined with the <see cref="Info"/> flag.
            /// </summary>
            Realtime = 0x40,

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later.
            /// Use the standard ToolTip. Normally, when uVersion is set
            /// to NOTIFYICON_VERSION_4, the standard ToolTip is replaced
            /// by the application-drawn pop-up user interface (UI).
            /// If the application wants to show the standard tooltip
            /// in that case, regardless of whether the on-hover UI is showing,
            /// it can specify NIF_SHOWTIP to indicate the standard tooltip
            /// should still be shown.<br/>
            /// Note that the NIF_SHOWTIP flag is effective until the next call 
            /// to Shell_NotifyIcon.
            /// </summary>
            UseLegacyToolTips = 0x80
        }

        internal enum BalloonFlags
        {
            /// <summary>
            /// No icon is displayed.
            /// </summary>
            None = 0x00,

            /// <summary>
            /// An information icon is displayed.
            /// </summary>
            Info = 0x01,

            /// <summary>
            /// A warning icon is displayed.
            /// </summary>
            Warning = 0x02,

            /// <summary>
            /// An error icon is displayed.
            /// </summary>
            Error = 0x03,

            /// <summary>
            /// Windows XP Service Pack 2 (SP2) and later.
            /// Use a custom icon as the title icon.
            /// </summary>
            User = 0x04,

            /// <summary>
            /// Windows XP (Shell32.dll version 6.0) and later.
            /// Do not play the associated sound. Applies only to balloon ToolTips.
            /// </summary>
            NoSound = 0x10,

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. The large version
            /// of the icon should be used as the balloon icon. This corresponds to the
            /// icon with dimensions SM_CXICON x SM_CYICON. If this flag is not set,
            /// the icon with dimensions XM_CXSMICON x SM_CYSMICON is used.<br/>
            /// - This flag can be used with all stock icons.<br/>
            /// - Applications that use older customized icons (NIIF_USER with hIcon) must
            ///   provide a new SM_CXICON x SM_CYICON version in the tray icon (hIcon). These
            ///   icons are scaled down when they are displayed in the System Tray or
            ///   System Control Area (SCA).<br/>
            /// - New customized icons (NIIF_USER with hBalloonIcon) must supply an
            ///   SM_CXICON x SM_CYICON version in the supplied icon (hBalloonIcon).
            /// </summary>
            LargeIcon = 0x20,

            /// <summary>
            /// Windows 7 and later.
            /// </summary>
            RespectQuietTime = 0x80
        }

        internal enum IconState
        {
            Visible = 0x00,
            Hidden = 0x01,
            //Shared = 0x02 // currently not supported, thus commented out.
        }

        internal struct NotifyIconData
        {
            /// <summary>
            /// Size of this structure, in bytes.
            /// </summary>
            public int cbSize;

            /// <summary>
            /// Handle to the window that receives notification messages associated with an icon in the
            /// taskbar status area. The Shell uses hWnd and uID to identify which icon to operate on
            /// when Shell_NotifyIcon is invoked.
            /// </summary>
            public IntPtr WindowHandle;

            /// <summary>
            /// Application-defined identifier of the taskbar icon. The Shell uses hWnd and uID to identify
            /// which icon to operate on when Shell_NotifyIcon is invoked. You can have multiple icons
            /// associated with a single hWnd by assigning each a different uID. This feature, however
            /// is currently not used.
            /// </summary>
            public uint TaskbarIconId;

            /// <summary>
            /// Flags that indicate which of the other members contain valid data. This member can be
            /// a combination of the NIF_XXX constants.
            /// </summary>
            public NotifyIconFlags Flags;

            /// <summary>
            /// Application-defined message identifier. The system uses this identifier to send
            /// notifications to the window identified in hWnd.
            /// </summary>
            public int CallbackMessageId;

            /// <summary>
            /// A handle to the icon that should be displayed. Just
            /// <c>Icon.Handle</c>.
            /// </summary>
            public IntPtr Hicon;

            /// <summary>
            /// String with the text for a standard ToolTip. It can have a maximum of 64 characters including
            /// the terminating NULL. For Version 5.0 and later, szTip can have a maximum of
            /// 128 characters, including the terminating NULL.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string ToolTipText;

            /// <summary>
            /// State of the icon. Remember to also set the <see cref="StateMask"/>.
            /// </summary>
            public IconState IconState;

            /// <summary>
            /// A value that specifies which bits of the state member are retrieved or modified.
            /// For example, setting this member to <see cref="TaskbarNotification.Interop.IconState.Hidden"/>
            /// causes only the item's hidden
            /// state to be retrieved.
            /// </summary>
            public IconState StateMask;

            /// <summary>
            /// String with the text for a balloon ToolTip. It can have a maximum of 255 characters.
            /// To remove the ToolTip, set the NIF_INFO flag in uFlags and set szInfo to an empty string.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string BalloonText;

            /// <summary>
            /// Mainly used to set the version when <see cref="WinApi.Shell_NotifyIcon"/> is invoked
            /// with <see cref="NotifyCommand.SetVersion"/>. However, for legacy operations,
            /// the same member is also used to set timeouts for balloon ToolTips.
            /// </summary>
            public uint VersionOrTimeout;

            /// <summary>
            /// String containing a title for a balloon ToolTip. This title appears in boldface
            /// above the text. It can have a maximum of 63 characters.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
            public string BalloonTitle;

            /// <summary>
            /// Adds an icon to a balloon ToolTip, which is placed to the left of the title. If the
            /// <see cref="BalloonTitle"/> member is zero-length, the icon is not shown.
            /// </summary>
            public BalloonFlags BalloonFlags;

            /// <summary>
            /// Windows XP (Shell32.dll version 6.0) and later.<br/>
            /// - Windows 7 and later: A registered GUID that identifies the icon.
            ///   This value overrides uID and is the recommended method of identifying the icon.<br/>
            /// - Windows XP through Windows Vista: Reserved.
            /// </summary>
            public Guid TaskbarIconGuid;

            /// <summary>
            /// Windows Vista (Shell32.dll version 6.0.6) and later. The handle of a customized
            /// balloon icon provided by the application that should be used independently
            /// of the tray icon. If this member is non-NULL and the <see cref="TaskbarNotification.Interop.BalloonFlags.User"/>
            /// flag is set, this icon is used as the balloon icon.<br/>
            /// If this member is NULL, the legacy behavior is carried out.
            /// </summary>
            public IntPtr CustomBalloonIconHandle;

            public NotifyIconIdentifier Identifier => new NotifyIconIdentifier
            {
                cbSize = Marshal.SizeOf<NotifyIconIdentifier>(),
                Hwnd = this.WindowHandle,
                Uid = this.TaskbarIconId,
                Guid = this.TaskbarIconGuid
            };
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NotifyIconIdentifier
        {
            public int cbSize;
            public IntPtr Hwnd;
            public uint Uid;
            public Guid Guid;
        }

        #endregion

        public static readonly DependencyProperty ContextMenuProperty =
              ContextMenuService.ContextMenuProperty.AddOwner(typeof(NotifyIcon), new FrameworkPropertyMetadata(null));
        public ContextMenu ContextMenu
        {
            get => (ContextMenu)GetValue(ContextMenuProperty);
            set => SetValue(ContextMenuProperty, value);
        }

        public static readonly DependencyProperty SourceProperty =
              DependencyProperty.Register("Source", typeof(ImageSource), typeof(NotifyIcon), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is NotifyIcon This)
                      {
                          if (!This.TryCreate())
                              return;

                          Bitmap Icon = CreateIcon(e.NewValue as ImageSource, 0d);
                          This.Data.Hicon = Icon?.GetHicon() ?? IntPtr.Zero;
                          This.Data.Flags = NotifyIconFlags.Icon;
                          Shell_NotifyIcon(NotifyCommand.Modify, ref This.Data);
                          Icon?.Dispose();
                      }
                  }));
        public ImageSource Source
        {
            get => (ImageSource)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public static readonly DependencyProperty ToolTipProperty =
              DependencyProperty.Register("ToolTip", typeof(string), typeof(NotifyIcon), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is NotifyIcon This)
                      {
                          if (!This.TryCreate())
                              return;

                          This.Data.ToolTipText = e.NewValue as string;
                          This.Data.Flags = NotifyIconFlags.Tip;
                          Shell_NotifyIcon(NotifyCommand.Modify, ref This.Data);
                      }
                  }));
        public string ToolTip
        {
            get => (string)GetValue(ToolTipProperty);
            set => SetValue(ToolTipProperty, value);
        }

        public static readonly DependencyProperty BalloonProperty =
              DependencyProperty.Register("Balloon", typeof(object), typeof(NotifyIcon), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is NotifyIcon This)
                      {
                          if (This.BalloonPopup is Popup OldPopup)
                          {
                              OldPopup.IsOpen = false;
                              OldPopup.Child = null;
                          }

                          This.BalloonPopup = This.CreateBalloonPopup(e.NewValue);
                          if (This.IsBalloonShown)
                              This.ShowBalloonPopup();
                      }
                  }));
        public object Balloon
        {
            get => GetValue(BalloonProperty);
            set => SetValue(BalloonProperty, value);
        }

        public static readonly DependencyProperty IsBalloonShownProperty =
              DependencyProperty.Register("IsBalloonShown", typeof(bool), typeof(NotifyIcon), new PropertyMetadata(false,
                  (d, e) =>
                  {
                      if (d is NotifyIcon This)
                      {
                          if (e.NewValue is true)
                          {
                              This.ShowBalloonPopup();
                              return;
                          }

                          This.HideBalloonPopup();
                      }
                  }));
        public bool IsBalloonShown
        {
            get => (bool)GetValue(IsBalloonShownProperty);
            set => SetValue(IsBalloonShownProperty, value);
        }

        static NotifyIcon()
        {
            VisibilityProperty.OverrideMetadata(typeof(NotifyIcon), new PropertyMetadata(Visibility.Visible,
                (d, e) =>
                {
                    if (d is NotifyIcon This)
                    {
                        if (Visibility.Visible.Equals(e.NewValue))
                            This.TryCreate();
                        else
                            This.Dispose();
                    }
                }));
        }

        protected override void OnRender(DrawingContext drawingContext)
            => TryCreate();

        private NotifyIconData Data;
        private bool TryCreate()
        {
            if (this.Visibility != Visibility.Visible)
                return false;

            if (IsCreated)
                return true;

            Window Parent = Application.Current.MainWindow;

            if (Parent is null)
                return false;

            WindowInteropHelper WindowInterop = new WindowInteropHelper(Parent);
            if (WindowInterop.Handle == IntPtr.Zero)
                WindowInterop.EnsureHandle();

            HwndSource HwndSource = HwndSource.FromHwnd(WindowInterop.Handle);
            HwndSource.AddHook(OnWndProc);

            Bitmap Icon = CreateIcon(this.Source, 0d);
            try
            {
                Data = new NotifyIconData
                {
                    TaskbarIconId = Uids.Dequeue(),
                    cbSize = Environment.OSVersion.Version.Major < 6 ? 952 : Marshal.SizeOf<NotifyIconData>(),
                    WindowHandle = HwndSource.Handle,
                    CallbackMessageId = CallbackMessageId,
                    Hicon = Icon?.GetHicon() ?? IntPtr.Zero,
                    ToolTipText = ToolTip,
                    Flags = NotifyIconFlags.Icon | NotifyIconFlags.Tip | NotifyIconFlags.Message
                };
                IsCreated = Shell_NotifyIcon(NotifyCommand.Add, ref Data);
            }
            finally
            {
                Icon?.Dispose();
            }

            return IsCreated;
        }

        private int DoubleClickTimeInterval { get; } = GetDoubleClickTime();
        private DelayActionToken DoubleClickChecker;
        private bool IsLeftMousePressed,
                     IsRightMousePressed;
        private IntPtr OnWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == TaskbarRestartMessageId)
            {
                // Restart
                this.Dispose();
                this.TryCreate();
            }
            else if (msg == CallbackMessageId &&
                     wParam.ToInt32() == Data.TaskbarIconId)
            {
                switch ((Win32Messages)lParam)
                {
                    case Win32Messages.WM_MouseMove:
                        {
                            // PreviewMouseMove
                            MouseEventArgs e = new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = PreviewMouseMoveEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseMove
                            e.RoutedEvent = MouseMoveEvent;
                            RaiseEvent(e);
                            break;
                        }
                    case Win32Messages.WM_LButtonDown:
                        {
                            // PreviewMouseLeftButtonDown
                            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = PreviewMouseLeftButtonDownEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // PreviewMouseDown
                            e.RoutedEvent = PreviewMouseDownEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseLeftButtonDown
                            e.RoutedEvent = MouseLeftButtonDownEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseDown
                            e.RoutedEvent = MouseDownEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            IsLeftMousePressed = true;
                            break;
                        }
                    case Win32Messages.WM_LButtonUp:
                        {
                            bool IsClick = IsLeftMousePressed is true;
                            IsLeftMousePressed = false;

                            // PreviewMouseLeftButtonUp
                            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = PreviewMouseLeftButtonUpEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // PreviewMouseUp
                            e.RoutedEvent = PreviewMouseUpEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseLeftButtonUp
                            e.RoutedEvent = MouseLeftButtonUpEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseUp
                            e.RoutedEvent = MouseUpEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            if (IsClick)
                            {
                                if (PreviewMouseDoubleClick is null &&
                                    MouseDoubleClick is null)
                                    OnClick();
                                else
                                    DoubleClickChecker = TimerHelper.DelayAction(DoubleClickTimeInterval, OnClick);
                            }
                            break;
                        }
                    case Win32Messages.WM_LButtonDoubleClick:
                        {
                            // Cancel Single Click
                            DoubleClickChecker?.Cancel();

                            // PreviewMouseDoubleClick
                            HandledEventArgs e = new HandledEventArgs();
                            PreviewMouseDoubleClick?.Invoke(this, e);

                            if (e.Handled)
                                break;

                            // MouseDoubleClick
                            MouseDoubleClick?.Invoke(this, e);

                            break;
                        }
                    case Win32Messages.WM_RButtonDown:
                        {
                            // PreviewMouseRightButtonDown
                            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right) { RoutedEvent = PreviewMouseRightButtonDownEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // PreviewMouseDown
                            e.RoutedEvent = PreviewMouseDownEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseRightButtonDown
                            e.RoutedEvent = MouseRightButtonDownEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseDown
                            e.RoutedEvent = MouseDownEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            IsRightMousePressed = true;
                            break;
                        }
                    case Win32Messages.WM_RButtonUp:
                        {
                            bool IsClick = IsRightMousePressed is true;
                            IsRightMousePressed = false;

                            // PreviewMouseRightButtonUp
                            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right) { RoutedEvent = PreviewMouseRightButtonUpEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // PreviewMouseUp
                            e.RoutedEvent = PreviewMouseUpEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseRightButtonUp
                            e.RoutedEvent = MouseRightButtonUpEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseUp
                            e.RoutedEvent = MouseUpEvent;
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            if (IsClick &&
                                ContextMenu != null)
                            {
                                ContextMenu.Opened += OnContextMenuOpened;
                                ContextMenu.IsOpen = true;
                            }
                            break;
                        }
                    case Win32Messages.WM_MButtonDown:
                        {
                            // PreviewMouseDown
                            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle) { RoutedEvent = PreviewMouseDownEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseDown
                            e.RoutedEvent = MouseDownEvent;
                            RaiseEvent(e);

                            break;
                        }
                    case Win32Messages.WM_MButtonUp:
                        {
                            // PreviewMouseUp
                            MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Middle) { RoutedEvent = PreviewMouseUpEvent, Source = this };
                            RaiseEvent(e);

                            if (e.Handled)
                                break;

                            // MouseUp
                            e.RoutedEvent = MouseUpEvent;
                            RaiseEvent(e);

                            break;
                        }

                        #region NotSupport
                        //case Win32Messages.WM_ContextMenu:
                        //    // TODO: Handle WM_CONTEXTMENU, see https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
                        //    Debug.WriteLine("Unhandled WM_CONTEXTMENU");
                        //    break;
                        //case Win32Messages.NIN_PopupOpen:
                        //    Debug.WriteLine("ToolTipOpen");
                        //    break;
                        //case Win32Messages.NIN_PopupClose:
                        //    Debug.WriteLine("ToolTipClose");
                        //    break;
                        //case Win32Messages.NIN_Select:
                        //    // TODO: Handle NIN_SELECT see https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
                        //    Debug.WriteLine("Unhandled NIN_SELECT");
                        //    break;
                        //case Win32Messages.NIN_KeySelect:
                        //    // TODO: Handle NIN_KEYSELECT see https://docs.microsoft.com/en-us/windows/win32/api/shellapi/nf-shellapi-shell_notifyiconw
                        //    Debug.WriteLine("Unhandled NIN_KEYSELECT");
                        //    break;
                        #endregion
                }
            }

            return IntPtr.Zero;
        }

        protected void OnClick()
            => Click?.Invoke(this, EventArgs.Empty);

        private Popup BalloonPopup;
        private Popup CreateBalloonPopup(object Content)
        {
            Popup NewPopup = new Popup
            {
                AllowsTransparency = true,
                StaysOpen = true,
                Placement = PlacementMode.AbsolutePoint,
                Child = Content as UIElement ?? new TextBlock { Text = Content?.ToString() }
            };

            return NewPopup;
        }

        private DelayActionToken BalloonCloseChecker;
        private void ShowBalloonPopup()
        {
            if (BalloonPopup is null)
                return;

            //// provide the popup with the taskbar icon's data context
            //UpdateDataContext(popup, null, DataContext);

            // don't animate by default - developers can use attached events or override
            BalloonPopup.PopupAnimation = PopupAnimation.Fade;

            //// in case the balloon is cleaned up through routed events, the
            //// control didn't remove the balloon from its parent popup when
            //// if was closed the last time - just make sure it doesn't have
            //// a parent that is a popup
            //Popup parent = LogicalTreeHelper.GetParent(balloon) as Popup;
            //if (parent != null)
            //    parent.Child = null;

            ScreenInfo Info = Screen.Current;
            if (Info.WorkArea.Bottom < Info.Bound.Bottom)       // Bottom
            {
                BalloonPopup.HorizontalOffset = (Info.WorkArea.Right - 1) / Info.DpiFactorX;
                BalloonPopup.VerticalOffset = Info.WorkArea.Bottom / Info.DpiFactorY;
            }
            else if (Info.WorkArea.Right < Info.Bound.Right)    // Right
            {

            }
            else if (Info.WorkArea.Top > Info.Bound.Top)        // Top
            {
                BalloonPopup.HorizontalOffset = (Info.WorkArea.Right - 1) / Info.DpiFactorX;
                BalloonPopup.VerticalOffset = Info.WorkArea.Bottom / Info.DpiFactorY;
            }
            else if (Info.WorkArea.Left > Info.Bound.Left)      // Left
            {

            }

            //Info.Bound

            //Point position = CustomPopupPosition != null ? CustomPopupPosition() : GetPopupTrayPosition();
            //popup.HorizontalOffset = position.X - 1;
            //popup.VerticalOffset = position.Y - 1;

            ////store reference
            //lock (lockObject)
            //{
            //    SetCustomBalloon(popup);
            //}

            //// assign this instance as an attached property
            //SetParentTaskbarIcon(balloon, this);

            //// fire attached event
            //RaiseBalloonShowingEvent(balloon, this);

            // display item
            BalloonPopup.IsOpen = true;

            BalloonCloseChecker = TimerHelper.DelayAction(3000, () => IsBalloonShown = false);
        }
        private void HideBalloonPopup()
        {
            if (BalloonPopup is null)
                return;

            BalloonCloseChecker?.Cancel();
            BalloonPopup.IsOpen = false;
        }

        private Point ContextPosition;
        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu Menu)
            {
                ContextPosition = Menu.PointToScreen(new Point());

                Menu.Opened -= OnContextMenuOpened;
                Menu.Closed += OnContextMenuClosed;
                GlobalMouse.MouseDown += OnGlobalMouseMouseDown;
            }
        }
        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu Menu)
            {
                Menu.Closed -= OnContextMenuClosed;
                GlobalMouse.MouseDown -= OnGlobalMouseMouseDown;
            }
        }
        private void OnGlobalMouseMouseDown(GlobalMouseEventArgs obj)
        {
            if (ContextMenu != null)
            {
                Int32Point MousePosition = GlobalMouse.Position;
                if (MousePosition.X < ContextPosition.X ||
                    MousePosition.Y < ContextPosition.Y ||
                    MousePosition.X > ContextPosition.X + ContextMenu.ActualWidth ||
                    MousePosition.Y > ContextPosition.Y + ContextMenu.ActualHeight)
                    ContextMenu.IsOpen = false;
            }
        }

        private static Bitmap CreateIcon(ImageSource Icon, double Angle = 0d)
        {
            if (Icon is null)
                return null;

            return ImageHelper.CreateBitmap(new Image
            {
                Source = Icon,
                Stretch = Stretch.UniformToFill,
                Width = Icon.Width,
                Height = Icon.Height,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = Angle != 0 ? new RotateTransform(Angle) : null
            }, 16, 16);
        }

        private bool IsCreated;
        public void Dispose()
        {
            if (!IsCreated)
                return;

            Data.Flags = NotifyIconFlags.Message;
            Shell_NotifyIcon(NotifyCommand.Delete, ref Data);
            Uids.Enqueue(ref Data.TaskbarIconId);

            IsCreated = false;
        }

        ~NotifyIcon()
        {
            Dispose();
        }

    }
}
