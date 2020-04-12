using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace MenthaAssembly.Helpers
{
    public class WindowBlur
    {
        #region Windows API
        [DllImport("user32")]
        private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private enum WindowCompositionAttribute
        {
            Accent_Policy = 19,
        }

        private enum AccentState
        {
            Disabled,
            Enable_Gradient,
            Enable_TransparentGradient,
            Enable_BlurBehind,
            Invalid_State,
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute { set; get; }
            public IntPtr Data { set; get; }
            public int SizeOfData { set; get; }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct AccentPolicy
        {
            public AccentState AccentState { set; get; }
            public int AccentFlags { set; get; }
            public int GradientColor { set; get; }
            public int AnimationId { set; get; }
        }

        #endregion

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(WindowBlur), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is Window window)
                    {
                        if (e.OldValue is true)
                        {
                            GetWindowBlur(window)?.Detach();
                            window.ClearValue(WindowBlurProperty);
                        }

                        if (e.NewValue is true)
                        {
                            WindowBlur blur = new WindowBlur();
                            blur.Attach(window);
                            window.SetValue(WindowBlurProperty, blur);
                        }
                    }
                }));
        public static void SetIsEnabled(DependencyObject element, bool value)
            => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element)
            => (bool)element.GetValue(IsEnabledProperty);


        public static readonly DependencyProperty WindowBlurProperty =
            DependencyProperty.RegisterAttached("WindowBlur", typeof(WindowBlur), typeof(WindowBlur), new PropertyMetadata(null,
                (d, e) =>
                {
                    if (d is Window window)
                    {
                        (e.OldValue as WindowBlur)?.Detach();
                        (e.NewValue as WindowBlur)?.Attach(window);
                    }
                }));
        public static void SetWindowBlur(DependencyObject element, WindowBlur value)
            => element.SetValue(WindowBlurProperty, value);
        public static WindowBlur GetWindowBlur(DependencyObject element)
            => (WindowBlur)element.GetValue(WindowBlurProperty);


        private Window _window;
        private void Attach(Window window)
        {
            _window = window;
            HwndSource source = (HwndSource)PresentationSource.FromVisual(window);
            if (source == null)
            {
                window.SourceInitialized += OnSourceInitialized;
            }
            else
            {
                EnableBlur(_window);
            }
        }

        private void Detach()
        {
            try
            {
                _window.SourceInitialized += OnSourceInitialized;
            }
            finally
            {
                _window = null;
            }
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            ((Window)sender).SourceInitialized -= OnSourceInitialized;
            EnableBlur(_window);
        }

        private unsafe static void EnableBlur(Window ThisWindow)
        {
            AccentPolicy Accent = new AccentPolicy
            {
                AccentState = AccentState.Enable_BlurBehind
            };
            
            WindowCompositionAttributeData data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.Accent_Policy,
                SizeOfData = Marshal.SizeOf(Accent),
                Data = (IntPtr)(&Accent)
            };

            SetWindowCompositionAttribute(new WindowInteropHelper(ThisWindow).Handle, ref data);

            //int accentStructSize = Marshal.SizeOf(Accent);
            //IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
            //Marshal.StructureToPtr(Accent, accentPtr, false);

            //WindowCompositionAttributeData data = new WindowCompositionAttributeData
            //{
            //    Attribute = WindowCompositionAttribute.Accent_Policy,
            //    SizeOfData = accentStructSize,
            //    Data = accentPtr
            //};

            //SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            //Marshal.FreeHGlobal(accentPtr);
        }

    }
}
