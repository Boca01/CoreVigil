namespace CoreVigil.Core.Models
{
    /// <summary>
    /// Representa un grupo de procesos que comparten el mismo nombre
    /// y expone su consumo total de CPU junto con sus instancias.
    /// </summary>
    public sealed class ProcessGroupSnapshot
    {
        public string Name { get; init; } = string.Empty;

        public int InstanceCount { get; init; }

        public double CpuUsagePercent { get; init; }

        public IReadOnlyList<ProcessSnapshot> Processes { get; init; } =
            Array.Empty<ProcessSnapshot>();
    }
}
