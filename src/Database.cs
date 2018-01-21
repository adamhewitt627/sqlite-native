using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteNative
{
    public static partial class Sqlite3
    {
        [DllImport(SQLITE3)] public static extern int sqlite3_open([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_open16([MarshalAs(UnmanagedType.LPWStr)] string filename, out IntPtr db);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_open_v2))] private static extern int sqlite3_openv2([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr db, int flags, IntPtr vfs);
        public static int sqlite3_open_v2(string filename, out IntPtr db, int flags, IntPtr vfs = default) => sqlite3_openv2(filename, out db, flags, vfs);

        [DllImport(SQLITE3)] public static extern int sqlite3_close(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_close_v2(IntPtr db);

        [DllImport(SQLITE3)] public static extern int sqlite3_total_changes(IntPtr db);
        [DllImport(SQLITE3)] public static extern long sqlite3_last_insert_rowid(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_busy_timeout(IntPtr db, int ms);


#region Error Codes And Messages
        //https://sqlite.org/capi3ref.html#sqlite3_errcode
        [DllImport(SQLITE3)] public static extern int sqlite3_errcode(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_extended_errcode(IntPtr db);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_errmsg16))] private static extern IntPtr errmsg16(IntPtr db);
        public static string sqlite3_errmsg(IntPtr db) => Marshal.PtrToStringUni(errmsg16(db));
        public static string sqlite3_errmsg16(IntPtr db) => sqlite3_errmsg(db);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_errstr))] private static extern IntPtr errstr(IntPtr db);
        public static string sqlite3_errstr(IntPtr db) => Marshal.PtrToStringAnsi(errstr(db));
#endregion
    }
}