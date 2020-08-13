namespace Microsoft.Win32
{
    // https://docs.microsoft.com/zh-tw/windows/win32/api/shobjidl_core/ne-shobjidl_core-_shgdnf
    internal enum SIGDN : uint
    {
        NormalDisplay = 0x00000000,                   // SHGDN_Normal
        ParentRelativeParsing = 0x80018001,           // SHGDN_InFolder     | SHGDN_ForParsing
        DesktopAbsoluteParsing = 0x80028000,          // SHGDN_ForParsing
        ParentRelativeEeiting = 0x80031001,           // SHGDN_InFolder     | SHGDN_ForEeiting
        DesktopAbsoluteEeiting = 0x8004c000,          // SHGDN_ForParsing   | SHGDN_ForAddressBar
        FileSysPath = 0x80058000,                     // SHGDN_ForParsing
        Url = 0x80068000,                             // SHGDN_ForParsing
        ParentRelativeForAddressBar = 0x8007c001,     // SHGDN_InFolder     | SHGDN_ForParsing      | SHGDN_ForAddressBar
        ParentRelative = 0x80080001                   // SHGDN_InFolder
    }
}
