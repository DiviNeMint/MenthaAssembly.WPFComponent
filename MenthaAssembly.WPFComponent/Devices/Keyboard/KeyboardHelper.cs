namespace System.Windows.Input
{
    public static class KeyboardHelper
    {
        public static bool IsLetter(this Key key)
            => key is >= Key.A and <= Key.Z;

        public static bool IsNumber(this Key key)
            => key is >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9;

        public static bool IsNumberOrDot(this Key key)
            => IsNumber(key) || key == Key.Decimal;

        public static bool IsLetterOrNumber(this Key key)
            => IsLetter(key) || IsNumber(key);

        public static bool IsOperator(this Key key)
            => key is Key.Divide or Key.Multiply or Key.Subtract or Key.Add;

    }
}