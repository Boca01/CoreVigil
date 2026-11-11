using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace CoreVigil.UI
{
    /// <summary>
    /// Gestiona el control de opacidad visual del HUD.
    /// </summary>
    public partial class VigilUI
    {
        private const double DefaultBackgroundOpacity =
            0.82;

        private double _backgroundOpacity =
            DefaultBackgroundOpacity;
        private bool _isOpacityPopupDragging;

        private System.Windows.Point _opacityPopupDragStart;

        private double _opacityPopupHorizontalOffsetStart;

        private double _opacityPopupVerticalOffsetStart;
        private double? _opacityPopupHorizontalOffset;
        private double? _opacityPopupVerticalOffset;

        private void OnOpacityRequested(
            object? sender,
            EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Show();

                if (WindowState == WindowState.Minimized)
                {
                    WindowState =
                        WindowState.Normal;
                }

                Activate();

                OpacitySlider.Value =
                    _backgroundOpacity * 100.0;

                UpdateOpacityValueText();

                if (_opacityPopupHorizontalOffset.HasValue &&
                    _opacityPopupVerticalOffset.HasValue)
                {
                    OpacityPopup.HorizontalOffset =
                        _opacityPopupHorizontalOffset.Value;

                    OpacityPopup.VerticalOffset =
                        _opacityPopupVerticalOffset.Value;
                }

                OpacityPopup.IsOpen =
                    true;
            });
        }

        private void OnOpacitySliderValueChanged(
            object sender,
            RoutedPropertyChangedEventArgs<double> e)
        {
            _backgroundOpacity =
                Math.Clamp(
                    e.NewValue / 100.0,
                    0.30,
                    1.0);

            ApplyBackgroundOpacity();
            UpdateOpacityValueText();
        }
        private void OnOpacitySliderReleased(
                object sender,
                MouseButtonEventArgs e)
        {
            SavePersistedHudState();
        }

        private void OnOpacityDragStarted(
    object sender,
    MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            _isOpacityPopupDragging =
                true;

            _opacityPopupDragStart =
                e.GetPosition(this);

            _opacityPopupHorizontalOffsetStart =
                OpacityPopup.HorizontalOffset;

            _opacityPopupVerticalOffsetStart =
                OpacityPopup.VerticalOffset;

            OpacityDragHandle.CaptureMouse();

            e.Handled =
                true;
        }

        private void OnOpacityDragMoved(
            object sender,
            System.Windows.Input.MouseEventArgs e)
        {
            if (!_isOpacityPopupDragging ||
                e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            System.Windows.Point currentPosition =
                e.GetPosition(this);

            double horizontalChange =
                currentPosition.X -
                _opacityPopupDragStart.X;

            double verticalChange =
                currentPosition.Y -
                _opacityPopupDragStart.Y;

            OpacityPopup.HorizontalOffset =
                _opacityPopupHorizontalOffsetStart +
                horizontalChange;

            OpacityPopup.VerticalOffset =
                _opacityPopupVerticalOffsetStart +
                verticalChange;
        }

        private void OnOpacityDragCompleted(
            object sender,
            MouseButtonEventArgs e)
        {
            if (!_isOpacityPopupDragging)
            {
                return;
            }

            _isOpacityPopupDragging =
                false;

            OpacityDragHandle.ReleaseMouseCapture();

            _opacityPopupHorizontalOffset =
                OpacityPopup.HorizontalOffset;

            _opacityPopupVerticalOffset =
                OpacityPopup.VerticalOffset;

            SavePersistedHudState();

            e.Handled =
                true;
        }

        private void ApplyBackgroundOpacity()
        {
            if (HudPanel.Background is SolidColorBrush fullBrush)
            {
                fullBrush.Opacity =
                    _backgroundOpacity;
            }

            if (MiniHudPanel.Background is SolidColorBrush miniBrush)
            {
                miniBrush.Opacity =
                    _backgroundOpacity;
            }
        }

        private void UpdateOpacityValueText()
        {
            if (OpacityValueText is null)
            {
                return;
            }

            OpacityValueText.Text =
                $"{_backgroundOpacity * 100.0:F0} %";
        }
    }
}