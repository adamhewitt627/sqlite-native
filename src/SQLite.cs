using Microsoft.Extensions.Logging;
using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface ISQLite
    {
        string Version { get; }
        IDatabase Open(string filename, OpenFlags flags);
        bool IsComplete(string query);
    }

    internal class SQLite : ISQLite
    {
        private readonly ILogger<Database> _databaseLogger;
        private readonly ILogger<Statement> _statementLogger;

        public SQLite(ILogger<Database> databaseLogger, ILogger<Statement> statementLogger)
        {
            _databaseLogger = databaseLogger ?? throw new ArgumentNullException(nameof(databaseLogger));
            _statementLogger = statementLogger ?? throw new ArgumentNullException(nameof(statementLogger));
        }

        string ISQLite.Version => sqlite3_libversion();
        bool ISQLite.IsComplete(string query) => sqlite3_complete(query);
        IDatabase ISQLite.Open(string filename, OpenFlags flags)
        {
            var db = new Database(_databaseLogger, _statementLogger);
            db.Open(filename, flags);
            return db;
        }
    }
}
