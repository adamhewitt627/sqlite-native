using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteNative
{
    public static partial class Sqlite3
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


        [DllImport(SQLITE3)] private static extern int sqlite3_prepare_v2(IntPtr db, IntPtr pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
        public static int sqlite3_prepare16_v2(IntPtr db, string pSql, out IntPtr stmt, out string remaining)
        {
            IntPtr utf8 = IntPtr.Zero;
            try
            {
                utf8 = pSql.ToUtf8(out var nBytes);
                var err = sqlite3_prepare_v2(db, utf8, nBytes, out stmt, out IntPtr ptrRemain);
                remaining = ptrRemain.FromUtf8();
                return err;
            } finally { if (utf8 != IntPtr.Zero) Marshal.FreeHGlobal(utf8); }
        }

        [DllImport(SQLITE3)] public static extern int sqlite3_bind_null(IntPtr stmt, int index);
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_int64(IntPtr stmt, int index, long value);
        [DllImport(SQLITE3)] public static extern int sqlite3_bind_double(IntPtr stmt, int index, double value);
        [DllImport(SQLITE3)] private static extern int sqlite3_bind_blob(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPArray)] byte[] value, int byteCount, IntPtr pvReserved);
        public static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value, int byteCount) => sqlite3_bind_blob(stmt, index, value, byteCount, SQLITE_TRANSIENT);
        public static int sqlite3_bind_blob(IntPtr stmt, int index, byte[] value) => sqlite3_bind_blob(stmt, index, value, value.Length);

        [DllImport(SQLITE3)] private static extern int sqlite3_bind_text16(IntPtr stmt, int index, [MarshalAs(UnmanagedType.LPWStr)] string value, int nlen, IntPtr pvReserved);
        public static int sqlite3_bind_text16(IntPtr stmt, int index, string value) => sqlite3_bind_text16(stmt, index, value, -1, SQLITE_TRANSIENT);

#region Bound Parameter Information
        //https://sqlite.org/capi3ref.html#sqlite3_bind_parameter_count
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
        //https://sqlite.org/capi3ref.html#sqlite3_column_name
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_column_name16))] private static extern IntPtr column_name16(IntPtr stmt, int index);
        public static string sqlite3_column_name(IntPtr stmt, int index) => Marshal.PtrToStringUni(column_name16(stmt, index));
        public static string sqlite3_column_name16(IntPtr stmt, int index) => sqlite3_column_name16(stmt, index);
#endregion

#region Determine If An SQL Statement Is Complete
        //https://sqlite.org/capi3ref.html#sqlite3_complete
        [DllImport(SQLITE3)] public static extern bool sqlite3_complete([MarshalAs(UnmanagedType.LPStr)]string sql);
        [DllImport(SQLITE3)] public static extern bool sqlite3_complete16([MarshalAs(UnmanagedType.LPWStr)]string sql);
#endregion

#region Result Values From A Query
        //https://sqlite.org/capi3ref.html#sqlite3_column_blob
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