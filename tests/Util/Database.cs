using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests.Util
{
    class Database : IDisposable
    {
        private IntPtr _db;

        public Database(params string[] statements)
        {
            sqlite3_open_v2(":memory:", out _db, SQLITE_OPEN_READWRITE);
            Execute(statements);
        }
        public static implicit operator IntPtr(Database database) => database._db;

        #region IDisposable Support
        private bool _isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            sqlite3_close(_db);
            _db = IntPtr.Zero;
            _isDisposed = true;
        }

        ~Database() => Dispose(false);
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

        public void Execute(params string[] statements)
        {
            foreach(var statement in statements)
            {
                string sql = statement;
                while (!string.IsNullOrWhiteSpace(sql))
                {
                    Assert.AreEqual(SQLITE_OK, sqlite3_prepare16_v2(_db, sql, out var stmt, out sql));
                    while (sqlite3_step(stmt) == SQLITE_ROW) ;
                }
            }
        }
    }
}