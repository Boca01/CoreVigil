using System.Threading;
using System.Windows;

namespace CoreVigil
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private const string SingleInstanceMutexName =
            "Local\\CoreVigil.SingleInstance";

        private Mutex? _singleInstanceMutex;

        protected override void OnStartup(
            StartupEventArgs e)
        {
            _singleInstanceMutex = new Mutex(
                initiallyOwned: true,
                name: SingleInstanceMutexName,
                createdNew: out bool createdNew);

            if (!createdNew)
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
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
            _singleInstanceMutex = null;

            base.OnExit(e);
        }
    }
}
