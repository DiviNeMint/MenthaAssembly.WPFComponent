using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Microsoft.Win32
{
    public sealed class FolderBrowserDialog : BaseDialog
    {
        public string Title { get; set; } = string.Empty;

        public string InitialDirectory { get; set; } = string.Empty;

        public string SelectedPath { get; set; } = string.Empty;

        private bool _UseLegacyDialog = Environment.OSVersion.Version.Major < 6;
        public bool UseLegacyDialog
        {
            get => _UseLegacyDialog;
            set => _UseLegacyDialog = value || Environment.OSVersion.Version.Major < 6;
        }

        public bool AutoCreateFolder { set; get; }

        public override void Reset()
        {
            Title = string.Empty;
            InitialDirectory = string.Empty;
            SelectedPath = string.Empty;
            AutoCreateFolder = false;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            if (UseLegacyDialog)
                return RunLegacyDialog(hwndOwner);

            return RunVistaDialog(hwndOwner);
        }

        #region Vista Dialog
        private class FolderBrowserDialogHandler : IFileDialogEvents, IDisposable
        {
            #region Windows API

            [DllImport("user32.dll")]
            private static extern int EnumChildWindows(IntPtr hwnd, EnumChildCallbackProc lpfn, IntPtr lParam);
            private delegate bool EnumChildCallbackProc(IntPtr hwnd, IntPtr lParam);

            [DllImport("user32.dll")]
            private static extern IntPtr GetDlgItem(IntPtr hwnd, int ID);

            [DllImport("user32.dll")]
            private static extern int GetDlgCtrlID(IntPtr hwnd);

            [DllImport("Comctl32.dll")]
            private static extern bool SetWindowSubclass(IntPtr hwnd, SubClassProc pfnSubclass, int uIdSubclass, IntPtr dwRefData);
            private delegate int SubClassProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam, int uIdSubclass, IntPtr dwRefData);

            [DllImport("Comctl32.dll")]
            private static extern int DefSubclassProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam);

            #endregion

            private IntPtr Hwnd = IntPtr.Zero;
            private void UpdateHwnd()
            {
                if (Hwnd == IntPtr.Zero)
                {
                    Guid OleWindowIID = new Guid("00000114-0000-0000-C000-000000000046");
                    Marshal.QueryInterface(Marshal.GetIUnknownForObject(Dialog), ref OleWindowIID, out IntPtr ppv);
                    IOleWindow OleWindow = (IOleWindow)Marshal.GetObjectForIUnknown(ppv);

                    OleWindow.GetWindow(out Hwnd);

                    //// PrintItemId() 
                    //EnumChildWindows(Hwnd,
                    //new EnumChildCallbackProc((hwnd, lParam) =>
                    //{
                    //    Debug.WriteLine($"Handle : {hwnd.ToString("X8")}, ID : {GetDlgCtrlID(hwnd)}");
                    //    return true;
                    //}),
                    //IntPtr.Zero);
                }
            }

            private FolderBrowserDialog Parent;
            private IFileDialog Dialog;
            private readonly uint EventCookie;
            public FolderBrowserDialogHandler(FolderBrowserDialog Parent, IFileDialog Dialog)
            {
                this.Parent = Parent;
                this.Dialog = Dialog;
                Dialog.Advise(this, out EventCookie);
            }

            public int OnFileOk([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd)
            {
                Parent.SelectedPath = GetFolderPath();
                return 0;
            }

            private bool IsAttach = false;
            private IntPtr pOpenButton;
            private SubClassProc AttachSubClassProc;
            public int OnFolderChanging([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                                        [In, MarshalAs(UnmanagedType.Interface)] IShellItem psiFolder)
            {
                if (Parent.AutoCreateFolder && !IsAttach)
                {
                    UpdateHwnd();
                    pOpenButton = GetDlgItem(Hwnd, 1);   // OKButton = 1, CancelButton = 2
                    if (pOpenButton != IntPtr.Zero)
                    {
                        AttachSubClassProc = new SubClassProc(SubClass);
                        IsAttach = SetWindowSubclass(pOpenButton, AttachSubClassProc, 0, IntPtr.Zero);
                    }
                }

                return 0;
            }
            private int SubClass(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam, int uIdSubclass, IntPtr dwRefData)
            {
                switch ((Win32Messages)uMsg)
                {
                    case Win32Messages.WM_LButtonUp:
                        {
                            Dialog.GetFileName(out string FileName);
                            if (!string.IsNullOrEmpty(FileName))
                            {
                                Dialog.GetFolder(out IShellItem Item);
                                Item.GetDisplayName(SIGDN.FileSysPath, out string FolderPath);
                                DirectoryInfo FolderInfo = new DirectoryInfo(FolderPath);
                                if (!FolderInfo.Name.Equals(FileName))
                                {
                                    string ResultFolder = Path.Combine(FolderPath, FileName);
                                    if (!Directory.Exists(ResultFolder))
                                        Directory.CreateDirectory(ResultFolder);
                                }
                            }
                            break;
                        }
                }

                return DefSubclassProc(hwnd, uMsg, wParam, lParam);
            }

            public void OnFolderChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd) { }

            public void OnSelectionChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd) { }

            private string GetFolderPath()
            {
                Dialog.GetFolder(out IShellItem Item);
                Item.GetDisplayName(SIGDN.FileSysPath, out string FolderPath);
                return FolderPath;
            }

            public void OnShareViolation([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                                         [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi,
                                         out FDE_ShareViolation_Response pResponse)
                => pResponse = FDE_ShareViolation_Response.Default;

            public void OnTypeChange([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd) { }

            public void OnOverwrite([In, MarshalAs(UnmanagedType.Interface)] IFileDialog pfd,
                                    [In, MarshalAs(UnmanagedType.Interface)] IShellItem psi,
                                    out FDE_Overwrite_Response pResponse)
                => pResponse = FDE_Overwrite_Response.Default;

            public void Dispose()
            {
                Dialog?.Unadvise(EventCookie);
                Dialog = null;
            }

        }

        private bool RunVistaDialog(IntPtr hwndOwner)
        {
            IFileDialog Dialog = (IFileDialog)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(CLSID_FileOpenDialog)));

            PrepareVistaDialog(Dialog);

            using FolderBrowserDialogHandler Handler = new FolderBrowserDialogHandler(this, Dialog);

            return Dialog.Show(hwndOwner) == 0;
        }

        private void PrepareVistaDialog(IFileDialog Dialog)
        {
            if (!string.IsNullOrEmpty(InitialDirectory) &&
                GetShellItemForPath(InitialDirectory) is IShellItem initialDirectory)
            {
                Dialog.SetDefaultFolder(initialDirectory);
                Dialog.SetFolder(initialDirectory);
            }

            Dialog.SetTitle(Title);
            Dialog.SetOptions(FOS.PickFolders);

        }

        #endregion

        #region Legacy Dialog

        #region Windows API
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, int msg, int wParam, string lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, int msg, int wParam, int lParam);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr SHBrowseForFolder([In] BrowseInfo lpbi);

        [DllImport("shell32.dll")]
        [ResourceExposure(ResourceScope.None)]
        private static extern int SHGetSpecialFolderLocation(IntPtr hwnd, Environment.SpecialFolder csidl, ref IntPtr ppidl);

        [DllImport("shell32.dll")]
        [ResourceExposure(ResourceScope.None)]
        private static extern bool SHGetPathFromIDListEx(IntPtr pidl, IntPtr pszPath, int cchPath, int flags);

        private static bool SHGetPathFromIDListLongPath(IntPtr pidl, ref IntPtr pszPath)
        {
            int noOfTimes = 1;
            // This is how size was allocated in the calling method.
            //int bufferSize = MaxPathLength * Marshal.SystemDefaultCharSize;
            int length = MaxPathLength;
            bool result;

            // SHGetPathFromIDListEx returns false in case of insufficient buffer.
            // This method does not distinguish between insufficient memory and an error. Until we get a proper solution,
            // this logic would work. In the worst case scenario, loop exits when length reaches unicode string length.
            while ((result = SHGetPathFromIDListEx(pidl, pszPath, length, 0)) == false && length < short.MaxValue)
            {
                string path = Marshal.PtrToStringAuto(pszPath);

                if (path.Length != 0 && path.Length < length)
                    break;

                noOfTimes += 2; //520 chars capacity increase in each iteration.
                length = Math.Min(noOfTimes * length, short.MaxValue);
                pszPath = Marshal.ReAllocHGlobal(pszPath, (IntPtr)((length + 1) * Marshal.SystemDefaultCharSize));
            }

            return result;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class BrowseInfo
        {
            public IntPtr hwndOwner;

            // LPCITEMIDLIST pidlRoot; 
            // Root ITEMIDLIST
            public IntPtr pidlRoot;

            // For interop purposes, send over a buffer of MAX_PATH size. 
            //LPWSTR pszDisplayName; 
            // Return display name of item selected.
            public IntPtr pszDisplayName;

            //LPCWSTR lpszTitle; 
            // text to go in the banner over the tree.
            public string lpszTitle;

            //UINT ulFlags; 
            // Flags that control the return stuff
            public BrowseOptions ulFlags;

            //BFFCALLBACK lpfn; 
            // Call back pointer
            public BrowseCallbackProc lpfn;

            //LPARAM lParam; 
            // extra info that's passed back in callbacks
            public IntPtr lParam;

            //int iImage; 
            // output var: where to return the Image index.
            public int iImage;

        }

        private delegate IntPtr BrowseCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);

        // NativeMethods
        private const int MaxPathLength = 260,      // MAX_PATH
                          BFFM_Initialized = 1,
                          BFFM_SELChanged = 2,
                          BFFM_EnableOk = (int)Win32Messages.BFFM_EnableOK;
        private static readonly int BFFM_SetSelection = (int)(Marshal.SystemDefaultCharSize == 1 ? Win32Messages.BFFM_SetSelectionA : Win32Messages.BFFM_SetSelectionW);

        #endregion

        private BrowseCallbackProc LegacyDialogCallback;
        private bool RunLegacyDialog(IntPtr hwndOwner)
        {
            IntPtr pidlRoot = IntPtr.Zero,
                   pidlRet = IntPtr.Zero,
                   pszDisplayName = Marshal.AllocHGlobal(MaxPathLength * Marshal.SystemDefaultCharSize),
                   pszSelectedPath = Marshal.AllocHGlobal((MaxPathLength + 1) * Marshal.SystemDefaultCharSize);

            // SetRootFolder
            SHGetSpecialFolderLocation(hwndOwner, Environment.SpecialFolder.Desktop, ref pidlRoot);

            try
            {
                this.LegacyDialogCallback = new BrowseCallbackProc(this.HookProc);

                BrowseInfo bi = new BrowseInfo
                {
                    hwndOwner = hwndOwner,
                    pidlRoot = pidlRoot,
                    pszDisplayName = pszDisplayName,
                    lpszTitle = string.Empty,
                    ulFlags = BrowseOptions.NewDialogStyle,
                    lpfn = LegacyDialogCallback
                };

                // Show
                pidlRet = SHBrowseForFolder(bi);

                if (pidlRet != IntPtr.Zero)
                {
                    // Then retrieve the path from the IDList
                    SHGetPathFromIDListLongPath(pidlRet, ref pszSelectedPath);

                    // Convert to a string
                    SelectedPath = Marshal.PtrToStringAuto(pszSelectedPath);

                    return true;
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(pidlRoot);
                if (pidlRet != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(pidlRet);

                // Then free all the stuff we've allocated or the SH API gave us
                if (pszSelectedPath != IntPtr.Zero)
                    Marshal.FreeHGlobal(pszSelectedPath);

                if (pszDisplayName != IntPtr.Zero)
                    Marshal.FreeHGlobal(pszDisplayName);

                this.LegacyDialogCallback = null;
            }

            return false;
        }

        protected override IntPtr HookProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr wParam)
        {
            switch (msg)
            {
                case BFFM_Initialized:
                    // Indicates the browse dialog box has finished initializing. The lpData value is zero. 
                    if (!string.IsNullOrEmpty(InitialDirectory))
                        // Try to select the folder specified by InitialDirectory
                        SendMessage(hwnd, BFFM_SetSelection, 1, InitialDirectory);
                    break;
                case BFFM_SELChanged:
                    // Indicates the selection has changed. The lpData parameter points to the item identifier list for the newly selected item. 
                    // lParam is selectedPidl
                    if (lParam != IntPtr.Zero)
                    {
                        IntPtr pszSelectedPath = Marshal.AllocHGlobal((MaxPathLength + 1) * Marshal.SystemDefaultCharSize);
                        // Try to retrieve the path from the IDList
                        bool IsFileSystemFolder = SHGetPathFromIDListLongPath(lParam, ref pszSelectedPath);
                        Marshal.FreeHGlobal(pszSelectedPath);

                        SendMessage(hwnd, BFFM_EnableOk, 0, IsFileSystemFolder ? 1 : 0);
                    }
                    break;
            }

            return IntPtr.Zero;
        }

        #endregion

    }
}
