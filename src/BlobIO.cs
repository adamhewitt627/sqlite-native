using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using SqliteNative.Util;
using static SqliteNative.Sqlite3;
using static System.Runtime.InteropServices.CallingConvention;

namespace SqliteNative
{
    partial class Sqlite3
    {
        //https://sqlite.org/c3ref/blob_open.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] private static extern int sqlite3_blob_open(IntPtr db, IntPtr zDb, IntPtr zTable, IntPtr zColumn, long iRow, int flags, out IntPtr ppBlob);
        public static int sqlite3_blob_open(IntPtr db, string zDb, string zTable, string zColumn, long iRow, int flags, out IntPtr ppBlob)
        {
            using (var strDb = new Utf8String(zDb))
            using (var strTable = new Utf8String(zTable))
            using (var strColumn = new Utf8String(zColumn))
                return sqlite3_blob_open(db, strDb, strTable, strColumn, iRow, flags, out ppBlob);
        }

        
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_blob_bytes(IntPtr ppBlob);                //https://sqlite.org/c3ref/blob_bytes.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_blob_close(IntPtr ppBlob);                //https://sqlite.org/c3ref/blob_close.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_blob_reopen(IntPtr ppBlob, long iRow);    //https://sqlite.org/c3ref/blob_reopen.html
        
        //https://sqlite.org/c3ref/blob_read.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] internal unsafe static extern int sqlite3_blob_read(IntPtr ppBlob, byte* buffer, int N, int iOffset);
        public unsafe static int sqlite3_blob_read(IntPtr ppBlob, byte[] buffer, int N, int iOffset)
        {
            fixed(byte* b = buffer) return sqlite3_blob_read(ppBlob, b, N, iOffset);
        }

        //https://sqlite.org/c3ref/blob_write.html
        [DllImport(SQLITE3, CallingConvention=Cdecl)] internal unsafe static extern int sqlite3_blob_write(IntPtr ppBlob, byte* buffer, int N, int iOffset);
        public unsafe static int sqlite3_blob_write(IntPtr ppBlob, byte[] buffer, int N, int iOffset)
        {
            fixed(byte* b = buffer) return sqlite3_blob_write(ppBlob, b, N, iOffset);
        }
    }

    public class Blob : Stream
    {
        private readonly bool _writable;
        private readonly IntPtr _ppBlob;

        public Blob(IntPtr db, string zDb, string zTable, string zColumn, long iRow, int flags = 0)
        {
            var err = sqlite3_blob_open(db, zDb, zTable, zColumn, iRow, flags, out _ppBlob);
            if (err == SQLITE_OK) Length = sqlite3_blob_bytes(_ppBlob);
            
            _writable = flags != 0 && Length > 0;
        }
        protected override void Dispose(bool disposing) => sqlite3_blob_close(_ppBlob);

        public unsafe override int Read(byte[] buffer, int offset, int count)
        {
            if (Position + count > Length) count = (int)(Length - Position);
            if (count <= 0) return 0;

            fixed (byte* b = buffer)
            {
                /* Explanation:
                    1. sqlite3_blob_read takes in 
                        - A byte* (where to copy blob to/from) - ***but always at offset=0 in the buffer***
                        - How many bytes to copy
                        - Offset in the blob itself to read/write (not offset in the buffer)
                    
                    2. Rather than a temporary buffer, we can use ***b + offset*** to simulate the buffer offset for the Stream API
                 */

                var read = sqlite3_blob_read(_ppBlob, b + offset, count, (int)Position) == SQLITE_OK ? count : 0;
                Position += read;
                return read;
            }
        }
        public unsafe override void Write(byte[] buffer, int offset, int count)
        {
            if (Position + count > Length) count = (int)(Length - Position);
            if (count <= 0) return;

            fixed (byte* b = buffer)
            {
                //See Explanation in Read
                if (sqlite3_blob_write(_ppBlob, b + offset, count, (int)Position) == SQLITE_OK)
                    Position += count;
            }
        }

        public override long Length { get; }
        public override long Position { get; set; }
        public override bool CanRead => Position <= Length;
        public override bool CanSeek => true;
        public override bool CanWrite => _writable && Position < Length;

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin: Position = offset; break;
                case SeekOrigin.Current: Position += offset; break;
                case SeekOrigin.End: Position = Length - offset; break;
            }
            return Position;
        }

        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
    }
}