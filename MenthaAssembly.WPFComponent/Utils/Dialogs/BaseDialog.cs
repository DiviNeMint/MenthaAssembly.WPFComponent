using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Win32
{
    public abstract class BaseDialog : CommonDialog
    {
        internal const string CLSID_FileOpenDialog = "DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7";
        internal const string CLSID_FileSaveDialog = "C0B4E2F3-BA21-4773-8DBA-335EC946EB8B";

        [DllImport("Shell32.dll")]
        private static extern int SHCreateItemFromParsingName([MarshalAs(UnmanagedType.LPWStr)] string pszPath, IBindCtx pbc, [In] ref Guid riid, [MarshalAs(UnmanagedType.Interface)] out object ppv);

        internal static IShellItem GetShellItemForPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            Guid IIDShellItem2 = new Guid("7E9FB0D3-919F-4307-AB2E-9B1860310C93");

            int Result = SHCreateItemFromParsingName(path, null, ref IIDShellItem2, out object unk);

            // Silently absorb errors such as ERROR_FILE_NOT_FOUND, ERROR_PATH_NOT_FOUND.
            // Let others pass through
            //if (Result == Win32Error.ERROR_FILE_NOT_FOUND || Result == Win32Error.ERROR_PATH_NOT_FOUND)
            if (Result == 2 || Result == 3)
            {
                Result = 0;
                unk = null;
            }

            if (Result < 0)
                throw new Win32Exception(Result);


            return (IShellItem)unk;
        }



    }
}
