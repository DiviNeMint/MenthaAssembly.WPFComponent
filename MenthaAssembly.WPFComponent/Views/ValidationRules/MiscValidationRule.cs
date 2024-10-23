using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.MarkupExtensions
{
    public sealed class MiscValidationRule : ValidationRule
    {
        public delegate bool Predicate<TSource, TValue>(TSource Source, TValue Value);

        public bool AllowEmpty { get; set; }

        public string EmptyMessage { get; set; } = "NoEmptyName";

        public bool AllowDuplicates { get; set; }

        public string DuplicatesMessage { get; set; } = "AlreadyExistsName";

        public Predicate<object, string> DuplicatePredicate { get; set; }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
        {
            string Value = value?.ToString();
            if (!AllowEmpty &&
                string.IsNullOrEmpty(Value))
                return new ValidationResult(false, EmptyMessage);

            if (DuplicatePredicate != null &&
                !AllowDuplicates &&
                DuplicatePredicate.Invoke(GetSource(owner), Value))
                return new ValidationResult(false, DuplicatesMessage);

            return ValidationResult.ValidResult;
        }

        private static object GetSource(BindingExpressionBase BindingBase)
        {
            if (BindingBase is BindingExpression Binding)
                return Binding.ResolvedSource;

            return null;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => throw new NotImplementedException();

    }
}