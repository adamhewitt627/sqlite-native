using Microsoft.Extensions.Logging;
using SqliteNative.Util;
using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IStatement : IDisposable
    {
        /// <summary>
        /// In general this shouldn't be needed, but is exposed as a convenience to access the
        /// SQLite API directly, especially where a more object-oriented approach hasn't been built.
        /// </summary>
        IntPtr Handle { get; }

        Status Step();
        bool Reset();

        IBindings Bindings { get; }
        IColumns Columns { get; }
    }

    internal class Statement : Disposable, IStatement
    {
        private readonly ILogger<Statement> _logger;
        private IntPtr _stmt;
        private Database _database;

        public Statement(Database database, string sql, out string remaining, ILogger<Statement> logger = null)
        {
            _logger = logger;
            _database = database ?? throw new ArgumentNullException(nameof(database));
            Bindings = new BindingsInfo(this);
            Columns = new ColumnsInfo(this);

            if (!(sqlite3_prepare16_v2(database, sql, out _stmt, out remaining) is SQLITE_OK))
                _logger?.LogError(_database.Error.Message);
        }

        public IntPtr Handle => _stmt;
        public IBindings Bindings { get; }
        public IColumns Columns { get; }

        protected override void Dispose(bool disposing)
        {
            var err = sqlite3_finalize(_stmt);
            if (err != SQLITE_OK && disposing)
                _logger?.LogError(_database.Error.Message);
        }

        public static implicit operator IntPtr(Statement statement) => statement._stmt;

        public bool Reset() => sqlite3_reset(this) is SQLITE_OK;
        public Status Step() => (Status)sqlite3_step(this);

        private class BindingsInfo : IBindings
        {
            private readonly Statement _statement;
            public BindingsInfo(Statement statement) => _statement = statement ?? throw new ArgumentNullException(nameof(statement));

            public bool Clear() => sqlite3_clear_bindings(_statement) is SQLITE_OK;

            public bool SetBlob(int index, byte[] value) => sqlite3_bind_blob(_statement, index, value) is SQLITE_OK;
            public bool SetText(int index, string value) => sqlite3_bind_text(_statement, index, value) is SQLITE_OK;
            public bool SetInt64(int index, long value) => sqlite3_bind_int64(_statement, index, value) is SQLITE_OK;
            public bool SetDouble(int index, double value) => sqlite3_bind_double(_statement, index, value) is SQLITE_OK;
            public bool SetNull(int index) => sqlite3_bind_null(_statement, index) is SQLITE_OK;

            public int Count => sqlite3_bind_parameter_count(_statement);
            public int IndexOf(string name) => sqlite3_bind_parameter_index(_statement, name);
            public string NameOf(int index) => sqlite3_bind_parameter_name(_statement, index);
        }

        private class ColumnsInfo : IColumns
        {
            private readonly Statement _statement;
            public ColumnsInfo(Statement statement) => _statement = statement ?? throw new ArgumentNullException(nameof(statement));

            public int Count => sqlite3_column_count(_statement);
            public int DataCount => sqlite3_data_count(_statement);
            public string NameOf(int index) => sqlite3_column_name(_statement, index);

            public byte[] GetBlob(int index) => sqlite3_column_blob(_statement, index);
            public string GetText(int index) => sqlite3_column_text(_statement, index);
            public int GetInt32(int index) => sqlite3_column_int(_statement, index);
            public long GetInt64(int index) => sqlite3_column_int64(_statement, index);
            public double GetDouble(int index) => sqlite3_column_double(_statement, index);
            public IValue GetValue(int index) => new ColumnValue(sqlite3_column_value(_statement, index));
            public ColumnType GetType(int index) => (ColumnType)sqlite3_column_type(_statement, index);
        }

        private class ColumnValue : IValue
        {
            private readonly IntPtr _valuePtr;
            public ColumnValue(IntPtr intPtr) => _valuePtr = intPtr;

            public ColumnType Type => (ColumnType)sqlite3_value_type(_valuePtr);
            public byte[] AsBlob() => sqlite3_value_blob(_valuePtr);
            public double AsDouble() => sqlite3_value_double(_valuePtr);
            public int AsInt32() => sqlite3_value_int(_valuePtr);
            public long AsInt64() => sqlite3_value_int64(_valuePtr);
            public string AsText() => sqlite3_value_text(_valuePtr);
        }
    }
}
