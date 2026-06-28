using System;
using System.Windows;

namespace MenthaAssembly.Views
{
    public class RuleVerifyEventArgs(object Source, string PropertyName, object Value, DependencyObject Target, DependencyProperty TargetProperty) : EventArgs
    {
        public object Source { get; } = Source;

        public string PropertyName { get; } = PropertyName;

        public object Value { get; } = Value;

        public DependencyObject Target { get; } = Target;

        public DependencyProperty TargetProperty { get; } = TargetProperty;

        public bool IsValid { get; set; }

        public string Message { get; set; }

        public bool VerifyEmpty()
            => VerifyEmpty("Cannot be empty.");
        public bool VerifyEmpty(string EmptyMessage)
        {
            if (string.IsNullOrEmpty(Value?.ToString()))
            {
                Message = EmptyMessage;
                IsValid = false;
                return false;
            }

            return true;
        }

        public bool VerifyDuplicates(Predicate<RuleVerifyEventArgs> Predicate)
            => VerifyDuplicates(Predicate, "Already exists.");
        public bool VerifyDuplicates(Predicate<RuleVerifyEventArgs> Predicate, string DuplicateMessage)
        {
            if (Predicate.Invoke(this))
            {
                Message = DuplicateMessage;
                IsValid = false;
                return false;
            }

            return true;
        }

    }
}