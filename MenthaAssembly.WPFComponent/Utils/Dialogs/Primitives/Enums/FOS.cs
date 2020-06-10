using System;

namespace Microsoft.Win32
{
    // https://referencesource.microsoft.com/#system.windows.forms/winforms/Managed/System/WinForms/FileDialog_Vista_Interop.cs,84df1568dd56aa88,references

    [Flags]
    internal enum FOS : uint
    {
        OverwritePrompt = 0x00000002,
        StrictFileTypes = 0x00000004,
        NoChangeDir = 0x00000008,
        PickFolders = 0x00000020,
        ForceFileSystem = 0x00000040,       // Ensure that items returned are filesystem items.
        AllNonStorageItems = 0x00000080,    // Allow choosing items that have no storage.
        NoValidate = 0x00000100,
        AllowMultiSelect = 0x00000200,
        PathMustExist = 0x00000800,
        FileMustExist = 0x00001000,
        CreatePrompt = 0x00002000,
        ShareAware = 0x00004000,
        NoReadOnlyReturn = 0x00008000,
        NoTestFileCreate = 0x00010000,
        HideMRUPlaces = 0x00020000,
        HidePinnedPlaces = 0x00040000,
        NoDereferenceLinks = 0x00100000,
        DontAddToRecent = 0x02000000,
        ForceShowHidden = 0x10000000,
        DefaultNoMiniMode = 0x20000000
    }
}
