using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MenthaAssembly.Views
{
    [DefaultEvent("ToggleChanged")]
    [TemplatePart(Name = nameof(PART_GradientStop), Type = typeof(GradientStop))]
    [TemplatePart(Name = nameof(PART_Thumb), Type = typeof(Thumb))]
    [TemplatePart(Name = nameof(PART_ThumbTranslateTransform), Type = typeof(TranslateTransform))]
    public class SwitchView : Control
    {
        public static readonly RoutedEvent ToggleChangedEvent =
            EventManager.RegisterRoutedEvent("ToggleChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<bool>), typeof(SwitchView));
        public event RoutedPropertyChangedEventHandler<bool> ToggleChanged
        {
            add { AddHandler(ToggleChangedEvent, value); }
            remove { RemoveHandler(ToggleChangedEvent, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            Border.CornerRadiusProperty.AddOwner(typeof(SwitchView), new PropertyMetadata(null));
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty EnableAnimationProperty =
            DependencyProperty.Register("EnableAnimation", typeof(bool), typeof(SwitchView), new PropertyMetadata(true));
        public bool EnableAnimation
        {
            get => (bool)GetValue(EnableAnimationProperty);
            set => SetValue(EnableAnimationProperty, value);
        }

        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register("IsToggled", typeof(bool?), typeof(SwitchView), new PropertyMetadata(false,
                (d, e) =>
                {
                    if (d is SwitchView This)
                        This.OnIsToggledChanged(e.ToRoutedPropertyChangedEventArgs<bool?>(ToggleChangedEvent));
                }));
        public bool? IsToggled
        {
            get => (bool?)GetValue(IsToggledProperty);
            set => SetValue(IsToggledProperty, value);
        }

        public static readonly DependencyPropertyKey IsPressedPropertyKey =
              DependencyProperty.RegisterReadOnly("IsPressed", typeof(bool), typeof(SwitchView), new PropertyMetadata(false));
        public bool IsPressed
            => (bool)GetValue(IsPressedPropertyKey.DependencyProperty);

        public static readonly DependencyProperty ThumbStyleProperty =
            DependencyProperty.Register("ThumbStyle", typeof(Style), typeof(SwitchView), new PropertyMetadata(null));
        public Style ThumbStyle
        {
            get => (Style)GetValue(ThumbStyleProperty);
            set => SetValue(ThumbStyleProperty, value);
        }

        static SwitchView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SwitchView), new FrameworkPropertyMetadata(typeof(SwitchView)));
        }

        private GradientStop PART_GradientStop;
        private Thumb PART_Thumb;
        private TranslateTransform PART_ThumbTranslateTransform;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_GradientStop") is GradientStop PART_GradientStop)
                this.PART_GradientStop = PART_GradientStop;

            if (GetTemplateChild("PART_Thumb") is Thumb PART_Thumb)
                this.PART_Thumb = PART_Thumb;

            if (GetTemplateChild("PART_ThumbTranslateTransform") is TranslateTransform PART_ThumbTranslateTransform)
                this.PART_ThumbTranslateTransform = PART_ThumbTranslateTransform;

            bool? Value = IsToggled;
            if (IsToggled is not false)
            {
                this.PART_GradientStop.Offset = Value.HasValue ? 1d : 0.5d;
                this.PART_Thumb.HorizontalAlignment = Value.HasValue ? HorizontalAlignment.Right : HorizontalAlignment.Center;
            }
        }

        private readonly TimeSpan AnimationInterval = TimeSpan.FromMilliseconds(100d);
        protected virtual void OnIsToggledChanged(RoutedPropertyChangedEventArgs<bool?> e)
        {
            bool? Value = e.NewValue;
            GradientStopOffsetTo(Value.HasValue ? Value.Value ? 1d : 0d : 0.5d);
            ThumbMoveTo(Value.HasValue ? Value.Value ? HorizontalAlignment.Right : HorizontalAlignment.Left : HorizontalAlignment.Center);
        }

        private void GradientStopOffsetTo(double To)
        {
            if (PART_GradientStop is null)
                return;

            if (EnableAnimation)
            {
                DoubleAnimation OffsetAnimation = new DoubleAnimation(To, AnimationInterval, FillBehavior.Stop);
                OffsetAnimation.Completed += (d, e) => PART_GradientStop.Offset = To;
                OffsetAnimation.Freeze();

                PART_GradientStop.ApplyAnimationClock(GradientStop.OffsetProperty, OffsetAnimation.CreateClock());
            }
            else
            {
                PART_GradientStop.Offset = To;
            }
        }
        private void ThumbMoveTo(HorizontalAlignment Alignment)
        {
            if (PART_Thumb is null ||
                PART_ThumbTranslateTransform is null)
                return;

            if (EnableAnimation)
            {
                Thickness Padding = this.Padding,
                          BorderThickness = this.BorderThickness,
                          Margin = PART_Thumb.Margin;
                double Delta = (ActualWidth - (BorderThickness.Left + BorderThickness.Right + Padding.Left + Padding.Right + PART_Thumb.ActualWidth + Margin.Left + Margin.Right));

                double From = PART_Thumb.HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => 0d,
                    HorizontalAlignment.Right => Delta,
                    _ => Delta / 2d
                };

                double To = Alignment switch
                {
                    HorizontalAlignment.Left => 0d,
                    HorizontalAlignment.Right => Delta,
                    _ => Delta / 2d
                };

                DoubleAnimation OffsetAnimation = new DoubleAnimation(To - From, AnimationInterval, FillBehavior.Stop);
                OffsetAnimation.Completed += (d, e) => PART_Thumb.HorizontalAlignment = Alignment;
                OffsetAnimation.Freeze();

                PART_ThumbTranslateTransform.ApplyAnimationClock(TranslateTransform.XProperty, OffsetAnimation.CreateClock());
            }
            else
            {
                PART_Thumb.HorizontalAlignment = Alignment;
            }
        }

        private bool IsLeftMouseDown;
        private Point MousePosition;
        private double MouseMoveDelta;
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _ = CaptureMouse();
                IsLeftMouseDown = true;
                SetValue(IsPressedPropertyKey, true);
                MousePosition = e.GetPosition(this);
                MouseMoveDelta = 0d;
                e.Handled = true;
            }
        }
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (IsLeftMouseDown)
            {
                Point Position = e.GetPosition(this);
                double Dx = Position.X - MousePosition.X,
                       Dy = Position.Y - MousePosition.Y;

                if (MouseMoveDelta <= 25d)
                    MouseMoveDelta += Dx * Dx + Dy * Dy;

                SetValue(IsPressedPropertyKey, !(Position.X > ActualWidth || Position.Y > ActualHeight));
                MousePosition = Position;
                e.Handled = true;
            }
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);
            if (IsLeftMouseDown)
            {
                ReleaseMouseCapture();
                IsLeftMouseDown = false;
                SetValue(IsPressedPropertyKey, false);

                if (MouseMoveDelta <= 25)
                {
                    bool? Value = IsToggled;
                    IsToggled = Value.HasValue ? !IsToggled : false;
                }
            }
        }

    }
}