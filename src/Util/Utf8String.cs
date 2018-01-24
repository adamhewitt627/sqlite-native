using System;
using System.Runtime.InteropServices;

namespace SqliteNative.Util
{
    internal class Utf8String : Disposable
    {
        private readonly IntPtr _utf8;
        private int _length;

        public Utf8String(string @string) => _utf8 = @string.ToUtf8(out _length);
        public static implicit operator IntPtr(Utf8String @string) => @string._utf8;
        public int Length => _length;

        protected override void Dispose(bool disposing) => Marshal.FreeHGlobal(_utf8);
    }
}