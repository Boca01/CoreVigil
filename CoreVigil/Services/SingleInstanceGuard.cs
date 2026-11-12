using System.Threading;

namespace CoreVigil.Services
{
    public sealed class SingleInstanceGuard : IDisposable
    {
        private readonly Mutex _mutex;
        private bool _ownsMutex;
        private bool _disposed;

        public SingleInstanceGuard(string name)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            _mutex = new Mutex(initiallyOwned: false, name);
        }

        public bool TryAcquire()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            if (_ownsMutex)
            {
                return true;
            }

            try
            {
                _ownsMutex = _mutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
                _ownsMutex = true;
            }

            return _ownsMutex;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_ownsMutex)
            {
                _mutex.ReleaseMutex();
                _ownsMutex = false;
            }

            _mutex.Dispose();
            _disposed = true;
        }
    }
}
