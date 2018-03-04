using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqliteNative.Tests
{
    [TestClass]
    public class TestSetup
    {
        [DllImport("kernel32")]
        private static extern IntPtr LoadLibrary(string dllname);

        [AssemblyInitialize]
        public static void Setup(TestContext context)
        {
            string @base = System.IO.Path.GetDirectoryName(new Uri(typeof(TestSetup).GetTypeInfo().Assembly.CodeBase).LocalPath),
                bitness = IntPtr.Size == 8 ? "x64" : "x86",
                file = $@"{bitness}\sqlite3.dll";
            var lib = LoadLibrary(System.IO.Path.Combine(@base, file));
            Assert.AreNotEqual(lib, IntPtr.Zero);
        }
    }

    internal static class DatabaseExtensions
    {
        public static IDatabase OpenTest(this ISQLite sqlite, params string[] statements) => OpenTest(sqlite, ":memory:", OpenFlags.ReadWrite, statements);
        public static IDatabase OpenTest(this ISQLite sqlite, string path, OpenFlags flags, params string[] statements)
        {
            var db = sqlite.Open(path, flags);
            foreach (var s in statements)
                db.Execute(s);
            return db;
        }
    }
}