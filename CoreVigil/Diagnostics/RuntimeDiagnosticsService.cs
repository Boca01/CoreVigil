using System.Diagnostics;

namespace CoreVigil.Diagnostics
{
    /// <summary>
    /// Obtiene métricas internas del proceso de CoreVigil
    /// para evaluar su impacto durante la ejecución.
    /// </summary>
    public sealed class RuntimeDiagnosticsService : IDisposable
    {
        private readonly Process _currentProcess;

        private TimeSpan _previousProcessorTime;
        private DateTime _previousTimestampUtc;
        private bool _hasPreviousSample;

        public RuntimeDiagnosticsService()
        {
            _currentProcess =
                Process.GetCurrentProcess();
        }

        public RuntimeSample Capture(
            TimeSpan applicationUptime,
            double systemCpuPercent,
            double cpuFrequencyMhz,
            double systemMemoryPercent,
            bool isHudVisible,
            bool isTopmost,
            string trayState)
        {
            _currentProcess.Refresh();

            DateTime timestampUtc =
                DateTime.UtcNow;

            TimeSpan processorTime =
                _currentProcess.TotalProcessorTime;

            double coreVigilCpuPercent =
                CalculateCpuUsage(
                    processorTime,
                    timestampUtc);

            long workingSetBytes =
                _currentProcess.WorkingSet64;

            long managedHeapBytes =
                GC.GetTotalMemory(
                    forceFullCollection: false);

            return new RuntimeSample
            {
                Timestamp = DateTime.Now,
                ApplicationUptime = applicationUptime,

                SystemCpuPercent =
                    systemCpuPercent,

                CpuFrequencyMhz =
                    cpuFrequencyMhz,

                SystemMemoryPercent =
                    systemMemoryPercent,

                CoreVigilCpuPercent =
                    coreVigilCpuPercent,

                WorkingSetBytes =
                    workingSetBytes,

                ManagedHeapBytes =
                    managedHeapBytes,

                IsHudVisible =
                    isHudVisible,

                IsTopmost =
                    isTopmost,

                TrayState =
                    trayState
            };
        }

        private double CalculateCpuUsage(
            TimeSpan currentProcessorTime,
            DateTime currentTimestampUtc)
        {
            if (!_hasPreviousSample)
            {
                _previousProcessorTime =
                    currentProcessorTime;

                _previousTimestampUtc =
                    currentTimestampUtc;

                _hasPreviousSample = true;

                return 0.0;
            }

            double elapsedMilliseconds =
                (currentTimestampUtc -
                 _previousTimestampUtc)
                .TotalMilliseconds;

            double processorMilliseconds =
                (currentProcessorTime -
                 _previousProcessorTime)
                .TotalMilliseconds;

            _previousProcessorTime =
                currentProcessorTime;

            _previousTimestampUtc =
                currentTimestampUtc;

            if (elapsedMilliseconds <= 0.0)
            {
                return 0.0;
            }

            double usagePercent =
                processorMilliseconds /
                elapsedMilliseconds /
                Environment.ProcessorCount *
                100.0;

            return Math.Clamp(
                usagePercent,
                0.0,
                100.0);
        }

        public void Dispose()
        {
            _currentProcess.Dispose();
        }
    }
}