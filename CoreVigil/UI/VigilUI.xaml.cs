using System.Windows;
using CoreVigil.Monitoring;
using CoreVigil.ViewModels;
using System.Windows.Input;
using CoreVigil.Services;
using System.ComponentModel;
using CoreVigil.Diagnostics;
using System.Diagnostics;
using System.IO;

namespace CoreVigil.UI
{
    /// <summary>
    /// Ventana principal de presentación de CoreVigil.
    /// </summary>
    public partial class VigilUI : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly CpuFrequencyMonitor _cpuFrequencyMonitor;
        private readonly CancellationTokenSource _monitoringCancellation;
        private readonly HudSettingsService _hudSettingsService;
        private readonly TrayIconService _trayIconService;
        private bool _isExitRequested;

        private readonly DiagnosticsSettingsService
            _diagnosticsSettingsService;

        private RuntimeDiagnosticsService?
            _runtimeDiagnosticsService;

        private RuntimeLogWriter?
            _runtimeLogWriter;

        private CancellationTokenSource?
            _diagnosticsCancellation;
        private readonly StartupService _startupService;
        private readonly HudLayoutService _hudLayoutService;

        private Task? _diagnosticsTask;

        public VigilUI()
        {
            InitializeComponent();

            var cpuMonitor = new CpuMonitor();

            _hudSettingsService = new HudSettingsService();
            _trayIconService = new TrayIconService();
            _startupService = new StartupService();
            _hudLayoutService = new HudLayoutService();
            _diagnosticsSettingsService =
                new DiagnosticsSettingsService();
            _trayIconService.ShowRequested += OnTrayShowRequested;
            _trayIconService.OpacityRequested +=
                OnOpacityRequested;
            _trayIconService.StartupToggleRequested +=
                OnStartupToggleRequested;
            _trayIconService.DiagnosticsToggleRequested +=
                OnDiagnosticsToggleRequested;
            _trayIconService.OpenLogsRequested +=
                 OnOpenLogsRequested;
            // _trayIconService.OpenLogsRequested +=
            //     OnOpenLogsRequested;
            _trayIconService.SetStartupEnabled(
            _startupService.IsEnabled());
            _trayIconService.SetDiagnosticsEnabled(
            _diagnosticsSettingsService.IsEnabled());
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

            _viewModel.PropertyChanged +=
                OnViewModelPropertyChanged;

            ApplyHudLayout();

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
            if (_diagnosticsSettingsService.IsEnabled())
            {
                StartDiagnostics();
            }

            await _viewModel.StartMonitoringAsync(
                _monitoringCancellation.Token);
        }
            private async Task RunDiagnosticsAsync(
             CancellationToken cancellationToken)
        {
            if (_runtimeDiagnosticsService is null ||
                _runtimeLogWriter is null)
            {
                return;
            }

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

        private void StartDiagnostics()
        {
            if (_diagnosticsTask is
                { IsCompleted: false })
            {
                return;
            }

            _runtimeDiagnosticsService =
                new RuntimeDiagnosticsService();

            _runtimeLogWriter =
                new RuntimeLogWriter();

            _diagnosticsCancellation =
                new CancellationTokenSource();

            _diagnosticsTask =
                RunDiagnosticsAsync(
                    _diagnosticsCancellation.Token);
        }

        private async Task StopDiagnosticsAsync()
        {
            if (_diagnosticsCancellation is null)
            {
                return;
            }

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

            if (_runtimeLogWriter is not null)
            {
                await _runtimeLogWriter.FlushAsync();
            }

            _runtimeDiagnosticsService?.Dispose();
            _diagnosticsCancellation.Dispose();

            _runtimeDiagnosticsService = null;
            _runtimeLogWriter = null;
            _diagnosticsCancellation = null;
            _diagnosticsTask = null;
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

            UpdateMiniStatusLed(
                _viewModel.CpuUsagePercent);
        }

        private void ApplyHudLayout()
        {
            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            Width =
                profile.WindowWidth;

            Height =
                profile.WindowHeight;

            HudRoot.Margin =
                new Thickness(
                    profile.OuterMargin);

            HudPanel.Width =
                profile.ContentWidth;

            HudPanel.Height =
                profile.ContentHeight;

            HudPanel.Padding =
                new Thickness(
                    profile.PanelPadding);

            Tag =
                profile.Mode;
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
                    await StopDiagnosticsAsync();

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

        private async void OnDiagnosticsToggleRequested(
    object? sender,
    EventArgs e)
        {
            bool enabled =
                !_diagnosticsSettingsService.IsEnabled();

            if (enabled)
            {
                _diagnosticsSettingsService.SetEnabled(
                    true);

                _trayIconService.SetDiagnosticsEnabled(
                    true);

                StartDiagnostics();
            }
            else
            {
                await StopDiagnosticsAsync();

                _diagnosticsSettingsService.SetEnabled(
                    false);

                _trayIconService.SetDiagnosticsEnabled(
                    false);
            }
        }

        private void OnOpenLogsRequested(
    object? sender,
    EventArgs e)
        {
            string logsDirectory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "CoreVigil",
                    "Logs");

            Directory.CreateDirectory(
                logsDirectory);

            Process.Start(
                new ProcessStartInfo
                {
                    FileName = logsDirectory,
                    UseShellExecute = true
                });
        }

        private void OnWindowClosed(
            object? sender,
            EventArgs e)
        {
            _monitoringCancellation.Cancel();

            _viewModel.PropertyChanged -=
                OnViewModelPropertyChanged;

            _cpuFrequencyMonitor.Dispose();
            _trayIconService.Dispose();
            _monitoringCancellation.Dispose();
        }
    }
}