using System.Windows;
using System.Windows.Controls;

namespace MenthaAssembly.Views
{
    public class LineChartDataPoint : Button
    {
        static LineChartDataPoint()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LineChartDataPoint), new FrameworkPropertyMetadata(typeof(LineChartDataPoint)));
        }
    }
}
