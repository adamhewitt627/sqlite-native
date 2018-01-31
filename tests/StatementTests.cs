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
    }
}