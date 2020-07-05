using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [DefaultEvent("ToggleChanged")]
    public class SwitchView : Control
    {
        public static readonly RoutedEvent ToggleChangedEvent =
            EventManager.RegisterRoutedEvent("ToggleChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(SwitchView));
        public event RoutedPropertyChangedEventHandler<bool> ToggleChanged
        {
            add { AddHandler(ToggleChangedEvent, value); }
            remove { RemoveHandler(ToggleChangedEvent, value); }
        }

        public static new readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register("Padding", typeof(Thickness), typeof(SwitchView), new PropertyMetadata(new Thickness(2)));
        public new Thickness Padding
        {
            get { return (Thickness)GetValue(PaddingProperty); }
            set { SetValue(PaddingProperty, value); }
        }

        public static readonly DependencyProperty IsPressedProperty =
            DependencyProperty.Register("IsPressed", typeof(bool), typeof(SwitchView), new PropertyMetadata(false));
        public bool IsPressed
        {
            get { return (bool)GetValue(IsPressedProperty); }
            protected set { SetValue(IsPressedProperty, value); }
        }

        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register("IsToggled", typeof(bool), typeof(SwitchView), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is SwitchView This)
                    {
                        This.RaiseEvent(new RoutedPropertyChangedEventArgs<bool>(
                            (bool)e.OldValue,
                            (bool)e.NewValue,
                            ToggleChangedEvent));
                    }
                }));
        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        public static readonly DependencyProperty IsAnimationEnabledProperty =
            DependencyProperty.Register("IsAnimationEnabled", typeof(bool), typeof(SwitchView), new PropertyMetadata(true));
        public bool IsAnimationEnabled
        {
            get { return (bool)GetValue(IsAnimationEnabledProperty); }
            set { SetValue(IsAnimationEnabledProperty, value); }
        }

        public static readonly DependencyProperty ToggledBackgroundProperty =
            DependencyProperty.Register("ToggledBackground", typeof(Brush), typeof(SwitchView), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x1A, 0xB9, 0x00))));
        public Brush ToggledBackground
        {
            get { return (Brush)GetValue(ToggledBackgroundProperty); }
            set { SetValue(ToggledBackgroundProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(SwitchView), new PropertyMetadata(new CornerRadius(10),
                (d, e) =>
                {
                    if (e.NewValue is CornerRadius corner)
                    {
                        Thickness Padding = (Thickness)d.GetValue(PaddingProperty);
                        corner.TopLeft = Math.Max(0, corner.TopLeft - Math.Max(Padding.Top, Padding.Left));
                        corner.TopRight = Math.Max(0, corner.TopRight - Math.Max(Padding.Top, Padding.Right));
                        corner.BottomLeft = Math.Max(0, corner.BottomLeft - Math.Max(Padding.Bottom, Padding.Left));
                        corner.BottomRight = Math.Max(0, corner.BottomRight - Math.Max(Padding.Bottom, Padding.Right));

                        d.SetValue(ToggleCornerRadiusProperty, corner);
                    }
                }));
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty ToggleCornerRadiusProperty =
            DependencyProperty.RegisterAttached("ToggleCornerRadius", typeof(CornerRadius), typeof(SwitchView), new PropertyMetadata(new CornerRadius(8)));
        public static CornerRadius GetToggleCornerRadius(DependencyObject obj)
            => (CornerRadius)obj.GetValue(ToggleCornerRadiusProperty);
        public static void SetToggleCornerRadius(DependencyObject obj, CornerRadius value)
            => obj.SetValue(ToggleCornerRadiusProperty, value);


        static SwitchView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchView), new FrameworkPropertyMetadata(typeof(SwitchView)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_Toggle") is Border PART_Toggle)
                AnimationHelper.AddUpdateHandler(PART_Toggle, new RoutedPropertyChangedEventHandler<double>(OnAnimationUpdate));
        }

        protected void OnAnimationUpdate(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement PART_Toggle &&
                e.NewValue is double NewValue)
            {
                Thickness Padding = this.Padding;
                if (NewValue > 0)
                    Padding.Left += Math.Abs(this.ActualWidth - this.ActualHeight) * NewValue;

                PART_Toggle.Margin = Padding;
            }
        }

        private bool IsMouseDown = false;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            CaptureMouse();
            IsMouseDown = true;
            IsPressed = true;
        }
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            ReleaseMouseCapture();
            if (IsPressed)
            {
                IsPressed = false;
                IsToggled = !IsToggled;
            }
            IsMouseDown = false;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsMouseDown)
            {
                Point Location = e.GetPosition(this);
                IsPressed = !(Location.X > ActualWidth || Location.Y > ActualHeight);
            }
        }
    }
}
