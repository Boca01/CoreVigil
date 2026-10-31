using CoreVigil.Core.Models;

namespace CoreVigil.Core.Abstractions
{
    /// <summary>
    /// Define el contrato para obtener los procesos
    /// con mayor consumo reciente de CPU.
    /// </summary>
    public interface IProcessMonitor
    {
        Task<IReadOnlyList<ProcessSnapshot>> GetTopProcessesAsync(
            int count,
            CancellationToken cancellationToken = default);
    }
}