using System.Windows;
using CoreVigil.Services;

namespace CoreVigil
{
    /// <summary>
    /// Persiste la geometria visible y el estado expandido del HUD.
    /// </summary>
    public partial class MainWindow
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
            if (_isRestoringPersistedHudState || !IsLoaded)
            {
                return;
            }

            SavePersistedHudState();
        }

        private void OnPersistedVisibilityChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
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
