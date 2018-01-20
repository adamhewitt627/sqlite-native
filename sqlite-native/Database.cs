using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteNative
{
    public static partial class Sqlite3
    {
        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int sqlite3_open_v2(IntPtr filename, out IntPtr db, int flags, IntPtr vfs);
        public static int sqlite3_open_v2(string filename, out IntPtr db, int flags, IntPtr vfs)
        {
            var bytes = Encoding.UTF8.GetBytes(filename);
            var ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                Marshal.WriteByte(ptr, bytes.Length, 0);
                return sqlite3_open_v2(ptr, out db, flags, vfs);
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_close(IntPtr db);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_total_changes(IntPtr db);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern long sqlite3_last_insert_rowid(IntPtr db);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_busy_timeout(IntPtr db, int ms);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_errcode(IntPtr db);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_errmsg16")]
        private static extern IntPtr errmsg16(IntPtr db);
        public static string sqlite3_errmsg16(IntPtr db) => Marshal.PtrToStringUni(errmsg16(db));
    }
}