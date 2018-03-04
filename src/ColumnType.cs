using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public enum ColumnType
    {
        Integer = SQLITE_INTEGER,
        Float = SQLITE_FLOAT,
        Text = SQLITE_TEXT,
        Blob = SQLITE_BLOB,
        Null = SQLITE_NULL,
    }
}
