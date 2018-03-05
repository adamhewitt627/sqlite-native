using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Util;
using System;
using System.Linq;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class BackupTests : IProgress<double>
    {
        void IProgress<double>.Report(double value) { }

        [TestMethod]
        public void SimpleBackup()
        {
            const string expected = "fizzbuzz";
            var sqlite = new Sqlite3();
            using (var source = (Database)sqlite.OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, name TEXT)", $"INSERT INTO t1(name) VALUES('{expected}')"))
            using (var dest = (Database)sqlite.OpenTest())
            {
                source.BackupTo(dest);
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
            using (var source = (Database)sqlite.OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY)"))
            using (var dest = (Database)sqlite.OpenTest())
            {
                source.Execute("BEGIN TRANSACTION");
                foreach (var i in Enumerable.Range(1, expected))
                    source.Execute($"INSERT INTO t1(id) VALUES({i})");
                source.Execute("COMMIT");

                source.BackupTo(dest, stepSize, this);

                var actual = 0;
                using (var stmt = dest.Prepare("SELECT * FROM t1"))
                    while (stmt.Step() == Status.Row) actual++;
                Assert.AreEqual(expected, actual);
            }
        }
    }
}
