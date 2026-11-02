using System.Windows;

namespace CoreVigil
{
    /// <summary>
    /// Alterna la presentacion de actividad de CPU
    /// entre procesos individuales y grupos por nombre.
    /// </summary>
    public partial class MainWindow
    {
        private void OnGroupedProcessViewClick(
            object sender,
            RoutedEventArgs e)
        {
            GroupedProcessView.Visibility = Visibility.Visible;
            IndividualProcessView.Visibility = Visibility.Collapsed;
            GroupedProcessViewButton.IsEnabled = false;
            IndividualProcessViewButton.IsEnabled = true;
        }

        private void OnIndividualProcessViewClick(
            object sender,
            RoutedEventArgs e)
        {
            GroupedProcessView.Visibility = Visibility.Collapsed;
            IndividualProcessView.Visibility = Visibility.Visible;
            GroupedProcessViewButton.IsEnabled = true;
            IndividualProcessViewButton.IsEnabled = false;
        }
    }
}
