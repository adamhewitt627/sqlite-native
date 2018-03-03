using Microsoft.Extensions.Logging;
using SqliteNative.Util;
using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    internal class Statement : Disposable
    {
        private readonly ILogger<Statement> _logger;
        private IntPtr _stmt;
        private Database _database;

        public Statement(ILoggerFactory loggerFactory, Database database, string sql, out string remaining)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<Statement>();

            _database = database ?? throw new ArgumentNullException(nameof(database));
            if (!(sqlite3_prepare16_v2(database, sql, out _stmt, out remaining) is SQLITE_OK))
                _logger.LogError(_database.ErrorMessage);
        }

        protected override void Dispose(bool disposing)
        {
            var err = sqlite3_finalize(_stmt);
            if (err != SQLITE_OK && disposing)
                _logger.LogError(_database.ErrorMessage);
        }

        public static implicit operator IntPtr(Statement statement) => statement._stmt;
    }
}
