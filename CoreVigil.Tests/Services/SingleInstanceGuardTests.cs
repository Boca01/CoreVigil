using CoreVigil.Services;

namespace CoreVigil.Tests.Services
{
    public sealed class SingleInstanceGuardTests
    {
        [Fact]
        public void TryAcquire_SoloLaInstanciaPropietariaLiberaElMutex()
        {
            string mutexName =
                $"Local\\CoreVigil.Tests.{Guid.NewGuid():N}";

            var first =
                new SingleInstanceGuard(mutexName);

            Assert.True(first.TryAcquire());

            Assert.False(
                TryAcquireOnAnotherThread(mutexName));

            Assert.False(
                TryAcquireOnAnotherThread(mutexName));

            first.Dispose();

            using var nextOwner =
                new SingleInstanceGuard(mutexName);

            Assert.True(nextOwner.TryAcquire());
        }

        private static bool TryAcquireOnAnotherThread(
            string mutexName)
        {
            bool acquired = false;
            Exception? error = null;

            var thread =
                new Thread(() =>
                {
                    try
                    {
                        using var guard =
                            new SingleInstanceGuard(mutexName);

                        acquired =
                            guard.TryAcquire();
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                });

            thread.Start();
            thread.Join();

            if (error is not null)
            {
                throw error;
            }

            return acquired;
        }
    }
}
