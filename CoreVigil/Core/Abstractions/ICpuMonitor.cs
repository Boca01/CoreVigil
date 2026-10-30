using CoreVigil.Core.Models;

namespace CoreVigil.Core.Abstractions
{
    /// <summary>
    /// Define el contrato para obtener muestras del uso global del procesador.
    /// </summary>
    public interface ICpuMonitor
    {
        Task<CpuSnapshot> GetSnapshotAsync(
            CancellationToken cancellationToken = default);
    }
}