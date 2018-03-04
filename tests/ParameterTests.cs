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
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            using (var stmt = db.Prepare("SELECT * FROM t1 WHERE name = ? OR name = ?; VACUUM"))
                Assert.AreEqual(2, stmt.Bindings.Count);
        }

        [DataTestMethod]
        [DataRow("?")]
        [DataRow("?1")]
        [DataRow(":FOO")]
        [DataRow("@FOO")]
        [DataRow("$FOO")]
        public void TestParameterName(string parameter)
        {
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            using (var stmt = db.Prepare($"SELECT * FROM t1 WHERE name = {parameter}"))
                Assert.AreEqual(parameter=="?"?null:parameter, stmt.Bindings.NameOf(1));
        }

        [DataTestMethod]
        [DataRow("?1")]
        [DataRow(":FOO")]
        [DataRow("@FOO")]
        [DataRow("$FOO")]
        public void TestParameterIndex(string parameter)
        {
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            using (var stmt = db.Prepare($"SELECT * FROM t1 WHERE name = {parameter}"))
                Assert.AreEqual(1, stmt.Bindings.IndexOf(parameter));
        }

        [TestMethod]
        public void TestBindText()
        {
            const string flibbety = "flibbetty gibbett";
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            {
                using (var stmt = db.Prepare($"INSERT INTO t1(data) VALUES(?)"))
                {
                    Assert.IsTrue(stmt.Bindings.SetText(1, flibbety));
                    Assert.AreEqual(Status.Done, stmt.Step());
                }
                using (var stmt = db.Prepare($"SELECT data FROM t1"))
                {
                    Assert.AreEqual(Status.Row, stmt.Step());
                    Assert.AreEqual(flibbety, stmt.Columns.GetText(0));
                }
            }
        }

        [TestMethod]
        public void BindsTextToNumber()
        {
            const int id = 7;
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)"))
            using (var stmt = db.Prepare($"INSERT INTO t1(id) VALUES(?)"))
            {
                Assert.IsTrue(stmt.Bindings.SetText(1, id.ToString()));
                Assert.AreEqual(Status.Done, stmt.Step());

                using (var query = db.Prepare($"SELECT id FROM t1"))
                {
                    Assert.AreEqual(Status.Row, query.Step());
                    Assert.AreEqual(id, query.Columns.GetInt32(0));
                }
            }
        }
    }
}