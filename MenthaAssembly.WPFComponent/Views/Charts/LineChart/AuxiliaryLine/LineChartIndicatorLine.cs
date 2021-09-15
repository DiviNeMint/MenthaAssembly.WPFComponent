using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class LineChartIndicatorLine : LineChartAuxiliaryLine
    {
        public static readonly DependencyProperty ValueProperty =
              DependencyProperty.Register("Value", typeof(double), typeof(LineChartIndicatorLine), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Value
        {
            get => (double)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public override void Update(LineChartItem Item)
            => this.InvalidateVisual();

        protected override void OnRender(DrawingContext DrawingContext)
        {
            double v = this.Value;
            if (this.Chart is null | double.IsNaN(v))
                return;

            switch (this.Axis)
            {
                case Axises.X:
                    {
                        v = this.Chart.CalcuateVisualCoordinateX(v);
                        DrawingContext.DrawLine(this.GetPen(), new Point(v, 0d), new Point(v, this.ActualHeight));
                        break;
                    }
                case Axises.Y:
                    {
                        v = this.Chart.CalcuateVisualCoordinateY(v);
                        DrawingContext.DrawLine(this.GetPen(), new Point(0d, v), new Point(this.ActualWidth, v));
                        break;
                    }
            }
        }

    }
}
