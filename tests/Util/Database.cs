using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Util;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests.Util
{
    class Database : Disposable
    {
        private IntPtr _db;

        public Database(string path, int flags, params string[] statements)
        {
            Assert.AreEqual(SQLITE_OK, sqlite3_open_v2(path, out _db, flags));
            Execute(statements);
        }
        public Database(params string[] statements) : this(":memory:", SQLITE_OPEN_READWRITE, statements) { }
        public static implicit operator IntPtr(Database database) => database._db;

        protected override void Dispose(bool disposing)
        {
            Assert.AreEqual(SQLITE_OK, sqlite3_close(_db));
            _db = IntPtr.Zero;
        }

        public void Execute(params string[] statements)
        {
            foreach(var statement in statements)
            {
                string sql = statement;
                while (!string.IsNullOrWhiteSpace(sql))
                {
                    using (var stmt = new Statement(this, sql, out sql))
                        while (sqlite3_step(stmt) == SQLITE_ROW) ;
                }
            }
        }
    }
}