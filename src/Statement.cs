using Microsoft.Extensions.Logging;
using SqliteNative.Util;
using System;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IStatement : IDisposable
    {
        Status Step();
        bool Reset();

        IBindings Bindings { get; }
        IColumns Columns { get; }
        IMetadata Metadata { get; }

        string SQL { get; }
        string ExpandedSQL { get; }
    }

    public class Statement : Disposable, IStatement
    {
        private readonly ILogger<Statement> _logger;
        private IntPtr _stmt;
        private Database _database;

        public Statement(Database database, string sql, out string remaining, ILogger<Statement> logger = null)
        {
            _logger = logger;
            _database = database ?? throw new ArgumentNullException(nameof(database));
            Bindings = new Bindings(this);
            Columns = new Columns(this);
            Metadata = new Metadata(this);

            if (!(sqlite3_prepare16_v2(database, sql, out _stmt, out remaining) is SQLITE_OK))
                _logger?.LogError(_database.Error.Message);
        }

        IBindings IStatement.Bindings => Bindings;
        IColumns IStatement.Columns => Columns;
        IMetadata IStatement.Metadata => Metadata;

        public Bindings Bindings { get; }
        public Columns Columns { get; }
        public Metadata Metadata { get; }
        public string SQL => sqlite3_sql(this);
        public string ExpandedSQL => sqlite3_expanded_sql(this);


        protected override void Dispose(bool disposing)
        {
            var err = sqlite3_finalize(_stmt);
            if (err != SQLITE_OK && disposing)
                _logger?.LogError(_database.Error.Message);
        }

        public static implicit operator IntPtr(Statement statement) => statement._stmt;

        public bool Reset() => sqlite3_reset(this) is SQLITE_OK;
        public Status Step() => (Status)sqlite3_step(this);
    }
}
