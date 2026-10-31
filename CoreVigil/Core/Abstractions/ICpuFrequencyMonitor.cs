using CoreVigil.Core.Models;

namespace CoreVigil.Core.Abstractions
{
    /// <summary>
    /// Define el contrato para obtener muestras de frecuencia del procesador.
    /// </summary>
    public interface ICpuFrequencyMonitor
    {
        Task<CpuFrequencySnapshot> GetSnapshotAsync(
            CancellationToken cancellationToken = default);
    }
}