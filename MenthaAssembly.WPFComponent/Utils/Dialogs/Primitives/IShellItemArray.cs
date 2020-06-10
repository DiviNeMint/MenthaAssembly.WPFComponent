using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    //internal const string IID_IShellItemArray = "B63EA76D-1F85-456F-A19C-48159EFA858B";
    [ComImport,Guid("B63EA76D-1F85-456F-A19C-48159EFA858B"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IShellItemArray
    {
        // Not supported: IBindCtx
        void BindToHandler([In, MarshalAs(UnmanagedType.Interface)] IntPtr pbc, [In] ref Guid rbhid, [In] ref Guid riid, out IntPtr ppvOut);

        void GetPropertyStore([In] int Flags, [In] ref Guid riid, out IntPtr ppv);

        void GetPropertyDescriptionList([In] ref PropertyKey keyType, [In] ref Guid riid, out IntPtr ppv);

        void GetAttributes([In] SIAttributesFlags dwAttribFlags, [In] uint sfgaoMask, out uint psfgaoAttribs);

        void GetCount(out uint pdwNumItems);

        void GetItemAt([In] uint dwIndex, [MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);

        void EnumItems([MarshalAs(UnmanagedType.Interface)] out IntPtr ppenumShellItems);
    }
}
