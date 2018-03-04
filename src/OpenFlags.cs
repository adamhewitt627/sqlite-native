using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public enum OpenFlags
    {
        ReadOnly = SQLITE_OPEN_READONLY,
        ReadWrite = SQLITE_OPEN_READWRITE,
        Create = SQLITE_OPEN_CREATE,
        Uri = SQLITE_OPEN_URI,
        Memory = SQLITE_OPEN_MEMORY,
        NoMutex = SQLITE_OPEN_NOMUTEX,
        FullMutex = SQLITE_OPEN_FULLMUTEX,
        SharedCache = SQLITE_OPEN_SHAREDCACHE,
        PrivateCache = SQLITE_OPEN_PRIVATECACHE,
    }
}
