using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IMetadata
    {
        string DatabaseName(int index);
        string OriginName(int index);
        string TableName(int index);
    }

    public class Metadata : IMetadata
    {
        private Statement _statement;
        public Metadata(Statement statement) => _statement = statement;

        public string DatabaseName(int index) => sqlite3_column_database_name(_statement, index);
        public string OriginName(int index) => sqlite3_column_origin_name(_statement, index);
        public string TableName(int index) => sqlite3_column_table_name(_statement, index);
    }
}
