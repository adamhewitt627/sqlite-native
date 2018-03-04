using Microsoft.Extensions.Logging;
using SqliteNative.Util;
using System;
using System.IO;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IDatabase : IDisposable
    {
        IStatement Prepare(string sql);
        IStatement Prepare(string sql, out string remaining);
        bool Execute(string sql);

        IError Error { get; }
        
        int Changes { get; }
        int TotalChanges { get; }
        long LastInsertedRowId { get; }
    }

    internal class Database : Disposable, IDatabase
    {
        private readonly ILogger<Database> _dbLogger;
        private readonly ILogger<Statement> _stmtLogger;
        private IntPtr _db;

        public Database(ILogger<Database> dbLogger, ILogger<Statement> stmtLogger)
        {
            _dbLogger = dbLogger ?? throw new ArgumentNullException(nameof(dbLogger));
            _stmtLogger = stmtLogger ?? throw new ArgumentNullException(nameof(stmtLogger));
            Error = new ErrorInfo(this);
        }

        public IError Error { get; }

        protected override void Dispose(bool disposing) => Close();
        public static implicit operator IntPtr(Database database) => database._db;

        public bool Open(string filename, OpenFlags flags)
        {
            var err = sqlite3_open_v2(filename, out _db, (int)flags);
            if (err != SQLITE_OK) _dbLogger.LogError(Error.Message);
            return err == SQLITE_OK;
        }

        public bool Close()
        {
            var err = sqlite3_close_v2(_db);
            if (err != SQLITE_OK) _dbLogger.LogError(Error.Message);
            return err == SQLITE_OK;
        }

        public IStatement Prepare(string sql) => Prepare(sql, out var remaining);
        public IStatement Prepare(string sql, out string remaining) => new Statement(_stmtLogger, this, sql, out remaining);
        public bool Execute(string sql)
        {
            var err = sqlite3_exec(this, sql);
            if (err != SQLITE_OK) _dbLogger.LogError(Error.Message);
            return err == SQLITE_OK;
        }


        public int Changes => sqlite3_changes(this);
        public int TotalChanges => sqlite3_total_changes(this);
        public long LastInsertedRowId => sqlite3_last_insert_rowid(this);


        private class ErrorInfo : IError
        {
            private Database _database;
            public ErrorInfo(Database database) => _database = database ?? throw new ArgumentNullException(nameof(database));
            public string Message => sqlite3_errmsg(_database);
            public string String => sqlite3_errstr(_database);
            public int Code => sqlite3_errcode(_database);
            public int ExtendedCode => sqlite3_extended_errcode(_database);
        }
    }
}
