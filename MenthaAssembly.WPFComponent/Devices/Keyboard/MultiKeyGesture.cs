using System.Globalization;
using System.Linq;

namespace System.Windows.Input
{
    public class MultiKeyGesture : KeyGesture
    {
        private readonly KeyGesture[] KeyGestures;
        public MultiKeyGesture(ModifierKeys Modifiers, Key First, Key Second) : this(Modifiers, First, Second, string.Empty)
        {
        }
        public MultiKeyGesture(ModifierKeys Modifiers, Key First, Key Second, string DisplayString) : base(Key.None, Modifiers, DisplayString)
        {
            KeyGestures = [new KeyGesture(First, Modifiers), new KeyGesture(Second, Modifiers)];
        }

        private int CurrentGestureIndex = 0;
        public override bool Matches(object TargetElement, InputEventArgs InputEventArgs)
        {
            if (InputEventArgs is not KeyEventArgs keyArgs || keyArgs.IsRepeat)
                return false;

            int Count = KeyGestures.Length;
            if (Count <= CurrentGestureIndex)
            {
                CurrentGestureIndex = 0;
                return false;
            }

            KeyGesture Gesture = KeyGestures[CurrentGestureIndex];
            if (Gesture.Matches(TargetElement, InputEventArgs))
            {
                InputEventArgs.Handled = true;
                return ++CurrentGestureIndex == Count;
            }
            else
            {
                CurrentGestureIndex = 0;
            }

            return false;
        }

        public new string GetDisplayStringForCulture(CultureInfo Culture)
            => string.IsNullOrEmpty(DisplayString) ? string.Join(",", KeyGestures.Select(i => i.GetDisplayStringForCulture(Culture))) :
                                                     DisplayString;

    }
}