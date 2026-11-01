using System.Windows;
using CoreVigil.Monitoring;
using CoreVigil.ViewModels;
using System.Windows.Input;
using CoreVigil.Services;
using System.ComponentModel;
using CoreVigil.Diagnostics;

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
        private readonly HudSettingsService _hudSettingsService;
        private readonly TrayIconService _trayIconService;
        private readonly StartupService _startupService;
        private bool _isExitRequested;

        private readonly RuntimeDiagnosticsService
    _runtimeDiagnosticsService;

        private readonly RuntimeLogWriter
            _runtimeLogWriter;

        private readonly CancellationTokenSource
            _diagnosticsCancellation;

        private Task? _diagnosticsTask;

        public MainWindow()
        {
            InitializeComponent();

            var cpuMonitor = new CpuMonitor();

            _hudSettingsService = new HudSettingsService();
            _trayIconService = new TrayIconService();
            _startupService = new StartupService();
            _runtimeDiagnosticsService =
    new RuntimeDiagnosticsService();

            _runtimeLogWriter =
                new RuntimeLogWriter();

            _diagnosticsCancellation =
                new CancellationTokenSource();
            _trayIconService.ShowRequested += OnTrayShowRequested;
            _trayIconService.StartupToggleRequested +=
                OnStartupToggleRequested;

            _trayIconService.SetStartupEnabled(
            _startupService.IsEnabled());
            _trayIconService.ExitRequested += OnTrayExitRequested;

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

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            RestoreHudSettings();

            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;
            Closed += OnWindowClosed;
            MouseLeftButtonDown += OnWindowMouseLeftButtonDown;
        }

        private void OnWindowMouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private async void OnWindowLoaded(
            object sender,
            RoutedEventArgs e)
        {
            _diagnosticsTask =
                RunDiagnosticsAsync(
                    _diagnosticsCancellation.Token);

            await _viewModel.StartMonitoringAsync(
                _monitoringCancellation.Token);
        }

        private async Task RunDiagnosticsAsync(
    CancellationToken cancellationToken)
        {
            DateTime nextFlushUtc =
                DateTime.UtcNow.AddSeconds(30);

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    RuntimeSample sample =
                        _runtimeDiagnosticsService.Capture(
                            _viewModel.ApplicationElapsed,
                            _viewModel.CpuUsagePercent,
                            _viewModel.CpuFrequencyMhz,
                            _viewModel.MemorySnapshot?.UsagePercent
                                ?? 0.0,
                            IsVisible &&
                                WindowState != WindowState.Minimized,
                            Topmost,
                            _trayIconService.CurrentState.ToString());

                    _runtimeLogWriter.Add(sample);

                    if (DateTime.UtcNow >= nextFlushUtc)
                    {
                        await _runtimeLogWriter.FlushAsync();

                        nextFlushUtc =
                            DateTime.UtcNow.AddSeconds(30);
                    }

                    await Task.Delay(
                        TimeSpan.FromSeconds(1),
                        cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private void OnViewModelPropertyChanged(
            object? sender,
            PropertyChangedEventArgs e)
        {
            if (e.PropertyName !=
                nameof(MainViewModel.CpuUsagePercent))
            {
                return;
            }

            _trayIconService.UpdateCpuUsage(
                _viewModel.CpuUsagePercent);
        }

        private void RestoreHudSettings()
        {
            HudSettings? settings =
                _hudSettingsService.Load();

            if (settings is null)
            {
                return;
            }

            double maxLeft =
                SystemParameters.VirtualScreenLeft +
                SystemParameters.VirtualScreenWidth -
                ActualWidth;

            double maxTop =
                SystemParameters.VirtualScreenTop +
                SystemParameters.VirtualScreenHeight -
                ActualHeight;

            Left =
                Math.Clamp(
                    settings.Left,
                    SystemParameters.VirtualScreenLeft,
                    maxLeft);

            Top =
                Math.Clamp(
                    settings.Top,
                    SystemParameters.VirtualScreenTop,
                    maxTop);

            Topmost = settings.Topmost;

            TopmostButton.Content =
                Topmost
                    ? "●"
                    : "○";

            TopmostButton.Foreground =
                Topmost
                    ? new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            111,
                            168,
                            255))
                    : new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            127,
                            137,
                            153));
        }
        private void OnTopmostButtonClick(
    object sender,
    RoutedEventArgs e)
        {
            Topmost = !Topmost;

            TopmostButton.Content =
                Topmost
                    ? "●"
                    : "○";

            TopmostButton.Foreground =
                Topmost
                    ? new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            111,
                            168,
                            255))
                    : new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            127,
                            137,
                            153));
        }

        private void OnMinimizeButtonClick(
            object sender,
            RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnCloseButtonClick(
            object sender,
            RoutedEventArgs e)
        {
            Hide();
        }
        private void OnTrayShowRequested(
    object? sender,
    EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Show();

                if (WindowState == WindowState.Minimized)
                {
                    WindowState = WindowState.Normal;
                }

                Activate();
            });
        }

        private async void OnTrayExitRequested(
            object? sender,
            EventArgs e)
        {
            await Dispatcher.InvokeAsync(
                async () =>
                {
                    _diagnosticsCancellation.Cancel();

                    if (_diagnosticsTask is not null)
                    {
                        try
                        {
                            await _diagnosticsTask;
                        }
                        catch (OperationCanceledException)
                        {
                        }
                    }

                    await _runtimeLogWriter.FlushAsync();

                    _isExitRequested = true;

                    Close();
                });
        }

        private void OnStartupToggleRequested(
    object? sender,
    EventArgs e)
        {
            bool enabled =
                !_startupService.IsEnabled();

            _startupService.SetEnabled(enabled);

            _trayIconService.SetStartupEnabled(
                enabled);
        }
        private void OnWindowClosing(
            object? sender,
            CancelEventArgs e)
        {
            if (!_isExitRequested)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            _hudSettingsService.Save(
                Left,
                Top,
                Topmost);
        }

        private void OnWindowClosed(
            object? sender,
            EventArgs e)
        {
            _monitoringCancellation.Cancel();
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _cpuFrequencyMonitor.Dispose();
            _trayIconService.Dispose();
            _runtimeDiagnosticsService.Dispose();
            _diagnosticsCancellation.Dispose();
            _monitoringCancellation.Dispose();
        }
    }
}