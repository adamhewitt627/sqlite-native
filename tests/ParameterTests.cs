using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests.Util;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        public void TestParameterCount()
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            {
                Assert.AreEqual(SQLITE_OK, sqlite3_prepare16_v2(db, "SELECT * FROM t1 WHERE name = ? OR name = ?", out var stmt, out var remain));
                Assert.AreEqual(2, sqlite3_bind_parameter_count(stmt));
            }
        }

        [DataTestMethod]
        [DataRow("?")]
        [DataRow("?1")]
        [DataRow(":FOO")]
        [DataRow("@FOO")]
        [DataRow("$FOO")]
        public void TestParameterName(string parameter)
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            {
                Assert.AreEqual(SQLITE_OK, sqlite3_prepare16_v2(db, $"SELECT * FROM t1 WHERE name = {parameter}", out var stmt, out var remain));
                Assert.AreEqual(parameter=="?"?null:parameter, sqlite3_bind_parameter_name(stmt, 1));
            }
        }

        [DataTestMethod]
        [DataRow("?1")]
        [DataRow(":FOO")]
        [DataRow("@FOO")]
        [DataRow("$FOO")]
        public void TestParameterIndex(string parameter)
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            {
                Assert.AreEqual(SQLITE_OK, sqlite3_prepare16_v2(db, $"SELECT * FROM t1 WHERE name = {parameter}", out var stmt, out var remain));
                Assert.AreEqual(1, sqlite3_bind_parameter_index(stmt, parameter));
            }
        }
    }
}