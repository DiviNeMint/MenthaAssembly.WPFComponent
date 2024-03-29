﻿using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    [ComImport, Guid("D57C7288-D4Ad-4768-BE02-9D969532D960"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileOpenDialog : IFileDialog
    {
        void SetFileTypes([In] uint cFileTypes, [In] ref ComDlg_FilterSpec rgFilterSpec);

        void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, FileDialogCustomPlace fdcp);

        void GetResults([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppenum);

        void GetSelectedItems([MarshalAs(UnmanagedType.Interface)] out IShellItemArray ppsai);
    }

}
