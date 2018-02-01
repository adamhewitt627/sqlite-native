using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteNative.Tests.Util;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class BlobTests
    {
        private static Database CreateDatabase(int length, out byte[] blob)
        {
            var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data BLOB)");
            new Random().NextBytes(blob = new byte[length]);

            using (var stmt = new Statement(db, $"INSERT INTO t1(data) VALUES(?)", out var remain))
            {
                Assert.AreEqual(SQLITE_OK, sqlite3_bind_blob(stmt, 1, blob));
                Assert.AreEqual(SQLITE_DONE, sqlite3_step(stmt));
            }
            return db;
        }

#region Binding & Non-incremental blobs
        [TestMethod]
        public void TestBindBlob()
        {
            using (var db = CreateDatabase(42, out var blob))
            using (var stmt = new Statement(db, $"SELECT data FROM t1", out var remain))
            {
                Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                Assert.IsTrue(blob.SequenceEqual(sqlite3_column_blob(stmt, 0)));
            }
        }

        [TestMethod]
        public void TestBlobValue()
        {
            using (var db = CreateDatabase(42, out var blob))
            using (var stmt = new Statement(db, $"SELECT data FROM t1", out var remain))
            {
                Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                var value = sqlite3_column_value(stmt, 0);
                Assert.IsTrue(blob.SequenceEqual(sqlite3_value_blob(value)));
            }
        }

        [TestMethod]
        public void TestEmptyBlob()
        {
            using (var db = CreateDatabase(0, out var blob))
            using (var stmt = new Statement(db, $"SELECT data FROM t1", out var remain))
            {
                Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                Assert.IsNull(sqlite3_column_blob(stmt, 0));
            }
        }
#endregion

        [TestMethod]
        public void CanOpenBlob()
        {
            const int length = 512;
            using (var db = CreateDatabase(length, out var blob))
            {
                var actual = new byte[length];
                Assert.AreEqual(SQLITE_OK, sqlite3_blob_open(db, "main", "t1", "data", 1, 0, out var ppBlob));
                Assert.AreEqual(SQLITE_OK, sqlite3_blob_read(ppBlob, actual, length, 0));
                Assert.IsTrue(blob.SequenceEqual(actual));
                sqlite3_blob_close(ppBlob);
            }
        }

        [TestMethod]
        public async Task CanReadBlob()
        {
            const int length = 512;
            using (var db = CreateDatabase(length, out var data))
            using (var blob = new Blob(db, "main", "t1", "data", 1))
            using (var stream = new MemoryStream())
            {
                await blob.CopyToAsync(stream, 60);
                Assert.IsTrue(data.SequenceEqual(stream.ToArray()));
            }
        }

        [TestMethod]
        public async Task CanWriteBlob()
        {
            const int length = 512;
            using (var db = new Database("CREATE TABLE t1(id INTEGER PRIMARY KEY, data BLOB)"))
            {
                using (var stmt = new Statement(db, $"INSERT INTO t1(data) VALUES(?)", out var remain))
                {
                    Assert.AreEqual(SQLITE_OK, sqlite3_bind_zeroblob(stmt, 1, length));
                    Assert.AreEqual(SQLITE_DONE, sqlite3_step(stmt));
                }

                var expected = new byte[length];
                new Random().NextBytes(expected);

                using (var blob = new Blob(db, "main", "t1", "data", 1, 1))
                using (var stream = new MemoryStream(expected))
                    await stream.CopyToAsync(blob, 60);

                using (var stmt = new Statement(db, $"SELECT data FROM t1", out var remain))
                {
                    Assert.AreEqual(SQLITE_ROW, sqlite3_step(stmt));
                    var actual = sqlite3_column_blob(stmt, 0);
                    Assert.IsTrue(expected.SequenceEqual(actual));
                }
            }
        }
    }
}