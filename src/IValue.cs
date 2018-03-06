using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IValue
    {
        ColumnType Type { get; }
        byte[] AsBlob();
        int AsInt32();
        long AsInt64();
        double AsDouble();
        string AsText();
    }

    public class Value : IValue
    {
        private readonly IntPtr _valuePtr;
        public Value(IntPtr intPtr) => _valuePtr = intPtr;

        public ColumnType Type => (ColumnType)sqlite3_value_type(_valuePtr);
        public byte[] AsBlob() => sqlite3_value_blob(_valuePtr);
        public double AsDouble() => sqlite3_value_double(_valuePtr);
        public int AsInt32() => sqlite3_value_int(_valuePtr);
        public long AsInt64() => sqlite3_value_int64(_valuePtr);
        public string AsText() => sqlite3_value_text(_valuePtr);
    }
}
