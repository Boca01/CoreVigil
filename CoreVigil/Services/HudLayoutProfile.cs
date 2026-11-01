namespace CoreVigil.Services
{
    /// <summary>
    /// Describe el perfil visual que CoreVigil utilizará
    /// según el espacio disponible y la escala de pantalla.
    /// </summary>
    public sealed class HudLayoutProfile
    {
        public HudLayoutMode Mode { get; init; }

        public double WindowWidth { get; init; }

        public double WindowHeight { get; init; }

        public double ContentWidth { get; init; }

        public double ContentHeight { get; init; }

        public double OuterMargin { get; init; }

        public double PanelPadding { get; init; }

        public double DpiScale { get; init; }
    }

    public enum HudLayoutMode
    {
        Compact,
        Standard,
        Spacious
    }
}