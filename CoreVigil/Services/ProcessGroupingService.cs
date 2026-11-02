using CoreVigil.Core.Models;

namespace CoreVigil.Services
{
    /// <summary>
    /// Agrupa procesos por nombre exacto y conserva las instancias
    /// individuales ordenadas por consumo reciente de CPU.
    /// </summary>
    public sealed class ProcessGroupingService
    {
        public IReadOnlyList<ProcessGroupSnapshot> GroupByName(
            IReadOnlyList<ProcessSnapshot> processes)
        {
            ArgumentNullException.ThrowIfNull(processes);

            return processes
                .GroupBy(
                    process => process.Name,
                    StringComparer.OrdinalIgnoreCase)
                .Select(group =>
                {
                    ProcessSnapshot[] instances = group
                        .OrderByDescending(
                            process => process.CpuUsagePercent)
                        .ThenBy(
                            process => process.ProcessId)
                        .ToArray();

                    return new ProcessGroupSnapshot
                    {
                        Name = group.Key,
                        InstanceCount = instances.Length,
                        CpuUsagePercent = instances.Sum(
                            process => process.CpuUsagePercent),
                        Processes = instances
                    };
                })
                .OrderByDescending(
                    group => group.CpuUsagePercent)
                .ThenBy(
                    group => group.Name,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
