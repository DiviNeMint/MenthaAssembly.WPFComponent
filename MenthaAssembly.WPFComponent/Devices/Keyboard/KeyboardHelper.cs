namespace System.Windows.Input
{
    /// <summary>
    /// Helper class for keyboard-related operations.
    /// </summary>
    public static class KeyboardHelper
    {
        /// <summary>
        /// Determines if the specified key is a letter.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a letter; otherwise, false.</returns>
        public static bool IsLetter(this Key key)
            => key is >= Key.A and <= Key.Z;

        /// <summary>
        /// Determines if the specified key is a number.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a number; otherwise, false.</returns>
        public static bool IsNumber(this Key key)
            => key is >= Key.D0 and <= Key.D9 or >= Key.NumPad0 and <= Key.NumPad9;

        /// <summary>
        /// Determines if the specified key is a number or a dot.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a number or a dot; otherwise, false.</returns>
        public static bool IsNumberOrDot(this Key key)
            => IsNumber(key) || IsDot(key);

        /// <summary>
        /// Determines if the specified key is a hexadecimal digit.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a hexadecimal digit; otherwise, false.</returns>
        public static bool IsHex(this Key key)
            => (key is >= Key.A and <= Key.F) || IsNumber(key);

        /// <summary>
        /// Determines if the specified key is a letter or a number.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a letter or a number; otherwise, false.</returns>
        public static bool IsLetterOrNumber(this Key key)
            => IsLetter(key) || IsNumber(key);

        /// <summary>
        /// Determines if the specified key is a dot.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a dot; otherwise, false.</returns>
        public static bool IsDot(this Key key)
            => key is Key.Decimal or Key.OemPeriod;

        /// <summary>
        /// Determines if the specified key is an operator.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is an operator; otherwise, false.</returns>
        public static bool IsOperator(this Key key)
            => IsAddOperator(key) || IsMinusOperator(key) || IsMultiplyOperator(key) || IsDivideOperator(key);

        /// <summary>
        /// Determines if the specified key is an addition operator.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is an addition operator; otherwise, false.</returns>
        public static bool IsAddOperator(this Key key)
            => key is Key.Add or Key.OemPlus;

        /// <summary>
        /// Determines if the specified key is a subtraction operator.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a subtraction operator; otherwise, false.</returns>
        public static bool IsMinusOperator(this Key key)
            => key is Key.Subtract or Key.OemMinus;

        /// <summary>
        /// Determines if the specified key is a multiplication operator.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a multiplication operator; otherwise, false.</returns>
        public static bool IsMultiplyOperator(this Key key)
            => key is Key.Multiply;

        /// <summary>
        /// Determines if the specified key is a division operator.
        /// </summary>
        /// <param name="key">The key to check.</param>
        /// <returns>True if the key is a division operator; otherwise, false.</returns>
        public static bool IsDivideOperator(this Key key)
            => key is Key.Divide or Key.Oem2;

    }
}