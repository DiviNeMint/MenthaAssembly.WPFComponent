using MenthaAssembly.Views;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MenthaAssembly.MarkupExtensions
{
    public sealed class MiscValidationRule : ValidationRule
    {
        public event EventHandler<RuleVerifyEventArgs> Verify;

        public override ValidationResult Validate(object Value, CultureInfo CultureInfo, BindingExpressionBase Owner)
        {
            if (Verify != null &&
                TryGetBindingDescription(Owner, out object Source, out string PropertyName, out DependencyObject Target, out DependencyProperty TargetProperty))
            {
                RuleVerifyEventArgs e = new(Source, PropertyName, Value, Target, TargetProperty);
                Verify.Invoke(this, e);

                if (!e.IsValid)
                    return new ValidationResult(false, e.Message);
            }

            return ValidationResult.ValidResult;
        }

        private static bool TryGetBindingDescription(BindingExpressionBase BindingBase, out object Source, out string PropertyName, out DependencyObject Target, out DependencyProperty TargetProperty)
        {
            if (BindingBase is not BindingExpression Binding)
            {
                Source = null;
                PropertyName = null;
                Target = null;
                TargetProperty = null;
                return false;
            }

            Source = Binding.ResolvedSource;
            PropertyName = Binding.ResolvedSourcePropertyName;
            Target = Binding.Target;
            TargetProperty = Binding.TargetProperty;
            return true;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
            => throw new NotImplementedException();

    }
}