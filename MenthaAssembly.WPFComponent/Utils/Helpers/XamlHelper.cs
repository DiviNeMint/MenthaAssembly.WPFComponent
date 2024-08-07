using System.IO;
using System.Reflection;
using System.Windows.Markup;

namespace System.Windows
{
    public static class XamlHelper
    {
        public static T CreateXamlObject<T>(string Xaml)
            => XamlReader.Parse(Xaml) is T Item ? Item : default;

        public static bool TryParseResource(string ResourceName, out ResourceDictionary Resource)
        {
            if (string.IsNullOrEmpty(ResourceName))
            {
                Resource = null;
                return false;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(ResourceName)))
                ResourceName += ".xaml";

            if (ResourceName.StartsWith("pack://"))
            {
                try
                {
                    Resource = new() { Source = new Uri(ResourceName, UriKind.Absolute) };
                }
                catch
                {
                    Resource = null;
                    return false;
                }
            }

            if (!ResourceName.StartsWith("/"))
                ResourceName = $"/{ResourceName}";

            foreach (Assembly Assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    string AssemblyName = Assembly.GetName().Name;
                    Resource = new() { Source = new Uri($"pack://application:,,,/{AssemblyName};component{ResourceName}", UriKind.Absolute) };

                    return true;
                }
                catch
                {

                }
            }

            Resource = null;
            return false;
        }
        public static bool TryParseResource(Assembly Assembly, string ResourceName, out ResourceDictionary Resource)
        {
            if (string.IsNullOrEmpty(ResourceName))
            {
                Resource = null;
                return false;
            }

            try
            {
                string FileName = Path.GetFileNameWithoutExtension(ResourceName);
                string AssemblyName = Assembly.GetName().Name;
                Resource = new() { Source = new Uri($"pack://application:,,,/{AssemblyName};component/{FileName}.xaml", UriKind.Absolute) };

                return true;
            }
            catch
            {
                Resource = null;
                return false;
            }
        }

    }
}