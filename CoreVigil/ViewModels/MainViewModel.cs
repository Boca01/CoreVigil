using System.ComponentModel;
using System.Runtime.CompilerServices;
using CoreVigil.Core.Abstractions;

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
        private double _cpuUsagePercent;
        private double _cpuFrequencyMhz;
        private double _cpuMaximumFrequencyMhz;
        private double _cpuFrequencyLimitMhz;
        private string _statusText = "Preparando monitor...";
        private TimeSpan _systemUptime;
        private string _processorName = "Procesador desconocido";
        private int _logicalProcessorCount;

        public MainViewModel(
            ICpuMonitor cpuMonitor,
            ICpuFrequencyMonitor cpuFrequencyMonitor,
            ISystemUptimeProvider systemUptimeProvider,
            IProcessorInfoProvider processorInfoProvider)
        {
            _cpuMonitor = cpuMonitor;
            _cpuFrequencyMonitor = cpuFrequencyMonitor;
            _systemUptimeProvider = systemUptimeProvider;
            _processorInfoProvider = processorInfoProvider;

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

        public string SystemUptimeText =>
            $"{(int)SystemUptime.TotalDays}d " +
            $"{SystemUptime.Hours:00}:" +
            $"{SystemUptime.Minutes:00}:" +
            $"{SystemUptime.Seconds:00}";

        public string ProcessorName =>
            _processorName;

        public int LogicalProcessorCount =>
            _logicalProcessorCount;
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