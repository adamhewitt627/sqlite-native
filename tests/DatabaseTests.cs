using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests.Util;
using SqliteNative.Util;
using static SqliteNative.Sqlite3;

namespace tests
{
    [TestClass]
    public class UnitTest1
    {
        [DataTestMethod]
        [DataRow(":memory:", SQLITE_OPEN_READWRITE)]
        [DataRow("test.db", SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE)]
        public void TestOpen(string path, int flags)
        {
            var err = sqlite3_open_v2(path, out var db, flags);
            try
            {
                Assert.AreEqual(0, err);
            }
            finally
            {
                sqlite3_close(db);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [DataTestMethod]
        [DataRow(":foobar:", SQLITE_OPEN_READWRITE)]
        [DataRow("badTest.db", SQLITE_OPEN_READWRITE)]
        public void TestOpenFailure(string path, int flags)
        {
            var err = sqlite3_open_v2(path, out var db, SQLITE_OPEN_READWRITE, IntPtr.Zero);
            try
            {
                Assert.AreNotEqual(0, err);
            } finally { sqlite3_close(db); }
        }

        [TestMethod]
        public void ExecutesSQL()
        {
            using (var db = new Database())
            {
                Assert.AreEqual(SQLITE_OK, sqlite3_exec(db, 
                    "CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT);\n"
                    + "INSERT INTO t1(name) VALUES('flibbety')"));
                using (var stmt = new Statement(db, "SELECT * FROM t1", out var remain))
                {
                    Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                    Assert.AreEqual("flibbety", sqlite3_column_text(stmt, 1));
                }
            }
        }

        [TestMethod]
        public void CallsUpdateCallback()
        {
            var called = false;
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            using (var cb = new Callback<UpdateHook>(updateHook))
            {
                Assert.AreEqual(IntPtr.Zero, sqlite3_update_hook(db, cb, IntPtr.Zero));
                using (var stmt = new Statement(db, "INSERT INTO t1(name) VALUES('fizzbuzz')", out var remain))
                    Assert.AreEqual(SQLITE_DONE, sqlite3_step(stmt));
            }
            Assert.IsTrue(called);

            void updateHook(IntPtr context, int change, IntPtr dbName, IntPtr tableName, long rowid)
            {
                called = true;
                Assert.AreEqual("main", dbName.FromUtf8());
                Assert.AreEqual("t1", tableName.FromUtf8());
            }
        }
    }
}
