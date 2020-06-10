using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
    internal struct ComDlg_FilterSpec
    {
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pszName;
        [MarshalAs(UnmanagedType.LPWStr)]
        internal string pszSpec;
    }
}
