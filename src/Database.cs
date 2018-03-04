using Microsoft.Extensions.Logging;
using SqliteNative.Events;
using SqliteNative.Util;
using System;
using System.IO;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IDatabase : IDisposable
    {
        /// <summary>
        /// In general this shouldn't be needed, but is exposed as a convenience to access the
        /// SQLite API directly, especially where a more object-oriented approach hasn't been built.
        /// </summary>
        IntPtr Handle { get; }

        IStatement Prepare(string sql);
        IStatement Prepare(string sql, out string remaining);
        bool Execute(string sql);

        IError Error { get; }
        
        int Changes { get; }
        int TotalChanges { get; }
        long LastInsertedRowId { get; }

        event EventHandler<(int, string, string, long)> OnUpdate;
        event EventHandler<CommitEventArgs> OnCommit;
        event EventHandler OnRollback;
    }

    internal class Database : Disposable, IDatabase
    {
        private readonly ILogger<Database> _dbLogger;
        private readonly ILogger<Statement> _stmtLogger;
        private IntPtr _db;

        public Database(ILogger<Database> dbLogger, ILogger<Statement> stmtLogger)
        {
            _dbLogger = dbLogger;
            _stmtLogger = stmtLogger;
            Error = new ErrorInfo(this);

            _updateHook = new Lazy<Callback<UpdateHook>>(() =>
            {
                var cb = new Callback<UpdateHook>(onUpdate);
                sqlite3_update_hook(this, cb, IntPtr.Zero);
                return cb;
                void onUpdate(IntPtr context, int change, IntPtr dbName, IntPtr tableName, long rowid)
                    => _onUpdate?.Invoke(this, (change, dbName.FromUtf8(), tableName.FromUtf8(), rowid));
            });
            _commitHook = new Lazy<Callback<CommitHook>>(() =>
            {
                var cb = new Callback<CommitHook>(onCommit);
                sqlite3_commit_hook(this, cb, IntPtr.Zero);
                return cb;

                int onCommit(IntPtr context)
                {
                    var args = new CommitEventArgs();
                    _onCommit?.Invoke(this, args);
                    return args.Result;
                }
            });
            _rollbackHook = new Lazy<Callback<RollbackHook>>(() =>
            {
                var cb = new Callback<RollbackHook>(onRollback);
                sqlite3_rollback_hook(this, cb, IntPtr.Zero);
                return cb;

                void onRollback(IntPtr context) => _onRollback?.Invoke(this, EventArgs.Empty);
            });
        }

        public IntPtr Handle => _db;
        public IError Error { get; }

        #region Events
        private event EventHandler<(int, string, string, long)> _onUpdate;
        private readonly Lazy<Callback<UpdateHook>> _updateHook;
        public event EventHandler<(int, string, string, long)> OnUpdate
        {
            add { var v = _updateHook.Value; _onUpdate += value; }
            remove => _onUpdate -= value;
        }

        private event EventHandler _onRollback;
        private readonly Lazy<Callback<RollbackHook>> _rollbackHook;
        public event EventHandler OnRollback
        {
            add { var v = _rollbackHook.Value; _onRollback += value; }
            remove { _onRollback -= value; }
        }

        private event EventHandler<CommitEventArgs> _onCommit;
        private readonly Lazy<Callback<CommitHook>> _commitHook;
        public event EventHandler<CommitEventArgs> OnCommit
        {
            add { var v = _commitHook.Value; _onCommit += value; }
            remove => _onCommit -= value;
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
