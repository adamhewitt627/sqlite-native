using System;
using System.Runtime.InteropServices;
using System.Text;
using static SqliteNative.Sqlite3;

namespace SqliteNative.Util
{
    public static class Extensions
    {
        public static unsafe IntPtr ToUtf8(this string @string, out int byteCount)
        {
            byteCount = 0;
            if (@string == null) return IntPtr.Zero;
            IntPtr utf8 = IntPtr.Zero;
            fixed (char* sqlString = @string)
            {
                var size = Encoding.UTF8.GetByteCount(@string);
                utf8 = Marshal.AllocHGlobal(size + 1);
                var bytes = (byte*)utf8.ToPointer();
                size = Encoding.UTF8.GetBytes(sqlString, @string.Length, bytes, size);
                bytes[size] = 0;
                byteCount = size + 1;
                return utf8;
            }
        }

        public static unsafe string FromUtf8(this IntPtr utf8)
        {
            if (@utf8 == IntPtr.Zero) return null;
            
            var byteCount = 0;
            byte* bytes = (byte*)utf8.ToPointer();
            while (bytes[byteCount] != 0) byteCount++;
            if (byteCount == 0) return string.Empty;

            var charCount = Encoding.UTF8.GetCharCount(bytes, byteCount);
            var result = new string('\0', charCount);
            fixed (char* resultPtr = result)
                Encoding.UTF8.GetChars(bytes, byteCount, resultPtr, charCount);
            return result;
        }

        public static bool BackupTo(this Database source, Database destination, int stepSize = -1, IProgress<double> progress = null)
        {
            try
            {
                using (var backup = new Backup(source, destination))
                {
                    progress?.Report(0);
                    while (backup.Step(stepSize))
                    {
                        if (progress == null) continue;
                        var total = backup.PageCount;
                        var remaining = backup.Remaining;
                        progress.Report((total - remaining) / (double)remaining);
                    }
                    progress?.Report(1);
                }

                return true;
            }
            catch { return false; }
        }
    }
}