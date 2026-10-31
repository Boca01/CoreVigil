using Microsoft.Win32;
using CoreVigil.Core.Abstractions;
using CoreVigil.Core.Models;

namespace CoreVigil.Monitoring
{
    /// <summary>
    /// Obtiene información estática del procesador
    /// disponible en Windows.
    /// </summary>
    public sealed class WindowsProcessorInfoProvider :
        IProcessorInfoProvider
    {
        private const string ProcessorRegistryPath =
            @"HARDWARE\DESCRIPTION\System\CentralProcessor\0";

        public ProcessorInfo GetProcessorInfo()
        {
            string processorName = "Procesador desconocido";

            using RegistryKey? processorKey =
                Registry.LocalMachine.OpenSubKey(
                    ProcessorRegistryPath);

            if (processorKey is not null)
            {
                processorName =
                    processorKey.GetValue("ProcessorNameString")
                        ?.ToString()
                        ?.Trim()
                    ?? processorName;
            }

            return new ProcessorInfo
            {
                Name = processorName,
                LogicalProcessorCount = Environment.ProcessorCount
            };
        }
    }
}