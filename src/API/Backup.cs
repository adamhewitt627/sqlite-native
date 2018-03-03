using System;
using System.Runtime.InteropServices;
using SqliteNative.Util;
using static System.Runtime.InteropServices.CallingConvention;


namespace SqliteNative
{
    //https://sqlite.org/backup.html
    partial class Sqlite3
    {
        [DllImport(SQLITE3, CallingConvention=Cdecl)] private static extern IntPtr sqlite3_backup_init(IntPtr pDest, IntPtr zDestName, IntPtr pSource, IntPtr zSourceName);
        public static IntPtr sqlite3_backup_init(IntPtr pDest, IntPtr pSource) => sqlite3_backup_init(pDest, "main", pSource, "main");
        public static IntPtr sqlite3_backup_init(IntPtr pDest, string zDestName, IntPtr pSource, string zSourceName)
        {
            using (var destName = new Utf8String(zDestName))
            using (var sourceName = new Utf8String(zSourceName))
                return sqlite3_backup_init(pDest, destName, pSource, sourceName);
        }

        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_backup_step(IntPtr pBackup, int page);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_backup_finish(IntPtr pBackup);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_backup_remaining(IntPtr pBackup);
        [DllImport(SQLITE3, CallingConvention=Cdecl)] public static extern int sqlite3_backup_pagecount(IntPtr pBackup);
    }
}