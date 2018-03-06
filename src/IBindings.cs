using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IBindings
    {
        bool Clear();
        bool SetInt64(int index, long value);
        bool SetDouble(int index, double value);
        bool SetText(int index, string value);
        bool SetBlob(int index, byte[] value);
        bool SetBlob(int index, int length);
        bool SetNull(int index);

        int Count { get; }
        int IndexOf(string name);
        string NameOf(int index);
    }

    public class Bindings : IBindings
    {
        private readonly Statement _statement;
        public Bindings(Statement statement) => _statement = statement ?? throw new ArgumentNullException(nameof(statement));

        public bool Clear() => sqlite3_clear_bindings(_statement) is SQLITE_OK;

        public bool SetInt64(int index, long value) => sqlite3_bind_int64(_statement, index, value) is SQLITE_OK;
        public bool SetDouble(int index, double value) => sqlite3_bind_double(_statement, index, value) is SQLITE_OK;
        public bool SetText(int index, string value) => sqlite3_bind_text(_statement, index, value) is SQLITE_OK;
        public bool SetBlob(int index, byte[] value) => sqlite3_bind_blob(_statement, index, value) is SQLITE_OK;
        public bool SetBlob(int index, int length) => sqlite3_bind_zeroblob(_statement, index, length) is SQLITE_OK;
        public bool SetNull(int index) => sqlite3_bind_null(_statement, index) is SQLITE_OK;

        public int Count => sqlite3_bind_parameter_count(_statement);
        public int IndexOf(string name) => sqlite3_bind_parameter_index(_statement, name);
        public string NameOf(int index) => sqlite3_bind_parameter_name(_statement, index);
    }
}
