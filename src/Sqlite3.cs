using Microsoft.Extensions.Logging;

namespace SqliteNative
{
    public interface ISQLite
    {
        string Version { get; }
        IDatabase Open(string filename, OpenFlags flags);
        bool IsComplete(string query);
    }

    public partial class Sqlite3 : ISQLite
    {
        private readonly ILogger<Database> _databaseLogger;
        private readonly ILogger<Statement> _statementLogger;

        public Sqlite3(ILoggerFactory loggerFactory = null)
        {
            _databaseLogger = loggerFactory?.CreateLogger<Database>();
            _statementLogger = loggerFactory?.CreateLogger<Statement>();
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
