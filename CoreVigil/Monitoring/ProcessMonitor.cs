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
                CaptureInitialSample();

                await Task.Delay(
                    TimeSpan.FromMilliseconds(500),
                    cancellationToken);
            }

            return CaptureUsage(count);
        }

        private void CaptureInitialSample()
        {
            _previousSamples.Clear();

            foreach (Process process in Process.GetProcesses())
            {
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

            _previousTimestamp = DateTime.UtcNow;
            _hasPreviousSample = true;
        }

        private IReadOnlyList<ProcessSnapshot> CaptureUsage(
            int count)
        {
            DateTime currentTimestamp = DateTime.UtcNow;

            double elapsedMilliseconds =
                (currentTimestamp - _previousTimestamp).TotalMilliseconds;

            var currentSamples =
                new Dictionary<int, ProcessSample>();

            var snapshots =
                new List<ProcessSnapshot>();

            foreach (Process process in Process.GetProcesses())
            {
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

        private sealed record ProcessSample(
            string Name,
            TimeSpan ProcessorTime);
    }
}