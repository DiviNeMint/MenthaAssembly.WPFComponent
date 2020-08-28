using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;

namespace MenthaAssembly
{
    public class DoubleAnimationEx : DoubleAnimation
    {
        public static readonly new DependencyProperty FromProperty =
              DependencyProperty.Register("From", typeof(double?), typeof(DoubleAnimationEx), new PropertyMetadata(
                  (d, e) =>
                  {
                      if (d is DoubleAnimationEx This)
                          This.UpdateFrom(e.NewValue as double?, This.FromFactor, This.FromPadding);
                  }));
        public new double? From
        {
            get => (double?)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty FromFactorProperty =
              DependencyProperty.Register("FromFactor", typeof(double?), typeof(DoubleAnimationEx), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is DoubleAnimationEx This)
                          This.UpdateFrom(This.From, e.NewValue as double?, This.FromPadding);
                  }));
        [Category("Common")]
        public double? FromFactor
        {
            get => (double?)GetValue(FromFactorProperty);
            set => SetValue(FromFactorProperty, value);
        }

        public static readonly DependencyProperty FromPaddingProperty =
              DependencyProperty.Register("FromPadding", typeof(double?), typeof(DoubleAnimationEx), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is DoubleAnimationEx This)
                          This.UpdateFrom(This.From, This.FromFactor, e.NewValue as double?);
                  }));
        [Category("Common")]
        public double? FromPadding
        {
            get => (double?)GetValue(FromPaddingProperty);
            set => SetValue(FromPaddingProperty, value);
        }

        public static readonly new DependencyProperty ToProperty =
              DependencyProperty.Register("To", typeof(double?), typeof(DoubleAnimationEx), new PropertyMetadata(
              (d, e) =>
              {
                  if (d is DoubleAnimationEx This)
                      This.UpdateTo(e.NewValue as double?, This.ToFactor, This.ToPadding);
              }));
        public new double? To
        {
            get => (double?)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty ToFactorProperty =
              DependencyProperty.Register("ToFactor", typeof(double?), typeof(DoubleAnimationEx), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is DoubleAnimationEx This)
                          This.UpdateTo(This.To, e.NewValue as double?, This.ToPadding);
                  }));
        [Category("Common")]
        public double? ToFactor
        {
            get => (double?)GetValue(ToFactorProperty);
            set => SetValue(ToFactorProperty, value);
        }

        public static readonly DependencyProperty ToPaddingProperty =
              DependencyProperty.Register("ToPadding", typeof(double?), typeof(DoubleAnimationEx), new PropertyMetadata(null,
                  (d, e) =>
                  {
                      if (d is DoubleAnimationEx This)
                          This.UpdateTo(This.To, This.ToFactor, e.NewValue as double?);
                  }));
        [Category("Common")]
        public double? ToPadding
        {
            get => (double?)GetValue(ToPaddingProperty);
            set => SetValue(ToPaddingProperty, value);
        }

        private void UpdateFrom(double? From, double? FromFactor, double? FromPadding)
            => base.From = (From * (FromFactor ?? 1d) + (FromPadding ?? 0d)) ?? null;

        private void UpdateTo(double? To, double? ToFactor, double? ToPadding)
            => base.To = (To * (ToFactor ?? 1d) + (ToPadding ?? 0d)) ?? null;

    }
}
