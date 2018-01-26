using System;
using System.Runtime.InteropServices;
using System.Text;
using SqliteNative.Util;

namespace SqliteNative
{
    public static partial class Sqlite3
    {

#region Compiling An SQL Statement
        //https://sqlite.org/c3ref/prepare.html
        [DllImport(SQLITE3)] private static extern int sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
        public static int sqlite3_prepare(IntPtr db, string pSql, out IntPtr stmt, out string remaining)
        {
            using (var utf8 = new Utf8String(pSql))
            {
                var err = sqlite3_prepare(db, utf8, utf8.Length, out stmt, out IntPtr ptrRemain);
                remaining = ptrRemain.FromUtf8();
                return err;
            }
        }
        public static int sqlite3_prepare16(IntPtr db, string pSql, out IntPtr stmt, out string remaining) => sqlite3_prepare(db, pSql, out stmt, out remaining);

        [DllImport(SQLITE3)] private static extern int sqlite3_prepare_v2(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
        public static int sqlite3_prepare_v2(IntPtr db, string pSql, out IntPtr stmt, out string remaining)
        {
            using (var utf8 = new Utf8String(pSql))
            {
                var err = sqlite3_prepare_v2(db, utf8, utf8.Length, out stmt, out IntPtr ptrRemain);
                remaining = ptrRemain.FromUtf8();
                return err;
            }
        }
        public static int sqlite3_prepare16_v2(IntPtr db, string pSql, out IntPtr stmt, out string remaining) => sqlite3_prepare_v2(db, pSql, out stmt, out remaining);

        [DllImport(SQLITE3)] private static extern int sqlite3_prepare_v3(IntPtr db, IntPtr pSql, int nBytes, uint prepFlags, out IntPtr stmt, out IntPtr ptrRemain);
        public static int sqlite3_prepare_v3(IntPtr db, string pSql, uint prepFlags, out IntPtr stmt, out string remaining)
        {
            using (var utf8 = new Utf8String(pSql))
            {
                var err = sqlite3_prepare_v3(db, utf8, utf8.Length, prepFlags, out stmt, out IntPtr ptrRemain);
                remaining = ptrRemain.FromUtf8();
                return err;
            }
        }
        public static int sqlite3_prepare16_v3(IntPtr db, string pSql, uint prepFlags, out IntPtr stmt, out string remaining) => sqlite3_prepare_v3(db, pSql, prepFlags, out stmt, out remaining);
#endregion

#region Binding Values To Prepared Statements
        //https://sqlite.org/capi3ref.html#sqlite3_bind_blob
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_null(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);
        [DllImport(SQLITE3)] private static extern int sqlite3_bind_blob(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPArray)] byte[] value, int byteCount, IntPtr pvReserved);
        public static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value, int byteCount) => sqlite3_bind_blob(stmt, index, value, byteCount, SQLITE_TRANSIENT);
        public static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value) => sqlite3_bind_blob(stmt, index, value, value.Length);

        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_bind_text))] private static extern int sqlite3_bind_text(IntPtr stmt, int index, IntPtr value, int nlen, IntPtr pvReserved);
        public static int sqlite3_bind_text(IntPtr stmt, int index, string value)
        {
            using (var utf8 = new Utf8String(value))
                return sqlite3_bind_text(stmt, index, utf8, utf8.Length, SQLITE_TRANSIENT);
        }
        public static int sqlite3_bind_text16(IntPtr stmt, int index, string value) => sqlite3_bind_text(stmt, index, value);
#endregion

#region Bound Parameter Information
        //http://www.sqlite.org/c3ref/bind_parameter_count.html
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_parameter_count(IntPtr stmt);
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_parameter_index(IntPtr stmt, [MarshalAs(UnmanagedType.LPStr)] string strName);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_bind_parameter_name))] private static extern IntPtr bind_parameter_name(IntPtr stmt, int index);
        public static string sqlite3_bind_parameter_name(IntPtr stmt, int index)
            => bind_parameter_name(stmt, index) is IntPtr ptr && ptr != IntPtr.Zero ? Marshal.PtrToStringAnsi(ptr) : null;
#endregion


        [DllImport(SQLITE3)] public static extern int sqlite3_step(IntPtr stmt);
        [DllImport(SQLITE3)] public static extern int sqlite3_reset(IntPtr stmt);
        [DllImport(SQLITE3)] public static extern int sqlite3_finalize(IntPtr stmt);
        [DllImport(SQLITE3)] public static extern int sqlite3_column_count(IntPtr stmt);


#region Column Names In A Result Set
        //https://sqlite.org/c3ref/column_name.html
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_column_name16))] private static extern IntPtr column_name16(IntPtr stmt, int index);
        public static string sqlite3_column_name(IntPtr stmt, int index) => column_name16(stmt, index).FromUtf8();
        public static string sqlite3_column_name16(IntPtr stmt, int index) => sqlite3_column_name(stmt, index);
#endregion

#region Determine If An SQL Statement Is Complete
        //https://sqlite.org/c3ref/complete.html
        [DllImport(SQLITE3)] public static extern bool sqlite3_complete([MarshalAs(UnmanagedType.LPStr)]string sql);
        [DllImport(SQLITE3)] public static extern bool sqlite3_complete16([MarshalAs(UnmanagedType.LPWStr)]string sql);
#endregion

#region Result Values From A Query
        //https://sqlite.org/c3ref/column_blob.html
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_column_blob))] private static extern IntPtr column_blob(IntPtr stmt, int index);
        public static byte[] sqlite3_column_blob(IntPtr stmt, int index)
        {
            var bytePtr = column_blob(stmt, index);
            if (bytePtr == IntPtr.Zero) return null;

            var byteCount = sqlite3_column_bytes(stmt, index);
            var rv = new byte[byteCount];
            Marshal.Copy(bytePtr, rv, 0, byteCount);
            return rv;
        }
        [DllImport(SQLITE3)] public static extern double sqlite3_column_double(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern int sqlite3_column_int(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern long sqlite3_column_int64(IntPtr stmt, int index);
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_column_text16))] private static extern IntPtr column_text16(IntPtr stmt, int index);
        public static string sqlite3_column_text(IntPtr stmt, int index) => Marshal.PtrToStringUni(column_text16(stmt, index));
        public static string sqlite3_column_text16(IntPtr stmt, int index) => sqlite3_column_text(stmt, index);
        [DllImport(SQLITE3)] public static extern IntPtr sqlite3_column_value(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern int sqlite3_column_bytes(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern int sqlite3_column_bytes16(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern int sqlite3_column_type(IntPtr stmt, int index);
#endregion
    }
}