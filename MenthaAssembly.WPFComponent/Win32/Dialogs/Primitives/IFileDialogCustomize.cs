using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    [ComImport, Guid("E6FDD21A-163F-4975-9C8C-A69F1BA37034"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IFileDialogCustomize
    {
        void EnableOpenDropDown([In] int dwIDCtl);

        void AddMenu([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        void AddPushButton([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        void AddComboBox([In] int dwIDCtl);

        void AddRadioButtonList([In] int dwIDCtl);

        void AddCheckButton([In] int dwIDCtl,
                            [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel,
                            [In] bool bChecked);

        void AddEditBox([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

        void AddSeparator([In] int dwIDCtl);

        void AddText([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

        void SetControlLabel([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        void GetControlState([In] int dwIDCtl, [Out] out CDControlState pdwState);

        void SetControlState([In] int dwIDCtl, [In] CDControlState dwState);

        void GetEditBoxText([In] int dwIDCtl, [Out] IntPtr ppszText);

        void SetEditBoxText([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

        void GetCheckButtonState([In] int dwIDCtl, [Out] out bool pbChecked);

        void SetCheckButtonState([In] int dwIDCtl, [In] bool bChecked);

        void AddControlItem([In] int dwIDCtl, [In] int dwIDItem,
                            [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        void RemoveControlItem([In] int dwIDCtl, [In] int dwIDItem);

        void RemoveAllControlItems([In] int dwIDCtl);

        void GetControlItemState([In] int dwIDCtl, [In] int dwIDItem, [Out] out CDControlState pdwState);

        void SetControlItemState([In] int dwIDCtl, [In] int dwIDItem, [In] CDControlState dwState);

        void GetSelectedControlItem([In] int dwIDCtl, [Out] out int pdwIDItem);

        void SetSelectedControlItem([In] int dwIDCtl, [In] int dwIDItem); // Not valid for OpenDropDown

        void StartVisualGroup([In] int dwIDCtl, [In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

        void EndVisualGroup();

        void MakeProminent([In] int dwIDCtl);
    }

}
