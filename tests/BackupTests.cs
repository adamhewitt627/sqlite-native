using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class BackupTests
    {
        [TestMethod]
        public void SimpleBackup()
        {
            const string expected = "fizzbuzz";
            using (var source = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)", $"INSERT INTO t1(name) VALUES('{expected}')"))
            using (var dest = new Database())
            {
                var backup = sqlite3_backup_init(dest, source);
                Assert.AreEqual(SQLITE_DONE, sqlite3_backup_step(backup, -1));//TODO multiple pages (positive page #)
                Assert.AreEqual(SQLITE_OK, sqlite3_backup_finish(backup));

                using (var stmt = new Statement(dest, "SELECT name FROM t1", out var remain))
                {
                    Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                    Assert.AreEqual(expected, sqlite3_column_text(stmt, 0));
                }
            }
        }

        [DataTestMethod]
        [DataRow(1)][DataRow(7)][DataRow(42)]
        public void PagedBackup(int stepSize)
        {
            const int expected = 2 << 16;
            using (var source = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY)"))
            using (var dest = new Database())
            {
                source.Execute("BEGIN TRANSACTION");
                foreach (var i in Enumerable.Range(1, expected))
                    source.Execute($"INSERT INTO t1(id) VALUES({i})");
                source.Execute("COMMIT");

                var backup = sqlite3_backup_init(dest, source);
                while (sqlite3_backup_step(backup, 1) is SQLITE_OK && sqlite3_backup_remaining(backup) > 0) ;
                Assert.IsTrue(sqlite3_backup_pagecount(backup) > 1);
                Assert.AreEqual(SQLITE_OK, sqlite3_backup_finish(backup));

                var actual = 0;
                using (var stmt = new Statement(dest, "SELECT * FROM t1", out var remain))
                    while (sqlite3_step(stmt) == SQLITE_ROW) actual++;
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
