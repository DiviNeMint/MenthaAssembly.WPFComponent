using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    [ComImport, Guid("84BCCD23-5FDE-4CDB-AEA4-AF64B83D78AB"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileSaveDialog : IFileDialog
    {
        void SetFileTypes(uint cFileTypes, ref ComDlg_FilterSpec rgFilterSpec);

        void AddPlace([MarshalAs(UnmanagedType.Interface)] IShellItem psi, FileDialogCustomPlace fdcp);

        void SetSaveAsItem([MarshalAs(UnmanagedType.Interface)] IShellItem psi);

        void SetProperties([MarshalAs(UnmanagedType.Interface)] IntPtr pStore);

        void SetCollectedProperties([MarshalAs(UnmanagedType.Interface)] IntPtr pList, int fAppendDefault);

        void GetProperties([MarshalAs(UnmanagedType.Interface)] out IntPtr ppStore);

        void ApplyProperties([MarshalAs(UnmanagedType.Interface)] IShellItem psi,
                             [MarshalAs(UnmanagedType.Interface)] IntPtr pStore,
                             [ComAliasName("ShellObjects.wireHWND")] ref IntPtr hwnd,
                             [MarshalAs(UnmanagedType.Interface)] IntPtr pSink);

    }
}
