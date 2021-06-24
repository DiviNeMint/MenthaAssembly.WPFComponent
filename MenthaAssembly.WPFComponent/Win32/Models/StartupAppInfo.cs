namespace MenthaAssembly.Win32
{
    public class StartupAppInfo
    {
        public string Name { get; }

        public string Path { get; }

        public bool AdministratorPermission { get; }

        internal StartupAppInfo(string Name, string Path, bool AdministratorPermission)
        {
            this.Name = Name;
            this.Path = Path;
            this.AdministratorPermission = AdministratorPermission;
        }

        public override string ToString()
            => $"{{{(AdministratorPermission ? "★" : "")}{Name}, {Path}}}";
    }
}
