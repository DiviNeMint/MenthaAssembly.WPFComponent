using System;

namespace Microsoft.Win32
{
    /// <summary>
    /// The set of flags that determine which members of this structure are being requested by the caller.
    /// </summary>
    [Flags]
    public enum DeskBandMode
    {
        /// <summary>
        /// ptMinSize is requested.
        /// </summary>
        DBIM_MINSIZE = 0x0001,

        /// <summary>
        /// ptMaxSize is requested.
        /// </summary>
        DBIM_MAXSIZE = 0x0002,

        /// <summary>
        /// ptIntegral is requested.
        /// </summary>
        DBIM_INTEGRAL = 0x0004,

        /// <summary>
        /// ptActual is requested.
        /// </summary>
        DBIM_ACTUAL = 0x0008,

        /// <summary>
        /// wszTitle is requested.
        /// </summary>
        DBIM_TITLE = 0x0010,

        /// <summary>
        /// dwModeFlags is requested.
        /// </summary>
        DBIM_MODEFLAGS = 0x0020,

        /// <summary>
        /// crBkgnd is requested.
        /// </summary>
        DBIM_BKCOLOR = 0x0040
    }
}
