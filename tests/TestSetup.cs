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
                file = $@"win-{bitness}\lib\netstandard1.4\sqlite3.dll";
            var lib = LoadLibrary(System.IO.Path.Combine(@base, file));
            Assert.AreNotEqual(lib, IntPtr.Zero);
        }
    }
}