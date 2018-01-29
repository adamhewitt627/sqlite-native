using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class MiscellaneousTests
    {
        [DataTestMethod]
        [DataRow("Bad SQL statement", false)]
        [DataRow("SELECT * FROM table;", true)]
        [DataRow("INSERT INTO table(id) VALUES(1);  ", true)]
        public void CheckCompleteStatement(string sql, bool expected)
            => Assert.AreEqual(sqlite3_complete(sql), expected);
    }
}