using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class StatementTests
    {
        [TestMethod]
        public void GetsColumnCount()
        {
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = db.Prepare($"SELECT * FROM t1"))
                Assert.AreEqual(2, stmt.Columns.Count);
        }

        [DataTestMethod]
        [DataRow("foo")]
        [DataRow("bar")]
        [DataRow("flibbetty")]
        public void GetsColumnNames(string expected)
        {
            using (var db = new Sqlite3().OpenTest($"CREATE TABLE t1(id INTEGER PRIMARY KEY, {expected} TEXT)"))
            using (var stmt = db.Prepare($"SELECT {expected} FROM t1"))
                Assert.AreEqual(expected, stmt.Columns.NameOf(0));
        }

        [TestMethod]
        public void NullColumnMetaDataOnExpression()
        {
            using (var db = new Sqlite3().OpenTest($"CREATE TABLE t1(id INTEGER PRIMARY KEY, data INTEGER)"))
            using (var statement = db.Prepare($"SELECT max(data) FROM t1"))
            {
                Assert.IsNull(statement.Metadata.DatabaseName(0));
                Assert.IsNull(statement.Metadata.TableName(0));
                Assert.IsNull(statement.Metadata.OriginName(0));
            }
        }

        [TestMethod]
        public void GetsOriginTableName()
        {
            const string expected = "t1";
            using (var db = new Sqlite3().OpenTest($"CREATE TABLE {expected}(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = db.Prepare($"SELECT data as flibbety FROM {expected} as flibbety"))
                Assert.AreEqual(expected, stmt.Metadata.TableName(0));
        }

        [TestMethod]
        public void GetsOriginColumnName()
        {
            const string expected = "data";
            using (var db = new Sqlite3().OpenTest($"CREATE TABLE t1(id INTEGER PRIMARY KEY, {expected} TEXT)"))
            using (var stmt = db.Prepare($"SELECT {expected} as flibbety FROM t1"))
                Assert.AreEqual(expected, stmt.Metadata.OriginName(0));
        }

        [TestMethod]
        public void GetsOriginalSql()
        {
            const string sql = "INSERT INTO t1(data) VALUES('flibbety')";
            using (var db = new Sqlite3().OpenTest($"CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = db.Prepare(sql))
                Assert.AreEqual(sql, stmt.SQL);
        }
        [TestMethod]
        public void GetsExpandedSql()
        {
            using (var db = new Sqlite3().OpenTest($"CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = db.Prepare("INSERT INTO t1(data) VALUES(?)"))
            {
                Assert.IsTrue(stmt.Bindings.SetText(1, "flibbety"));
                Assert.AreEqual("INSERT INTO t1(data) VALUES('flibbety')", stmt.ExpandedSQL);
            }
        }
    }
}