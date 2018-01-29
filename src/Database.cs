using System;
using System.Runtime.InteropServices;
using System.Text;
using SqliteNative.Util;

namespace SqliteNative
{
    public static partial class Sqlite3
    {
#region Opening A New Database Connection
        //https://sqlite.org/c3ref/open.html
        [DllImport(SQLITE3)] private static extern int sqlite3_open(IntPtr filename, out IntPtr db);
        [DllImport(SQLITE3)] private static extern int sqlite3_open_v2(IntPtr filename, out IntPtr db, int flags, IntPtr vfs);
        public static int sqlite3_open16(string filename, out IntPtr db) => sqlite3_open(filename, out db);
        public static int sqlite3_open(string filename, out IntPtr db)
        {
            using (var utf8 = new Utf8String(filename))
                return sqlite3_open(utf8, out db);
        }
        public static int sqlite3_open_v2(string filename, out IntPtr db, int flags, IntPtr vfs = default)
        {
            using (var utf8 = new Utf8String(filename))
                return sqlite3_open_v2(utf8, out db, flags, vfs);
        }
#endregion

        [DllImport(SQLITE3)] public static extern int sqlite3_close(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_close_v2(IntPtr db);

        [DllImport(SQLITE3)] public static extern int sqlite3_total_changes(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_changes(IntPtr db);
        [DllImport(SQLITE3)] public static extern long sqlite3_last_insert_rowid(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_busy_timeout(IntPtr db, int ms);

#region One-Step Query Execution Interface
        //https://sqlite.org/c3ref/exec.html
        [DllImport(SQLITE3)] public static extern int sqlite3_exec(IntPtr db, IntPtr sql, IntPtr callback, IntPtr firstArg, IntPtr errMsg);
        public static int sqlite3_exec(IntPtr db, string sql)
        {
            using (var utf8 = new Utf8String(sql))
                return sqlite3_exec(db, utf8, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }
#endregion

#region Error Codes And Messages
        //https://sqlite.org/capi3ref.html#sqlite3_errcode
        [DllImport(SQLITE3)] public static extern int sqlite3_errcode(IntPtr db);
        [DllImport(SQLITE3)] public static extern int sqlite3_extended_errcode(IntPtr db);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_errmsg))] private static extern IntPtr errmsg(IntPtr db);
        public static string sqlite3_errmsg(IntPtr db) => errmsg(db).FromUtf8();
        public static string sqlite3_errmsg16(IntPtr db) => sqlite3_errmsg(db);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_errstr))] private static extern IntPtr errstr(IntPtr db);
        public static string sqlite3_errstr(IntPtr db) => errstr(db).FromUtf8();
#endregion
    }
}