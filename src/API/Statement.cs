using SqliteNative.Util;
using System;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.CallingConvention;

namespace SqliteNative
{
    partial class Sqlite3
    {
#region Compiling An SQL Statement
        //https://sqlite.org/c3ref/prepare.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] private static extern int sqlite3_prepare(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
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

        [DllImport(SQLITE3, CallingConvention=Cdecl)] private static extern int sqlite3_prepare_v2(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
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

        [DllImport(SQLITE3, CallingConvention=Cdecl)] private static extern int sqlite3_prepare_v3(IntPtr db, IntPtr pSql, int nBytes, uint prepFlags, out IntPtr stmt, out IntPtr ptrRemain);
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

#region Statement lifecycle
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_step(IntPtr stmt);                //https://sqlite.org/c3ref/step.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_reset(IntPtr stmt);               //https://sqlite.org/c3ref/reset.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_clear_bindings(IntPtr stmt);      //https://sqlite.org/c3ref/clear_bindings.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_finalize(IntPtr stmt);            //https://sqlite.org/c3ref/finalize.html
#endregion

#region Binding Values To Prepared Statements
        //https://sqlite.org/c3ref/bind_blob.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] private unsafe static extern int sqlite3_bind_blob(IntPtr stmt, int index, byte* value, int byteCount, IntPtr pvReserved);
        public unsafe static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value, int byteCount)
        {
            fixed (byte* data = value)
                return sqlite3_bind_blob(stmt, index, data, byteCount, SQLITE_TRANSIENT);
        }
        public static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value) => sqlite3_bind_blob(stmt, index, value, value.Length);

        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_bind_null(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);

        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_bind_text))] private static extern int sqlite3_bind_text(IntPtr stmt, int index, IntPtr value, int nlen, IntPtr pvReserved);
        public static int sqlite3_bind_text(IntPtr stmt, int index, string value)
        {
            using (var utf8 = new Utf8String(value))
                return sqlite3_bind_text(stmt, index, utf8, utf8.Length - 1/*we don't include the 0-terminator here*/, SQLITE_TRANSIENT);
        }
        public static int sqlite3_bind_text16(IntPtr stmt, int index, string value) => sqlite3_bind_text(stmt, index, value);

        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_bind_zeroblob(IntPtr stmt, int index, int nBytes);
#endregion

#region Bound Parameter Information
        //http://www.sqlite.org/c3ref/bind_parameter_count.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_bind_parameter_count(IntPtr stmt);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_bind_parameter_index(IntPtr stmt, IntPtr strName);
        public static int sqlite3_bind_parameter_index(IntPtr stmt, string strName)
        {
            using (var utf8 = new Utf8String(strName))
                return sqlite3_bind_parameter_index(stmt, utf8);
        }
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_bind_parameter_name))] private static extern IntPtr bind_parameter_name(IntPtr stmt, int index);
        public static string sqlite3_bind_parameter_name(IntPtr stmt, int index) => bind_parameter_name(stmt, index).FromUtf8();
#endregion

#region Number Of Columns In A Result Set
        //https://sqlite.org/c3ref/column_count.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_column_count(IntPtr stmt);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_data_count(IntPtr stmt);
#endregion

#region Retrieving Statement SQL
        //https://sqlite.org/c3ref/expanded_sql.html
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_sql))] private static extern IntPtr sql(IntPtr stmt);
        public static string sqlite3_sql(IntPtr stmt) => sql(stmt).FromUtf8();
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_expanded_sql))] private static extern IntPtr expanded_sql(IntPtr stmt);
        public static string sqlite3_expanded_sql(IntPtr stmt) => expanded_sql(stmt).FromUtf8();
#endregion

#region Column Names In A Result Set
        //https://sqlite.org/c3ref/column_name.html
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_column_name))] private static extern IntPtr column_name(IntPtr stmt, int index);
        public static string sqlite3_column_name(IntPtr stmt, int index) => column_name(stmt, index).FromUtf8();
        public static string sqlite3_column_name16(IntPtr stmt, int index) => sqlite3_column_name(stmt, index);
#endregion

#region Source Of Data In A Query Result
        //https://sqlite.org/c3ref/column_database_name.html
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_column_database_name))] private static extern IntPtr column_database_name(IntPtr stmt, int index);
        public static string sqlite3_column_database_name(IntPtr stmt, int index) => column_database_name(stmt, index).FromUtf8();
        public static string sqlite3_column_database_name16(IntPtr stmt, int index) => sqlite3_column_database_name(stmt, index);
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_column_table_name))] private static extern IntPtr column_table_name(IntPtr stmt, int index);
        public static string sqlite3_column_table_name(IntPtr stmt, int index) => column_table_name(stmt, index).FromUtf8();
        public static string sqlite3_column_table_name16(IntPtr stmt, int index) => sqlite3_column_table_name(stmt, index);
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_column_origin_name))] private static extern IntPtr column_origin_name(IntPtr stmt, int index);
        public static string sqlite3_column_origin_name(IntPtr stmt, int index) => column_origin_name(stmt, index).FromUtf8();
        public static string sqlite3_column_origin_name16(IntPtr stmt, int index) => sqlite3_column_origin_name(stmt, index);
#endregion

#region Determine If An SQL Statement Is Complete
        //https://sqlite.org/c3ref/complete.html
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_complete))] private static extern bool sqlite3_complete(IntPtr sql);
        public static bool sqlite3_complete(string sql)
        {
            using (var utf8 = new Utf8String(sql)) 
                return sqlite3_complete(utf8);
        }
        public static bool sqlite3_complete16(string sql) => sqlite3_complete(sql);
#endregion

#region Result Values From A Query
        //https://sqlite.org/c3ref/column_blob.html
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_column_blob))] private static extern IntPtr column_blob(IntPtr stmt, int index);
        public unsafe static byte[] sqlite3_column_blob(IntPtr stmt, int index)
        {
            var source = column_blob(stmt, index);
            if (source == IntPtr.Zero) return null; //SQLite returns NULL for a zero-length blob

            var byteCount = sqlite3_column_bytes(stmt, index);
            var rv = new byte[byteCount];
            fixed(byte* dest = rv) Buffer.MemoryCopy(source.ToPointer(), dest, byteCount, byteCount);
            return rv;
        }
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern double sqlite3_column_double(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_column_int(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern long sqlite3_column_int64(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl, EntryPoint = nameof(sqlite3_column_text))] private static extern IntPtr column_text(IntPtr stmt, int index);
        public static string sqlite3_column_text(IntPtr stmt, int index) => column_text(stmt, index).FromUtf8();
        public static string sqlite3_column_text16(IntPtr stmt, int index) => sqlite3_column_text(stmt, index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern IntPtr sqlite3_column_value(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_column_bytes(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_column_bytes16(IntPtr stmt, int index);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_column_type(IntPtr stmt, int index);
#endregion
    }
}