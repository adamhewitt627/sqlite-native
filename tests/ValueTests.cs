using System;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests.Util;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class ValueTests
    {
        private static void TestText(Action<IntPtr, string> test)
        {
            var expected = "flibbety";
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data TEXT)", $"INSERT INTO t1(data) VALUES('{expected}')"))
            using (var stmt = new Statement(db, "SELECT data FROM t1", out var remain))
            {
                Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                test(stmt, expected);
            }
        }

        [TestMethod]
        public void GetsTextFromValue() => TestText((stmt, expected) =>
        {
            var value = sqlite3_column_value(stmt, 0);
            Assert.AreEqual(expected, sqlite3_value_text(value));
            Assert.AreEqual(expected, sqlite3_value_text16(value));
        });

        [TestMethod]
        public void GetsUTF16LE() => TestText((stmt, expectedStr) =>
        {
            var expected = Encoding.Unicode.GetBytes(expectedStr);
            var value = sqlite3_column_value(stmt, 0);
            var actual = sqlite3_value_text16le(value);
            Assert.IsTrue(expected.SequenceEqual(actual));
        });

        [TestMethod]
        public void GetsUTF16BE() => TestText((stmt, expectedStr) =>
        {
            var expected = Encoding.BigEndianUnicode.GetBytes(expectedStr);
            var value = sqlite3_column_value(stmt, 0);
            var actual = sqlite3_value_text16be(value);
            Assert.IsTrue(expected.SequenceEqual(actual));
        });
    }
}