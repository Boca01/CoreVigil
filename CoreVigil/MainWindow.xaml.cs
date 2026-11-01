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
        private readonly HudPositionService _hudPositionService;

        public MainWindow()
        {
            InitializeComponent();

            var cpuMonitor = new CpuMonitor();

            _hudPositionService = new HudPositionService();

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

            RestoreHudPosition();

            Loaded += OnWindowLoaded;
            Closing += OnWindowClosing;
            Closed += OnWindowClosed;
            MouseLeftButtonDown += OnWindowMouseLeftButtonDown;
        }


        private async void OnWindowLoaded(
            object sender,
            RoutedEventArgs e)
        {
            await _viewModel.StartMonitoringAsync(
                _monitoringCancellation.Token);
        }

        private void RestoreHudPosition()
        {
            HudPosition? position =
                _hudPositionService.Load();

            if (position is null)
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
                    position.Left,
                    SystemParameters.VirtualScreenLeft,
                    maxLeft);

            Top =
                Math.Clamp(
                    position.Top,
                    SystemParameters.VirtualScreenTop,
                    maxTop);
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
            Close();
        }
        private void OnWindowClosing(
    object? sender,
    CancelEventArgs e)
        {
            _hudPositionService.Save(
                Left,
                Top);
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