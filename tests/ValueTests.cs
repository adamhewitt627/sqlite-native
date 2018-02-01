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
                var value = sqlite3_column_value(stmt, 0);
                test(value, expected);
            }
        }

        [TestMethod]
        public void GetsTextFromValue() => TestText((value, expected) =>
        {
            Assert.AreEqual(expected, sqlite3_value_text(value));
            Assert.AreEqual(expected, sqlite3_value_text16(value));
        });

        [TestMethod]
        public void GetsUTF16LE() => TestText((value, expectedStr) =>
        {
            var expected = Encoding.Unicode.GetBytes(expectedStr);
            var actual = sqlite3_value_text16le(value);
            Assert.IsTrue(expected.SequenceEqual(actual));
        });

        [TestMethod]
        public void GetsUTF16BE() => TestText((value, expectedStr) =>
        {
            var expected = Encoding.BigEndianUnicode.GetBytes(expectedStr);
            var actual = sqlite3_value_text16be(value);
            Assert.IsTrue(expected.SequenceEqual(actual));
        });

        private static void TestNumber<T>(T expected, Action<IntPtr, T> test)
        {
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data)", $"INSERT INTO t1(data) VALUES({expected})"))
            using (var stmt = new Statement(db, "SELECT data FROM t1", out var remain))
            {
                Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                var value = sqlite3_column_value(stmt, 0);
                test(value, expected);
            }
        }

        [TestMethod]
        public void GetsDouble() => TestNumber(42.31415d, (value, expected) => {
            Assert.AreEqual(expected, sqlite3_value_double(value), .00001);
        });
        [TestMethod]
        public void GetsInt32() => TestNumber(31415, (value, expected) => {
            Assert.AreEqual(expected, sqlite3_value_int(value));
        });
        [TestMethod]
        public void GetsInt64() => TestNumber(31415L, (value, expected) => {
            Assert.AreEqual(expected, sqlite3_value_int64(value));
        });
    }
}