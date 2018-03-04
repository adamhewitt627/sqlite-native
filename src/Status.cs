using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public enum Status
    {
        Busy = SQLITE_BUSY,
        Row = SQLITE_ROW,
        Done = SQLITE_DONE,
    }
}
