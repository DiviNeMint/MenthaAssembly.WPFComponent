using MenthaAssembly.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        #region Axises
        public static readonly DependencyProperty XAxisInternalCountProperty =
              DependencyProperty.Register("XAxisInternalCount", typeof(int), typeof(LineChart), new PropertyMetadata(6, (d, e) =>
              {
                  if (d is LineChart ThisChart)
                      ThisChart.UpdateXAxis();
              }));
        public int XAxisInternalCount
        {
            get => (int)GetValue(XAxisInternalCountProperty);
            set => SetValue(XAxisInternalCountProperty, value);
        }

        public static readonly DependencyProperty YAxisInternalCountProperty =
              DependencyProperty.Register("YAxisInternalCount", typeof(int), typeof(LineChart), new PropertyMetadata(6, (d, e) =>
              {
                  if (d is LineChart ThisChart)
                      ThisChart.UpdateYAxis();
              }));
        public int YAxisInternalCount
        {
            get => (int)GetValue(YAxisInternalCountProperty);
            set => SetValue(YAxisInternalCountProperty, value);
        }

        public static readonly DependencyProperty XAxisLabelStyleProperty =
              DependencyProperty.Register("XAxisLabelStyle", typeof(Style), typeof(LineChart), new PropertyMetadata(null));
        public Style XAxisLabelStyle
        {
            get => (Style)GetValue(XAxisLabelStyleProperty);
            set => SetValue(XAxisLabelStyleProperty, value);
        }

        public static readonly DependencyProperty YAxisLabelStyleProperty =
              DependencyProperty.Register("YAxisLabelStyle", typeof(Style), typeof(LineChart), new PropertyMetadata(null));
        public Style YAxisLabelStyle
        {
            get => (Style)GetValue(YAxisLabelStyleProperty);
            set => SetValue(YAxisLabelStyleProperty, value);
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
            get => (bool)GetValue(ShowClosestDataPointProperty);
            set => SetValue(ShowClosestDataPointProperty, value);
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

                              if (ThisChart.AuxiliaryLines is LineChartAuxiliaryLineCollection Lines)
                              {
                                  foreach (LineChartAuxiliaryLine Line in Lines)
                                      Line.Update(NewItem);
                              }
                          }
                      }
                  }));
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty PaletteProperty =
              DependencyProperty.Register("Palette", typeof(IList<Brush>), typeof(LineChart), new PropertyMetadata(null));
        public IList<Brush> Palette
        {
            get => (IList<Brush>)GetValue(PaletteProperty);
            set => SetValue(PaletteProperty, value);
        }

        #endregion
        #region AuxiliaryLine
        public static readonly DependencyProperty AuxiliaryLinesProperty =
              DependencyProperty.Register("AuxiliaryLines", typeof(LineChartAuxiliaryLineCollection), typeof(LineChart), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is LineChart This)
                      {
                          if (e.OldValue is LineChartAuxiliaryLineCollection OldCollection)
                          {
                              OldCollection.CollectionChanged -= This.OnAuxiliaryLineCollectionChanged;
                              foreach (LineChartAuxiliaryLine Line in OldCollection)
                              {
                                  This.PART_Studio.Children.Remove(Line);
                                  Line.Chart = null;
                              }
                          }

                          if (e.NewValue is LineChartAuxiliaryLineCollection NewCollection)
                          {
                              NewCollection.CollectionChanged += This.OnAuxiliaryLineCollectionChanged;

                              if (This.IsLoaded)
                              {
                                  foreach (LineChartAuxiliaryLine Line in NewCollection)
                                  {
                                      This.PART_Studio.Children.Add(Line);
                                      Line.Chart = This;
                                  }
                              }
                          }
                      }
                  }));
        public LineChartAuxiliaryLineCollection AuxiliaryLines
        {
            get => (LineChartAuxiliaryLineCollection)GetValue(AuxiliaryLinesProperty);
            set => SetValue(AuxiliaryLinesProperty, value);
        }

        private void OnAuxiliaryLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (LineChartAuxiliaryLine Line in e.NewItems.OfType<LineChartAuxiliaryLine>())
                        {
                            PART_Studio.Children.Add(Line);
                            Line.Chart = this;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (LineChartAuxiliaryLine Line in e.OldItems.OfType<LineChartAuxiliaryLine>())
                        {
                            PART_Studio.Children.Remove(Line);
                            Line.Chart = null;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Replace:
                    {
                        foreach (LineChartAuxiliaryLine Line in e.OldItems.OfType<LineChartAuxiliaryLine>())
                        {
                            PART_Studio.Children.Remove(Line);
                            Line.Chart = null;
                        }

                        foreach (LineChartAuxiliaryLine Line in e.NewItems.OfType<LineChartAuxiliaryLine>())
                        {
                            PART_Studio.Children.Add(Line);
                            Line.Chart = this;
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        foreach (LineChartAuxiliaryLine Line in PART_Studio.Children.OfType<LineChartAuxiliaryLine>())
                        {
                            PART_Studio.Children.Remove(Line);
                            Line.Chart = null;
                        }

                        break;
                    }
            }
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

            if (GetTemplateChild("PART_Studio") is Grid PART_Studio)
            {
                this.PART_Studio = PART_Studio;
                PART_Studio.SizeChanged += (s, e) =>
                {
                    UpdateYAxis();
                    UpdateXAxis();
                    UpdateLines();
                    UpdateAuxiliaryLines();
                };

                PART_Studio.MouseEnter += (s, e) =>
                {
                    if (ShowClosestDataPoint &&
                        SelectedItem is LineChartItem Item)
                        Item.ShowClosestDataPoint = true;
                };
                PART_Studio.MouseLeave += (s, e) =>
                {

                    if (ShowClosestDataPoint &&
                        SelectedItem is LineChartItem Item)
                        Item.ShowClosestDataPoint = false;
                };
                PART_Studio.MouseMove += (s, e) =>
                {
                    if (SelectedItem is LineChartItem Item)
                        Item.UpdateClosestDataPoint();
                };

                if (AuxiliaryLines is LineChartAuxiliaryLineCollection Lines)
                {
                    foreach (LineChartAuxiliaryLine Line in Lines)
                    {
                        PART_Studio.Children.Add(Line);
                        Line.Chart = this;
                    }
                }
            }

            if (GetTemplateChild("PART_XAxisStudio") is Canvas PART_XAxisStudio)
                this.PART_XAxisStudio = PART_XAxisStudio;

            if (GetTemplateChild("PART_YAxisStudio") is Grid PART_YAxisStudio)
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
            int Count = XAxisInternalCount;
            TextBlock Label;
            for (int i = CurrentXLabels.Count; i < Count; i++)
            {
                // DequeueAxisLabel
                if (!CacheLabelQueue.TryDequeue(out Label))
                    Label = new TextBlock();

                PrepareXAxisLabel(Label);

                CurrentXLabels.Add(Label);
                PART_XAxisStudio.Children.Add(Label);
            }

            for (int i = CurrentXLabels.Count - 1; i >= Count; i--)
            {
                Label = CurrentXLabels[i];

                CurrentXLabels.RemoveAt(i);
                PART_XAxisStudio.Children.Remove(Label);

                BindingOperations.ClearAllBindings(Label);
                CacheLabelQueue.Enqueue(Label);
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
                Label.Margin = new Thickness(CalcuateVisualCoordinateX(RealValue) - TextData.Width * 0.5d, 0, 0, 0);

                Value += Dx;
            }
        }
        protected virtual void UpdateYAxis()
        {
            int Count = YAxisInternalCount;
            TextBlock Label;
            for (int i = CurrentYLabels.Count; i < Count; i++)
            {
                // DequeueAxisLabel
                if (!CacheLabelQueue.TryDequeue(out Label))
                    Label = new TextBlock();

                PrepareYAxisLabel(Label);

                CurrentYLabels.Add(Label);
                PART_YAxisStudio.Children.Add(Label);
            }

            for (int i = CurrentYLabels.Count - 1; i >= Count; i--)
            {
                Label = CurrentYLabels[i];

                CurrentYLabels.RemoveAt(i);
                PART_YAxisStudio.Children.Remove(Label);

                BindingOperations.ClearAllBindings(Label);
                CacheLabelQueue.Enqueue(Label);
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

                Label.Margin = new Thickness(0, CalcuateVisualCoordinateY(RealValue) - TextData.Height / 2d, 0, 0);

                Value += Dy;
            }
        }

        protected virtual void PrepareXAxisLabel(TextBlock Label)
        {
            Label.SetBinding(StyleProperty, new Binding(nameof(XAxisLabelStyle)) { Source = this });
            Label.VerticalAlignment = VerticalAlignment.Top;
        }
        protected virtual void PrepareYAxisLabel(TextBlock Label)
        {
            Label.SetBinding(StyleProperty, new Binding(nameof(YAxisLabelStyle)) { Source = this });
            Label.VerticalAlignment = VerticalAlignment.Top;
        }

        protected virtual void UpdateLines(IEnumerable<LineChartItem> Items = null)
        {
            foreach (LineChartItem Item in Items ?? GetLineChartItems())
            {
                Item.InvalidatePolyLine();
                Item.InvalidateFillArea();
                Item.InvalidateVisual();
            }
        }
        protected virtual void UpdateAuxiliaryLines()
        {
            if (SelectedItem is LineChartItem Item &&
                AuxiliaryLines is LineChartAuxiliaryLineCollection Lines)
            {
                foreach (LineChartAuxiliaryLine Line in Lines)
                    Line.Update(Item);
            }
        }

        protected internal Point CalcuateVisualCoordinate(Point DataPoint)
            => new Point(CalcuateVisualCoordinateX(DataPoint.X), CalcuateVisualCoordinateY(DataPoint.Y));
        protected internal Point CalcuateVisualCoordinate(double DataPointX, double DataPointY)
            => new Point(CalcuateVisualCoordinateX(DataPointX), CalcuateVisualCoordinateY(DataPointY));
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
            => new Point(CalcuateDataCoordinateX(VisualPoint.X), CalcuateDataCoordinateY(VisualPoint.Y));
        protected internal Point CalcuateDataCoordinate(double VisualPointX, double VisualPointY)
            => new Point(CalcuateDataCoordinateX(VisualPointX), CalcuateDataCoordinateY(VisualPointY));
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

            foreach (LineChartItem Item in GetLineChartItems())
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

            UpdateXAxis();
            UpdateYAxis();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
            => item is LineChartItem;
        protected override DependencyObject GetContainerForItemOverride()
            => new LineChartItem();

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is LineChartItem LineChartItem)
                PrepareLineChartItem(LineChartItem, item);
        }
        private void PrepareLineChartItem(LineChartItem Item, object Data)
        {
            Item.Chart = this;
            Item.DatasUpdated += OnItemDatasUpdated;
            Item.PreviewMouseDown += OnItemPreviewMouseDown;

            if (Item.ItemsSource is null &&
                Item.GetBindingExpression(LineChartItem.ItemsSourceProperty) is null)
                Item.SetBinding(LineChartItem.ItemsSourceProperty, new Binding(DisplayMemberPath) { Source = Data });
            else
                OnItemDatasUpdated();

            if (Item.Stroke is null)
            {
                IList<Brush> Palette = this.Palette;
                Brush Color = null;
                if (Palette != null)
                {
                    int Index = ItemContainerGenerator.IndexFromContainer(Item);
                    if (-1 < Index && Index < Palette.Count)
                        Color = Palette[Index];
                }

                if (Color is null)
                    Color = Brushes.Black;

                Item.Stroke = Color;

                if (Item.AllowFillArea & Item.Fill is null)
                    Item.Fill = Color;
            }

            SelectedItem = Item;
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            base.ClearContainerForItemOverride(element, item);
            if (element is LineChartItem LineChartItem)
                ResetLineChartItem(LineChartItem, item);
        }
        private void ResetLineChartItem(LineChartItem Item, object Data)
        {
            Item.Chart = null;
            Item.DatasUpdated -= OnItemDatasUpdated;
            Item.PreviewMouseDown -= OnItemPreviewMouseDown;

            if (Data is not LineChartItem)
            {
                Item.ClearValue(LineChartItem.ItemsSourceProperty);
                Item.ClearValue(LineChartItem.StrokeProperty);
            }
        }

        private DelayActionToken UpdateToken;
        protected virtual void OnItemDatasUpdated(object sender = null, EventArgs e = null)
        {
            UpdateMaxAndMinValue();

            UpdateToken?.Cancel();
            UpdateToken = DispatcherHelper.DelayAction(100d, () =>
            {
                UpdateLines(GetLineChartItems().Where(i => !i.Equals(sender)));
                UpdateAuxiliaryLines();
            });
        }
        protected virtual void OnItemPreviewMouseDown(object sender, MouseButtonEventArgs e)
            => SelectedItem = sender;

        protected IEnumerable<LineChartItem> GetLineChartItems()
        {
            for (int i = 0; i < Items.Count; i++)
                if (ItemContainerGenerator.ContainerFromIndex(i) is LineChartItem Item)
                    yield return Item;
        }

    }
}
