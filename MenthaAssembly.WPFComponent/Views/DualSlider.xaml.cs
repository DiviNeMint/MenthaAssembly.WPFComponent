using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MenthaAssembly.Views
{
    public class DualSlider : Control
    {
        public static readonly RoutedEvent LeftValueChangedEvent =
              EventManager.RegisterRoutedEvent("LeftValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventArgs<double>), typeof(DualSlider));
        public static void AddLeftValueChangedHandler(UIElement d, RoutedPropertyChangedEventHandler<double> handler)
            => d.AddHandler(LeftValueChangedEvent, handler);
        public static void RemoveLeftValueChangedHandler(UIElement d, RoutedPropertyChangedEventHandler<double> handler)
            => d.RemoveHandler(LeftValueChangedEvent, handler);

        public static readonly RoutedEvent RightValueChangedEvent =
              EventManager.RegisterRoutedEvent("RightValueChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventArgs<double>), typeof(DualSlider));
        public static void AddRightValueChangedHandler(UIElement d, RoutedPropertyChangedEventHandler<double> handler)
            => d.AddHandler(RightValueChangedEvent, handler);
        public static void RemoveRightValueChangedHandler(UIElement d, RoutedPropertyChangedEventHandler<double> handler)
            => d.RemoveHandler(RightValueChangedEvent, handler);

        public static readonly DependencyProperty OrientationProperty =
              Slider.OrientationProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(Orientation.Horizontal));
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty IsSelectionRangeEnabledProperty =
              Slider.IsSelectionRangeEnabledProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(false,
                  (d, e) =>
                  {
                      if (d is DualSlider This &&
                          This.IsLoaded &&
                          e.NewValue is true)
                          This.UpdateSelectionRange();
                  }));
        public bool IsSelectionRangeEnabled
        {
            get => (bool)GetValue(IsSelectionRangeEnabledProperty);
            set => SetValue(IsSelectionRangeEnabledProperty, value);
        }

        public static readonly DependencyProperty SelectionRangeBrushProperty =
              DependencyProperty.Register("SelectionRangeBrush", typeof(Brush), typeof(DualSlider), new PropertyMetadata(SystemColors.HighlightBrush));
        public Brush SelectionRangeBrush
        {
            get => (Brush)GetValue(SelectionRangeBrushProperty);
            set => SetValue(SelectionRangeBrushProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
              RangeBase.MinimumProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(0d));
        public double Minimum
        {
            get => (double)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
              RangeBase.MaximumProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(1d));
        public double Maximum
        {
            get => (double)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty LeftValueProperty =
              DependencyProperty.Register("LeftValue", typeof(double), typeof(DualSlider), new FrameworkPropertyMetadata(0d,
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  (d, e) =>
                  {
                      if (d is DualSlider This &&
                          This.IsLoaded)
                      {
                          ChangedEventArgs<double> Arg = e.ToChangedEventArgs<double>();
                          if (This.RightValue < Arg.NewValue)
                              This.RightValue = Arg.NewValue;

                          This.OnLeftValueChanged(Arg);

                          if (This.IsSelectionRangeEnabled)
                              This.UpdateSelectionRange();
                      }
                  }));
        public double LeftValue
        {
            get => (double)GetValue(LeftValueProperty);
            set => SetValue(LeftValueProperty, value);
        }

        public static readonly DependencyProperty RightValueProperty =
              DependencyProperty.Register("RightValue", typeof(double), typeof(DualSlider), new FrameworkPropertyMetadata(0d,
                  FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                  (d, e) =>
                  {
                      if (d is DualSlider This &&
                          This.IsLoaded)
                      {
                          ChangedEventArgs<double> Arg = e.ToChangedEventArgs<double>();
                          if (Arg.NewValue < This.LeftValue)
                              This.LeftValue = Arg.NewValue;

                          This.OnRightValueChanged(Arg);

                          if (This.IsSelectionRangeEnabled)
                              This.UpdateSelectionRange();
                      }
                  }));
        public double RightValue
        {
            get => (double)GetValue(RightValueProperty);
            set => SetValue(RightValueProperty, value);
        }

        public static readonly DependencyProperty LargeChangeProperty =
              RangeBase.LargeChangeProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(0.1d));
        public double LargeChange
        {
            get => (double)GetValue(LargeChangeProperty);
            set => SetValue(LargeChangeProperty, value);
        }

        public static readonly DependencyProperty SmallChangeProperty =
              RangeBase.SmallChangeProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(0.01d));
        public double SmallChange
        {
            get => (double)GetValue(SmallChangeProperty);
            set => SetValue(SmallChangeProperty, value);
        }

        public static readonly DependencyProperty DelayProperty =
              Slider.DelayProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(500));
        public int Delay
        {
            get => (int)GetValue(DelayProperty);
            set => SetValue(DelayProperty, value);
        }

        public static readonly DependencyProperty IntervalProperty =
              Slider.IntervalProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(100));
        public int Interval
        {
            get => (int)GetValue(IntervalProperty);
            set => SetValue(IntervalProperty, value);
        }

        //public static readonly DependencyProperty IsSnapToTickEnabledProperty =
        //      Slider.IsSnapToTickEnabledProperty.AddOwner(typeof(DualSlider), new PropertyMetadata(true));
        //public bool IsSnapToTickEnabled
        //{
        //    get => (bool)GetValue(IsSnapToTickEnabledProperty);
        //    set => SetValue(IsSnapToTickEnabledProperty, value);
        //}

        public static readonly DependencyProperty ThumbStyleProperty =
              DependencyProperty.Register("ThumbStyle", typeof(Style), typeof(DualSlider), new PropertyMetadata(null));
        public Style ThumbStyle
        {
            get => (Style)GetValue(ThumbStyleProperty);
            set => SetValue(ThumbStyleProperty, value);
        }

        private Grid PART_Root;
        private Rectangle PART_SelectionRange;
        private Thumb PART_LeftThumb,
                      PART_RightThumb;
        static DualSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DualSlider), new FrameworkPropertyMetadata(typeof(DualSlider)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_Root") is Grid PART_Root)
            {
                PART_Root.MouseDown += OnRootMouseDown;
                PART_Root.MouseMove += OnRootMouseMove;
                PART_Root.MouseUp += OnRootMouseUp;
                this.PART_Root = PART_Root;
            }

            if (GetTemplateChild("PART_SelectionRange") is Rectangle PART_SelectionRange)
                this.PART_SelectionRange = PART_SelectionRange;

            if (GetTemplateChild("PART_LeftThumb") is Thumb PART_LeftThumb)
            {
                PART_LeftThumb.DragDelta += OnLeftThumbDragDelta;
                this.PART_LeftThumb = PART_LeftThumb;
            }

            if (GetTemplateChild("PART_RightThumb") is Thumb PART_RightThumb)
            {
                PART_RightThumb.DragDelta += OnRightThumbDragDelta;
                this.PART_RightThumb = PART_RightThumb;
            }

            Loaded += (s, e) =>
            {
                double Temp = LeftValue;
                if (Temp != 0)
                    OnLeftValueChanged(new ChangedEventArgs<double>(0d, Temp));

                Temp = RightValue;
                if (Temp != 1d)
                    OnRightValueChanged(new ChangedEventArgs<double>(1d, Temp));

                if (IsSelectionRangeEnabled)
                    UpdateSelectionRange();
            };
        }

        private bool IsLeftMouseDown = false;
        private Point LeftMouseDownPosition;
        private DelayActionToken MouseDownToken = null;
        private void OnRootMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                IsLeftMouseDown = true;
                LeftMouseDownPosition = e.GetPosition(this);

                void Task()
                {
                    OnRootClicked(LeftMouseDownPosition);
                    MouseDownToken = DispatcherHelper.DelayAction(Interval, Task, Cancel);
                };
                void Cancel() => MouseDownToken = null;
                MouseDownToken = DispatcherHelper.DelayAction(Delay, Task, Cancel);

                PART_Root.CaptureMouse();
            }
        }
        private void OnRootMouseMove(object sender, MouseEventArgs e)
            => MouseDownToken?.Cancel();
        private void OnRootMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (IsLeftMouseDown &&
                e.ChangedButton == MouseButton.Left)
            {
                MouseDownToken?.Cancel();

                Vector Delta = LeftMouseDownPosition - e.GetPosition(this);
                if (Delta.LengthSquared < 25)
                    OnRootClicked(LeftMouseDownPosition);

                IsLeftMouseDown = false;
                PART_Root.ReleaseMouseCapture();
            }
        }
        protected void OnRootClicked(Point Position)
        {
            switch (Orientation)
            {
                case Orientation.Vertical:
                    {
                        double Py = Position.Y;
                        if (Py < PART_LeftThumb.Margin.Top)
                        {
                            double TempValue = LeftValue,
                                   NewValue = Math.Max(TempValue - LargeChange, Minimum);

                            if (TempValue != NewValue)
                                LeftValue = NewValue;
                        }
                        else if (PART_RightThumb.Margin.Top + PART_RightThumb.ActualHeight < Py)
                        {
                            double TempValue = RightValue,
                                   NewValue = Math.Min(TempValue + LargeChange, Maximum);

                            if (TempValue != NewValue)
                                RightValue = NewValue;
                        }
                        break;
                    }
                case Orientation.Horizontal:
                default:
                    {
                        double Px = Position.X;
                        if (Px < PART_LeftThumb.Margin.Left)
                        {
                            double TempValue = LeftValue,
                                   NewValue = Math.Max(TempValue - LargeChange, Minimum);

                            if (TempValue != NewValue)
                                LeftValue = NewValue;
                        }
                        else if (PART_RightThumb.Margin.Left + PART_RightThumb.ActualWidth < Px)
                        {
                            double TempValue = RightValue,
                                   NewValue = Math.Min(TempValue + LargeChange, Maximum);

                            if (TempValue != NewValue)
                                RightValue = NewValue;
                        }
                        break;
                    }
            }
        }

        protected virtual void OnLeftThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double Max = Maximum,
                   Min = Minimum,
                   TempValue = LeftValue,
                   Delta = (Orientation == Orientation.Horizontal ? e.HorizontalChange / (ActualWidth - PART_LeftThumb.ActualWidth) :
                                                                    e.VerticalChange / (ActualHeight - PART_LeftThumb.ActualHeight)) * (Max - Min),
                   NewValue = MathHelper.Clamp(TempValue + Delta, Min, Max);

            if (NewValue != Min && NewValue != Max)
                NewValue = MathHelper.Round(NewValue, SmallChange);

            if (TempValue != NewValue)
                LeftValue = NewValue;
        }
        protected virtual void OnRightThumbDragDelta(object sender, DragDeltaEventArgs e)
        {
            double Max = Maximum,
                   Min = Minimum,
                   TempValue = RightValue,
                   Delta = (Orientation == Orientation.Horizontal ? e.HorizontalChange / (ActualWidth - PART_RightThumb.ActualWidth) :
                                                                    e.VerticalChange / (ActualHeight - PART_RightThumb.ActualHeight)) * (Max - Min),
                   NewValue = MathHelper.Clamp(TempValue + Delta, Min, Max);

            if (NewValue != Min && NewValue != Max)
                NewValue = MathHelper.Round(NewValue, SmallChange);

            if (TempValue != NewValue)
                RightValue = NewValue;
        }

        protected virtual void OnLeftValueChanged(ChangedEventArgs<double> e)
        {
            Thickness Margin = PART_LeftThumb.Margin;
            double Factor = (e.NewValue - Minimum) / (Maximum - Minimum);

            switch (Orientation)
            {
                case Orientation.Vertical:
                    Margin.Top = (ActualHeight - PART_LeftThumb.ActualHeight - PART_RightThumb.ActualHeight + 1) * Factor;
                    break;
                case Orientation.Horizontal:
                default:
                    Margin.Left = (ActualWidth - PART_LeftThumb.ActualWidth - PART_RightThumb.ActualWidth + 1) * Factor;
                    break;
            }

            PART_LeftThumb.Margin = Margin;

            RaiseEvent(e.ToRoutedPropertyChangedEventArgs(LeftValueChangedEvent));
        }
        protected virtual void OnRightValueChanged(ChangedEventArgs<double> e)
        {
            Thickness Margin = PART_RightThumb.Margin;
            double Factor = (e.NewValue - Minimum) / (Maximum - Minimum);

            switch (Orientation)
            {
                case Orientation.Vertical:
                    {
                        double LH = PART_LeftThumb.ActualHeight - 1,
                               Length = ActualHeight - PART_RightThumb.ActualHeight - LH;

                        Margin.Top = LH + Length * Factor;
                        break;
                    }
                case Orientation.Horizontal:
                default:
                    {
                        double LW = PART_LeftThumb.ActualWidth - 1,
                               Length = ActualWidth - PART_RightThumb.ActualWidth - LW;

                        Margin.Left = LW + Length * Factor;
                    }
                    break;
            }

            PART_RightThumb.Margin = Margin;

            RaiseEvent(e.ToRoutedPropertyChangedEventArgs(RightValueChangedEvent));
        }

        protected virtual void UpdateSelectionRange()
        {
            switch (Orientation)
            {
                case Orientation.Vertical:
                    {
                        double Top = PART_LeftThumb.Margin.Top;
                        PART_SelectionRange.Margin = new Thickness(-1, Top, -1, 0);
                        PART_SelectionRange.Width = double.NaN;
                        PART_SelectionRange.Height = PART_RightThumb.Margin.Top + (PART_RightThumb.ActualHeight - PART_LeftThumb.ActualHeight) / 2 - Top;
                        break;
                    }
                case Orientation.Horizontal:
                default:
                    {
                        double Left = PART_LeftThumb.Margin.Left;

                        PART_SelectionRange.Margin = new Thickness(Left, -1, 0, -1);
                        PART_SelectionRange.Width = PART_RightThumb.Margin.Left + (PART_RightThumb.ActualWidth - PART_LeftThumb.ActualWidth) / 2 - Left;
                        PART_SelectionRange.Height = double.NaN;
                        break;
                    }
            }
        }

    }
}
