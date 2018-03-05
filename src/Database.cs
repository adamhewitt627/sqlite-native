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
        Stream OpenBlob(string dbName, string tableName, string columnName, long rowid, int flags = 0);
        
        int Changes { get; }
        int TotalChanges { get; }
        long LastInsertedRowId { get; }

        Func<int, int> BusyHandler { set; }
        Action<int, string, string, long> UpdateHandler { set; }
        Action RollbackHandler { set; }
        Func<int> CommitHandler { set; }
        Func<IDatabase, string, int, int> WalHandler { set; }
    }

    public class Database : Disposable, IDatabase
    {
        private readonly object _lock = new object();
        private readonly ILogger<Database> _dbLogger;
        private readonly ILogger<Statement> _stmtLogger;
        private IntPtr _db;

        public Database(ILogger<Database> dbLogger, ILogger<Statement> stmtLogger)
        {
            _dbLogger = dbLogger;
            _stmtLogger = stmtLogger;
            Error = new ErrorInfo(this);
        }

        public IError Error { get; }

        #region Events
        private Callback<UpdateHook> _updateHook;
        public Action<int, string, string, long> UpdateHandler
        {
            set
            {
                sqlite3_update_hook(this, _updateHook = new Callback<UpdateHook>(onUpdate), IntPtr.Zero);
                void onUpdate(IntPtr context, int change, IntPtr dbName, IntPtr tableName, long rowid)
                    => value(change, dbName.FromUtf8(), tableName.FromUtf8(), rowid);
            }
        }

        private Callback<RollbackHook> _rollbackHook;
        public Action RollbackHandler
        {
            set
            {
                sqlite3_rollback_hook(this, _rollbackHook = new Callback<RollbackHook>(handler), IntPtr.Zero);
                void handler(IntPtr context) => value();
            }
        }

        private Callback<CommitHook> _commitHook;
        public Func<int> CommitHandler
        {
            set
            {
                sqlite3_commit_hook(this, _commitHook = new Callback<CommitHook>(onCommit), IntPtr.Zero);
                int onCommit(IntPtr context) => value();
            }
        }

        private Callback<BusyHandler> _busyHook;
        public Func<int, int> BusyHandler
        {
            set
            {
                sqlite3_busy_handler(this, _busyHook = new Callback<BusyHandler>(onBusy), IntPtr.Zero);
                int onBusy(IntPtr context, int lockCount) => value(lockCount);
            }
        }

        private Callback<WriteAheadLogHook> _walHook;
        public Func<IDatabase, string, int, int> WalHandler
        {
            set
            {
                sqlite3_wal_hook(this, _walHook = new Callback<WriteAheadLogHook>(onWalHook), IntPtr.Zero);
                int onWalHook(IntPtr context, IntPtr db, IntPtr dbName, int pageCount)
                    => value(this, dbName.FromUtf8(), pageCount);
            }
        }
        #endregion

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

        public IStatement Prepare(string sql) => Prepare(sql, out var remaining);
        public IStatement Prepare(string sql, out string remaining) => new Statement(this, sql, out remaining, _stmtLogger);
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
