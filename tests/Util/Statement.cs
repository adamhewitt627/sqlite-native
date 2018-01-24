using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Util;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests.Util
{
    class Statement : Disposable
    {
        private IntPtr _stmt;

        public Statement(Database database, string sql, out string remain)
            => Assert.AreEqual(SQLITE_OK, sqlite3_prepare16_v2(database, sql, out _stmt, out remain));
        public static implicit operator IntPtr(Statement statement) => statement._stmt;

        protected override void Dispose(bool disposing)
        {
            Assert.AreEqual(SQLITE_OK, sqlite3_finalize(_stmt));
            _stmt = IntPtr.Zero;
        }
    }
}