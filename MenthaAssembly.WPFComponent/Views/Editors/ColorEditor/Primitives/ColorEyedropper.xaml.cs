using MenthaAssembly.Devices;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static MenthaAssembly.Win32.Desktop;
using static MenthaAssembly.Win32.Graphic;

namespace MenthaAssembly.Views.Primitives
{
    public class ColorEyedropper : ContentControl
    {
        #region Window API
        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern uint GetPixel(IntPtr dc, int x, int y);

        #endregion

        public static readonly DependencyProperty IsCapturingProperty =
              DependencyProperty.Register("IsCapturing", typeof(bool), typeof(ColorEyedropper), new PropertyMetadata(false));
        public bool IsCapturing
        {
            get => (bool)GetValue(IsCapturingProperty);
            set => SetValue(IsCapturingProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
              DependencyProperty.Register("Color", typeof(Color), typeof(ColorEyedropper), new PropertyMetadata(Colors.Transparent));
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty OriginalColorProperty =
              DependencyProperty.Register("OriginalColor", typeof(Color), typeof(ColorEyedropper), new PropertyMetadata(Colors.Transparent));
        public Color OriginalColor
        {
            get => (Color)GetValue(OriginalColorProperty);
            set => SetValue(OriginalColorProperty, value);
        }

        static ColorEyedropper()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorEyedropper), new FrameworkPropertyMetadata(typeof(ColorEyedropper)));
        }

        private Image PART_IMAGE;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.GetTemplateChild("PART_IMAGE") is Image PART_IMAGE)
            {
                this.PART_IMAGE = PART_IMAGE;
                PART_IMAGE.Source = CursorHelper.Resource.Eyedropper;
            }
        }

        private IntPtr pDesk;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left &&
                !IsCapturing)
            {
                IsCapturing = true;

                // Backup
                OriginalColor = Color;

                // Mouse Detect
                GlobalMouse.MouseDown += OnGlobalMouseDown;
                GlobalMouse.MouseMove += OnGlobalMouseMove;
                GlobalMouse.MouseUp += OnGlobalMouseUp;
                GlobalKeyboard.KeyDown += OnGlobalKeyboardKeyDown;

                // Init Cursor
                CursorHelper.SetAllGlobalCursor(CursorHelper.EyedropperCursor);

                // UI
                if (PART_IMAGE != null)
                    PART_IMAGE.Visibility = Visibility.Collapsed;

                // Init Desk Intptr
                pDesk = GetDesktopWindow();

                // Update Color
                if (Background is SolidColorBrush Brush)
                {
                    Color = Brush.Color;
                }
                else
                {
                    Int32Point Position = GlobalMouse.Position;
                    UpdateColor(Position);
                }
            }
        }

        private void OnGlobalMouseMove(Int32Point Position)
            => UpdateColor(Position);
        private void OnGlobalMouseDown(GlobalMouseEventArgs e)
        {
            GlobalMouse.MouseDown -= OnGlobalMouseDown;
            GlobalMouse.MouseMove -= OnGlobalMouseMove;
            GlobalKeyboard.KeyDown -= OnGlobalKeyboardKeyDown;

            if (e.ChangedButton == MouseKey.Right)
                Color = OriginalColor;

            // UI
            if (PART_IMAGE != null)
                PART_IMAGE.Visibility = Visibility.Visible;

            // Release Cursor
            CursorHelper.SetAllGlobalCursor(null);

            IsCapturing = false;
            e.Handled = true;
        }
        private void OnGlobalMouseUp(GlobalMouseEventArgs e)
        {
            if (IsCapturing)
                e.Handled = true;
            else
            {
                GlobalMouse.MouseUp -= OnGlobalMouseUp;

                if (e.ChangedButton == MouseKey.Right)
                    GlobalMouse.DoMouseUp(MouseKey.Left);
            }
        }

        private void OnGlobalKeyboardKeyDown(GlobalKeyboardEventArgs e)
        {
            if (e.Key == KeyboardKey.Esc)
            {
                GlobalMouse.MouseDown -= OnGlobalMouseDown;
                GlobalMouse.MouseMove -= OnGlobalMouseMove;
                GlobalKeyboard.KeyDown -= OnGlobalKeyboardKeyDown;

                Color = OriginalColor;

                // UI
                if (PART_IMAGE != null)
                    PART_IMAGE.Visibility = Visibility.Visible;

                // Release Cursor
                CursorHelper.SetAllGlobalCursor(null);

                IsCapturing = false;
            }
        }

        private void UpdateColor(Int32Point Position)
        {
            // GetPixel
            IntPtr pWindowDC = GetWindowDC(pDesk);
            int ColorValue = (int)GetPixel(pWindowDC, Position.X, Position.Y);
            ReleaseDC(pDesk, pWindowDC);

            Color Result = Color.FromArgb(255, (byte)ColorValue, (byte)(ColorValue >> 8), (byte)(ColorValue >> 16));
            this.Dispatcher.Invoke(() => Color = Result);
        }

    }
}
