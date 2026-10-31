namespace CoreVigil.Core.Models
{
    /// <summary>
    /// Representa una muestra del consumo de CPU de un proceso.
    /// </summary>
    public sealed class ProcessSnapshot
    {
        public int ProcessId { get; init; }

        public string Name { get; init; } = string.Empty;

        public double CpuUsagePercent { get; init; }
    }
}