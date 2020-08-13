using System;

namespace Microsoft.Win32
{
    /// <summary>
    /// The view mode of the band object.
    /// </summary>
    [Flags]
    public enum DeskBandDisplayFlags
    {
        /// <summary>
        /// Band object is displayed in a horizontal band.
        /// </summary>
        DBIF_VIEWMODE_NORMAL = 0x0000,

        /// <summary>
        /// Band object is displayed in a vertical band.
        /// </summary>
        DBIF_VIEWMODE_VERTICAL = 0x0001,

        /// <summary>
        /// Band object is displayed in a floating band.
        /// </summary>
        DBIF_VIEWMODE_FLOATING = 0x0002,

        /// <summary>
        /// Band object is displayed in a transparent band.
        /// </summary>
        DBIF_VIEWMODE_TRANSPARENT = 0x0004

    }
}
