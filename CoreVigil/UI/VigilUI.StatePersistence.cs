using System.Windows;
using CoreVigil.Services;

namespace CoreVigil.UI
{
    /// <summary>
    /// Persiste la geometria visible y el estado expandido del HUD.
    /// Los cambios internos de layout no se interpretan como movimientos del usuario.
    /// </summary>
    public partial class VigilUI
    {
        private bool _isRestoringPersistedHudState;

        private void OnPersistedStateLoaded(
            object sender,
            RoutedEventArgs e)
        {
            RestorePersistedVisualState();
        }

        private void OnPersistedLocationChanged(
            object? sender,
            EventArgs e)
        {
            // No guardar aqui.
            // LocationChanged tambien se dispara cuando el propio HUD cambia
            // Top durante expandir/contraer, lo que contaminaba la posicion.
        }

        private void OnPersistedVisibilityChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (_isRestoringPersistedHudState || (bool)e.NewValue)
            {
                return;
            }

            SavePersistedHudState();
        }

        private void RestorePersistedVisualState()
        {
            HudSettings? settings =
                _hudSettingsService.Load();

            if (settings is null)
            {
                return;
            }

            _isRestoringPersistedHudState = true;

            try
            {
                CpuActivityExpander.IsExpanded =
                    settings.IsCpuActivityExpanded;

                Topmost =
                    settings.Topmost;

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

                _fullHudLeft =
                    settings.FullLeft ??
                    settings.Left ??
                    Left;

                _fullHudTop =
                    settings.FullTop ??
                    settings.Top ??
                    Top;

                _miniHudLeft =
                    settings.MiniLeft ??
                    double.NaN;

                _miniHudTop =
                    settings.MiniTop ??
                    double.NaN;

                if (settings.IsMiniMode)
                {
                    RestorePersistedMiniMode();
                }
                else
                {
                    RestorePersistedFullMode(
                        settings.IsCpuActivityExpanded);
                }
            }
            finally
            {
                _isRestoringPersistedHudState =
                    false;
            }
        }

        private void RestorePersistedFullMode(
    bool isExpanded)
        {
            MiniHudPanel.Visibility =
                Visibility.Collapsed;

            HudPanel.Visibility =
                Visibility.Visible;

            // El modo Full restaurado al iniciar vuelve a comportarse
            // como una ventana normal en la barra de tareas.
            ShowInTaskbar =
                true;

            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            HudRoot.Margin =
                new Thickness(
                    profile.OuterMargin);

            HudPanel.Padding =
                new Thickness(
                    profile.PanelPadding);

            ApplyPersistedExpansionLayout(
                isExpanded);

            Left =
                _fullHudLeft;

            Top =
                _fullHudTop;

            KeepFullHudInsideWorkArea();
        }

        private void RestorePersistedMiniMode()
        {
            HudPanel.Visibility =
                Visibility.Collapsed;

            MiniHudPanel.Visibility =
                Visibility.Visible;

            // El modo Mini restaurado al iniciar conserva el mismo comportamiento
            // que cuando el usuario cambia manualmente desde el modo Full.
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
                    _fullHudTop;
            }

            KeepMiniHudInsideWorkArea();
        }

        private void ApplyPersistedExpansionLayout(
            bool isExpanded)
        {
            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            Width = profile.WindowWidth;
            HudPanel.Width = profile.ContentWidth;

            if (isExpanded)
            {
                Height = profile.WindowHeight;
                HudPanel.Height = profile.ContentHeight;
                return;
            }

            double removedContentHeight =
                profile.ContentHeight -
                CollapsedContentHeight;

            Height = Math.Max(
                320.0,
                profile.WindowHeight -
                removedContentHeight);

            HudPanel.Height = CollapsedContentHeight;
        }

        private void SavePersistedHudState()
        {
            bool isMiniMode =
                IsMiniHudMode;

            if (isMiniMode)
            {
                _miniHudLeft =
                    Left;

                _miniHudTop =
                    Top;
            }
            else
            {
                _fullHudLeft =
                    Left;

                _fullHudTop =
                    Top;
            }

            _hudSettingsService.Save(
                fullLeft:
                    _fullHudLeft,
                fullTop:
                    _fullHudTop,
                miniLeft:
                    double.IsNaN(_miniHudLeft)
                        ? null
                        : _miniHudLeft,
                miniTop:
                    double.IsNaN(_miniHudTop)
                        ? null
                        : _miniHudTop,
                topmost:
                    Topmost,
                isCpuActivityExpanded:
                    CpuActivityExpander.IsExpanded,
                isMiniMode:
                    isMiniMode);
        }
    }
}
