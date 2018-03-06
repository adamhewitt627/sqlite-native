using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IColumns
    {
        int Count { get; }
        int DataCount { get; }
        string NameOf(int index);

        byte[] GetBlob(int index);
        string GetText(int index);
        long GetInt64(int index);
        int GetInt32(int index);
        double GetDouble(int index);
        IValue GetValue(int index);
        ColumnType GetType(int index);
    }

    public class Columns : IColumns
    {
        private readonly Statement _statement;
        public Columns(Statement statement) => _statement = statement ?? throw new ArgumentNullException(nameof(statement));

        public int Count => sqlite3_column_count(_statement);
        public int DataCount => sqlite3_data_count(_statement);
        public string NameOf(int index) => sqlite3_column_name(_statement, index);

        public byte[] GetBlob(int index) => sqlite3_column_blob(_statement, index);
        public string GetText(int index) => sqlite3_column_text(_statement, index);
        public int GetInt32(int index) => sqlite3_column_int(_statement, index);
        public long GetInt64(int index) => sqlite3_column_int64(_statement, index);
        public double GetDouble(int index) => sqlite3_column_double(_statement, index);
        IValue IColumns.GetValue(int index) => GetValue(index);
        public Value GetValue(int index) => new Value(sqlite3_column_value(_statement, index));
        public ColumnType GetType(int index) => (ColumnType)sqlite3_column_type(_statement, index);
    }
}
