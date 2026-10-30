using System.ComponentModel;
using System.Runtime.CompilerServices;
using CoreVigil.Core.Abstractions;

namespace CoreVigil.ViewModels
{
    /// <summary>
    /// Expone a la interfaz los datos obtenidos por el monitor del sistema.
    /// </summary>
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private readonly ICpuMonitor _cpuMonitor;

        private double _cpuUsagePercent;
        private string _statusText = "Preparando monitor...";

        public MainViewModel(ICpuMonitor cpuMonitor)
        {
            _cpuMonitor = cpuMonitor;
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

        public async Task StartMonitoringAsync(
            CancellationToken cancellationToken = default)
        {
            StatusText = "Monitorizando CPU";

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var snapshot =
                        await _cpuMonitor.GetSnapshotAsync(cancellationToken);

                    CpuUsagePercent = snapshot.UsagePercent;

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
                    StatusText = "No fue posible leer la CPU";

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