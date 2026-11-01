using System.Windows;
using CoreVigil.Monitoring;
using CoreVigil.ViewModels;

namespace CoreVigil
{
    /// <summary>
    /// Ventana principal de presentación de CoreVigil.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly CpuFrequencyMonitor _cpuFrequencyMonitor;
        private readonly CancellationTokenSource _monitoringCancellation;

        public MainWindow()
        {
            InitializeComponent();

            var cpuMonitor = new CpuMonitor();

            var systemUptimeProvider =
                new SystemUptimeProvider();

            var processorInfoProvider =
                new WindowsProcessorInfoProvider();

            var processMonitor =
                new ProcessMonitor();

            var memoryMonitor =
                new MemoryMonitor();

            _cpuFrequencyMonitor =
                new CpuFrequencyMonitor();

            _viewModel = new MainViewModel(
                cpuMonitor,
                _cpuFrequencyMonitor,
                systemUptimeProvider,
                processorInfoProvider,
                processMonitor,
                memoryMonitor);

            _monitoringCancellation = new CancellationTokenSource();

            DataContext = _viewModel;

            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
        }

        private async void OnWindowLoaded(
            object sender,
            RoutedEventArgs e)
        {
            await _viewModel.StartMonitoringAsync(
                _monitoringCancellation.Token);
        }

        private void OnWindowClosed(
            object? sender,
            EventArgs e)
        {
            _monitoringCancellation.Cancel();

            _cpuFrequencyMonitor.Dispose();
            _monitoringCancellation.Dispose();
        }
    }
}