using Microsoft.Win32;
using System.Collections.Generic;

namespace MenthaAssembly.Win32
{
    public static class Startup
    {
        public static IEnumerable<StartupAppInfo> GetStartupAppInfos()
        {
            using (RegistryKey Key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                foreach (string Name in Key.GetValueNames())
                    yield return new StartupAppInfo(Name, Key.GetValue(Name).ToString(), true);
            }

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                foreach (string Name in Key.GetValueNames())
                    yield return new StartupAppInfo(Name, Key.GetValue(Name).ToString(), false);
            }
        }

        public static bool TryAddStartup(string AppName, string AppPath, bool AdministratorPermission)
        {
            using (RegistryKey Key = (AdministratorPermission ? Registry.LocalMachine : Registry.CurrentUser)
                                                                .OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                try
                {
                    Key.SetValue(AppName, $@"""{AppPath}""", RegistryValueKind.String);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        public static bool TryRemoveStartup(string AppName)
        {
            using (RegistryKey Key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                try
                {
                    Key.DeleteValue(AppName, false);
                }
                catch
                {
                    return false;
                }
            }

            using (RegistryKey Key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
            {
                try
                {
                    Key.DeleteValue(AppName, false);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

    }
}
