using System;
using System.Runtime.InteropServices;
using SqliteNative.Util;

namespace SqliteNative
{
    public static partial class Sqlite3
    {
        [DllImport(SQLITE3, EntryPoint = nameof(sqlite3_libversion))] private static extern IntPtr libversion();
        public static string sqlite3_libversion() => libversion().FromUtf8();
    }
}