using System.Windows;

namespace MenthaAssembly.Views
{
    public class DataGridRow : System.Windows.Controls.DataGridRow
    {
        internal static readonly DependencyPropertyKey IsNewItemPlaceholderPropertyKey =
              DependencyProperty.RegisterReadOnly(nameof(IsNewItemPlaceholder), typeof(bool), typeof(DataGridRow), new PropertyMetadata(false));
        public static readonly DependencyProperty IsNewItemPlaceholderProperty = IsNewItemPlaceholderPropertyKey.DependencyProperty;
        public bool IsNewItemPlaceholder
        {
            get => (bool)GetValue(IsNewItemPlaceholderProperty);
            private set => SetValue(IsNewItemPlaceholderPropertyKey, value);
        }

        static DataGridRow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataGridRow), new FrameworkPropertyMetadata(typeof(DataGridRow)));
        }

        public DataGridRow() : base()
        {
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
            => IsNewItemPlaceholder = DataGrid.IsNewItemPlaceholder(e.NewValue);

    }
}