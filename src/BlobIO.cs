using System;
using System.Runtime.InteropServices;
using SqliteNative.Util;

namespace SqliteNative
{
    partial class Sqlite3
    {
        //https://sqlite.org/c3ref/blob_open.html
        [DllImport(SQLITE3)] private static extern int sqlite3_blob_open(IntPtr db, IntPtr zDb, IntPtr zTable, IntPtr zColumn, long iRow, int flags, out IntPtr ppBlob);
        public static int sqlite3_blob_open(IntPtr db, string zDb, string zTable, string zColumn, long iRow, int flags, out IntPtr ppBlob)
        {
            using (var strDb = new Utf8String(zDb))
            using (var strTable = new Utf8String(zTable))
            using (var strColumn = new Utf8String(zColumn))
                return sqlite3_blob_open(db, strDb, strTable, strColumn, iRow, flags, out ppBlob);
        }

        
        [DllImport(SQLITE3)] public static extern int sqlite3_blob_bytes(IntPtr ppBlob);                //https://sqlite.org/c3ref/blob_bytes.html
        [DllImport(SQLITE3)] public static extern int sqlite3_blob_close(IntPtr ppBlob);                //https://sqlite.org/c3ref/blob_close.html
        [DllImport(SQLITE3)] public static extern int sqlite3_blob_reopen(IntPtr ppBlob, long iRow);    //https://sqlite.org/c3ref/blob_reopen.html
        
        //https://sqlite.org/c3ref/blob_read.html
        [DllImport(SQLITE3)] private unsafe static extern int sqlite3_blob_read(IntPtr ppBlob, byte* buffer, int N, int iOffset);
        public unsafe static int sqlite3_blob_read(IntPtr ppBlob, byte[] buffer, int N, int iOffset)
        {
            fixed(byte* b = buffer) return sqlite3_blob_read(ppBlob, b, N, iOffset);
        }

        //https://sqlite.org/c3ref/blob_write.html
        [DllImport(SQLITE3)] private unsafe static extern int sqlite3_blob_write(IntPtr ppBlob, byte* buffer, int N, int iOffset);
        public unsafe static int sqlite3_blob_write(IntPtr ppBlob, byte[] buffer, int N, int iOffset)
        {
            fixed(byte* b = buffer) return sqlite3_blob_write(ppBlob, b, N, iOffset);
        }
    }
}