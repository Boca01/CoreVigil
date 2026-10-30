namespace CoreVigil.Core.Models
{
    /// <summary>
    /// Representa una muestra del estado global del procesador.
    /// </summary>
    public sealed class CpuSnapshot
    {
        public double UsagePercent { get; init; }

        public DateTime Timestamp { get; init; }
    }
}