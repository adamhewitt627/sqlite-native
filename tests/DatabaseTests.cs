using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests;
using SqliteNative.Util;
using System;
using System.IO;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
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
            }
            finally { sqlite3_close(db); }
        }

        [TestMethod]
        public void ExecutesSQL()
        {
            using (var db = new Sqlite3().OpenTest())
            {
                Assert.IsTrue(db.Execute(@"CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT);
                                           INSERT INTO t1(name) VALUES('flibbety')"));
                using (var stmt = db.Prepare("SELECT * FROM t1"))
                {
                    Assert.AreEqual(Status.Row, stmt.Step());
                    Assert.AreEqual("flibbety", stmt.Columns.GetText(1));
                }
            }
        }

        [TestMethod]
        public void CallsUpdateCallback()
        {
            var called = false;
            var ctx = (IntPtr)(42);
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"))
            using (var cb = new Callback<UpdateHook>(updateHook))
            {
                Assert.AreEqual(IntPtr.Zero, sqlite3_update_hook(db.Handle, cb, ctx));
                Assert.IsTrue(db.Execute("INSERT INTO t1(name) VALUES('fizzbuzz')"));
            }
            Assert.IsTrue(called);

            void updateHook(IntPtr context, int change, IntPtr dbName, IntPtr tableName, long rowid)
            {
                called = true;
                Assert.AreEqual(ctx, context);
                Assert.AreEqual("main", dbName.FromUtf8());
                Assert.AreEqual("t1", tableName.FromUtf8());
            }
        }

        [TestMethod]
        public void CallsCommit()
        {
            var called = false;
            var ctx = (IntPtr)(42);
            using (var db = new Sqlite3().OpenTest())
            using (var cb = new Callback<CommitHook>(commitHook))
            {
                Assert.AreEqual(IntPtr.Zero, sqlite3_commit_hook(db.Handle, cb, ctx));
                Assert.IsTrue(db.Execute(string.Join(";", "BEGIN",
                    "CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)",
                    "COMMIT")));
            }
            Assert.IsTrue(called);

            int commitHook(IntPtr context)
            {
                called = true;
                Assert.AreEqual(ctx, context);
                return 0;
            }
        }

        [TestMethod]
        public void CallsRollback()
        {
            var called = false;
            var ctx = (IntPtr)(42);
            using (var db = new Sqlite3().OpenTest())
            using (var commit = new Callback<CommitHook>(commitHook))
            using (var rollback = new Callback<RollbackHook>(rollbackHook))
            {
                Assert.AreEqual(IntPtr.Zero, sqlite3_commit_hook(db.Handle, commit, ctx));
                Assert.AreEqual(IntPtr.Zero, sqlite3_rollback_hook(db.Handle, rollback, ctx));
                db.Execute(string.Join(";", "BEGIN",
                    "CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)",
                    "COMMIT"));
            }
            Assert.IsTrue(called);

            int commitHook(IntPtr context) => 7;
            void rollbackHook(IntPtr context)
            {
                called = true;
                Assert.AreEqual(ctx, context);
            }
        }

        [DataTestMethod]    //TODO - still haven't gotten this to work
        [DataRow(0)]
        [DataRow(7)]
        public void InvokesBusyHandler(int busyCallCount)
        {
            var called = false;
            var ctx = (IntPtr)(42);

            var path = Path.GetTempFileName();
            var sqlite = new Sqlite3();
            using (var db1 = sqlite.OpenTest(path, OpenFlags.ReadWrite | OpenFlags.Create, "BEGIN IMMEDIATE TRANSACTION"))
            using (var callback = new Callback<BusyHandler>(busyHandler))
            using (var db2 = sqlite.OpenTest(path, OpenFlags.ReadWrite))
            {
                sqlite3_busy_handler(db2.Handle, callback, ctx);
                using (var stms = db2.Prepare("BEGIN IMMEDIATE TRANSACTION"))
                    Assert.AreEqual(Status.Busy, stms.Step());
            }
            Assert.IsTrue(called);
            File.Delete(path);

            int busyHandler(IntPtr context, int callCount)
            {
                if (callCount < busyCallCount) return 1;
                called = true;
                Assert.AreEqual(ctx, context);
                return 0;
            }
        }

        [TestMethod]
        public void LogsWALCommit()
        {
            var called = false;
            var ctx = (IntPtr)(42);
            var path = Path.GetTempFileName();
            using (var db = new Sqlite3().OpenTest(path, OpenFlags.ReadWrite | OpenFlags.Create, "PRAGMA journal_mode=WAL"))
            using (var callback = new Callback<WriteAheadLogHook>(walHook))
            {
                sqlite3_wal_hook(db.Handle, callback, ctx);
                Assert.IsTrue(db.Execute("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)"));
            }
            Assert.IsTrue(called);
            File.Delete(path);

            int walHook(IntPtr context, IntPtr db, IntPtr dbName, int pages)
            {
                called = true;
                Assert.AreEqual(ctx, context);
                return SQLITE_OK;
            }
        }
    }
}
