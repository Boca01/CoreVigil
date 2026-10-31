using CoreVigil.Core.Models;

namespace CoreVigil.Core.Abstractions
{
    /// <summary>
    /// Define el contrato para obtener información estática
    /// del procesador instalado.
    /// </summary>
    public interface IProcessorInfoProvider
    {
        ProcessorInfo GetProcessorInfo();
    }
}