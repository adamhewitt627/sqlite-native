using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteNative
{
    public static class Sqlite3
    {
        public const string SQLITE3 = "sqlite3";
        static Sqlite3() => Debug.WriteLine($"Using SQLite version {sqlite3_libversion()}");

        public const int SQLITE_ERROR = 1;
        public const int SQLITE_OK = 0;
        public const int SQLITE_CANTOPEN = 14;
        public const int SQLITE_BUSY = 5;
        public const int SQLITE_ROW = 100;
        public const int SQLITE_DONE = 101;  /* sqlite3_step() has finished executing */

        public const int SQLITE_IOERR = 10;
        public const int SQLITE_IOERR_BLOCKED = (SQLITE_IOERR | (11 << 8));

        public const int SQLITE_OPEN_READONLY = 0x00000001;
        public const int SQLITE_OPEN_READWRITE = 0x00000002;
        public const int SQLITE_OPEN_CREATE = 0x00000004;
        public const int SQLITE_OPEN_URI = 0x00000040;
        public const int SQLITE_OPEN_MEMORY = 0x00000080;
        public const int SQLITE_OPEN_NOMUTEX = 0x00008000;
        public const int SQLITE_OPEN_FULLMUTEX = 0x00010000;
        public const int SQLITE_OPEN_SHAREDCACHE = 0x00020000;
        public const int SQLITE_OPEN_PRIVATECACHE = 0x00080000;

        public const int SQLITE_INTEGER = 1;
        public const int SQLITE_FLOAT = 2;
        public const int SQLITE_BLOB = 4;
        public const int SQLITE_NULL = 5;
        public const int SQLITE_TEXT = 3;
        public const int SQLITE3_TEXT = 3;

        public static IntPtr SQLITE_TRANSIENT = (IntPtr)(-1);
        public const int SQLITE_MISSING_DB_HANDLE = 1000;


        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_libversion")]
        private static extern IntPtr libversion();
        public static string sqlite3_libversion() => Marshal.PtrToStringAnsi(libversion());


#region Database Imports

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

#endregion

#region Statement Imports

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_prepare16_v2")]
        public static extern int sqlite3_prepare16_v2(IntPtr db, [MarshalAs(UnmanagedType.LPWStr)] string pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_bind_null(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);
        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);
        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int sqlite3_bind_blob(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPArray)] byte[] value, int byteCount, IntPtr pvReserved);
        public static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value, int byteCount)
            => sqlite3_bind_blob(stmt, index, value, byteCount, SQLITE_TRANSIENT);


        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int sqlite3_bind_text16(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string value, int nlen, IntPtr pvReserved);
        public static int sqlite3_bind_text16(IntPtr stmt, int index, string value)
            => sqlite3_bind_text16(stmt, index, value, -1, SQLITE_TRANSIENT);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        private static extern int sqlite3_bind_parameter_index(IntPtr stmt, IntPtr strName);
        public static int sqlite3_bind_parameter_index(IntPtr stmt, string strName)
        {
            var bytes = Encoding.UTF8.GetBytes(strName);
            var ptr = IntPtr.Zero;
            try
            {
                ptr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                Marshal.WriteByte(ptr, bytes.Length, 0);
                return sqlite3_bind_parameter_index(stmt, ptr);
            }
            finally { Marshal.FreeHGlobal(ptr); }
        }


        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_bind_parameter_name")]
        private static extern IntPtr bind_parameter_name(IntPtr stmt, int index);
        public static string sqlite3_bind_parameter_name(IntPtr stmt, int index)
        {
            var ptr = bind_parameter_name(stmt, index);
            if (ptr == IntPtr.Zero)
                return null;
            return Marshal.PtrToStringAnsi(ptr);
        }

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_bind_parameter_count(IntPtr stmt);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_step(IntPtr stmt);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_finalize(IntPtr stmt);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_column_count(IntPtr stmt);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_column_name16")]
        private static extern IntPtr column_name16(IntPtr stmt, int index);
        public static string sqlite3_column_name16(IntPtr stmt, int index) => Marshal.PtrToStringUni(column_name16(stmt, index));

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_reset(IntPtr stmt);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_column_bytes(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_column_blob")]
        private static extern IntPtr column_blob(IntPtr stmt, int index);
        public static byte[] sqlite3_column_blob(IntPtr stmt, int index)
        {
            var bytePtr = column_blob(stmt, index);
            var byteCount = sqlite3_column_bytes(stmt, index);

            var rv = new byte[byteCount];
            Marshal.Copy(bytePtr, rv, 0, byteCount);
            return rv;
        }

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern long sqlite3_column_int64(IntPtr stmt, int index);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern double sqlite3_column_double(IntPtr stmt, int index);

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_column_text16")]
        private static extern IntPtr column_text16(IntPtr stmt, int index);
        public static string sqlite3_column_text16(IntPtr stmt, int index) => Marshal.PtrToStringUni(column_text16(stmt, index));

        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        public static extern int sqlite3_column_type(IntPtr stmt, int index);

#endregion

    }
}
