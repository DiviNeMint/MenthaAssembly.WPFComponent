using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace MenthaAssembly
{
    public class LinearDoubleKeyFrameEx : LinearDoubleKeyFrame
    {
        public static new readonly DependencyProperty ValueProperty =
              DependencyProperty.Register("Value", typeof(double), typeof(LinearDoubleKeyFrameEx), new PropertyMetadata(default,
                  (d, e) =>
                  {
                      if (d is LinearDoubleKeyFrameEx This &&
                          e.NewValue is double NewValue)
                          This.UpdateValue(NewValue, This.Factor);
                  }));

        public new double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty FactorProperty =
              DependencyProperty.Register("Factor", typeof(double?), typeof(LinearDoubleKeyFrameEx), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is LinearDoubleKeyFrameEx This)
                          This.UpdateValue(This.Value, e.NewValue as double?);
                  }));
        [Category("Common")]
        public double? Factor
        {
            get => (double?)GetValue(FactorProperty);
            set => SetValue(FactorProperty, value);
        }

        private void UpdateValue(double Value, double? Factor)
            => base.Value = Value * (Factor ?? 1d);

    }
}
