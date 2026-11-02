using System.Windows;
using CoreVigil.Services;

namespace CoreVigil.UI
{
    /// <summary>
    /// Ajusta el tamano del HUD conservando exactamente
    /// la posicion de su borde inferior.
    /// </summary>
    public partial class VigilUI
    {
        private const double CollapsedContentHeight = 350.0;

        private void OnCpuActivityExpanded(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsLoaded || _isRestoringPersistedHudState)
            {
                return;
            }

            ApplyCpuActivityLayout(
                isExpanded: true);
        }

        private void OnCpuActivityCollapsed(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsLoaded || _isRestoringPersistedHudState)
            {
                return;
            }

            ApplyCpuActivityLayout(
                isExpanded: false);
        }

        private void ApplyCpuActivityLayout(
            bool isExpanded)
        {
            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            // El borde inferior es la referencia estable.
            // Evita acumular desplazamientos en ciclos repetidos.
            double anchoredBottom = Top + Height;

            Width = profile.WindowWidth;
            HudPanel.Width = profile.ContentWidth;

            if (isExpanded)
            {
                Height = profile.WindowHeight;
                HudPanel.Height = profile.ContentHeight;
            }
            else
            {
                double removedContentHeight =
                    profile.ContentHeight -
                    CollapsedContentHeight;

                Height = Math.Max(
                    320.0,
                    profile.WindowHeight -
                    removedContentHeight);

                HudPanel.Height = CollapsedContentHeight;
            }

            Top = anchoredBottom - Height;

            KeepHudInsideWorkArea();
            SavePersistedHudState();
        }

        private void KeepHudInsideWorkArea()
        {
            Rect workArea = SystemParameters.WorkArea;

            double maxLeft = workArea.Right - Width;
            double maxTop = workArea.Bottom - Height;

            Left = Math.Clamp(
                Left,
                workArea.Left,
                maxLeft);

            Top = Math.Clamp(
                Top,
                workArea.Top,
                maxTop);
        }
    }
}
