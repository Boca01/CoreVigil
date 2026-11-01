using CoreVigil.Core.Models;

namespace CoreVigil.Core.Abstractions
{
    /// <summary>
    /// Define el contrato para obtener muestras
    /// del estado de la memoria física del sistema.
    /// </summary>
    public interface IMemoryMonitor
    {
        MemorySnapshot GetSnapshot();
    }
}