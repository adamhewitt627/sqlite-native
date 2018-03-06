using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Tests
{
    [TestClass]
    public class BlobTests
    {
        private static IDatabase CreateDatabase(int length, out byte[] blob)
        {
            var db = new Sqlite3().OpenTest();
            db.Execute("CREATE TABLE t1(id INTEGER PRIMARY KEY, data BLOB)");
            new Random().NextBytes(blob = new byte[length]);

            using (var stmt = db.Prepare($"INSERT INTO t1(data) VALUES(?)"))
            {
                Assert.IsTrue(stmt.Bindings.SetBlob(1, blob));
                Assert.AreEqual(Status.Done, stmt.Step());
            }
            return db;
        }

#region Binding & Non-incremental blobs
        [TestMethod]
        public void TestBindBlob()
        {
            using (var db = CreateDatabase(42, out var blob))
            using (var stmt = db.Prepare($"SELECT data FROM t1"))
            {
                Assert.AreEqual(Status.Row, stmt.Step());
                Assert.IsTrue(blob.SequenceEqual(stmt.Columns.GetBlob(0)));
            }
        }

        [TestMethod]
        public void TestBlobValue()
        {
            using (var db = CreateDatabase(42, out var blob))
            using (var stmt = db.Prepare($"SELECT data FROM t1"))
            {
                Assert.AreEqual(Status.Row, stmt.Step());
                var value = stmt.Columns.GetValue(0);
                Assert.IsTrue(blob.SequenceEqual(value.AsBlob()));
            }
        }

        [TestMethod]
        public void TestEmptyBlob()
        {
            using (var db = CreateDatabase(0, out var blob))
            using (var stmt = db.Prepare($"SELECT data FROM t1"))
            {
                Assert.AreEqual(Status.Row, stmt.Step());
                Assert.IsNull(stmt.Columns.GetBlob(0));
            }
        }
#endregion

        [TestMethod]
        public void CanOpenBlob()
        {
            const int length = 512;
            using (var db = CreateDatabase(length, out var blob))
            using (var blobIO = db.OpenBlob("main", "t1", "data", 1, 0))
            {
                var actual = new byte[length];
                Assert.AreEqual(length, blobIO.Read(actual, 0, length));
                Assert.IsTrue(blob.SequenceEqual(actual));
            }
        }

        [TestMethod]
        public async Task CanReadBlob()
        {
            const int length = 512;
            using (var db = CreateDatabase(length, out var data))
            using (var blob = db.OpenBlob("main", "t1", "data", 1))
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
            using (var db = new Sqlite3().OpenTest("CREATE TABLE t1(id INTEGER PRIMARY KEY, data BLOB)"))
            {
                using (var stmt = db.Prepare($"INSERT INTO t1(data) VALUES(?)"))
                {
                    Assert.IsTrue(stmt.Bindings.SetBlob(1, length));
                    Assert.AreEqual(Status.Done, stmt.Step());
                }

                var expected = new byte[length];
                new Random().NextBytes(expected);

                using (var blob = db.OpenBlob("main", "t1", "data", 1, 1))
                using (var stream = new MemoryStream(expected))
                    await stream.CopyToAsync(blob, 60);

                using (var stmt = db.Prepare($"SELECT data FROM t1"))
                {
                    Assert.AreEqual(Status.Row, stmt.Step());
                    Assert.IsTrue(expected.SequenceEqual(stmt.Columns.GetBlob(0)));
                }
            }
        }
    }
}