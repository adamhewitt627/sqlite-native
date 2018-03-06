using SqliteNative.Util;
using System;
using System.Collections.Generic;
using System.Text;
using static SqliteNative.Sqlite3;

namespace SqliteNative
{
    public interface IHooks
    {
        Func<int, int> Busy { set; }
        Func<int> Commit { set; }
        Action Rollback { set; }
        Action<int, string, string, long> Update { set; }
        Func<IDatabase, string, int, int> Wal { set; }
    }

    public class Hooks : IHooks
    {
        private Callback<UpdateHook> _updateHook;
        private Callback<RollbackHook> _rollbackHook;
        private Callback<CommitHook> _commitHook;
        private Callback<BusyHandler> _busyHook;
        private Callback<WriteAheadLogHook> _walHook;
        private readonly Database _database;

        public Hooks(Database database) => _database = database;

        public Action<int, string, string, long> Update
        {
            set
            {
                sqlite3_update_hook(_database, _updateHook = new Callback<UpdateHook>(onUpdate), IntPtr.Zero);
                void onUpdate(IntPtr context, int change, IntPtr dbName, IntPtr tableName, long rowid)
                    => value(change, dbName.FromUtf8(), tableName.FromUtf8(), rowid);
            }
        }

        public Action Rollback
        {
            set
            {
                sqlite3_rollback_hook(_database, _rollbackHook = new Callback<RollbackHook>(handler), IntPtr.Zero);
                void handler(IntPtr context) => value();
            }
        }

        public Func<int> Commit
        {
            set
            {
                sqlite3_commit_hook(_database, _commitHook = new Callback<CommitHook>(onCommit), IntPtr.Zero);
                int onCommit(IntPtr context) => value();
            }
        }

        public Func<int, int> Busy
        {
            set
            {
                sqlite3_busy_handler(_database, _busyHook = new Callback<BusyHandler>(onBusy), IntPtr.Zero);
                int onBusy(IntPtr context, int lockCount) => value(lockCount);
            }
        }

        public Func<IDatabase, string, int, int> Wal
        {
            set
            {
                sqlite3_wal_hook(_database, _walHook = new Callback<WriteAheadLogHook>(onWalHook), IntPtr.Zero);
                int onWalHook(IntPtr context, IntPtr db, IntPtr dbName, int pageCount)
                    => value(_database, dbName.FromUtf8(), pageCount);
            }
        }
    }
}
