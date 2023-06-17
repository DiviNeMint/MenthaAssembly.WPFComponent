using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MenthaAssembly.Views
{
    public class ColorAnimation : ColorAnimationBase
    {
        public static readonly DependencyProperty FromProperty =
              DependencyProperty.Register("From", typeof(Color?), typeof(ColorAnimation), new PropertyMetadata(null));
        public Color? From
        {
            get => (Color?)GetValue(FromProperty);
            set => SetValue(FromProperty, value);
        }

        public static readonly DependencyProperty ToProperty =
              DependencyProperty.Register("To", typeof(Color?), typeof(ColorAnimation), new PropertyMetadata(null));
        public Color? To
        {
            get => (Color?)GetValue(ToProperty);
            set => SetValue(ToProperty, value);
        }

        public static readonly DependencyProperty EasingFunctionProperty =
              DependencyProperty.Register("EasingFunction", typeof(IEasingFunction), typeof(ColorAnimation), new PropertyMetadata(null));
        public IEasingFunction EasingFunction
        {
            get => (IEasingFunction)GetValue(EasingFunctionProperty);
            set => SetValue(EasingFunctionProperty, value);
        }

        protected override Color GetCurrentValueCore(Color defaultOriginValue, Color defaultDestinationValue, AnimationClock animationClock)
        {
            Color? To = this.To;
            if (!To.HasValue)
                return defaultOriginValue;

            double num = animationClock.CurrentProgress.Value;
            if (EasingFunction != null)
                num = EasingFunction.Ease(num);

            Color From = this.From ?? defaultOriginValue,
                  NTo = To.Value;

            return Color.FromArgb(GetColorValue(num, From.A, NTo.A),
                                  GetColorValue(num, From.R, NTo.R),
                                  GetColorValue(num, From.G, NTo.G),
                                  GetColorValue(num, From.B, NTo.B));
        }
        private byte GetColorValue(double Timeline, byte From, byte To)
            => (byte)(From + (To - From) * Timeline);

        protected override bool FreezeCore(bool isChecking)
            => true;

        public new ColorAnimation Clone()
            => (ColorAnimation)base.Clone();

        protected override Freezable CreateInstanceCore()
            => new ColorAnimation();

    }
}