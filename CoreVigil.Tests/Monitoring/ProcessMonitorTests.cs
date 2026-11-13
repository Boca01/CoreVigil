using CoreVigil.Monitoring;

namespace CoreVigil.Tests.Monitoring
{
    public sealed class ProcessMonitorTests
    {
        [Fact]
        public async Task GetTopProcessesAsync_CancelledDuringInitialInterval_StopsPromptly()
        {
            using var cancellation =
                new CancellationTokenSource();

            var monitor =
                new ProcessMonitor();

            Task<IReadOnlyList<CoreVigil.Core.Models.ProcessSnapshot>> capture =
                monitor.GetTopProcessesAsync(
                    count: 8,
                    cancellationToken: cancellation.Token);

            cancellation.CancelAfter(
                TimeSpan.FromMilliseconds(50));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => capture);
        }

        [Fact]
        public void CalculateUsagePercent_SinTiempoTranscurrido_DevuelveCero()
        {
            double usagePercent =
                ProcessMonitor.CalculateUsagePercent(
                    processorMilliseconds: 100.0,
                    elapsedMilliseconds: 0.0,
                    processorCount: 8);

            Assert.Equal(0.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_SinProcesadores_DevuelveCero()
        {
            double usagePercent =
                ProcessMonitor.CalculateUsagePercent(
                    processorMilliseconds: 100.0,
                    elapsedMilliseconds: 1000.0,
                    processorCount: 0);

            Assert.Equal(0.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_ActividadNormal_NormalizaPorProcesadores()
        {
            double usagePercent =
                ProcessMonitor.CalculateUsagePercent(
                    processorMilliseconds: 400.0,
                    elapsedMilliseconds: 1000.0,
                    processorCount: 4);

            Assert.Equal(10.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_ActividadNegativa_NoDevuelveNegativo()
        {
            double usagePercent =
                ProcessMonitor.CalculateUsagePercent(
                    processorMilliseconds: -100.0,
                    elapsedMilliseconds: 1000.0,
                    processorCount: 4);

            Assert.Equal(0.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_ActividadExcesiva_NoSuperaCien()
        {
            double usagePercent =
                ProcessMonitor.CalculateUsagePercent(
                    processorMilliseconds: 10000.0,
                    elapsedMilliseconds: 1000.0,
                    processorCount: 2);

            Assert.Equal(100.0, usagePercent);
        }
    }
}
