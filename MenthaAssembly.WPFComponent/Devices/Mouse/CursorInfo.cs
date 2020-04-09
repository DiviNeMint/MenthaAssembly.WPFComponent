using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace MenthaAssembly.Devices
{
    public class CursorInfo
    {
        public Cursor Cursor { get; }

        public IntPtr Handle => _Handle.DangerousGetHandle();

        private readonly SafeHandle _Handle;
        public CursorInfo(Cursor Cursor, SafeHandle Handle)
        {
            this.Cursor = Cursor;
            this._Handle = Handle;
        }

        ~CursorInfo()
        {
            _Handle?.Dispose();
        }

    }
}
