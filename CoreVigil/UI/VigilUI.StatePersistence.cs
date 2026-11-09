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
            HudSettings? settings = _hudSettingsService.Load();
            if (settings is null)
            {
                return;
            }

            _isRestoringPersistedHudState = true;

            try
            {
                CpuActivityExpander.IsExpanded =
                    settings.IsCpuActivityExpanded;

                ApplyPersistedExpansionLayout(
                    settings.IsCpuActivityExpanded);

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

                Rect workArea =
                    SystemParameters.WorkArea;

                double maxLeft =
                    workArea.Right - Width;

                double maxTop =
                    workArea.Bottom - Height;

                Left = Math.Clamp(
                    settings.Left,
                    workArea.Left,
                    maxLeft);

                Top = Math.Clamp(
                    settings.Top,
                    workArea.Top,
                    maxTop);
            }
            finally
            {
                _isRestoringPersistedHudState = false;
            }
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
            _hudSettingsService.Save(
                Left,
                Top,
                Topmost,
                CpuActivityExpander.IsExpanded);
        }
    }
}
