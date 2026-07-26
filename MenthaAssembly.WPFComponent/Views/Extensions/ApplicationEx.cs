using System.ComponentModel;
using System.Security.Principal;
using System.Windows;

namespace MenthaAssembly.MarkupExtensions
{
    public static class ApplicationEx
    {
        public static bool IsDesignMode
            => (bool)DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue;

        public static bool IsRunningAsAdministrator
        {
            get
            {
                using WindowsIdentity Identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal Principal = new(Identity);
                return Principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

    }
}
