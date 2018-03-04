using System;
using System.Collections.Generic;
using System.Text;

namespace SqliteNative.Events
{
    public class CommitEventArgs : EventArgs
    {
        public int Result { get; set; }
    }
}
