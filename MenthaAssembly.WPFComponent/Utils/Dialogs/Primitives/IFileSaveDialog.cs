using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    //internal const string IID_IFileSaveDialog = "84bccd23-5fde-4cdb-aea4-af64b83d78ab";
    [ComImport, Guid("84BCCD23-5FDE-4CDB-AEA4-AF64B83D78AB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileSaveDialog : IFileDialog
    {
        void SetFileTypes([In] uint cFileTypes, [In] ref ComDlg_FilterSpec rgFilterSpec);

        void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, FileDialogCustomPlace fdcp);

        void SetSaveAsItem([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

        void SetProperties([In, MarshalAs(UnmanagedType.Interface)] IntPtr pStore);

        void SetCollectedProperties([In, MarshalAs(UnmanagedType.Interface)] IntPtr pList, [In] int fAppendDefault);

        void GetProperties([MarshalAs(UnmanagedType.Interface)] out IntPtr ppStore);

        void ApplyProperties([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pStore, [In, ComAliasName("ShellObjects.wireHWND")] ref IntPtr hwnd, [In, MarshalAs(UnmanagedType.Interface)] IntPtr pSink);
    
    }
}
