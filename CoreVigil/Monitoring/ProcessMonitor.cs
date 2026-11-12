using System.Diagnostics;
using CoreVigil.Core.Abstractions;
using CoreVigil.Core.Models;

namespace CoreVigil.Monitoring
{
    /// <summary>
    /// Obtiene los procesos con mayor consumo reciente de CPU
    /// comparando muestras acumuladas de tiempo de procesador.
    /// </summary>
    public sealed class ProcessMonitor : IProcessMonitor
    {
        private readonly Dictionary<int, ProcessSample> _previousSamples =
            new();

        private DateTime _previousTimestamp;
        private bool _hasPreviousSample;

        public async Task<IReadOnlyList<ProcessSnapshot>> GetTopProcessesAsync(
            int count,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (count <= 0)
            {
                return Array.Empty<ProcessSnapshot>();
            }

            if (!_hasPreviousSample)
            {
                CaptureInitialSample(
                    cancellationToken);

                await Task.Delay(
                    TimeSpan.FromMilliseconds(500),
                    cancellationToken);
            }

            return CaptureUsage(
                count,
                cancellationToken);
        }

        private void CaptureInitialSample(
            CancellationToken cancellationToken)
        {
            _previousSamples.Clear();

            Process[] processes =
                Process.GetProcesses();

            try
            {
                foreach (Process process in processes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        _previousSamples[process.Id] =
                            new ProcessSample(
                                process.ProcessName,
                                process.TotalProcessorTime);
                    }
                    catch
                    {
                        // Algunos procesos pueden finalizar o impedir
                        // consultar sus tiempos mientras se enumeran.
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            finally
            {
                DisposeProcesses(processes);
            }

            _previousTimestamp = DateTime.UtcNow;
            _hasPreviousSample = true;
        }

        private IReadOnlyList<ProcessSnapshot> CaptureUsage(
            int count,
            CancellationToken cancellationToken)
        {
            DateTime currentTimestamp = DateTime.UtcNow;

            double elapsedMilliseconds =
                (currentTimestamp - _previousTimestamp).TotalMilliseconds;

            var currentSamples =
                new Dictionary<int, ProcessSample>();

            var snapshots =
                new List<ProcessSnapshot>();

            Process[] processes =
                Process.GetProcesses();

            try
            {
                foreach (Process process in processes)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var currentSample =
                            new ProcessSample(
                                process.ProcessName,
                                process.TotalProcessorTime);

                        currentSamples[process.Id] =
                            currentSample;

                        if (!_previousSamples.TryGetValue(
                                process.Id,
                                out ProcessSample previousSample))
                        {
                            continue;
                        }

                        double processorMilliseconds =
                            (currentSample.ProcessorTime -
                             previousSample.ProcessorTime)
                            .TotalMilliseconds;

                        double usagePercent = 0.0;

                        if (elapsedMilliseconds > 0)
                        {
                            usagePercent =
                                processorMilliseconds /
                                elapsedMilliseconds /
                                Environment.ProcessorCount *
                                100.0;
                        }

                        snapshots.Add(
                            new ProcessSnapshot
                            {
                                ProcessId = process.Id,
                                Name = currentSample.Name,
                                CpuUsagePercent = Math.Clamp(
                                    usagePercent,
                                    0.0,
                                    100.0)
                            });
                    }
                    catch
                    {
                        // El proceso puede desaparecer entre la enumeración
                        // y la lectura de sus propiedades.
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            finally
            {
                DisposeProcesses(processes);
            }

            _previousSamples.Clear();

            foreach (var sample in currentSamples)
            {
                _previousSamples[sample.Key] =
                    sample.Value;
            }

            _previousTimestamp = currentTimestamp;

            return snapshots
                .Where(snapshot => snapshot.CpuUsagePercent > 0.0)
                .OrderByDescending(snapshot => snapshot.CpuUsagePercent)
                .Take(count)
                .ToArray();
        }

        private static void DisposeProcesses(
            IEnumerable<Process> processes)
        {
            foreach (Process process in processes)
            {
                process.Dispose();
            }
        }

        private sealed record ProcessSample(
            string Name,
            TimeSpan ProcessorTime);
    }
}