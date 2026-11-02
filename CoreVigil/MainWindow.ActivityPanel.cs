using System.Windows;
using CoreVigil.Services;

namespace CoreVigil
{
    /// <summary>
    /// Ajusta el tamano y mantiene anclado el borde inferior del HUD
    /// cuando la actividad de CPU se despliega o se contrae.
    /// </summary>
    public partial class MainWindow
    {
        private const double CollapsedContentHeight = 350.0;
        private double? _expandedTop;

        private void OnCpuActivityExpanded(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            double previousHeight = Height;
            double targetHeight = profile.WindowHeight;

            Width = profile.WindowWidth;
            Height = targetHeight;
            HudPanel.Width = profile.ContentWidth;
            HudPanel.Height = profile.ContentHeight;

            if (_expandedTop.HasValue)
            {
                Top = _expandedTop.Value;
                _expandedTop = null;
            }
            else
            {
                Top -= targetHeight - previousHeight;
            }

            KeepHudInsideWorkArea();
            SavePersistedHudState();
        }

        private void OnCpuActivityCollapsed(
            object sender,
            RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }

            HudLayoutProfile profile =
                _hudLayoutService.GetProfile(this);

            _expandedTop = Top;

            double previousHeight = Height;
            double removedContentHeight =
                profile.ContentHeight -
                CollapsedContentHeight;

            double targetHeight = Math.Max(
                320.0,
                profile.WindowHeight -
                removedContentHeight);

            Width = profile.WindowWidth;
            Height = targetHeight;
            HudPanel.Width = profile.ContentWidth;
            HudPanel.Height = CollapsedContentHeight;

            // Mantiene fijo el borde inferior: el HUD se recoge hacia abajo.
            Top += previousHeight - targetHeight;

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
