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
        Stream OpenBlob(string dbName, string tableName, string columnName, long rowid, int flags = 0);

        IError Error { get; }
        IHooks Hooks { get; }
        
        int Changes { get; }
        int TotalChanges { get; }
        long LastInsertedRowId { get; }
    }

    public class Database : Disposable, IDatabase
    {
        private readonly object _lock = new object();
        private readonly ILogger<Database> _dbLogger;
        private readonly ILogger<Statement> _stmtLogger;
        private IntPtr _db;

        public Database(ILogger<Database> dbLogger = null, ILogger<Statement> stmtLogger = null)
        {
            _dbLogger = dbLogger;
            _stmtLogger = stmtLogger;
            Error = new Error(this);
            Hooks = new Hooks(this);
        }

        IError IDatabase.Error => Error;
        public Error Error { get; }
        IHooks IDatabase.Hooks => Hooks;
        public Hooks Hooks { get; }

        protected override void Dispose(bool disposing) => Close();
        public static implicit operator IntPtr(Database database) => database._db;

        public bool Open(string filename, OpenFlags flags)
        {
            var err = sqlite3_open_v2(filename, out _db, (int)flags);
            if (err != SQLITE_OK) _dbLogger?.LogError(Error.Message);
            return err == SQLITE_OK;
        }

        public bool Close()
        {
            var err = sqlite3_close_v2(_db);
            if (err != SQLITE_OK) _dbLogger?.LogError(Error.Message);
            return err == SQLITE_OK;
        }

        IStatement IDatabase.Prepare(string sql) => Prepare(sql);
        public Statement Prepare(string sql) => Prepare(sql, out var remaining);
        IStatement IDatabase.Prepare(string sql, out string remaining) => Prepare(sql, out remaining);
        public Statement Prepare(string sql, out string remaining) => new Statement(this, sql, out remaining, _stmtLogger);

        public bool Execute(string sql)
        {
            var err = sqlite3_exec(this, sql);
            if (err != SQLITE_OK) _dbLogger?.LogError(Error.Message);
            return err == SQLITE_OK;
        }

        public int Changes => sqlite3_changes(this);
        public int TotalChanges => sqlite3_total_changes(this);
        public long LastInsertedRowId => sqlite3_last_insert_rowid(this);

        public Stream OpenBlob(string dbName, string tableName, string columnName, long rowid, int flags = 0)
            => new Blob(this, dbName, tableName, columnName, rowid, flags);

    }
}
