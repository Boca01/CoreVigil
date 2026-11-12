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
    }
}
