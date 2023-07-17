using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace MenthaAssembly.Devices
{
    public class CursorInfo : IDisposable
    {
        public Cursor Cursor { get; }

        public IntPtr Handle => _Handle.DangerousGetHandle();

        private readonly SafeHandle _Handle;
        public CursorInfo(Cursor Cursor, SafeHandle Handle)
        {
            this.Cursor = Cursor;
            _Handle = Handle;
        }

        public void Dispose()
            => _Handle.Dispose();

        public static implicit operator Cursor(CursorInfo This)
            => This.Cursor;

    }
}