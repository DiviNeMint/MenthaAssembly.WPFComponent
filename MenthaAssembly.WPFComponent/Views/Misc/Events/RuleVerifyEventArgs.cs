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

    }
}