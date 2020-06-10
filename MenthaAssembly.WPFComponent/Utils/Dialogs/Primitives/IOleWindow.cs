using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    //internal const string IID_IOleWindow = "00000114-0000-0000-C000-000000000046";
    [ComConversionLoss, Guid("00000114-0000-0000-C000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleWindow
    {
        void GetWindow(out IntPtr phwnd);

        void ContextSensitiveHelp([In] bool fEnterMode);
    }
}
