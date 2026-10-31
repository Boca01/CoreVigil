namespace CoreVigil.Core.Models
{
    /// <summary>
    /// Representa una muestra de la frecuencia del procesador.
    /// </summary>
    public sealed class CpuFrequencySnapshot
    {
        public double CurrentMhz { get; init; }

        public double MaximumMhz { get; init; }

        public double LimitMhz { get; init; }

        public DateTime Timestamp { get; init; }

        public double CurrentGhz =>
            CurrentMhz / 1000.0;

        public double MaximumGhz =>
            MaximumMhz / 1000.0;
    }
}