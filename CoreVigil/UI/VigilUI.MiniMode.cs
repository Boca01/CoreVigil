using CoreVigil.Services;
using System.Windows;
using System.Windows.Media;

namespace CoreVigil.UI
{
    /// <summary>
    /// Gestiona el modo Mini del HUD sin alterar
    /// el sistema de monitorizacion de CoreVigil.
    /// </summary>
    public partial class VigilUI

    {
        private double _fullHudLeft;
        private double _fullHudTop;

        private double _miniHudLeft =
            double.NaN;

        private double _miniHudTop =
            double.NaN;
        private bool IsMiniHudMode =>
    MiniHudPanel.Visibility ==
    Visibility.Visible;
        private void UpdateMiniStatusLed(
    double cpuUsagePercent)
        {
            System.Windows.Media.Color ledColor =
                cpuUsagePercent switch
                {
                    < 40.0 => System.Windows.Media. Color.FromRgb(
                        76,
                        141,
                        255),

                    < 70.0 => System.Windows.Media.Color.FromRgb(
                        69,
                        201,
                        116),

                    < 90.0 => System.Windows.Media.Color.FromRgb(
                        255,
                        170,
                        64),

                    _ => System.Windows.Media.Color.FromRgb(
                        255,
                        82,
                        82)
                };

            MiniStatusLed.Fill =
                new SolidColorBrush(
                    ledColor);

            MiniStatusLedGlow.Color =
                ledColor;
        }

        private void OnRestoreFullHudClick(
            object sender,
            RoutedEventArgs e)
        {
            _miniHudLeft =
                Left;

            _miniHudTop =
                Top;

            MiniHudPanel.Visibility =
                Visibility.Collapsed;

            HudPanel.Visibility =
                Visibility.Visible;

            // El modo Full vuelve a comportarse como una ventana normal
            // y recupera su acceso desde la barra de tareas.
            ShowInTaskbar =
                true;

            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            Width =
                profile.WindowWidth;

            Height =
                CpuActivityExpander.IsExpanded
                    ? profile.WindowHeight
                    : Math.Max(
                        320.0,
                        profile.WindowHeight -
                        (profile.ContentHeight -
                         CollapsedContentHeight));

            HudRoot.Margin =
                new Thickness(
                    profile.OuterMargin);

            HudPanel.Width =
                profile.ContentWidth;

            HudPanel.Height =
                CpuActivityExpander.IsExpanded
                    ? profile.ContentHeight
                    : CollapsedContentHeight;

            HudPanel.Padding =
                new Thickness(
                    profile.PanelPadding);

            Left =
                _fullHudLeft;

            Top =
                _fullHudTop;

            KeepFullHudInsideWorkArea();
        }

        private void KeepMiniHudInsideWorkArea()
        {
            Rect workArea =
                SystemParameters.WorkArea;

            double maxLeft =
                workArea.Right - Width;

            double maxTop =
                workArea.Bottom - Height;

            Left =
                Math.Clamp(
                    Left,
                    workArea.Left,
                    maxLeft);

            Top =
                Math.Clamp(
                    Top,
                    workArea.Top,
                    maxTop);
        }

        private void KeepFullHudInsideWorkArea()
        {
            Rect workArea =
                SystemParameters.WorkArea;

            double maxLeft =
                workArea.Right - Width;

            double maxTop =
                workArea.Bottom - Height;

            Left =
                Math.Clamp(
                    Left,
                    workArea.Left,
                    maxLeft);

            Top =
                Math.Clamp(
                    Top,
                    workArea.Top,
                    maxTop);
        }

        private void OnMiniModeButtonClick(
            object sender,
            RoutedEventArgs e)
        {
            _fullHudLeft =
                Left;

            _fullHudTop =
                Top;

            double fullHudBottom =
                Top + ActualHeight;

            HudPanel.Visibility =
                Visibility.Collapsed;

            MiniHudPanel.Visibility =
                Visibility.Visible;

            // El modo Mini permanece accesible desde la bandeja del sistema
            // y no necesita ocupar espacio en la barra de tareas.
            ShowInTaskbar =
                false;

            HudRoot.Margin =
                new Thickness(0);

            Width =
                230.0;

            Height =
                42.0;

            if (!double.IsNaN(_miniHudLeft) &&
                !double.IsNaN(_miniHudTop))
            {
                Left =
                    _miniHudLeft;

                Top =
                    _miniHudTop;
            }
            else
            {
                Left =
                    _fullHudLeft;

                Top =
                    fullHudBottom - Height;
            }

            KeepMiniHudInsideWorkArea();
        }
    }
}