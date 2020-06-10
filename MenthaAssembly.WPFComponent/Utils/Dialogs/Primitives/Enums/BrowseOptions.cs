using System;

namespace Microsoft.Win32
{
    // https://docs.microsoft.com/zh-tw/windows/win32/api/shlobj_core/ns-shlobj_core-browseinfoa
    [Flags]
    public enum BrowseOptions
    {
        ReturnOnlyFileSystemDirs = 0x0001,
        DontGoBelowDomain = 0x0002,
        StatusText = 0x0004,                // The callback function can set the status text by sending messages to the dialog box. This flag is not supported when NewDialogStyle is specified.
        ReturnFileSystemAncestors = 0x0008,
        EditBox = 0x0010,
        Validate = 0x0020,                  // This flag is ignored if Editbox is not specified.
        NewDialogStyle = 0x0040,            // Use the new dialog layout with the ability to resize
        BrowseIncludeUrls = 0x0080,
        HideNewFolderButton = 0x0200,       // Don't display the 'New Folder' button
        NoTranslateTargets = 0x00400,
        BrowseForComputer = 0x01000,
        BrowseForPrinter = 0x02000,
        BrowseIncludeFiles = 0x04000,
        Shareable = 0x08000,
        BrowseFileJunctions = 0x10000
    }
}
