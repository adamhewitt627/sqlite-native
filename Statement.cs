using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SqliteNative
{
    public static partial class Sqlite3
    {
        [DllImport(SQLITE3, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode, EntryPoint = "sqlite3_prepare16_v2")]
        private static extern int sqlite3_prepare16_v2(IntPtr db, [MarshalAs(UnmanagedType.LPWStr)] string pSql, int nBytes, out IntPtr stmt, out IntPtr ptrRemain);
        public static int sqlite3_prepare16_v2(IntPtr db, [MarshalAs(UnmanagedType.LPWStr)] string pSql, int nBytes, out IntPtr stmt, out string remaining)
        {
            var err = sqlite3_prepare16_v2(db, pSql, nBytes, out stmt, out IntPtr ptrRemain);
            remaining = ptrRemain == IntPtr.Zero ? null : Marshal.PtrToStringUni(ptrRemain);
            return err;
        }

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
    }
}