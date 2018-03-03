using Microsoft.Extensions.Logging;
using SqliteNative.Util;
using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public enum OpenFlags
    {
        ReadOnly = SQLITE_OPEN_READONLY,
        ReadWrite = SQLITE_OPEN_READWRITE,
        Create = SQLITE_OPEN_CREATE,
        Uri = SQLITE_OPEN_URI,
        Memory = SQLITE_OPEN_MEMORY,
        NoMutex = SQLITE_OPEN_NOMUTEX,
        FullMutex = SQLITE_OPEN_FULLMUTEX,
        SharedCache = SQLITE_OPEN_SHAREDCACHE,
        PrivateCache = SQLITE_OPEN_PRIVATECACHE,
    }

    internal class Database : Disposable
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<Database> _logger;
        private IntPtr _db;

        public Database(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<Database>();
        }

        protected override void Dispose(bool disposing) => Close();
        public static implicit operator IntPtr(Database database) => database._db;

        public bool Open(string filename, OpenFlags flags)
        {
            var err = sqlite3_open_v2(filename, out _db, (int)flags);
            if (err != SQLITE_OK) _logger.LogError(ErrorMessage);
            return err == SQLITE_OK;
        }

        public bool Close()
        {
            var err = sqlite3_close_v2(_db);
            if (err != SQLITE_OK) _logger.LogError(ErrorMessage);
            return err == SQLITE_OK;
        }

        public Statement Prepare(string sql) => Prepare(sql, out var remaining);
        public Statement Prepare(string sql, out string remaining) => new Statement(_loggerFactory, this, sql, out remaining);
        public bool Execute(string sql)
        {
            var err = sqlite3_exec(this, sql);
            if (err != SQLITE_OK) _logger.LogError(ErrorMessage);
            return err == SQLITE_OK;
        }

        public string ErrorMessage => sqlite3_errmsg(this);
        public string ErrorString => sqlite3_errstr(this);
        public int ErrorCode => sqlite3_errcode(this);
        public int ExtendedErrorCode => sqlite3_extended_errcode(this);

        public int Changes => sqlite3_changes(this);
        public int TotalChanges => sqlite3_total_changes(this);
        public long LastInsertedRowId => sqlite3_last_insert_rowid(this);
    }
}
