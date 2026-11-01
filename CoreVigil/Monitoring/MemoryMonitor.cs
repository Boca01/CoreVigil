using System.Runtime.InteropServices;
using CoreVigil.Core.Abstractions;
using CoreVigil.Core.Models;

namespace CoreVigil.Monitoring
{
    /// <summary>
    /// Obtiene el estado actual de la memoria física
    /// mediante información proporcionada por Windows.
    /// </summary>
    public sealed class MemoryMonitor : IMemoryMonitor
    {
        public MemorySnapshot GetSnapshot()
        {
            var memoryStatus =
                new MEMORYSTATUSEX
                {
                    dwLength =
                        (uint)Marshal.SizeOf<MEMORYSTATUSEX>()
                };

            if (!GlobalMemoryStatusEx(ref memoryStatus))
            {
                throw new InvalidOperationException(
                    $"No fue posible obtener el estado de la memoria. " +
                    $"Error de Windows: {Marshal.GetLastWin32Error()}.");
            }

            return new MemorySnapshot
            {
                TotalBytes = memoryStatus.ullTotalPhys,
                AvailableBytes = memoryStatus.ullAvailPhys,
                UsagePercent = memoryStatus.dwMemoryLoad,
                Timestamp = DateTime.Now
            };
        }

        [DllImport(
            "kernel32.dll",
            SetLastError = true,
            ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx(
            ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }
    }
}