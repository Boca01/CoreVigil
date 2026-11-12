using System.Windows;
using CoreVigil.Services;

namespace CoreVigil
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const string SingleInstanceMutexName =
            "Local\\CoreVigil.SingleInstance";

        private SingleInstanceGuard? _singleInstanceGuard;

        protected override void OnStartup(
            StartupEventArgs e)
        {
            _singleInstanceGuard =
                new SingleInstanceGuard(
                    SingleInstanceMutexName);

            if (!_singleInstanceGuard.TryAcquire())
            {
                System.Windows.MessageBox.Show(
                    "CoreVigil ya esta en ejecucion.",
                    "CoreVigil",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            _singleInstanceGuard?.Dispose();
            _singleInstanceGuard = null;

            base.OnExit(e);
        }
    }
}
