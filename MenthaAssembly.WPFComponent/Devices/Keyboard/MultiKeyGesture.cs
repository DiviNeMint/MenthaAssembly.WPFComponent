using MenthaAssembly;
using System.Globalization;
using System.Linq;

namespace System.Windows.Input
{
    public class MultiKeyGesture : KeyGesture
    {
        private readonly KeyGesture[] KeyGestures;
        private DelayActionToken TimeoutToken;
        private int CurrentGestureIndex = 0;
        public double Timeout { get; set; } = 1500d;

        public MultiKeyGesture(ModifierKeys Modifiers, Key First, Key Second) : this(Modifiers, First, Second, string.Empty)
        {
        }

        public MultiKeyGesture(ModifierKeys Modifiers, Key First, Key Second, string DisplayString) : base(Key.None, Modifiers, string.IsNullOrEmpty(DisplayString) ? BuildDisplayString(Modifiers, First, Second) : DisplayString)
        {
            KeyGestures = [new KeyGesture(First, Modifiers), new KeyGesture(Second, Modifiers)];
        }

        public override bool Matches(object TargetElement, InputEventArgs InputEventArgs)
        {
            if (InputEventArgs is not KeyEventArgs keyArgs || keyArgs.IsRepeat)
            {
                Reset();
                return false;
            }

            int Count = KeyGestures.Length;
            if (Count <= CurrentGestureIndex)
            {
                Reset();
                return false;
            }

            KeyGesture Gesture = KeyGestures[CurrentGestureIndex];
            if (Gesture.Matches(TargetElement, InputEventArgs))
            {
                InputEventArgs.Handled = true;
                CurrentGestureIndex++;

                if (CurrentGestureIndex == Count)
                {
                    Reset();
                    return true;
                }

                RestartTimeout();
                return false;
            }

            Reset();
            return false;
        }

        private void RestartTimeout()
        {
            TimeoutToken?.Cancel();
            TimeoutToken = DispatcherHelper.DelayAction(Timeout, Reset);
        }

        private void Reset()
        {
            TimeoutToken?.Cancel();
            TimeoutToken = null;
            CurrentGestureIndex = 0;
        }

        public new string GetDisplayStringForCulture(CultureInfo Culture)
            => string.IsNullOrEmpty(DisplayString) ? string.Join(", ", KeyGestures.Select(i => i.GetDisplayStringForCulture(Culture))) :
                                                     DisplayString;

        private static string BuildDisplayString(ModifierKeys Modifiers, Key First, Key Second)
            => string.Join(", ", new KeyGesture(First, Modifiers).GetDisplayStringForCulture(CultureInfo.CurrentCulture),
                            new KeyGesture(Second, Modifiers).GetDisplayStringForCulture(CultureInfo.CurrentCulture));

    }
}
