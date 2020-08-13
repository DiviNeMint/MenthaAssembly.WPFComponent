namespace Microsoft.Win32
{
    internal enum SIAttributesFlags
    {
        And = 1,          // if multiple items and the attributes together.
        Or = 2,           // if multiple items or the attributes together.
        AppCompat = 3,    // Call GetAttributes directly on the ShellFolder for multiple attributes
    }
}
