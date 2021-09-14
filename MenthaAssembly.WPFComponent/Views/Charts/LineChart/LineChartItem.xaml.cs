using MenthaAssembly.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class LineChartItem : FrameworkElement
    {
        internal event EventHandler DatasUpdated;
        public event EventHandler DataPointClick;

        #region Datas
        public static readonly DependencyProperty ItemsSourceProperty =
              DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(LineChartItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) =>
              {
                  if (d is LineChartItem ThisItem)
                  {
                      if (e.OldValue is INotifyCollectionChanged OldCollection)
                          OldCollection.CollectionChanged -= ThisItem.OnItemsSourceCollectionChanged;

                      if (e.NewValue is INotifyCollectionChanged NewCollection)
                          NewCollection.CollectionChanged += ThisItem.OnItemsSourceCollectionChanged;

                      ThisItem.DataExtractor = null;
                      ThisItem.UpdateDatas();
                  }
              }));
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty XValuePathProperty =
              DependencyProperty.Register("XValuePath", typeof(string), typeof(LineChartItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) =>
              {
                  if (d is LineChartItem ThisItem)
                  {
                      ThisItem.DataExtractor = null;
                      ThisItem.UpdateDatas();
                  }
              }));
        public string XValuePath
        {
            get => (string)this.GetValue(XValuePathProperty);
            set => this.SetValue(XValuePathProperty, value);
        }

        public static readonly DependencyProperty YValuePathProperty =
              DependencyProperty.Register("YValuePath", typeof(string), typeof(LineChartItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) =>
              {
                  if (d is LineChartItem ThisItem)
                  {
                      ThisItem.DataExtractor = null;
                      ThisItem.UpdateDatas();
                  }
              }));
        public string YValuePath
        {
            get => (string)this.GetValue(YValuePathProperty);
            set => this.SetValue(YValuePathProperty, value);
        }

        #endregion
        #region PolyLine
        public static readonly DependencyProperty StrokeProperty =
              DependencyProperty.Register("Stroke", typeof(Brush), typeof(LineChartItem), new FrameworkPropertyMetadata(null,
                                                                                                                        FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                        FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                                                                                                                        OnPenChanged));
        public Brush Stroke
        {
            get => (Brush)this.GetValue(StrokeProperty);
            set => this.SetValue(StrokeProperty, value);
        }

        public static readonly DependencyProperty StrokeThicknessProperty =
              DependencyProperty.Register("StrokeThickness", typeof(double), typeof(LineChartItem), new FrameworkPropertyMetadata(2d, FrameworkPropertyMetadataOptions.AffectsRender, OnPenChanged));
        public double StrokeThickness
        {
            get => (double)this.GetValue(StrokeThicknessProperty);
            set => this.SetValue(StrokeThicknessProperty, value);
        }

        public static readonly DependencyProperty StrokeStartLineCapProperty =
              DependencyProperty.Register("StrokeStartLineCap", typeof(PenLineCap), typeof(LineChartItem), new FrameworkPropertyMetadata(PenLineCap.Flat,
                                                                                                                                         FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                         FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                         OnPenChanged));
        public PenLineCap StrokeStartLineCap
        {
            get => (PenLineCap)this.GetValue(StrokeStartLineCapProperty);
            set => this.SetValue(StrokeStartLineCapProperty, value);
        }

        public static readonly DependencyProperty StrokeEndLineCapProperty =
                DependencyProperty.Register("StrokeEndLineCap", typeof(PenLineCap), typeof(LineChartItem), new FrameworkPropertyMetadata(PenLineCap.Flat,
                                                                                                                                         FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                         FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                         OnPenChanged));
        public PenLineCap StrokeEndLineCap
        {
            get => (PenLineCap)this.GetValue(StrokeEndLineCapProperty);
            set => this.SetValue(StrokeEndLineCapProperty, value);
        }

        public static readonly DependencyProperty StrokeDashCapProperty =
                DependencyProperty.Register("StrokeDashCap", typeof(PenLineCap), typeof(LineChartItem), new FrameworkPropertyMetadata(PenLineCap.Flat,
                                                                                                                                      FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                      FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                      OnPenChanged));
        public PenLineCap StrokeDashCap
        {
            get => (PenLineCap)this.GetValue(StrokeDashCapProperty);
            set => this.SetValue(StrokeDashCapProperty, value);
        }

        public static readonly DependencyProperty StrokeLineJoinProperty =
                DependencyProperty.Register("StrokeLineJoin", typeof(PenLineJoin), typeof(LineChartItem), new FrameworkPropertyMetadata(PenLineJoin.Miter,
                                                                                                                                        FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                        FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                        OnPenChanged));
        public PenLineJoin StrokeLineJoin
        {
            get => (PenLineJoin)this.GetValue(StrokeLineJoinProperty);
            set => this.SetValue(StrokeLineJoinProperty, value);
        }

        public static readonly DependencyProperty StrokeMiterLimitProperty =
                DependencyProperty.Register("StrokeMiterLimit", typeof(double), typeof(LineChartItem), new FrameworkPropertyMetadata(10d,
                                                                                                                                     FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                     FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                     OnPenChanged));
        public double StrokeMiterLimit
        {
            get => (double)this.GetValue(StrokeMiterLimitProperty);
            set => this.SetValue(StrokeMiterLimitProperty, value);
        }

        public static readonly DependencyProperty StrokeDashOffsetProperty =
                DependencyProperty.Register("StrokeDashOffset", typeof(double), typeof(LineChartItem), new FrameworkPropertyMetadata(0d,
                                                                                                                                     FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                     FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                     OnPenChanged));
        public double StrokeDashOffset
        {
            get => (double)this.GetValue(StrokeDashOffsetProperty);
            set => this.SetValue(StrokeDashOffsetProperty, value);
        }

        public static readonly DependencyProperty StrokeDashArrayProperty =
                DependencyProperty.Register("StrokeDashArray", typeof(DoubleCollection), typeof(LineChartItem), new FrameworkPropertyMetadata(null,
                                                                                                                                              FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                                                                                                              FrameworkPropertyMetadataOptions.AffectsRender,
                                                                                                                                              OnPenChanged));
        public DoubleCollection StrokeDashArray
        {
            get => (DoubleCollection)this.GetValue(StrokeDashArrayProperty);
            set => this.SetValue(StrokeDashArrayProperty, value);
        }

        private static void OnPenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LineChartItem ThisLine)
                ThisLine.InvalidatePen();
        }

        #endregion
        #region Area
        public static readonly DependencyProperty FillProperty =
              DependencyProperty.Register("Fill", typeof(Brush), typeof(LineChartItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                            FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public Brush Fill
        {
            get => (Brush)this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
        }

        internal void OnAdornersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) => throw new NotImplementedException();

        public static readonly DependencyProperty AllowFillAreaProperty =
              DependencyProperty.Register("AllowFillArea", typeof(bool), typeof(LineChartItem), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender));
        public bool AllowFillArea
        {
            get => (bool)this.GetValue(AllowFillAreaProperty);
            set => this.SetValue(AllowFillAreaProperty, value);
        }

        public static readonly DependencyProperty AllowDragAreaBoundProperty =
              DependencyProperty.Register("AllowDragAreaBound", typeof(bool), typeof(LineChartItem), new PropertyMetadata(true, (d, e) =>
              {
                  if (d is LineChartItem ThisItem)
                  {
                      Brush Fill = ThisItem.Fill;
                      bool AllowDisplay = e.NewValue is true,
                           CanDisplay = Fill != null & Fill != Brushes.Transparent & ThisItem.FillGeometry != null;

                      ThisItem.UpdateDragThumbs(ThisItem.AllowFillArea & AllowDisplay & CanDisplay ? Visibility.Visible : Visibility.Collapsed);
                  }
              }));
        public bool AllowDragAreaBound
        {
            get => (bool)this.GetValue(AllowDragAreaBoundProperty);
            set => this.SetValue(AllowDragAreaBoundProperty, value);
        }

        public static readonly DependencyProperty DragThumbStyleProperty =
              DependencyProperty.Register("DragThumbStyle", typeof(Style), typeof(LineChartItem), new PropertyMetadata(null));
        public Style DragThumbStyle
        {
            get => (Style)this.GetValue(DragThumbStyleProperty);
            set => this.SetValue(DragThumbStyleProperty, value);
        }

        public static readonly DependencyProperty LeftAreaBoundProperty =
              DependencyProperty.Register("LeftAreaBound", typeof(double), typeof(LineChartItem), new FrameworkPropertyMetadata(double.NegativeInfinity, FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                                                         FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                  (d, e) =>
                  {
                      if (d is LineChartItem ThisItem &&
                          e.NewValue is double NewValue)
                      {
                          if (ThisItem.RightAreaBound < NewValue)
                              ThisItem.RightAreaBound = NewValue;
                          else
                              ThisItem.InvalidateFillArea();
                      }
                  },
                  CoerceBoundValue));
        public double LeftAreaBound
        {
            get => (double)this.GetValue(LeftAreaBoundProperty);
            set => this.SetValue(LeftAreaBoundProperty, value);
        }

        public static readonly DependencyProperty RightAreaBoundProperty =
              DependencyProperty.Register("RightAreaBound", typeof(double), typeof(LineChartItem), new FrameworkPropertyMetadata(double.PositiveInfinity, FrameworkPropertyMetadataOptions.AffectsRender |
                                                                                                                                                          FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                  (d, e) =>
                  {
                      if (d is LineChartItem ThisItem &&
                         e.NewValue is double NewValue)
                      {
                          if (NewValue < ThisItem.LeftAreaBound)
                              ThisItem.LeftAreaBound = NewValue;
                          else
                              ThisItem.InvalidateFillArea();
                      }
                  },
                  CoerceBoundValue));
        public double RightAreaBound
        {
            get => (double)this.GetValue(RightAreaBoundProperty);
            set => this.SetValue(RightAreaBoundProperty, value);
        }

        private static object CoerceBoundValue(DependencyObject sender, object value)
        {
            if (sender is LineChartItem ThisItem &&
                value is double NewValue)
            {
                double MinX = ThisItem.MinX;
                if (!double.IsNaN(MinX))
                    NewValue = Math.Max(NewValue, MinX);

                double MaxX = ThisItem.MaxX;
                if (!double.IsNaN(MaxX))
                    NewValue = Math.Min(NewValue, MaxX);

                return NewValue;
            }

            return value;
        }

        #endregion
        #region DataPoint
        public static readonly DependencyProperty ShowDataPointsProperty =
              DependencyProperty.Register("ShowDataPoints", typeof(bool), typeof(LineChartItem), new PropertyMetadata(true, (d, e) =>
              {
                  if (d is LineChartItem ThisItem)
                      ThisItem.UpdateDataPoints();
              }));
        public bool ShowDataPoints
        {
            get => (bool)this.GetValue(ShowDataPointsProperty);
            set => this.SetValue(ShowDataPointsProperty, value);
        }

        internal static readonly DependencyProperty ShowClosestDataPointProperty =
              DependencyProperty.Register("ShowClosestDataPoint", typeof(bool), typeof(LineChartItem), new PropertyMetadata(true, (d, e) =>
              {
                  if (d is LineChartItem ThisItem &&
                      ThisItem.Chart != null)
                      ThisItem.UpdateClosestDataPoint();
              }));
        internal bool ShowClosestDataPoint
        {
            get => (bool)this.GetValue(ShowClosestDataPointProperty);
            set => this.SetValue(ShowClosestDataPointProperty, value);
        }

        public static readonly DependencyProperty DataPointStyleProperty =
              DependencyProperty.Register("DataPointStyle", typeof(Style), typeof(LineChartItem), new PropertyMetadata(null));
        public Style DataPointStyle
        {
            get => (Style)this.GetValue(DataPointStyleProperty);
            set => this.SetValue(DataPointStyleProperty, value);
        }

        #endregion

        internal LineChart Chart;
        protected Thumb[] Thumbs;

        private DelayActionToken UpdateToken;
        static LineChartItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChartItem), new FrameworkPropertyMetadata(typeof(LineChartItem)));
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            // DragThumbs
            Thumbs = new Thumb[2];
            for (int i = 0; i < Thumbs.Length; i++)
            {
                Thumb Child = new Thumb { Visibility = Visibility.Collapsed };
                Child.SetBinding(StyleProperty, new Binding(nameof(this.DragThumbStyle)) { Source = this });

                if (Child.BorderBrush is null)
                    Child.SetBinding(Thumb.BorderBrushProperty, new Binding(nameof(this.Stroke)) { Source = this });

                this.AddVisualChild(Child);
                this.AddLogicalChild(Child);

                Thumbs[i] = Child;
            }

            Thumbs[0].DragDelta += (s, arg) =>
            {
                if (Chart != null)
                    this.LeftAreaBound = Chart.CalcuateDataCoordinateX(Thumbs[0].Margin.Left + arg.HorizontalChange);
            };
            Thumbs[1].DragDelta += (s, arg) =>
            {
                if (Chart != null)
                    this.RightAreaBound = Chart.CalcuateDataCoordinateX(Thumbs[1].Margin.Left + arg.HorizontalChange);
            };
        }

        protected override int VisualChildrenCount
        {
            get
            {
                int Count = 0;

                if (Thumbs != null)
                    Count += Thumbs.Length;

                Count += CurrentDataPoints.Count;

                if (ClosestDataPoint != null)
                    Count++;

                return Count;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            if (Thumbs is null)
                return CurrentDataPoints[index];

            int ThumbLength = Thumbs.Length;
            if (index < ThumbLength)
                return Thumbs[index];

            index -= ThumbLength;
            if (index < CurrentDataPoints.Count)
                return CurrentDataPoints[index];

            return ClosestDataPoint;
        }

        protected override Size MeasureOverride(Size AvailableSize)
        {
            if (Thumbs != null)
                foreach (Thumb Child in Thumbs)
                    Child.Measure(AvailableSize);

            foreach (LineChartDataPoint Child in CurrentDataPoints)
                Child.Measure(AvailableSize);

            if (ClosestDataPoint != null)
                ClosestDataPoint.Measure(AvailableSize);

            return AvailableSize;
        }
        protected override Size ArrangeOverride(Size FinalSize)
        {
            Rect FinalRect = new Rect(new Point(0d, 0d), FinalSize);

            if (Thumbs != null)
                foreach (Thumb Child in Thumbs)
                    Child.Arrange(FinalRect);

            foreach (LineChartDataPoint Child in CurrentDataPoints)
                Child.Arrange(FinalRect);

            if (ClosestDataPoint != null)
                ClosestDataPoint.Arrange(FinalRect);

            return FinalSize;
        }

        protected override void OnRender(DrawingContext DrawingContext)
        {
            if (Chart is null)
                return;

            if (!Chart.IsLoaded)
            {
                UpdateToken?.Cancel();
                UpdateToken = DispatcherHelper.DelayAction(100d, () => this.InvalidateVisual());
                return;
            }

            // Fill Area
            Brush Fill = this.Fill;
            bool CanDragThumb = false;
            if (this.AllowFillArea & Fill != null & Fill != Brushes.Transparent &&
                this.GetFillGeometry() is Geometry FillGeometry)
            {
                CanDragThumb = true;
                DrawingContext.DrawGeometry(Fill, null, FillGeometry);
            }

            // Poly Line
            DrawingContext.DrawGeometry(null, this.GetPen(), this.GetLineGeometry());

            // Drag Thumbs
            this.UpdateDragThumbs(CanDragThumb & this.AllowDragAreaBound ? Visibility.Visible : Visibility.Collapsed);

            // DataPoints
            this.UpdateDataPoints();
            this.UpdateClosestDataPoint();
        }

        private readonly Pool<LineChartDataPoint> CacheDataPointQueue = new Pool<LineChartDataPoint>();
        private readonly List<LineChartDataPoint> CurrentDataPoints = new List<LineChartDataPoint>();
        private LineChartDataPoint ClosestDataPoint;
        protected void UpdateDataPoints()
        {
            int Count = this.ShowDataPoints ? Datas.Count : 0;
            LineChartDataPoint DataPoint;
            for (int i = CurrentDataPoints.Count; i < Count; i++)
            {
                if (!CacheDataPointQueue.TryDequeue(out DataPoint))
                {
                    DataPoint = new LineChartDataPoint();
                    this.PrepareDataPoint(DataPoint);
                }

                CurrentDataPoints.Add(DataPoint);

                this.AddVisualChild(DataPoint);
                this.AddLogicalChild(DataPoint);

                DataPoint.Measure(this.RenderSize);
            }

            for (int i = CurrentDataPoints.Count - 1; i >= Count; i--)
            {
                DataPoint = CurrentDataPoints[i];

                CurrentDataPoints.RemoveAt(i);

                this.RemoveVisualChild(DataPoint);
                this.RemoveLogicalChild(DataPoint);

                CacheDataPointQueue.Enqueue(ref DataPoint);
            }

            if (Count > 0 &&
                LineGeometry is PathGeometry PathGeometry &&
                PathGeometry.Figures.FirstOrDefault() is PathFigure Path &&
                Path.Segments.FirstOrDefault() is PolyLineSegment Poly)
            {
                Point Location = Path.StartPoint;

                DataPoint = CurrentDataPoints[0];
                DataPoint.DataContext = Datas[0];

                DataPoint.Margin = new Thickness(Location.X, Location.Y, 0, 0);

                for (int i = 1; i < Count; i++)
                {
                    Location = Poly.Points[i - 1];

                    DataPoint = CurrentDataPoints[i];
                    DataPoint.DataContext = Datas[i];

                    DataPoint.Margin = new Thickness(Location.X, Location.Y, 0, 0);
                }
            }
        }
        protected internal void UpdateClosestDataPoint()
        {
            void RemoveDataPoint()
            {
                if (ClosestDataPoint is null)
                    return;

                this.RemoveVisualChild(ClosestDataPoint);
                this.RemoveLogicalChild(ClosestDataPoint);

                CacheDataPointQueue.Enqueue(ref ClosestDataPoint);
                ClosestDataPoint = null;
            }

            // Show Closest DataPoint
            int Length = Datas.Count;
            if (Length > 0 && this.ShowClosestDataPoint & !this.ShowDataPoints & Chart != null)
            {
                if (ClosestDataPoint is null)
                {
                    if (!CacheDataPointQueue.TryDequeue(out ClosestDataPoint))
                    {
                        ClosestDataPoint = new LineChartDataPoint();
                        this.PrepareDataPoint(ClosestDataPoint);
                    }

                    this.AddVisualChild(ClosestDataPoint);
                    this.AddLogicalChild(ClosestDataPoint);

                    ClosestDataPoint.Measure(this.RenderSize);
                }

                double VisualX = Mouse.GetPosition(this).X;
                if (VisualX < 0d || this.ActualWidth < VisualX)
                {
                    RemoveDataPoint();
                    return;
                }

                double DataX = Chart.CalcuateDataCoordinateX(VisualX);
                int Index = Datas.FindIndex(i => DataX < i.X);
                Point p;
                if (Index > -1)
                {
                    p = Datas[Index];
                    Index--;

                    if (Index > -1)
                    {
                        Point Temp = Datas[Index];
                        if (DataX - Temp.X < p.X - DataX)
                            p = Temp;
                    }
                }
                else
                {
                    p = Datas[Length - 1];
                }

                ClosestDataPoint.DataContext = p;
                ClosestDataPoint.Margin = new Thickness(Chart.CalcuateVisualCoordinateX(p.X), Chart.CalcuateVisualCoordinateY(p.Y), 0, 0);

                Panel.SetZIndex(ClosestDataPoint, 2 + CurrentDataPoints.Count);
            }
            else if (ClosestDataPoint != null)
            {
                RemoveDataPoint();
            }
        }
        protected virtual void PrepareDataPoint(LineChartDataPoint DataPoint)
        {
            DataPoint.SetBinding(StyleProperty, new Binding(nameof(this.DataPointStyle)) { Source = this });

            if (DataPoint.BorderBrush is null &&
                DataPoint.GetBindingExpression(LineChartDataPoint.BorderBrushProperty) is null)
                DataPoint.SetBinding(LineChartDataPoint.BorderBrushProperty, new Binding(nameof(this.Stroke)) { Source = this });

            if (DataPoint.Background is null &&
                DataPoint.GetBindingExpression(LineChartDataPoint.BackgroundProperty) is null)
                DataPoint.SetBinding(LineChartDataPoint.BackgroundProperty, new Binding(nameof(this.Stroke)) { Source = this });

            DataPoint.VerticalAlignment = VerticalAlignment.Top;
            DataPoint.HorizontalAlignment = HorizontalAlignment.Left;

            DataPoint.Click += this.OnDataPointClick;
        }
        protected virtual void OnDataPointClick(object sender, EventArgs e)
            => this.DataPointClick?.Invoke(sender, e);

        protected Pen Pen;
        protected virtual Pen GetPen()
        {
            double Thickness = this.StrokeThickness;
            if (double.IsInfinity(Thickness) || Thickness is 0d || double.IsNaN(Thickness))
                return null;

            Brush Brush = this.Stroke;
            if (Brush is null || Brush.Equals(Brushes.Transparent))
                return null;

            if (Pen == null)
            {
                // This pen is internal to the system and
                // must not participate in freezable treeness
                Pen = new Pen
                {
                    Thickness = Math.Abs(Thickness),
                    Brush = Brush,
                    StartLineCap = StrokeStartLineCap,
                    EndLineCap = StrokeEndLineCap,
                    DashCap = StrokeDashCap,
                    LineJoin = StrokeLineJoin,
                    MiterLimit = Math.Max(Math.Abs(this.StrokeMiterLimit), 1d),
                };

                if (this.StrokeDashArray is DoubleCollection DashData && DashData.Count > 0)
                    Pen.DashStyle = new DashStyle(DashData, this.StrokeDashOffset);
            }

            return Pen;
        }
        public void InvalidatePen()
            => Pen = null;

        protected Geometry LineGeometry;
        protected virtual Geometry GetLineGeometry()
        {
            if (LineGeometry != null)
                return LineGeometry;

            if (Datas.Count < 2)
            {
                LineGeometry = Geometry.Empty;
                return LineGeometry;
            }

            PathFigure Path = new PathFigure { StartPoint = Chart.CalcuateVisualCoordinate(Datas[0]) };
            Path.Segments.Add(new PolyLineSegment(Datas.Skip(1).Select(i => Chart.CalcuateVisualCoordinate(i)), true));

            PathGeometry TempGeometry = new PathGeometry();
            TempGeometry.Figures.Add(Path);

            LineGeometry = TempGeometry.Bounds.IsEmpty ? Geometry.Empty : TempGeometry;

            return LineGeometry;
        }
        public virtual void InvalidatePolyLine()
            => LineGeometry = null;

        protected internal double[] LastFillAreaX = { double.NegativeInfinity, double.PositiveInfinity };
        protected Geometry FillGeometry;
        protected virtual Geometry GetFillGeometry()
        {
            if (FillGeometry != null)
                return FillGeometry;

            int DatasCount = Datas.Count;
            if (DatasCount < 2)
                return null;

            double Lx = this.LeftAreaBound,
                   Rx = this.RightAreaBound;

            IEnumerable<Point> GetAreaPoints()
            {
                int i = 0;
                double X1 = 0d;
                Point LastPoint = default,
                      CurrPoint = default;
                for (; i < DatasCount; i++)
                {
                    LastPoint = CurrPoint;
                    CurrPoint = Datas[i];

                    X1 = CurrPoint.X;
                    if (X1 >= Lx)
                        break;
                }

                double X0 = LastPoint.X,
                       Y0 = LastPoint.Y,
                       Y1 = CurrPoint.Y,
                       Dx = X1 - X0,
                       Dy = Y1 - Y0;

                yield return X1 == Lx ? CurrPoint : new Point(Lx, Dx == 0d ? Y0 : (Y0 + (Lx - X0) * Dy / Dx));

                if (Rx < X1)
                {
                    CurrPoint = new Point(Rx, Dx == 0d ? Y0 : (Y0 + (Rx - X0) * Dy / Dx));
                    yield return CurrPoint;
                }
                else
                {
                    for (; i < DatasCount; i++)
                    {
                        CurrPoint = Datas[i];

                        X1 = CurrPoint.X;
                        if (Rx < X1)
                        {
                            X0 = LastPoint.X;
                            Y0 = LastPoint.Y;
                            Y1 = CurrPoint.Y;
                            Dx = X1 - X0;
                            Dy = Y1 - Y0;

                            CurrPoint = new Point(Rx, Dx == 0d ? Y0 : (Y0 + (Rx - X0) * Dy / Dx));
                            yield return CurrPoint;
                            break;
                        }

                        yield return CurrPoint;
                        LastPoint = CurrPoint;
                    }
                }

                yield return new Point(CurrPoint.X, 0d);
            }

            Point[] AreaDataPoints = GetAreaPoints().ToArray();

            LastFillAreaX[0] = Chart.CalcuateVisualCoordinateX(!double.IsNaN(MinX) && double.IsInfinity(Lx) ? MinX : Lx);
            LastFillAreaX[1] = Chart.CalcuateVisualCoordinateX(!double.IsNaN(MaxX) && double.IsInfinity(Rx) ? MaxX : Rx);

            if (AreaDataPoints.Length < 3)
                return Geometry.Empty;

            PathFigure Path = new PathFigure
            {
                StartPoint = Chart.CalcuateVisualCoordinate(AreaDataPoints[0].X, 0d),
                IsClosed = true,
                IsFilled = true
            };

            Path.Segments.Add(new PolyLineSegment(AreaDataPoints.Select(i => Chart.CalcuateVisualCoordinate(i)), false));

            PathGeometry TempGeometry = new PathGeometry();
            TempGeometry.Figures.Add(Path);

            FillGeometry = TempGeometry.Bounds.IsEmpty ? Geometry.Empty : TempGeometry;

            return FillGeometry;
        }
        public virtual void InvalidateFillArea()
            => FillGeometry = null;

        private void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.InvalidatePolyLine();
            this.InvalidateFillArea();

            if (DataExtractor is null)
            {
                this.UpdateDatas();
            }
            else
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (Point NewPoint in e.NewItems.Select(i => DataExtractor(i)))
                            {
                                int Index = Datas.FindIndex(i => NewPoint.X < i.X);
                                if (Index < 0)
                                {
                                    MaxX = NewPoint.X;
                                    Datas.Add(NewPoint);
                                }
                                else if (Index == 0)
                                {
                                    MinX = NewPoint.X;
                                    Datas.Insert(0, NewPoint);
                                }
                                else
                                {
                                    Datas.Insert(Index, NewPoint);
                                }

                                if (double.IsNaN(MinY) && double.IsNaN(MaxY))
                                {
                                    MinY = NewPoint.Y;
                                    MaxY = MinY;
                                    continue;
                                }

                                MathHelper.MinAndMax(out MinY, out MaxY, MinY, MaxY, NewPoint.Y);
                            }

                            this.OnDatasUpdate();
                            break;
                        }
                    case NotifyCollectionChangedAction.Remove:
                        {
                            foreach (Point OldPoint in e.NewItems.Select(i => DataExtractor(i)))
                            {
                                int Index = Datas.FindIndex(i => i.Equals(OldPoint));
                                if (Index < 0)
                                    continue;

                                Datas.RemoveAt(Index);

                                int Length = Datas.Count;
                                if (Length > 0)
                                {
                                    if (Index == 0)
                                        MinX = Datas[0].X;
                                    else if (Index == Length)
                                        MaxX = Datas[Length - 1].X;

                                    MathHelper.MinAndMax(Datas.Select(i => i.Y), out MinY, out MaxY);
                                }
                                else
                                {
                                    this.ResetMaxAndMin();
                                }
                            }

                            this.OnDatasUpdate();
                            break;
                        }
                    case NotifyCollectionChangedAction.Replace:
                        {
                            foreach (Point OldPoint in e.NewItems.Select(i => DataExtractor(i)))
                                Datas.Remove(OldPoint);

                            foreach (Point NewPoint in e.NewItems.Select(i => DataExtractor(i)))
                                Datas.Insert(Math.Max(Datas.FindIndex(i => NewPoint.X < i.X), 0), NewPoint);

                            MinX = Datas[0].X;
                            MaxX = Datas[Datas.Count - 1].X;
                            MathHelper.MinAndMax(Datas.Select(i => i.Y), out MinY, out MaxY);

                            this.OnDatasUpdate();
                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            Datas.Clear();
                            this.ResetMaxAndMin();
                            this.OnDatasUpdate();
                            break;
                        }
                    case NotifyCollectionChangedAction.Move:
                    default:
                        this.UpdateDatas();
                        break;
                }
            }

            UpdateToken?.Cancel();
            UpdateToken = DispatcherHelper.DelayAction(100d, () => this.InvalidateVisual());
        }

        protected virtual void UpdateDragThumbs(Visibility State)
        {
            if (Thumbs is null)
                return;

            for (int i = 0; i < Thumbs.Length; i++)
            {
                double Left = LastFillAreaX[i];
                if (double.IsInfinity(Left))
                {
                    Thumbs[i].Visibility = Visibility.Collapsed;
                    continue;
                }

                Thumbs[i].Visibility = State;
                Thumbs[i].Margin = new Thickness(Left, 0, 0, 0);
            }
        }

        protected Func<object, Point> DataExtractor;
        protected internal readonly List<Point> Datas = new List<Point>();
        protected internal double MaxX = double.NaN,
                                  MinX = double.NaN,
                                  MaxY = double.NaN,
                                  MinY = double.NaN;
        private void UpdateDatas()
        {
            Datas.Clear();

            IEnumerable Source = this.ItemsSource?.Where(i => i != null);
            if (Source is null)
            {
                this.ResetMaxAndMin();
            }
            else
            {
                object CurrentData = Source.FirstOrNull();

                if (CurrentData != null)
                {
                    if (DataExtractor is null)
                        DataExtractor = this.ParseDataExtractor(CurrentData.GetType());

                    Datas.AddRange(Source.Select(i => DataExtractor(i)).OrderBy(i => i.X));

                    MinX = Datas[0].X;
                    MaxX = Datas[Datas.Count - 1].X;
                    MathHelper.MinAndMax(Datas.Select(i => i.Y), out MinY, out MaxY);
                }
                else
                {
                    this.ResetMaxAndMin();
                }
            }

            this.OnDatasUpdate();
        }
        protected void ResetMaxAndMin()
        {
            MaxX = double.NaN;
            MinX = double.NaN;
            MaxY = double.NaN;
            MinY = double.NaN;
        }
        protected Func<object, Point> ParseDataExtractor(Type DataType)
        {
            if (typeof(Point).Equals(DataType))
                return i => (Point)i;

            string XPath = this.XValuePath,
                   YPath = this.YValuePath;

            if (string.IsNullOrEmpty(XPath) && DataType.Name.StartsWith("KeyValuePair"))
                XPath = "Key";

            if (string.IsNullOrEmpty(YPath) && DataType.Name.StartsWith("KeyValuePair"))
                YPath = "Value";

            Func<object, double> XExtractor = ExpressionHelper.CreateExtractor<double>(DataType, XPath),
                                 YExtractor = ExpressionHelper.CreateExtractor<double>(DataType, YPath);

            return i => new Point(XExtractor(i), YExtractor(i));
        }

        protected virtual void OnDatasUpdate()
            => DatasUpdated?.Invoke(this, EventArgs.Empty);

    }
}
