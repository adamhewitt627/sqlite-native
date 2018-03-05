using SqliteNative.Util;
using System;
using System.Collections.Generic;
using System.Text;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IBackup
    {
        bool Step(int pageCount);
        int Remaining { get; }
        int PageCount { get; }
    }

    internal class Backup : Disposable, IBackup
    {
        private readonly IntPtr _ptr;
        public Backup(Database source, Database destination) => _ptr = sqlite3_backup_init(destination, source);

        public int Remaining => sqlite3_backup_remaining(_ptr);
        public int PageCount => sqlite3_backup_pagecount(_ptr);
        public bool Step(int pageCount) => sqlite3_backup_step(_ptr, pageCount) is SQLITE_OK && Remaining > 0;

        protected override void Dispose(bool disposing) => sqlite3_backup_finish(_ptr);
    }
}
