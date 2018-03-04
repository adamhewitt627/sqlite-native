using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
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
            var sqlite = new Sqlite3();
            using (var source = sqlite.OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)", $"INSERT INTO t1(name) VALUES('{expected}')"))
            using (var dest = sqlite.OpenTest())
            {
                var backup = sqlite3_backup_init(dest.Handle, source.Handle);
                Assert.AreEqual(SQLITE_DONE, sqlite3_backup_step(backup, -1));//TODO multiple pages (positive page #)
                Assert.AreEqual(SQLITE_OK, sqlite3_backup_finish(backup));

                using (var stmt = dest.Prepare("SELECT name FROM t1"))
                {
                    Assert.AreEqual(Status.Row, stmt.Step());
                    Assert.AreEqual(expected, stmt.Columns.GetText(0));
                }
            }
        }

        [DataTestMethod]
        [DataRow(1)][DataRow(7)][DataRow(42)]
        public void PagedBackup(int stepSize)
        {
            const int expected = 2 << 16;
            var sqlite = new Sqlite3();
            using (var source = sqlite.OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY)"))
            using (var dest = sqlite.OpenTest())
            {
                source.Execute("BEGIN TRANSACTION");
                foreach (var i in Enumerable.Range(1, expected))
                    source.Execute($"INSERT INTO t1(id) VALUES({i})");
                source.Execute("COMMIT");

                var backup = sqlite3_backup_init(dest.Handle, source.Handle);
                while (sqlite3_backup_step(backup, 1) is SQLITE_OK && sqlite3_backup_remaining(backup) > 0) ;
                Assert.IsTrue(sqlite3_backup_pagecount(backup) > 1);
                Assert.AreEqual(SQLITE_OK, sqlite3_backup_finish(backup));

                var actual = 0;
                using (var stmt = dest.Prepare("SELECT * FROM t1"))
                    while (stmt.Step() == Status.Row) actual++;
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
