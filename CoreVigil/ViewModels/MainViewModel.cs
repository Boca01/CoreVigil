using CoreVigil.Core.Abstractions;
using CoreVigil.Core.Models;
using CoreVigil.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace CoreVigil.ViewModels
{
    /// <summary>
    /// Expone a la interfaz los datos obtenidos por los monitores del sistema.
    /// </summary>
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ICpuMonitor _cpuMonitor;
        private readonly ICpuFrequencyMonitor _cpuFrequencyMonitor;
        private readonly ISystemUptimeProvider _systemUptimeProvider;
        private readonly IProcessorInfoProvider _processorInfoProvider;
        private readonly IProcessMonitor _processMonitor;
        private readonly IMemoryMonitor _memoryMonitor;
        private readonly ProcessGroupingService _processGroupingService;
        private double _cpuUsagePercent;
        private double _cpuFrequencyMhz;
        private double _cpuMaximumFrequencyMhz;
        private double _cpuFrequencyLimitMhz;
        private string _statusText = "Preparando monitor...";
        private TimeSpan _systemUptime;
        private string _processorName = "Procesador desconocido";
        private int _logicalProcessorCount;
        private IReadOnlyList<ProcessSnapshot> _topProcesses =
            Array.Empty<ProcessSnapshot>();
        private IReadOnlyList<ProcessGroupSnapshot> _processGroups =
            Array.Empty<ProcessGroupSnapshot>();
        private DateTime _nextProcessRefreshUtc =
            DateTime.MinValue;
        private readonly Stopwatch _applicationUptime =
            Stopwatch.StartNew();
        private MemorySnapshot? _memorySnapshot;
        private TimeSpan _applicationElapsed;

        public MainViewModel(
            ICpuMonitor cpuMonitor,
            ICpuFrequencyMonitor cpuFrequencyMonitor,
            ISystemUptimeProvider systemUptimeProvider,
            IProcessorInfoProvider processorInfoProvider,
            IProcessMonitor processMonitor,
            IMemoryMonitor memoryMonitor)
        {
            _cpuMonitor = cpuMonitor;
            _cpuFrequencyMonitor = cpuFrequencyMonitor;
            _systemUptimeProvider = systemUptimeProvider;
            _processorInfoProvider = processorInfoProvider;
            _processMonitor = processMonitor;
            _memoryMonitor = memoryMonitor;
            _processGroupingService =
                new ProcessGroupingService();

            var processorInfo =
                _processorInfoProvider.GetProcessorInfo();
            _processorName = processorInfo.Name;
            _logicalProcessorCount =
                processorInfo.LogicalProcessorCount;
        }

        public double CpuUsagePercent
        {
            get => _cpuUsagePercent;
            private set
            {
                if (Math.Abs(_cpuUsagePercent - value) < 0.01)
                {
                    return;
                }

                _cpuUsagePercent = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CpuUsageText));
            }
        }

        public string CpuUsageText =>
            $"{CpuUsagePercent:F1} %";

        public double CpuFrequencyMhz
        {
            get => _cpuFrequencyMhz;
            private set
            {
                if (Math.Abs(_cpuFrequencyMhz - value) < 0.01)
                {
                    return;
                }

                _cpuFrequencyMhz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CpuFrequencyText));
            }
        }

        public string CpuFrequencyText =>
            $"{CpuFrequencyMhz / 1000.0:F2} GHz";

        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText == value)
                {
                    return;
                }

                _statusText = value;
                OnPropertyChanged();
            }
        }

        public double CpuMaximumFrequencyMhz
        {
            get => _cpuMaximumFrequencyMhz;
            private set
            {
                if (Math.Abs(_cpuMaximumFrequencyMhz - value) < 0.01)
                {
                    return;
                }

                _cpuMaximumFrequencyMhz = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CpuMaximumFrequencyText));
            }
        }

        public string CpuMaximumFrequencyText =>
            $"{CpuMaximumFrequencyMhz / 1000.0:F2} GHz";

        public double CpuFrequencyLimitMhz
        {
            get => _cpuFrequencyLimitMhz;
            private set
            {
                if (Math.Abs(_cpuFrequencyLimitMhz - value) < 0.01)
                {
                    return;
                }

                _cpuFrequencyLimitMhz = value;
                OnPropertyChanged();
            }
        }

        public TimeSpan SystemUptime
        {
            get => _systemUptime;
            private set
            {
                if (_systemUptime == value)
                {
                    return;
                }

                _systemUptime = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SystemUptimeText));
            }
        }

        public TimeSpan ApplicationElapsed
        {
            get => _applicationElapsed;
            private set
            {
                if (_applicationElapsed == value)
                {
                    return;
                }

                _applicationElapsed = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ApplicationElapsedText));
            }
        }

        public string ApplicationElapsedText =>
            $"{(int)ApplicationElapsed.TotalDays}d " +
            $"{ApplicationElapsed.Hours:00}:" +
            $"{ApplicationElapsed.Minutes:00}:" +
            $"{ApplicationElapsed.Seconds:00}";

        public string SystemUptimeText =>
            $"{(int)SystemUptime.TotalDays}d " +
            $"{SystemUptime.Hours:00}:" +
            $"{SystemUptime.Minutes:00}:" +
            $"{SystemUptime.Seconds:00}";

        public string ProcessorName =>
            _processorName;

        public int LogicalProcessorCount =>
            _logicalProcessorCount;

        public IReadOnlyList<ProcessSnapshot> TopProcesses
        {
            get => _topProcesses;
            private set
            {
                _topProcesses = value;
                OnPropertyChanged();
            }
        }

        public IReadOnlyList<ProcessGroupSnapshot> ProcessGroups
        {
            get => _processGroups;
            private set
            {
                _processGroups = value;
                OnPropertyChanged();
            }
        }

        public MemorySnapshot? MemorySnapshot
        {
            get => _memorySnapshot;
            private set
            {
                _memorySnapshot = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MemoryUsageText));
                OnPropertyChanged(nameof(MemoryUsagePercentText));
            }
        }

        public string MemoryUsageText =>
            MemorySnapshot is null
                ? "0.00 / 0.00 GB"
                : $"{MemorySnapshot.UsedGigabytes:F2} / " +
                  $"{MemorySnapshot.TotalGigabytes:F2} GB";

        public string MemoryUsagePercentText =>
            MemorySnapshot is null
                ? "0.0 %"
                : $"{MemorySnapshot.UsagePercent:F1} %";

        public async Task StartMonitoringAsync(
            CancellationToken cancellationToken = default)
        {
            StatusText = "Monitorizando CPU";

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var cpuSnapshot =
                        await _cpuMonitor.GetSnapshotAsync(
                            cancellationToken);
                    var frequencySnapshot =
                        await _cpuFrequencyMonitor.GetSnapshotAsync(
                            cancellationToken);

                    CpuUsagePercent =
                        cpuSnapshot.UsagePercent;
                    CpuFrequencyMhz =
                        frequencySnapshot.CurrentMhz;
                    CpuMaximumFrequencyMhz =
                        frequencySnapshot.MaximumMhz;
                    CpuFrequencyLimitMhz =
                        frequencySnapshot.LimitMhz;
                    SystemUptime =
                        _systemUptimeProvider.GetUptime();
                    ApplicationElapsed =
                        _applicationUptime.Elapsed;
                    MemorySnapshot =
                        _memoryMonitor.GetSnapshot();

                    if (DateTime.UtcNow >= _nextProcessRefreshUtc)
                    {
                        IReadOnlyList<ProcessSnapshot> measuredProcesses =
                            await _processMonitor.GetTopProcessesAsync(
                                int.MaxValue,
                                cancellationToken);

                        TopProcesses =
                            measuredProcesses
                                .OrderByDescending(
                                    process =>
                                        process.CpuUsagePercent)
                                .ThenBy(
                                    process =>
                                        process.ProcessId)
                                .Take(8)
                                .ToArray();

                        ProcessGroups =
                            _processGroupingService
                                .GroupByName(
                                    measuredProcesses)
                                .Take(8)
                                .ToArray();

                        _nextProcessRefreshUtc =
                            DateTime.UtcNow.AddSeconds(3);
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(1),
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                    StatusText =
                        "No fue posible leer la CPU";
                    await Task.Delay(
                        TimeSpan.FromSeconds(2),
                        cancellationToken);
                }
            }

            StatusText = "Monitor detenido";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(
            [CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }
    }
}
