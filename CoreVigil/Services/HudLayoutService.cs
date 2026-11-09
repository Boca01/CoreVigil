using System.Windows;
using System.Windows.Media;

namespace CoreVigil.Services
{
    /// <summary>
    /// Selecciona un perfil visual apropiado para el HUD
    /// según el área útil y la escala DPI de la pantalla.
    /// </summary>
    public sealed class HudLayoutService
    {
        public HudLayoutProfile GetProfile(
            Window window)
        {
            ArgumentNullException.ThrowIfNull(window);

            Rect workArea =
                SystemParameters.WorkArea;

            DpiScale dpi =
                VisualTreeHelper.GetDpi(window);

            double effectiveWidth =
                workArea.Width / dpi.DpiScaleX;

            double effectiveHeight =
                workArea.Height / dpi.DpiScaleY;

            bool requiresCompactLayout =
                effectiveWidth < 1200.0 ||
                effectiveHeight < 720.0 ||
                dpi.DpiScaleX >= 1.50 ||
                dpi.DpiScaleY >= 1.50;

            if (requiresCompactLayout)
            {
                return new HudLayoutProfile
                {
                    Mode = HudLayoutMode.Compact,
                    WindowWidth = 320.0,
                    WindowHeight = 390.0,
                    ContentWidth = 330.0,
                    ContentHeight = 438.0,
                    OuterMargin = 8.0,
                    PanelPadding = 14.0,
                    DpiScale = dpi.DpiScaleX
                };
            }

            bool supportsSpaciousLayout =
                effectiveWidth >= 1800.0 &&
                effectiveHeight >= 1000.0 &&
                dpi.DpiScaleX <= 1.25 &&
                dpi.DpiScaleY <= 1.25;

            if (supportsSpaciousLayout)
            {
                return new HudLayoutProfile
                {
                    Mode = HudLayoutMode.Spacious,
                    WindowWidth = 450.0,
                    WindowHeight = 485.0,
                    ContentWidth = 404.0,
                    ContentHeight = 438.0,
                    OuterMargin = 18.0,
                    PanelPadding = 22.0,
                    DpiScale = dpi.DpiScaleX
                };
            }

            return new HudLayoutProfile
            {
                Mode = HudLayoutMode.Standard,
                WindowWidth = 432.0,
                WindowHeight = 466.0,
                ContentWidth = 404.0,
                ContentHeight = 438.0,
                OuterMargin = 14.0,
                PanelPadding = 18.0,
                DpiScale = dpi.DpiScaleX
            };
        }
    }
}