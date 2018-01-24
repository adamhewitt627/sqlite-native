using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteNative.Util
{
    internal static class Extensions
    {
        public static unsafe IntPtr ToUtf8(this string @string, out int byteCount)
        {
            byteCount = 0;
            if (@string == null) return IntPtr.Zero;
            IntPtr utf8 = IntPtr.Zero;
            fixed (char* sqlString = @string)
            {
                byteCount = Encoding.UTF8.GetByteCount(@string);
                utf8 = Marshal.AllocHGlobal(byteCount);
                Encoding.UTF8.GetBytes(sqlString, @string.Length, (byte*)utf8.ToPointer(), byteCount);
                return utf8;
            }
        }
        public static unsafe string FromUtf8(this IntPtr utf8)
        {
            if (@utf8 == IntPtr.Zero) return null;
            var byteCount = 0;
            while (Marshal.ReadByte(utf8, byteCount) != 0) byteCount++;
            var charCount = Encoding.UTF8.GetCharCount((byte*)utf8.ToPointer(), byteCount);
            var result = new string('\0', charCount);
            fixed (char* resultPtr = result)
                Encoding.UTF8.GetChars((byte*)utf8.ToPointer(), byteCount, resultPtr, charCount);
            return result;
        }
    }
}