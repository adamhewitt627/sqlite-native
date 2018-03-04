using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public enum Status
    {
        Row = SQLITE_ROW,
        Done = SQLITE_DONE,
    }
}
