using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    [StyleTypedProperty(Property = "XAxisLabelStyle", StyleTargetType = typeof(TextBlock))]
    [StyleTypedProperty(Property = "YAxisLabelStyle", StyleTargetType = typeof(TextBlock))]
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(LineChartItem))]
    public class LineChart : ItemsControl
    {
        public static readonly DependencyProperty PaletteProperty =
              DependencyProperty.Register("Palette", typeof(IList<Brush>), typeof(LineChart), new PropertyMetadata(null));
        public IList<Brush> Palette
        {
            get => (IList<Brush>)this.GetValue(PaletteProperty);
            set => this.SetValue(PaletteProperty, value);
        }

        #region Axises
        public static readonly DependencyProperty XAxisInternalCountProperty =
              DependencyProperty.Register("XAxisInternalCount", typeof(int), typeof(LineChart), new PropertyMetadata(6, (d, e) =>
              {
                  if (d is LineChart ThisChart)
                      ThisChart.UpdateXAxis();
              }));
        public int XAxisInternalCount
        {
            get => (int)this.GetValue(XAxisInternalCountProperty);
            set => this.SetValue(XAxisInternalCountProperty, value);
        }

        public static readonly DependencyProperty YAxisInternalCountProperty =
              DependencyProperty.Register("YAxisInternalCount", typeof(int), typeof(LineChart), new PropertyMetadata(6, (d, e) =>
              {
                  if (d is LineChart ThisChart)
                      ThisChart.UpdateYAxis();
              }));
        public int YAxisInternalCount
        {
            get => (int)this.GetValue(YAxisInternalCountProperty);
            set => this.SetValue(YAxisInternalCountProperty, value);
        }

        public static readonly DependencyProperty XAxisLabelStyleProperty =
              DependencyProperty.Register("XAxisLabelStyle", typeof(Style), typeof(LineChart), new PropertyMetadata(null));
        public Style XAxisLabelStyle
        {
            get => (Style)this.GetValue(XAxisLabelStyleProperty);
            set => this.SetValue(XAxisLabelStyleProperty, value);
        }

        public static readonly DependencyProperty YAxisLabelStyleProperty =
              DependencyProperty.Register("YAxisLabelStyle", typeof(Style), typeof(LineChart), new PropertyMetadata(null));
        public Style YAxisLabelStyle
        {
            get => (Style)this.GetValue(YAxisLabelStyleProperty);
            set => this.SetValue(YAxisLabelStyleProperty, value);
        }

        #endregion
        #region Items
        public static readonly DependencyProperty ShowClosestDataPointProperty =
              DependencyProperty.Register("ShowClosestDataPoint", typeof(bool), typeof(LineChart), new PropertyMetadata(true,
                  (d, e) =>
                  {
                      if (d is LineChart ThisChart &&
                          ThisChart.SelectedItem is LineChartItem Item)
                      {
                          bool NewValue = e.NewValue is true;
                          if (Item.ShowClosestDataPoint != NewValue)
                              Item.ShowClosestDataPoint = NewValue;
                      }
                  }));
        public bool ShowClosestDataPoint
        {
            get => (bool)this.GetValue(ShowClosestDataPointProperty);
            set => this.SetValue(ShowClosestDataPointProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
              DependencyProperty.Register("SelectedItem", typeof(object), typeof(LineChart), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is LineChart ThisChart)
                      {
                          if (e.OldValue is LineChartItem OldItem)
                          {
                              int Index = ThisChart.ItemContainerGenerator.IndexFromContainer(OldItem);
                              if (Index > 0)
                                  Panel.SetZIndex(OldItem, Index);

                              OldItem.ShowClosestDataPoint = false;
                          }

                          if (e.NewValue is LineChartItem NewItem)
                          {
                              Panel.SetZIndex(NewItem, ThisChart.Items.Count);
                              NewItem.ShowClosestDataPoint = ThisChart.ShowClosestDataPoint;
                          }
                      }
                  }));
        public object SelectedItem
        {
            get => this.GetValue(SelectedItemProperty);
            set => this.SetValue(SelectedItemProperty, value);
        }

        #endregion

        protected static Style DefaultXAxisLabelStyle, DefaultYAxisLabelStyle;
        static LineChart()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChart), new FrameworkPropertyMetadata(typeof(LineChart)));
        }

        protected Grid PART_Studio, PART_YAxisStudio;
        protected Canvas PART_XAxisStudio;
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.GetTemplateChild("PART_Studio") is Grid PART_Studio)
            {
                this.PART_Studio = PART_Studio;
                PART_Studio.SizeChanged += (s, e) =>
                {
                    this.UpdateYAxis();
                    this.UpdateXAxis();
                    this.UpdateLines();
                };

                PART_Studio.MouseEnter += (s, e) =>
                {
                    if (this.ShowClosestDataPoint &&
                        this.SelectedItem is LineChartItem Item)
                        Item.ShowClosestDataPoint = true;
                };
                PART_Studio.MouseLeave += (s, e) =>
                {

                    if (this.ShowClosestDataPoint &&
                        this.SelectedItem is LineChartItem Item)
                        Item.ShowClosestDataPoint = false;
                };
                PART_Studio.MouseMove += (s, e) =>
                {
                    if (this.SelectedItem is LineChartItem Item)
                        Item.UpdateClosestDataPoint();
                };
            }

            if (this.GetTemplateChild("PART_XAxisStudio") is Canvas PART_XAxisStudio)
                this.PART_XAxisStudio = PART_XAxisStudio;

            if (this.GetTemplateChild("PART_YAxisStudio") is Grid PART_YAxisStudio)
                this.PART_YAxisStudio = PART_YAxisStudio;
        }

        private double MaxXValue = 1d,
                       MaxYValue = 1d,
                       MinXValue = 0d,
                       MinYValue = 0d;

        private readonly Pool<TextBlock> CacheLabelQueue = new Pool<TextBlock>();
        private readonly List<TextBlock> CurrentXLabels = new List<TextBlock>(),
                                         CurrentYLabels = new List<TextBlock>();
        protected virtual void UpdateXAxis()
        {
            int Count = this.XAxisInternalCount;
            TextBlock Label;
            for (int i = CurrentXLabels.Count; i < Count; i++)
            {
                // DequeueAxisLabel
                if (!CacheLabelQueue.TryDequeue(out Label))
                    Label = new TextBlock();

                this.PrepareXAxisLabel(Label);

                CurrentXLabels.Add(Label);
                PART_XAxisStudio.Children.Add(Label);
            }

            for (int i = CurrentXLabels.Count - 1; i >= Count; i--)
            {
                Label = CurrentXLabels[i];

                CurrentXLabels.RemoveAt(i);
                PART_XAxisStudio.Children.Remove(Label);

                BindingOperations.ClearAllBindings(Label);
                CacheLabelQueue.Enqueue(ref Label);
            }

            Typeface Typeface = null;
            double Dx = (MaxXValue - MinXValue) * 10d / (Count - 1),
                   Value = MinXValue * 10d;
            for (int i = 0; i < Count; i++)
            {
                double RealValue = Math.Ceiling(Value) / 10d;
                Label = CurrentXLabels[i];
                Label.Text = $"{RealValue}";

                // Check render
                if (Label.ActualWidth is 0d)
                    Label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (Typeface is null)
                    Typeface = new Typeface(Label.FontFamily, Label.FontStyle, Label.FontWeight, Label.FontStretch);

                FormattedText TextData = new FormattedText(Label.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, Label.FontSize, Brushes.Black, 1);

                Label.Width = TextData.Width;
                Label.Margin = new Thickness(this.CalcuateVisualCoordinateX(RealValue) - TextData.Width * 0.5d, 0, 0, 0);

                Value += Dx;
            }
        }
        protected virtual void UpdateYAxis()
        {
            int Count = this.YAxisInternalCount;
            TextBlock Label;
            for (int i = CurrentYLabels.Count; i < Count; i++)
            {
                // DequeueAxisLabel
                if (!CacheLabelQueue.TryDequeue(out Label))
                    Label = new TextBlock();

                this.PrepareYAxisLabel(Label);

                CurrentYLabels.Add(Label);
                PART_YAxisStudio.Children.Add(Label);
            }

            for (int i = CurrentYLabels.Count - 1; i >= Count; i--)
            {
                Label = CurrentYLabels[i];

                CurrentYLabels.RemoveAt(i);
                PART_YAxisStudio.Children.Remove(Label);

                BindingOperations.ClearAllBindings(Label);
                CacheLabelQueue.Enqueue(ref Label);
            }

            Typeface Typeface = null;
            double Dy = (MaxYValue - MinYValue) * 10d / (Count - 1),
                   Value = MinYValue * 10d;
            for (int i = 0; i < Count; i++)
            {
                double RealValue = Math.Ceiling(Value) / 10d;
                Label = CurrentYLabels[i];
                Label.Text = $"{RealValue}";

                // Check render
                if (Label.ActualWidth is 0d)
                    Label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

                if (Typeface is null)
                    Typeface = new Typeface(Label.FontFamily, Label.FontStyle, Label.FontWeight, Label.FontStretch);

                FormattedText TextData = new FormattedText(Label.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface, Label.FontSize, Brushes.Black, 1);

                Label.Margin = new Thickness(0, this.CalcuateVisualCoordinateY(RealValue) - TextData.Height / 2d, 0, 0);

                Value += Dy;
            }
        }

        protected virtual void PrepareXAxisLabel(TextBlock Label)
        {
            Label.SetBinding(StyleProperty, new Binding(nameof(this.XAxisLabelStyle)) { Source = this });
            Label.VerticalAlignment = VerticalAlignment.Top;
        }
        protected virtual void PrepareYAxisLabel(TextBlock Label)
        {
            Label.SetBinding(StyleProperty, new Binding(nameof(this.YAxisLabelStyle)) { Source = this });
            Label.VerticalAlignment = VerticalAlignment.Top;
        }

        protected virtual void UpdateLines(IEnumerable<LineChartItem> Items = null)
        {
            foreach (LineChartItem Item in Items ?? this.GetLineChartItems())
            {
                Item.InvalidatePolyLine();
                Item.InvalidateFillArea();
                Item.InvalidateVisual();
            }
        }

        protected internal Point CalcuateVisualCoordinate(Point DataPoint)
            => new Point(this.CalcuateVisualCoordinateX(DataPoint.X), this.CalcuateVisualCoordinateY(DataPoint.Y));
        protected internal Point CalcuateVisualCoordinate(double DataPointX, double DataPointY)
            => new Point(this.CalcuateVisualCoordinateX(DataPointX), this.CalcuateVisualCoordinateY(DataPointY));
        protected internal virtual double CalcuateVisualCoordinateX(double DataPointX)
        {
            if (double.IsNegativeInfinity(DataPointX))
                return 0d;

            double MaxWidth = PART_Studio.ActualWidth;
            if (double.IsPositiveInfinity(DataPointX))
                return MaxWidth;

            return MaxWidth * MathHelper.Clamp((DataPointX - MinXValue) / (MaxXValue - MinXValue), 0d, 1d);
        }
        protected internal virtual double CalcuateVisualCoordinateY(double DataPointY)
        {
            if (double.IsPositiveInfinity(DataPointY))
                return 0d;

            double Oy = PART_Studio.ActualHeight;
            if (double.IsNegativeInfinity(DataPointY))
                return Oy;

            return Oy * MathHelper.Clamp(1 - (DataPointY - MinYValue) / (MaxYValue - MinYValue), 0d, 1d);
        }

        protected internal Point CalcuateDataCoordinate(Point VisualPoint)
            => new Point(this.CalcuateDataCoordinateX(VisualPoint.X), this.CalcuateDataCoordinateY(VisualPoint.Y));
        protected internal Point CalcuateDataCoordinate(double VisualPointX, double VisualPointY)
            => new Point(this.CalcuateDataCoordinateX(VisualPointX), this.CalcuateDataCoordinateY(VisualPointY));
        protected internal virtual double CalcuateDataCoordinateX(double VisualPointX)
        {
            double MaxWidth = PART_Studio.ActualWidth;

            return MinXValue + VisualPointX * (MaxXValue - MinXValue) / MaxWidth;
        }
        protected internal virtual double CalcuateDataCoordinateY(double VisualPointY)
        {
            double Oy = PART_Studio.ActualHeight;

            return MinYValue + (VisualPointY - Oy) * (MaxYValue - MinYValue) / Oy;
        }

        protected void UpdateMaxAndMinValue()
        {
            double MinX = double.PositiveInfinity,
                   MaxX = double.NegativeInfinity,
                   MinY = double.PositiveInfinity,
                   MaxY = double.NegativeInfinity,
                   Temp;

            foreach (LineChartItem Item in this.GetLineChartItems())
            {
                Temp = Item.MinY;
                if (!double.IsNaN(Temp) && Temp < MinY)
                    MinY = Temp;

                Temp = Item.MaxY;
                if (!double.IsNaN(Temp) && MaxY < Temp)
                    MaxY = Temp;

                Temp = Item.MinX;
                if (!double.IsNaN(Temp) && Temp < MinX)
                    MinX = Temp;

                Temp = Item.MaxX;
                if (!double.IsNaN(Temp) && MaxX < Temp)
                    MaxX = Temp;
            }

            if (double.IsPositiveInfinity(MinX) || double.IsNegativeInfinity(MaxX) ||
                double.IsPositiveInfinity(MinY) || double.IsNegativeInfinity(MaxY))
            {
                MinXValue = MinYValue = 0d;
                MaxXValue = MaxYValue = 1d;
            }
            else
            {
                // X
                MinXValue = MinX;
                MaxXValue = MinX == MaxX ? MinX + 1d : MaxX;

                // Y
                double Dy = MaxY - MinY;
                if (Dy is 0d)
                {
                    MinYValue = MinY - 0.5d;
                    MaxYValue = MaxY + 0.5d;
                }
                else if (Dy < 1d)
                {
                    MinYValue = Math.Floor(MinY * 8d) / 10d;
                    MaxYValue = Math.Ceiling(MaxY * 12.5d) / 10d;
                }
                else
                {
                    MinYValue = Math.Floor(MinY);
                    MaxYValue = Math.Floor(MaxY * 1.25d);
                }
            }

            this.UpdateXAxis();
            this.UpdateYAxis();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is LineChartItem;
        protected override DependencyObject GetContainerForItemOverride()
            => new LineChartItem();

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is LineChartItem LineChartItem)
                this.PrepareLineChartItem(LineChartItem, item);
        }
        private void PrepareLineChartItem(LineChartItem Item, object Data)
        {
            Item.Chart = this;
            Item.DatasUpdated += this.OnItemDatasUpdated;
            Item.PreviewMouseDown += this.OnItemPreviewMouseDown;

            if (Item.ItemsSource is null &&
                Item.GetBindingExpression(LineChartItem.ItemsSourceProperty) is null)
                Item.SetBinding(LineChartItem.ItemsSourceProperty, new Binding(nameof(this.DataContext)) { Source = Item });
            else
                this.OnItemDatasUpdated();

            if (Item.Stroke is null)
            {
                IList<Brush> Palette = this.Palette;
                Brush Color = null;
                if (Palette != null)
                {
                    int Index = this.ItemContainerGenerator.IndexFromContainer(Item);
                    if (-1 < Index && Index < Palette.Count)
                        Color = Palette[Index];
                }

                if (Color is null)
                    Color = Brushes.Black;

                Item.Stroke = Color;

                if (Item.AllowFillArea & Item.Fill is null)
                    Item.Fill = Color;
            }

            this.SelectedItem = Item;
        }

        private DelayActionToken UpdateToken;
        protected virtual void OnItemDatasUpdated(object sender = null, EventArgs e = null)
        {
            this.UpdateMaxAndMinValue();

            UpdateToken?.Cancel();
            UpdateToken = DispatcherHelper.DelayAction(100d, () => this.UpdateLines(this.GetLineChartItems().Where(i => !i.Equals(sender))));
        }
        protected virtual void OnItemPreviewMouseDown(object sender, MouseButtonEventArgs e)
            => this.SelectedItem = sender;

        protected IEnumerable<LineChartItem> GetLineChartItems()
        {
            for (int i = 0; i < this.Items.Count; i++)
                if (this.ItemContainerGenerator.ContainerFromIndex(i) is LineChartItem Item)
                    yield return Item;
        }

    }
}
