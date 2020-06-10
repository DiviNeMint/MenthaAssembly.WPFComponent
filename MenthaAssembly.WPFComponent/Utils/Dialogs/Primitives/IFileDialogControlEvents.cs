using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    //internal const string IID_IFileDialogControlEvents = "36116642-D713-4B97-9B83-7484A9D00433";
    [ComImport, Guid("36116642-D713-4B97-9B83-7484A9D00433"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileDialogControlEvents : IFileDialogEvents
    {
        void OnItemSelected([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc,
                            [In] int dwIDCtl,
                            [In] int dwIDItem);

        void OnButtonClicked([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc,
                             [In] int dwIDCtl);

        void OnCheckButtonToggled([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc,
                                  [In] int dwIDCtl,
                                  [In] bool bChecked);

        void OnControlActivating([In, MarshalAs(UnmanagedType.Interface)] IFileDialogCustomize pfdc,
                                 [In] int dwIDCtl);
    }

}
