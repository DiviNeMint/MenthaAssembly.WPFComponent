using System;

namespace Microsoft.Win32
{
    /// <summary>
    /// A value that receives a set of flags that specify the mode of operation for the band object.
    /// </summary>
    [Flags]
    public enum DeskBandModeFlags : uint
    {
        /// <summary>
        /// The band uses default properties. The other mode flags modify this flag.
        /// </summary>
        DBIMF_NORMAL = 0x0000,

        /// <summary>
        /// Windows XP and later: The band object is of a fixed sized and position. With this flag, a sizing grip is not displayed on the band object.
        /// </summary>
        DBIMF_FIXED = 0x0001,

        /// <summary>
        /// DBIMF_FIXEDBMP
        /// Windows XP and later: The band object uses a fixed bitmap (.bmp) file as its background. Note that backgrounds are not supported in all cases, so the bitmap may not be seen even when this flag is set.
        /// </summary>
        DBIMF_FIXEDBMP = 0x0004,

        /// <summary>
        /// The height of the band object can be changed. The ptIntegral member defines the step value by which the band object can be resized.
        /// </summary>
        DBIMF_VARIABLEHEIGHT = 0x0008,

        /// <summary>
        /// Windows XP and later: The band object cannot be removed from the band container.
        /// </summary>
        DBIMF_UNDELETEABLE = 0x0010,

        /// <summary>
        /// The band object is displayed with a sunken appearance.
        /// </summary>
        DBIMF_DEBOSSED = 0x0020,

        /// <summary>
        /// The band is displayed with the background color specified in crBkgnd.
        /// </summary>
        DBIMF_BKCOLOR = 0x0040,

        /// <summary>
        /// Windows XP and later: If the full band object cannot be displayed (that is, the band object is smaller than ptActual, a chevron is shown to indicate that there are more options available. These options are displayed when the chevron is clicked.
        /// </summary>
        DBIMF_USECHEVRON = 0x0080,

        /// <summary>
        /// Windows XP and later: The band object is displayed in a new row in the band container.
        /// </summary>
        DBIMF_BREAK = 0x0100,

        /// <summary>
        /// Windows XP and later: The band object is the first object in the band container.
        /// </summary>
        DBIMF_ADDTOFRONT = 0x0200,

        /// <summary>
        /// Windows XP and later: The band object is displayed in the top row of the band container.
        /// </summary>
        DBIMF_TOPALIGN = 0x0400,

        /// <summary>
        /// Windows Vista and later: No sizing grip is ever displayed to allow the user to move or resize the band object.
        /// </summary>
        DBIMF_NOGRIPPER = 0x0800,

        /// <summary>
        /// Windows Vista and later: A sizing grip that allows the user to move or resize the band object is always shown, even if that band object is the only one in the container.
        /// </summary>
        DBIMF_ALWAYSGRIPPER = 0x1000,

        /// <summary>
        /// Windows Vista and later: The band object should not display margins.
        /// </summary>
        DBIMF_NOMARGINS = 0x2000
    }
}
