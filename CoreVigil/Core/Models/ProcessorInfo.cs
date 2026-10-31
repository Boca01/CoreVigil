namespace CoreVigil.Core.Models
{
    /// <summary>
    /// Representa información estática del procesador instalado.
    /// </summary>
    public sealed class ProcessorInfo
    {
        public string Name { get; init; } = "Procesador desconocido";

        public int LogicalProcessorCount { get; init; }
    }
}