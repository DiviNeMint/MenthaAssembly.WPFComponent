using MenthaAssembly.Devices;
using MenthaAssembly.Utils;
using MenthaAssembly.Win32;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using static MenthaAssembly.Win32.Desktop;
using static MenthaAssembly.Win32.Graphic;
using static MenthaAssembly.Win32.System;

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
        public event EventHandler BalloonOpened;
        public event EventHandler BalloonClosed;

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

                          if (This.Data.HIcon != IntPtr.Zero)
                              DestroyIcon(This.Data.HIcon);

                          This.Data.HIcon = CreateHIcon(e.NewValue as ImageSource);
                          This.Data.Flags = NotifyIconFlags.Icon;
                          Shell_NotifyIcon(NotifyCommand.Modify, ref This.Data);
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
                          if (e.NewValue is null)
                          {
                              This.CloseBalloonPopup(PopupAnimation.None);
                              return;
                          }

                          if (This.BalloonPopup != null)
                              This.BalloonPopup.Child = e.NewValue as UIElement ?? new TextBlock { Text = e.NewValue?.ToString() };
                      }
                  }));
        [Category("Balloon")]
        public object Balloon
        {
            get => GetValue(BalloonProperty);
            set => SetValue(BalloonProperty, value);
        }

        public static readonly DependencyProperty BalloonShowDurationProperty =
            DependencyProperty.Register("BalloonShowDuration", typeof(double), typeof(NotifyIcon), new PropertyMetadata(double.PositiveInfinity,
                (d, e) =>
                {
                    if (d is NotifyIcon This &&
                        e.NewValue is double ShowDuration &&
                        This.BalloonCloseChecker != null)
                    {
                        This.BalloonCloseChecker.Cancel();
                        This.BalloonCloseChecker = double.IsInfinity(ShowDuration) ? null :
                                                                                     TimerHelper.DelayAction(ShowDuration, () => This.IsBalloonShown = false);
                    }
                },
                (d, v) =>
                {
                    if (v is double Value)
                        return Math.Max(Value, 500d);

                    return Binding.DoNothing;
                }));
        [Category("Balloon")]
        public double BalloonShowDuration
        {
            get => (double)GetValue(BalloonShowDurationProperty);
            set => SetValue(BalloonShowDurationProperty, value);
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

                          This.CloseBalloonPopup(PopupAnimation.Fade);
                      }
                  }));
        [Category("Balloon")]
        public bool IsBalloonShown
        {
            get => (bool)GetValue(IsBalloonShownProperty);
            set => SetValue(IsBalloonShownProperty, value);
        }

        public static readonly DependencyProperty IsBalloonStaysOpenProperty =
              DependencyProperty.Register("IsBalloonStaysOpen", typeof(bool), typeof(NotifyIcon), new PropertyMetadata(false,
                  (d, e) =>
                  {
                      if (d is NotifyIcon This &&
                          This.BalloonPopup != null)
                      {
                          if (e.NewValue is true)
                          {
                              This.BalloonPopup.Closed -= This.OnBalloonPopupClosed;
                              GlobalMouse.MouseDown -= This.OnBalloonPopupGlobalMouseDown;
                          }
                          else
                          {
                              This.BalloonPopup.Closed += This.OnBalloonPopupClosed;
                              GlobalMouse.MouseDown += This.OnBalloonPopupGlobalMouseDown;
                          }
                      }
                  }));
        [Category("Balloon")]
        public bool IsBalloonStaysOpen
        {
            get => (bool)GetValue(IsBalloonStaysOpenProperty);
            set => SetValue(IsBalloonStaysOpenProperty, value);
        }

        static NotifyIcon()
        {
            VisibilityProperty.OverrideMetadata(typeof(NotifyIcon), new PropertyMetadata(Visibility.Visible,
                (d, e) =>
                {
                    if (d is NotifyIcon This)
                    {
                        This.Data.IconState = Visibility.Visible.Equals(e.NewValue) ? NotifyIconState.Shared : NotifyIconState.Hidden;
                        This.Data.StateMask = NotifyIconState.Hidden;
                        This.Data.Flags = NotifyIconFlags.State;
                        Shell_NotifyIcon(NotifyCommand.Modify, ref This.Data);
                    }
                }));
        }

        protected override void OnRender(DrawingContext drawingContext)
            => TryCreate();

        private bool Attached;
        private NotifyIconData Data;
        private bool TryCreate()
        {
            if (this.Visibility != Visibility.Visible)
                return false;

            return LockHandle(() =>
            {
                if (IsCreated)
                    return true;

                Window Parent = Application.Current.MainWindow ??
                                Application.Current.Windows.OfType<Window>().FirstOrDefault();

                if (Parent is null)
                    return false;

                WindowInteropHelper WindowInterop = new WindowInteropHelper(Parent);
                if (WindowInterop.Handle == IntPtr.Zero)
                    WindowInterop.EnsureHandle();

                HwndSource HwndSource = HwndSource.FromHwnd(WindowInterop.Handle);
                HwndSource.AddHook(OnWndProc);

                Data = new NotifyIconData
                {
                    TaskbarIconId = Uids.Dequeue(),
                    cbSize = Environment.OSVersion.Version.Major < 6 ? 952 : Marshal.SizeOf<NotifyIconData>(),
                    WindowHandle = HwndSource.Handle,
                    CallbackMessageId = CallbackMessageId,
                    HIcon = CreateHIcon(this.Source),
                    ToolTipText = ToolTip,
                    Flags = NotifyIconFlags.Icon | NotifyIconFlags.Tip | NotifyIconFlags.Message
                };
                IsCreated = Shell_NotifyIcon(NotifyCommand.Add, ref Data);

                if (IsCreated &&
                    !Attached)
                {
                    Parent.Closed += (s, e) => this.Dispose();
                    Attached = true;
                }

                return IsCreated;
            });
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
                            DoubleClickChecker = null;

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
        private const double AnimationDuration = 150d,
                             AnimationLengthScale = 0.8d;
        private DelayActionToken BalloonCloseChecker;
        private Int32Bound BalloonPosition;
        private void ShowBalloonPopup()
        {
            if (Balloon is null)
                return;

            LockHandle(() =>
            {
                if (BalloonPopup is null)
                    BalloonPopup = new Popup
                    {
                        AllowsTransparency = true,
                        Placement = PlacementMode.AbsolutePoint,
                        Child = Balloon as UIElement ?? new TextBlock { Text = Balloon?.ToString() }
                    };
            });

            // Size
            Size ChildSize = BalloonPopup.Child.DesiredSize;
            if (ChildSize.Width == 0d && ChildSize.Height == 0d)
                BalloonPopup.Child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            ChildSize = BalloonPopup.Child.DesiredSize;

            // Location Info
            ScreenInfo Info = Screen.Current;

            // Bottom
            if (Info.WorkArea.Bottom < Info.Bound.Bottom)
            {
                // Real Position
                BalloonPosition = new Int32Bound(Info.WorkArea.Right - ChildSize.Width * Info.DpiFactorX,
                                                 Info.WorkArea.Bottom - ChildSize.Height * Info.DpiFactorY,
                                                 Info.WorkArea.Right,
                                                 Info.WorkArea.Bottom);

                // Virtual Position
                BalloonPopup.HorizontalOffset = Info.WorkArea.Right / Info.DpiFactorX - ChildSize.Width;
                BalloonPopup.VerticalOffset = Info.WorkArea.Bottom / Info.DpiFactorY;

                // Animation
                BalloonPopup.BeginAnimation(FrameworkElement.HeightProperty,
                                            AnimationHelper.CreateDoubleAnimation(ChildSize.Height * AnimationLengthScale,
                                                                                  ChildSize.Height,
                                                                                  AnimationDuration,
                                                                                  () => BalloonPopup.BeginAnimation(FrameworkElement.HeightProperty, null)));
            }
            // Right
            else if (Info.WorkArea.Right < Info.Bound.Right)
            {
                // Real Position
                BalloonPosition = new Int32Bound(Info.WorkArea.Right - ChildSize.Width * Info.DpiFactorX,
                                                 Info.WorkArea.Bottom - ChildSize.Height * Info.DpiFactorY,
                                                 Info.WorkArea.Right,
                                                 Info.WorkArea.Bottom);

                // Position
                BalloonPopup.HorizontalOffset = Info.WorkArea.Right / Info.DpiFactorX;
                BalloonPopup.VerticalOffset = Info.WorkArea.Bottom / Info.DpiFactorY;

                // Animation
                BalloonPopup.BeginAnimation(FrameworkElement.WidthProperty,
                                            AnimationHelper.CreateDoubleAnimation(ChildSize.Width * AnimationLengthScale,
                                                                                  ChildSize.Width,
                                                                                  AnimationDuration,
                                                                                  () => BalloonPopup.BeginAnimation(FrameworkElement.WidthProperty, null)));
            }
            // Top
            else if (Info.WorkArea.Top > Info.Bound.Top)
            {
                // Real Position
                BalloonPosition = new Int32Bound(Info.WorkArea.Right - ChildSize.Width * Info.DpiFactorX,
                                                 Info.WorkArea.Top,
                                                 Info.WorkArea.Right,
                                                 Info.WorkArea.Top + ChildSize.Height * Info.DpiFactorY);

                // Position
                BalloonPopup.HorizontalOffset = Info.WorkArea.Right / Info.DpiFactorX - ChildSize.Width;
                BalloonPopup.VerticalOffset = Info.WorkArea.Top / Info.DpiFactorY;

                // Animation
                BalloonPopup.BeginAnimation(FrameworkElement.HeightProperty,
                                            AnimationHelper.CreateDoubleAnimation(ChildSize.Height * AnimationLengthScale,
                                                                                  ChildSize.Height,
                                                                                  AnimationDuration,
                                                                                  () => BalloonPopup.BeginAnimation(FrameworkElement.HeightProperty, null)));
            }
            // Left
            else if (Info.WorkArea.Left > Info.Bound.Left)
            {
                // Real Position
                BalloonPosition = new Int32Bound(Info.WorkArea.Left,
                                                 Info.WorkArea.Bottom - ChildSize.Height * Info.DpiFactorY,
                                                 Info.WorkArea.Left + ChildSize.Width * Info.DpiFactorX,
                                                 Info.WorkArea.Bottom);

                // Position
                BalloonPopup.HorizontalOffset = Info.WorkArea.Left / Info.DpiFactorX;
                BalloonPopup.VerticalOffset = Info.WorkArea.Bottom / Info.DpiFactorY;

                // Animation
                BalloonPopup.BeginAnimation(FrameworkElement.WidthProperty,
                                            AnimationHelper.CreateDoubleAnimation(ChildSize.Width * AnimationLengthScale,
                                                                                  ChildSize.Width,
                                                                                  AnimationDuration,
                                                                                  () => BalloonPopup.BeginAnimation(FrameworkElement.WidthProperty, null)));
            }

            // Show
            BalloonPopup.PopupAnimation = PopupAnimation.None;
            BalloonPopup.StaysOpen = IsBalloonStaysOpen;

            if (!IsBalloonStaysOpen)
                BalloonPopup.Opened += OnBalloonPopupOpened;

            BalloonPopup.IsOpen = true;

            BalloonCloseChecker?.Cancel();
            BalloonCloseChecker = double.IsInfinity(BalloonShowDuration) ? null :
                                                                           TimerHelper.DelayAction(BalloonShowDuration, () => IsBalloonShown = false);
            // Raise Open Event
            BalloonOpened?.Invoke(this, EventArgs.Empty);
        }
        private void CloseBalloonPopup(PopupAnimation Animation)
        {
            if (BalloonPopup is null)
                return;

            BalloonCloseChecker?.Cancel();
            BalloonCloseChecker = null;

            BalloonPopup.BeginAnimation(FrameworkElement.HeightProperty, null, HandoffBehavior.SnapshotAndReplace);
            BalloonPopup.BeginAnimation(FrameworkElement.WidthProperty, null, HandoffBehavior.SnapshotAndReplace);

            BalloonPopup.PopupAnimation = Animation;
            BalloonPopup.IsOpen = false;

            // Raise Close Event
            if (Animation != PopupAnimation.None)
                BalloonClosed?.Invoke(this, EventArgs.Empty);
        }

        private void OnBalloonPopupOpened(object sender, EventArgs e)
        {
            if (sender is Popup ThisPopup)
            {
                ThisPopup.Opened -= OnBalloonPopupOpened;
                ThisPopup.Closed += OnBalloonPopupClosed;

                if (PresentationSource.FromVisual(BalloonPopup.Child) is HwndSource Source)
                {
                    // Activate the BalloonPopup's Child to track deactivation, 
                    SetForegroundWindow(Source.Handle);
                    ThisPopup.PopupAnimation = PopupAnimation.Fade;
                }
                else
                {
                    // Track deactivation by GlobalMouseDown with mouse position.
                    GlobalMouse.MouseDown += OnBalloonPopupGlobalMouseDown;
                }
            }
        }
        private void OnBalloonPopupClosed(object sender, EventArgs e)
        {
            if (sender is Popup ThisPopup)
            {
                ThisPopup.Closed -= OnBalloonPopupClosed;
                GlobalMouse.MouseDown -= OnBalloonPopupGlobalMouseDown;
            }
            IsBalloonShown = false;
        }
        private void OnBalloonPopupGlobalMouseDown(GlobalMouseEventArgs e)
        {
            if (BalloonPopup != null &&
                !BalloonPosition.Contains(e.Position.X, e.Position.Y))
                IsBalloonShown = false;
        }

        private Point ContextPosition;
        private void OnContextMenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu Menu)
            {
                Menu.Opened -= OnContextMenuOpened;
                if (PresentationSource.FromVisual(ContextMenu) is HwndSource Source)
                {
                    // Activate the ContextMenu to track deactivation, 
                    SetForegroundWindow(Source.Handle);
                }
                else
                {
                    // Get ContextPosition at Screen.
                    ContextPosition = Menu.PointToScreen(new Point());

                    // Track deactivation by GlobalMouseDown with mouse position.
                    Menu.Closed += OnContextMenuClosed;
                    GlobalMouse.MouseDown += OnContextMenuGlobalMouseDown;
                }
            }
        }
        private void OnContextMenuClosed(object sender, RoutedEventArgs e)
        {
            if (sender is ContextMenu Menu)
            {
                Menu.Closed -= OnContextMenuClosed;
                GlobalMouse.MouseDown -= OnContextMenuGlobalMouseDown;
            }
        }
        private void OnContextMenuGlobalMouseDown(GlobalMouseEventArgs e)
        {
            if (ContextMenu != null &&
                (e.Position.X < ContextPosition.X ||
                 e.Position.Y < ContextPosition.Y ||
                 e.Position.X > ContextPosition.X + ContextMenu.ActualWidth ||
                 e.Position.Y > ContextPosition.Y + ContextMenu.ActualHeight))
                ContextMenu.IsOpen = false;
        }

        private readonly object LockObject = new object();
        private void LockHandle(Action Action)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObject, ref Token);
                Action();
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObject);
            }
        }
        private T LockHandle<T>(Func<T> Func)
        {
            bool Token = false;
            try
            {
                Monitor.Enter(LockObject, ref Token);
                return Func();
            }
            finally
            {
                if (Token)
                    Monitor.Exit(LockObject);
            }
        }

        private static IntPtr CreateHIcon(ImageSource Icon)
        {
            if (Icon is null)
                return IntPtr.Zero;

            if (Icon is DrawingImage Image)
                return ImageHelper.CreateHIcon(Image, 16, 16, 0d);

            return ImageHelper.CreateHIcon(new Image
            {
                Source = Icon,
                Stretch = Stretch.UniformToFill,
                Width = Icon.Width,
                Height = Icon.Height
            }, 16, 16);
        }

        private bool IsCreated;
        public void Dispose()
        {
            if (!IsCreated)
                return;

            // Notify Icon
            Data.Flags = NotifyIconFlags.Message;
            Shell_NotifyIcon(NotifyCommand.Delete, ref Data);
            Uids.Enqueue(ref Data.TaskbarIconId);

            // Release HIcon
            if (Data.HIcon != IntPtr.Zero)
                DestroyIcon(Data.HIcon);

            // ContextMenu
            if (ContextMenu != null)
            {
                ContextMenu.IsOpen = false;
                GlobalMouse.MouseDown -= OnContextMenuGlobalMouseDown;
            }

            // BalloonPopup
            BalloonCloseChecker?.Cancel();
            BalloonCloseChecker = null;

            if (BalloonPopup != null)
            {
                BalloonPopup.BeginAnimation(FrameworkElement.HeightProperty, null, HandoffBehavior.SnapshotAndReplace);
                BalloonPopup.BeginAnimation(FrameworkElement.WidthProperty, null, HandoffBehavior.SnapshotAndReplace);
                BalloonPopup.PopupAnimation = PopupAnimation.None;
                BalloonPopup.IsOpen = false;
                BalloonPopup.Child = null;
                BalloonPopup = null;

                GlobalMouse.MouseDown -= OnBalloonPopupGlobalMouseDown;
            }

            // Click Event
            DoubleClickChecker?.Cancel();
            DoubleClickChecker = null;

            IsCreated = false;
        }

        ~NotifyIcon()
        {
            Dispose();
        }

    }
}
