using System;
using System.Linq;
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
            using (var stmt = new Statement(db, "SELECT * FROM t1 WHERE name = ? OR name = ?; VACUUM", out var remain))
                Assert.AreEqual(2, sqlite3_bind_parameter_count(stmt));
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
            using (var stmt = new Statement(db, $"SELECT * FROM t1 WHERE name = {parameter}", out var remain))
                Assert.AreEqual(parameter=="?"?null:parameter, sqlite3_bind_parameter_name(stmt, 1));
        }

        [DataTestMethod]
        [DataRow("?1")]
        [DataRow(":FOO")]
        [DataRow("@FOO")]
        [DataRow("$FOO")]
        public void TestParameterIndex(string parameter)
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            using (var stmt = new Statement(db, $"SELECT * FROM t1 WHERE name = {parameter}", out var remain))
                Assert.AreEqual(1, sqlite3_bind_parameter_index(stmt, parameter));
        }

        [TestMethod]
        public void TestColumnCount()
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = new Statement(db, $"SELECT * FROM t1", out var remain))
                Assert.AreEqual(2, sqlite3_column_count(stmt));
        }

        [TestMethod]
        public void TestBindText()
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            {
                const string flibbety = "flibbetty gibbett";
                using (var stmt = new Statement(db, $"INSERT INTO t1(data) VALUES(?)", out var remain))
                {
                    Assert.AreEqual(SQLITE_OK, sqlite3_bind_text16(stmt, 1, flibbety));
                    Assert.AreEqual(SQLITE_DONE, sqlite3_step(stmt));
                }
                using (var stmt = new Statement(db, $"SELECT data FROM t1", out var remain))
                {
                    Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                    Assert.AreEqual(flibbety, sqlite3_column_text16(stmt, 0));
                }
            }
        }
    }
}