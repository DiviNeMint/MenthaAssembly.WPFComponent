using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MenthaAssembly.MarkupExtensions
{
    public static class ComboBoxEx
    {
        public static readonly DependencyProperty AllowDirectlyWheelProperty =
                DependencyProperty.RegisterAttached("AllowDirectlyWheel", typeof(bool), typeof(ComboBoxEx), new PropertyMetadata(false,
                    (d, e) =>
                    {
                        if (d is ComboBox ThisBox)
                        {
                            if (e.NewValue is true)
                                ThisBox.PreviewMouseWheel += OnComboBoxPreviewMouseWheel;
                            else
                                ThisBox.PreviewMouseWheel -= OnComboBoxPreviewMouseWheel;
                        }
                    }));
        public static bool GetAllowDirectlyWheel(ComboBox obj)
            => (bool)obj.GetValue(AllowDirectlyWheelProperty);
        public static void SetAllowDirectlyWheel(ComboBox obj, bool value)
            => obj.SetValue(AllowDirectlyWheelProperty, value);

        private static void OnComboBoxPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ComboBox ThisBox)
                ThisBox.Focus();
        }



    }
}
