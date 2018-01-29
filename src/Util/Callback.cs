using System;
using System.Runtime.InteropServices;
using SqliteNative.Util;

namespace SqliteNative.Util
{
    public class Callback<T> : Disposable
    {
        private readonly IntPtr _pointer;
        private readonly GCHandle _handle;

        public Callback(T @delegate)
        {
            _pointer = Marshal.GetFunctionPointerForDelegate(@delegate);
            _handle = GCHandle.Alloc(@delegate);
        }
        public static implicit operator IntPtr(Callback<T> callback) => callback._pointer;

        protected override void Dispose(bool disposing) => _handle.Free();
    }
}