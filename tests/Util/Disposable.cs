using System;

namespace SqliteNative.Tests.Util
{
    public abstract class Disposable : IDisposable
    {
        protected abstract void Dispose(bool disposing);

        ~Disposable()  => Dispose(false);
        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}