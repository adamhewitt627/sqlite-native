using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests.Util;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class StatementTests
    {
        [TestMethod]
        public void GetsColumnCount()
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = new Statement(db, $"SELECT * FROM t1", out var remain))
                Assert.AreEqual(2, sqlite3_column_count(stmt));
        }

        [DataTestMethod]
        [DataRow("foo")]
        [DataRow("bar")]
        [DataRow("flibbetty")]
        public void GetsColumnNames(string expected)
        {
            using (var db = new Database($"CREATE TABLE t1(id INTEGER PRIMARY KEY, {expected} TEXT)"))
            using (var stmt = new Statement(db, $"SELECT {expected} FROM t1", out var remain))
            {
                Assert.AreEqual(expected, sqlite3_column_name(stmt, 0));
                Assert.AreEqual(expected, sqlite3_column_name16(stmt, 0));
            }
        }

        [TestMethod]
        public void NullColumnMetaDataOnExpression()
        {
            using (var db = new Database($"CREATE TABLE t1(id INTEGER PRIMARY KEY, data INTEGER)"))
            using (var stmt = new Statement(db, $"SELECT max(data) FROM t1", out var remain))
            {
                Assert.IsNull(sqlite3_column_database_name(stmt, 0));
                Assert.IsNull(sqlite3_column_database_name16(stmt, 0));
                Assert.IsNull(sqlite3_column_table_name(stmt, 0));
                Assert.IsNull(sqlite3_column_table_name16(stmt, 0));
                Assert.IsNull(sqlite3_column_origin_name(stmt, 0));
                Assert.IsNull(sqlite3_column_origin_name16(stmt, 0));
            }
        }

        [TestMethod]
        public void GetsOriginTableName()
        {
            const string expected = "t1";
            using (var db = new Database($"CREATE TABLE {expected}(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = new Statement(db, $"SELECT data as flibbety FROM {expected} as flibbety", out var remain))
            {
                Assert.AreEqual(expected, sqlite3_column_table_name(stmt, 0));
                Assert.AreEqual(expected, sqlite3_column_table_name16(stmt, 0));
            }
        }

        [TestMethod]
        public void GetsOriginColumnName()
        {
            const string expected = "data";
            using (var db = new Database($"CREATE TABLE t1(id INTEGER PRIMARY KEY, {expected} TEXT)"))
            using (var stmt = new Statement(db, $"SELECT {expected} as flibbety FROM t1", out var remain))
            {
                Assert.AreEqual(expected, sqlite3_column_origin_name(stmt, 0));
                Assert.AreEqual(expected, sqlite3_column_origin_name16(stmt, 0));
            }
        }
    }
}