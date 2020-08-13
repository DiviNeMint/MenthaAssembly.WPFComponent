using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    [ComImport, Guid("973510DB-7D7F-452B-8975-74A85828D354"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileDialogEvents
    {
        // NoTE: some of these callbacks are cancelable - returning S_FALSE means that 
        // the dialog should not proceed (e.g. with closing, changing folder); to 
        // support this, we need to use the PreserveSig attribute to enable us to return
        // the proper HRESULT
        [PreserveSig]
        int OnFileOk([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

        [PreserveSig]
        int OnFolderChanging([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] IShellItem psiFolder);

        void OnFolderChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

        void OnSelectionChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

        void OnShareViolation([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, out FDE_ShareViolation_Response pResponse);

        void OnTypeChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd);

        void OnOverwrite([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd, [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, out FDE_Overwrite_Response pResponse);

    }
}
