using MenthaAssembly.Devices;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class ColorBox : Control
    {
        public static readonly DependencyProperty IsOpenProperty =
              DependencyProperty.Register("IsOpen", typeof(bool), typeof(ColorBox), new PropertyMetadata(false));
        public bool IsOpen
        {
            get => (bool)GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public static readonly DependencyProperty ColorProperty =
              DependencyProperty.Register("Color", typeof(Color?), typeof(ColorBox), new PropertyMetadata(null));
        public Color? Color
        {
            get => (Color?)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        static ColorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorBox), new FrameworkPropertyMetadata(typeof(ColorBox)));
        }

        private ColorEditor PART_ColorEditor;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("Root") is Button Root)
                Root.Click += OnRootClick;

            if (this.GetTemplateChild("PART_Popup") is Popup PART_Popup)
                PART_Popup.Closed += OnClosed;

            if (this.GetTemplateChild("PART_ColorEditor") is ColorEditor PART_ColorEditor)
                this.PART_ColorEditor = PART_ColorEditor;
        }

        private void OnRootClick(object sender, RoutedEventArgs e)
        {
            if (IsOpen)
            {
                IsOpen = false;
                GlobalMouse.MouseDown -= OnGlobalMouseDown;
            }
            else
            {
                IsOpen = true;
                GlobalMouse.MouseDown += OnGlobalMouseDown;
            }
        }

        protected virtual void OnClosed(object sender, EventArgs e)
        {
            if (IsOpen)
                IsOpen = false;
        }

        private void OnGlobalMouseDown(GlobalMouseEventArgs e)
        {
            if (!IsMouseOver &&
                (!PART_ColorEditor?.IsColorCapturing ?? true))
            {
                GlobalMouse.MouseDown -= OnGlobalMouseDown;
                IsOpen = false;
            }
        }

    }
}
