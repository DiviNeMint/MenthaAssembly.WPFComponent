using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MenthaAssembly.Views
{
    public class Button : System.Windows.Controls.Button
    {
        public static readonly DependencyProperty EnableAnimateProperty =
            DependencyProperty.Register("EnableAnimate", typeof(bool), typeof(Button), new PropertyMetadata(true));
        public bool EnableAnimate
        {
            get => (bool)GetValue(EnableAnimateProperty);
            set => SetValue(EnableAnimateProperty, value);
        }

        public static readonly DependencyProperty AnimationDurationProperty =
              DependencyProperty.Register("AnimationDuration", typeof(Duration), typeof(Button), new PropertyMetadata((Duration)TimeSpan.FromMilliseconds(150d)));
        public Duration AnimationDuration
        {
            get => (Duration)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public static readonly DependencyProperty MouseOverBackgroundProperty =
            DependencyProperty.Register("MouseOverBackground", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush MouseOverBackground
        {
            get => (Brush)GetValue(MouseOverBackgroundProperty);
            set => SetValue(MouseOverBackgroundProperty, value);
        }

        public static readonly DependencyProperty MouseOverForegroundProperty =
            DependencyProperty.Register("MouseOverForeground", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush MouseOverForeground
        {
            get => (Brush)GetValue(MouseOverForegroundProperty);
            set => SetValue(MouseOverForegroundProperty, value);
        }

        public static readonly DependencyProperty MouseOverBorderBrushProperty =
            DependencyProperty.Register("MouseOverBorderBrush", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush MouseOverBorderBrush
        {
            get => (Brush)GetValue(MouseOverBorderBrushProperty);
            set => SetValue(MouseOverBorderBrushProperty, value);
        }

        public static readonly DependencyProperty MousePressBackgroundProperty =
            DependencyProperty.Register("MousePressBackground", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush MousePressBackground
        {
            get => (Brush)GetValue(MousePressBackgroundProperty);
            set => SetValue(MousePressBackgroundProperty, value);
        }

        public static readonly DependencyProperty MousePressForegroundProperty =
            DependencyProperty.Register("MousePressForeground", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush MousePressForeground
        {
            get => (Brush)GetValue(MousePressForegroundProperty);
            set => SetValue(MousePressForegroundProperty, value);
        }

        public static readonly DependencyProperty MousePressBorderBrushProperty =
            DependencyProperty.Register("MousePressBorderBrush", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush MousePressBorderBrush
        {
            get => (Brush)GetValue(MousePressBorderBrushProperty);
            set => SetValue(MousePressBorderBrushProperty, value);
        }

        public static readonly DependencyProperty DisabledBackgroundProperty =
            DependencyProperty.Register("DisabledBackground", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush DisabledBackground
        {
            get => (Brush)GetValue(DisabledBackgroundProperty);
            set => SetValue(DisabledBackgroundProperty, value);
        }

        public static readonly DependencyProperty DisabledForegroundProperty =
            DependencyProperty.Register("DisabledForeground", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush DisabledForeground
        {
            get => (Brush)GetValue(DisabledForegroundProperty);
            set => SetValue(DisabledForegroundProperty, value);
        }

        public static readonly DependencyProperty DisabledBorderBrushProperty =
            DependencyProperty.Register("DisabledBorderBrush", typeof(Brush), typeof(Button), new PropertyMetadata(null));
        public Brush DisabledBorderBrush
        {
            get => (Brush)GetValue(DisabledBorderBrushProperty);
            set => SetValue(DisabledBorderBrushProperty, value);
        }

        private static readonly DependencyProperty IsAnimatingProperty =
              DependencyProperty.Register("IsAnimating", typeof(bool), typeof(Button),
                  new PropertyMetadata(false, (d, e) =>
                  {
                      if (d is Button This)
                          This.OnIsAnimatingChanged(e.NewValue is true);
                  }));
        internal bool IsAnimating
        {
            get => (bool)GetValue(IsAnimatingProperty);
            set => SetValue(IsAnimatingProperty, value);
        }

        private static readonly DependencyProperty IsPressedWhenAnimatingProperty =
              DependencyProperty.Register("IsPressedWhenAnimating", typeof(bool), typeof(Button), new PropertyMetadata(false));
        internal bool IsPressedWhenAnimating
        {
            get => (bool)GetValue(IsPressedWhenAnimatingProperty);
            set => SetValue(IsPressedWhenAnimatingProperty, value);
        }

        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
        }

        private Storyboard StartStoryboard, EndStoryboard, StartToEndStoryboard;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Root") is FrameworkElement PART_Root &&
                GetTemplateChild("PART_Presenter") is FrameworkElement PART_Presenter)
            {
                PropertyPath BackgroundPath = new("Background.(SolidColorBrush.Color)"),
                             ForegroundPath = new("(TextElement.Foreground).(SolidColorBrush.Color)"),
                             BorderBrushPath = new("BorderBrush.(SolidColorBrush.Color)");
                Binding DurationBinding = new(nameof(AnimationDuration)) { Source = this },
                        MouseOverBackgroundBinding = new($"{nameof(MouseOverBackground)}.Color") { Source = this },
                        MouseOverForegroundBinding = new($"{nameof(MouseOverForeground)}.Color") { Source = this },
                        MouseOverBorderBrushBinding = new($"{nameof(MouseOverBorderBrush)}.Color") { Source = this };

                // Start Storyboard
                ColorAnimation BackgroundAnimation = new();
                Storyboard.SetTarget(BackgroundAnimation, PART_Root);
                Storyboard.SetTargetProperty(BackgroundAnimation, BackgroundPath);
                BindingOperations.SetBinding(BackgroundAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(BackgroundAnimation, ColorAnimation.ToProperty, MouseOverBackgroundBinding);

                ColorAnimation ForegroundAnimation = new();
                Storyboard.SetTarget(ForegroundAnimation, PART_Presenter);
                Storyboard.SetTargetProperty(ForegroundAnimation, ForegroundPath);
                BindingOperations.SetBinding(ForegroundAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(ForegroundAnimation, ColorAnimation.ToProperty, MouseOverForegroundBinding);

                ColorAnimation BorderBrushAnimation = new();
                Storyboard.SetTarget(BorderBrushAnimation, PART_Root);
                Storyboard.SetTargetProperty(BorderBrushAnimation, BorderBrushPath);
                BindingOperations.SetBinding(BorderBrushAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(BorderBrushAnimation, ColorAnimation.ToProperty, MouseOverBorderBrushBinding);

                StartStoryboard = new();
                StartStoryboard.Children.Add(BackgroundAnimation);
                StartStoryboard.Children.Add(ForegroundAnimation);
                StartStoryboard.Children.Add(BorderBrushAnimation);
                StartStoryboard.Freeze();

                // End Storyboard
                BackgroundAnimation = new();
                Storyboard.SetTarget(BackgroundAnimation, PART_Root);
                Storyboard.SetTargetProperty(BackgroundAnimation, BackgroundPath);
                BindingOperations.SetBinding(BackgroundAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(BackgroundAnimation, ColorAnimation.ToProperty, new Binding($"{nameof(Background)}.Color") { Source = this });

                ForegroundAnimation = new();
                Storyboard.SetTarget(ForegroundAnimation, PART_Presenter);
                Storyboard.SetTargetProperty(ForegroundAnimation, ForegroundPath);
                BindingOperations.SetBinding(ForegroundAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(ForegroundAnimation, ColorAnimation.ToProperty, new Binding($"{nameof(Foreground)}.Color") { Source = this });

                BorderBrushAnimation = new();
                Storyboard.SetTarget(BorderBrushAnimation, PART_Root);
                Storyboard.SetTargetProperty(BorderBrushAnimation, BorderBrushPath);
                BindingOperations.SetBinding(BorderBrushAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(BorderBrushAnimation, ColorAnimation.ToProperty, new Binding($"{nameof(BorderBrush)}.Color") { Source = this });

                EndStoryboard = new();
                EndStoryboard.Children.Add(BackgroundAnimation);
                EndStoryboard.Children.Add(ForegroundAnimation);
                EndStoryboard.Children.Add(BorderBrushAnimation);
                EndStoryboard.Freeze();

                // Start To End Storyboard
                BackgroundAnimation = new();
                Storyboard.SetTarget(BackgroundAnimation, PART_Root);
                Storyboard.SetTargetProperty(BackgroundAnimation, BackgroundPath);
                BindingOperations.SetBinding(BackgroundAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(BackgroundAnimation, ColorAnimation.FromProperty, MouseOverBackgroundBinding);
                BindingOperations.SetBinding(BackgroundAnimation, ColorAnimation.ToProperty, new Binding($"{nameof(Background)}.Color") { Source = this });

                ForegroundAnimation = new();
                Storyboard.SetTarget(ForegroundAnimation, PART_Presenter);
                Storyboard.SetTargetProperty(ForegroundAnimation, ForegroundPath);
                BindingOperations.SetBinding(ForegroundAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(ForegroundAnimation, ColorAnimation.FromProperty, MouseOverForegroundBinding);
                BindingOperations.SetBinding(ForegroundAnimation, ColorAnimation.ToProperty, new Binding($"{nameof(Foreground)}.Color") { Source = this });

                BorderBrushAnimation = new();
                Storyboard.SetTarget(BorderBrushAnimation, PART_Root);
                Storyboard.SetTargetProperty(BorderBrushAnimation, BorderBrushPath);
                BindingOperations.SetBinding(BorderBrushAnimation, Timeline.DurationProperty, DurationBinding);
                BindingOperations.SetBinding(BorderBrushAnimation, ColorAnimation.FromProperty, MouseOverBorderBrushBinding);
                BindingOperations.SetBinding(BorderBrushAnimation, ColorAnimation.ToProperty, new Binding($"{nameof(BorderBrush)}.Color") { Source = this });

                StartToEndStoryboard = new();
                StartToEndStoryboard.Children.Add(BackgroundAnimation);
                StartToEndStoryboard.Children.Add(ForegroundAnimation);
                StartToEndStoryboard.Children.Add(BorderBrushAnimation);
                StartToEndStoryboard.Freeze();
            }
        }

        private void OnIsAnimatingChanged(bool Start)
        {
            if (!IsVisible)
                return;

            bool IsPressed = IsPressedWhenAnimating;
            IsPressedWhenAnimating = false;
            (Start ? StartStoryboard : IsPressed ? StartToEndStoryboard : EndStoryboard)?.Begin();
        }

        protected override void OnIsPressedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsPressedChanged(e);

            if (e.NewValue is false &&
                IsAnimating)
                IsPressedWhenAnimating = true;
        }

    }
}