using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class LineChartAverageLine : LineChartAuxiliaryLine
    {
        public static readonly DependencyProperty AverageProperty =
              DependencyProperty.Register("Average", typeof(double), typeof(LineChartAverageLine), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsRender));
        public double Average
        {
            get => (double)this.GetValue(AverageProperty);
            internal set => this.SetValue(AverageProperty, value);
        }

        public override void Update(LineChartItem Item)
        {
            if (Item is null || Item.Datas.Count == 0)
            {
                this.Average = double.NaN;
                return;
            }

            double Avg = this.Axis == Axises.X ? Item.Datas.Average(i => i.X) :
                                                 Item.Datas.Average(i => i.Y);

            if (this.Average.Equals(Avg))
            {
                this.InvalidateVisual();
                return;
            }

            this.Average = Avg;
        }

        protected override void OnRender(DrawingContext DrawingContext)
        {
            double Avg = this.Average;
            if (this.Chart is null | double.IsNaN(Avg))
                return;

            switch (this.Axis)
            {
                case Axises.X:
                    {
                        Avg = this.Chart.CalcuateVisualCoordinateX(Avg);
                        DrawingContext.DrawLine(this.GetPen(), new Point(Avg, 0d), new Point(Avg, this.ActualHeight));
                        break;
                    }
                case Axises.Y:
                    {
                        Avg = this.Chart.CalcuateVisualCoordinateY(Avg);
                        DrawingContext.DrawLine(this.GetPen(), new Point(0d, Avg), new Point(this.ActualWidth, Avg));
                        break;
                    }
            }
        }

    }
}
