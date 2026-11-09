using System.Windows;
using System.Windows.Threading;
using CoreVigil.Services;

namespace CoreVigil.UI
{
    /// <summary>
    /// Ajusta el tamano del HUD conservando el borde inferior
    /// sin persistir posiciones intermedias del layout.
    /// </summary>
    public partial class VigilUI
    {
        private const double CollapsedContentHeight = 350.0;
        private bool _isChangingCpuActivityLayout;

        private async void OnCpuActivityExpanded(
            object sender,
            RoutedEventArgs e)
        {
            await ApplyCpuActivityLayoutAsync(
                isExpanded: true);
        }

        private async void OnCpuActivityCollapsed(
            object sender,
            RoutedEventArgs e)
        {
            await ApplyCpuActivityLayoutAsync(
                isExpanded: false);
        }

        private async Task ApplyCpuActivityLayoutAsync(
            bool isExpanded)
        {
            if (!IsLoaded ||
                _isRestoringPersistedHudState ||
                _isChangingCpuActivityLayout)
            {
                return;
            }

            _isChangingCpuActivityLayout = true;

            try
            {
                double preservedLeft = Left;

                Rect workArea =
                    SystemParameters.WorkArea;

                double currentBottom =
                    Top + ActualHeight;

                const double taskbarSnapTolerance = 24.0;

                bool isAnchoredToTaskbar =
                    Math.Abs(
                        workArea.Bottom -
                        currentBottom) <=
                    taskbarSnapTolerance;

                double preservedBottom =
                    isAnchoredToTaskbar
                        ? workArea.Bottom
                        : currentBottom;

                HudLayoutProfile profile =
                    _hudLayoutService.GetProfile(this);

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

                    HudPanel.Height =
                        CollapsedContentHeight;
                }

                await Dispatcher.InvokeAsync(
                    () => { },
                    DispatcherPriority.Loaded);

                Left = preservedLeft;
                Top = preservedBottom - ActualHeight;
                KeepHudInsideWorkArea();

                await Dispatcher.InvokeAsync(
                    () => { },
                    DispatcherPriority.Loaded);

                SavePersistedHudState();
            }
            finally
            {
                _isChangingCpuActivityLayout = false;
            }
        }

        private void KeepHudInsideWorkArea()
        {
            Rect workArea = SystemParameters.WorkArea;

            double maxLeft =
                workArea.Right - ActualWidth;

            double maxTop =
                workArea.Bottom - ActualHeight;

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
