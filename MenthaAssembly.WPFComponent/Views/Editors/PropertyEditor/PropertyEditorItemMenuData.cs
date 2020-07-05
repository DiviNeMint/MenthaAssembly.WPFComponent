using System;
using System.Windows.Input;
using System.Windows.Media;

namespace MenthaAssembly.Views
{
    public class PropertyEditorItemMenuData : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public ImageSource Icon { set; get; }

        public string Header { set; get; }

        private bool _IsEnabled = true;
        public bool IsEnabled
        {
            get => _IsEnabled;
            set
            {
                _IsEnabled = value;
                this.OnCanExecuteChanged();
            }
        }

        public Action<object> Handler { set; get; }

        public bool CanExecute(object parameter)
            => IsEnabled;

        public void Execute(object parameter)
            => Handler(parameter);

        private void OnCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);

    }
}
