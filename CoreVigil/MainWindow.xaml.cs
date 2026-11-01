using System.Windows;
using CoreVigil.Monitoring;
using CoreVigil.ViewModels;
using System.Windows.Input;
using CoreVigil.Services;
using System.ComponentModel;

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
        private bool _isExitRequested;

        public MainWindow()
        {
            InitializeComponent();

            var cpuMonitor = new CpuMonitor();

            _hudSettingsService = new HudSettingsService();
            _trayIconService = new TrayIconService();
            _trayIconService.ShowRequested += OnTrayShowRequested;

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
            await _viewModel.StartMonitoringAsync(
                _monitoringCancellation.Token);
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

        private void OnTrayExitRequested(
                object? sender,
                EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _isExitRequested = true;
                Close();
            });
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
            _cpuFrequencyMonitor.Dispose();
            _trayIconService.Dispose();
            _monitoringCancellation.Dispose();
        }
    }
}