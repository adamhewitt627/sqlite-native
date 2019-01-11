using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IError
    {
        string Message { get; }
        int Code { get; }
        int ExtendedCode { get; }
    }

    public class Error : IError
    {
        private Database _database;
        public Error(Database database) => _database = database ?? throw new ArgumentNullException(nameof(database));
        public string Message => sqlite3_errmsg(_database);
        public int Code => sqlite3_errcode(_database);
        public int ExtendedCode => sqlite3_extended_errcode(_database);
    }
}
