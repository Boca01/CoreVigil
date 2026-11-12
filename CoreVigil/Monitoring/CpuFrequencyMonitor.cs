using System.Runtime.InteropServices;
using CoreVigil.Core.Abstractions;
using CoreVigil.Core.Models;

namespace CoreVigil.Monitoring
{
    /// <summary>
    /// Obtiene información de frecuencia del procesador combinando
    /// información de energía de Windows con el rendimiento dinámico
    /// reportado por PDH.
    /// </summary>
    public sealed class CpuFrequencyMonitor :
        ICpuFrequencyMonitor,
        IDisposable
    {
        private const int ProcessorInformation = 11;
        private const uint ErrorSuccess = 0;
        private const uint PdhCStatusValidData = 0;
        private const uint PdhCStatusNewData = 1;
        private const uint PdhFormatDouble = 0x00000200;

        private const string ProcessorPerformancePath =
            @"\Processor Information(_Total)\% Processor Performance";

        private IntPtr _queryHandle;
        private IntPtr _counterHandle;
        private bool _hasInitialSample;
        private bool _disposed;

        public CpuFrequencyMonitor()
        {
            OpenPerformanceQuery();
        }

        public async Task<CpuFrequencySnapshot> GetSnapshotAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GetProcessorPowerInformation(
                out double currentMhz,
                out double maximumMhz,
                out double limitMhz);

            double? performancePercent =
                ReadProcessorPerformance();

            if (!performancePercent.HasValue && currentMhz <= 0.0)
            {
                await Task.Delay(
                    TimeSpan.FromMilliseconds(250),
                    cancellationToken);

                GetProcessorPowerInformation(
                    out currentMhz,
                    out maximumMhz,
                    out limitMhz);

                performancePercent =
                    ReadProcessorPerformance();
            }

            double measuredCurrentMhz =
                CalculateCurrentFrequencyMhz(
                    currentMhz,
                    maximumMhz,
                    performancePercent);

            return new CpuFrequencySnapshot
            {
                CurrentMhz = measuredCurrentMhz,
                MaximumMhz = maximumMhz,
                LimitMhz = limitMhz,
                Timestamp = DateTime.Now
            };
        }

        internal static double CalculateCurrentFrequencyMhz(
            double nativeCurrentMhz,
            double maximumMhz,
            double? performancePercent)
        {
            if (!performancePercent.HasValue ||
                !double.IsFinite(performancePercent.Value) ||
                performancePercent.Value < 0.0)
            {
                return Math.Max(0.0, nativeCurrentMhz);
            }

            return Math.Max(
                0.0,
                maximumMhz * performancePercent.Value / 100.0);
        }

        private void OpenPerformanceQuery()
        {
            uint status = PdhOpenQuery(
                null,
                IntPtr.Zero,
                out _queryHandle);

            ThrowIfPdhFailed(
                status,
                "No fue posible abrir la consulta PDH.");

            status = PdhAddEnglishCounter(
                _queryHandle,
                ProcessorPerformancePath,
                IntPtr.Zero,
                out _counterHandle);

            if (status != ErrorSuccess)
            {
                PdhCloseQuery(_queryHandle);
                _queryHandle = IntPtr.Zero;

                ThrowIfPdhFailed(
                    status,
                    "No fue posible agregar el contador de rendimiento.");
            }
        }

        private double? ReadProcessorPerformance()
        {
            uint status =
                PdhCollectQueryData(_queryHandle);

            ThrowIfPdhFailed(
                status,
                "No fue posible recopilar los datos de rendimiento.");

            if (!_hasInitialSample)
            {
                _hasInitialSample = true;
                return null;
            }

            status = PdhGetFormattedCounterValue(
                _counterHandle,
                PdhFormatDouble,
                out _,
                out PDH_FMT_COUNTERVALUE value);

            ThrowIfPdhFailed(
                status,
                "No fue posible leer el rendimiento del procesador.");

            if (value.CStatus != PdhCStatusValidData &&
                value.CStatus != PdhCStatusNewData)
            {
                return null;
            }

            return double.IsFinite(value.DoubleValue) &&
                value.DoubleValue >= 0.0
                    ? value.DoubleValue
                    : null;
        }

        private static void GetProcessorPowerInformation(
            out double currentMhz,
            out double maximumMhz,
            out double limitMhz)
        {
            int processorCount = Environment.ProcessorCount;

            int structureSize =
                Marshal.SizeOf<PROCESSOR_POWER_INFORMATION>();

            int bufferSize =
                structureSize * processorCount;

            IntPtr buffer =
                Marshal.AllocHGlobal(bufferSize);

            try
            {
                uint status = CallNtPowerInformation(
                    ProcessorInformation,
                    IntPtr.Zero,
                    0,
                    buffer,
                    (uint)bufferSize);

                if (status != ErrorSuccess)
                {
                    throw new InvalidOperationException(
                        $"No fue posible obtener la información " +
                        $"del procesador. Código: 0x{status:X8}.");
                }

                double maximumTotal = 0;
                double limitTotal = 0;
                double currentTotal = 0;

                for (int i = 0; i < processorCount; i++)
                {
                    IntPtr currentAddress =
                        IntPtr.Add(
                            buffer,
                            i * structureSize);

                    var information =
                        Marshal.PtrToStructure<PROCESSOR_POWER_INFORMATION>(
                            currentAddress);

                    maximumTotal += information.MaxMhz;
                    limitTotal += information.MhzLimit;
                    currentTotal += information.CurrentMhz;
                }

                currentMhz =
                    currentTotal / processorCount;

                maximumMhz =
                    maximumTotal / processorCount;

                limitMhz =
                    limitTotal / processorCount;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static void ThrowIfPdhFailed(
            uint status,
            string message)
        {
            if (status != ErrorSuccess)
            {
                throw new InvalidOperationException(
                    $"{message} Código: 0x{status:X8}.");
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_queryHandle != IntPtr.Zero)
            {
                PdhCloseQuery(_queryHandle);
                _queryHandle = IntPtr.Zero;
                _counterHandle = IntPtr.Zero;
            }

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        [DllImport(
            "pdh.dll",
            CharSet = CharSet.Unicode)]
        private static extern uint PdhOpenQuery(
            string? dataSource,
            IntPtr userData,
            out IntPtr query);

        [DllImport(
            "pdh.dll",
            CharSet = CharSet.Unicode,
            EntryPoint = "PdhAddEnglishCounterW")]
        private static extern uint PdhAddEnglishCounter(
            IntPtr query,
            string fullCounterPath,
            IntPtr userData,
            out IntPtr counter);

        [DllImport("pdh.dll")]
        private static extern uint PdhCollectQueryData(
            IntPtr query);

        [DllImport("pdh.dll")]
        private static extern uint PdhGetFormattedCounterValue(
            IntPtr counter,
            uint format,
            out uint counterType,
            out PDH_FMT_COUNTERVALUE value);

        [DllImport("pdh.dll")]
        private static extern uint PdhCloseQuery(
            IntPtr query);

        [DllImport("powrprof.dll")]
        private static extern uint CallNtPowerInformation(
            int informationLevel,
            IntPtr inputBuffer,
            uint inputBufferLength,
            IntPtr outputBuffer,
            uint outputBufferLength);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSOR_POWER_INFORMATION
        {
            public uint Number;
            public uint MaxMhz;
            public uint CurrentMhz;
            public uint MhzLimit;
            public uint MaxIdleState;
            public uint CurrentIdleState;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PDH_FMT_COUNTERVALUE
        {
            public uint CStatus;
            public double DoubleValue;
        }
    }
}