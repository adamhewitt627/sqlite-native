using System;
using System.Diagnostics;

namespace SqliteNative
{
    public static partial class Sqlite3
    {
        public const string SQLITE3 = "sqlite3";    //DllImport name
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
    }
}