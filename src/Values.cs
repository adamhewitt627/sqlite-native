using System;
using System.Runtime.InteropServices;
using SqliteNative.Util;
using static System.Runtime.InteropServices.CallingConvention;

namespace SqliteNative
{
    partial class Sqlite3
    {
        internal unsafe static byte[] ToArray(byte* array, int length)
        {
            var result = new byte[length];
            fixed (byte* dest = result)
                Buffer.MemoryCopy(array, dest, length, length);
            return result;
        }

        //https://sqlite.org/c3ref/value_blob.html
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_value_blob))] private unsafe static extern byte* value_blob(IntPtr value);
        public unsafe static byte[] sqlite3_value_blob(IntPtr value)
            => ToArray(value_blob(value), sqlite3_value_bytes(value));

        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern double sqlite3_value_double(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_value_int(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern long sqlite3_value_int64(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern IntPtr sqlite3_value_pointer(IntPtr value);

        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_value_text))] private static extern IntPtr value_text(IntPtr value);
        public static string sqlite3_value_text(IntPtr value) => value_text(value).FromUtf8();
        public static string sqlite3_value_text16(IntPtr value) => sqlite3_value_text(value);
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_value_text16le))] private unsafe static extern byte* value_text16le(IntPtr value);
        public unsafe static byte[] sqlite3_value_text16le(IntPtr value) 
        {
            var length = sqlite3_value_bytes16(value);  //Calling this *after* value_text16be invalidates the pointer
            return ToArray(value_text16le(value), length);
        }
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_value_text16be))] private unsafe static extern byte* value_text16be(IntPtr value);
        public unsafe static byte[] sqlite3_value_text16be(IntPtr value)
        {
            var length = sqlite3_value_bytes16(value);  //Calling this *after* value_text16be invalidates the pointer
            return ToArray(value_text16be(value), length);
        }

        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_value_bytes(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_value_bytes16(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_value_type(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_value_numeric_type(IntPtr value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_value_nochange(IntPtr value);
    }
}