namespace CoreVigil.Core.Models
{
    /// <summary>
    /// Representa una muestra del estado de la memoria física del sistema.
    /// </summary>
    public sealed class MemorySnapshot
    {
        public ulong TotalBytes { get; init; }

        public ulong AvailableBytes { get; init; }

        public ulong UsedBytes =>
            TotalBytes >= AvailableBytes
                ? TotalBytes - AvailableBytes
                : 0;

        public double UsagePercent { get; init; }

        public DateTime Timestamp { get; init; }

        public double TotalGigabytes =>
            TotalBytes / 1024.0 / 1024.0 / 1024.0;

        public double AvailableGigabytes =>
            AvailableBytes / 1024.0 / 1024.0 / 1024.0;

        public double UsedGigabytes =>
            UsedBytes / 1024.0 / 1024.0 / 1024.0;
    }
}