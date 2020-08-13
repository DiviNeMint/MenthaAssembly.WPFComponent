using System;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct PropertyKey
    {
        internal Guid fmtid;
        internal uint pid;
    }
}
