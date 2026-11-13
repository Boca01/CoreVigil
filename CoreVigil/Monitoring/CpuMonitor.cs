using System.Runtime.InteropServices;
using CoreVigil.Core.Abstractions;
using CoreVigil.Core.Models;

namespace CoreVigil.Monitoring
{
    /// <summary>
    /// Obtiene el uso global del procesador mediante información
    /// de tiempo proporcionada directamente por Windows.
    /// </summary>
    public sealed class CpuMonitor : ICpuMonitor
    {
        private ulong _previousIdleTime;
        private ulong _previousKernelTime;
        private ulong _previousUserTime;
        private bool _hasPreviousSample;

        public async Task<CpuSnapshot> GetSnapshotAsync(
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_hasPreviousSample)
            {
                CaptureInitialSample();

                await Task.Delay(
                    TimeSpan.FromMilliseconds(500),
                    cancellationToken);
            }

            double usagePercent = CaptureUsage();

            return new CpuSnapshot
            {
                UsagePercent = usagePercent,
                Timestamp = DateTime.Now
            };
        }

        private void CaptureInitialSample()
        {
            ReadSystemTimes(
                out _previousIdleTime,
                out _previousKernelTime,
                out _previousUserTime);

            _hasPreviousSample = true;
        }

        private double CaptureUsage()
        {
            ReadSystemTimes(
                out ulong currentIdleTime,
                out ulong currentKernelTime,
                out ulong currentUserTime);

            ulong idleDelta =
                currentIdleTime - _previousIdleTime;

            ulong kernelDelta =
                currentKernelTime - _previousKernelTime;

            ulong userDelta =
                currentUserTime - _previousUserTime;

            double usagePercent = CalculateUsagePercent(
                idleDelta,
                kernelDelta,
                userDelta);

            _previousIdleTime = currentIdleTime;
            _previousKernelTime = currentKernelTime;
            _previousUserTime = currentUserTime;

            return usagePercent;
        }

        internal static double CalculateUsagePercent(
            ulong idleDelta,
            ulong kernelDelta,
            ulong userDelta)
        {
            ulong totalDelta = kernelDelta + userDelta;

            if (totalDelta == 0)
            {
                return 0.0;
            }

            ulong busyDelta =
                totalDelta > idleDelta
                    ? totalDelta - idleDelta
                    : 0;

            double usagePercent =
                (double)busyDelta / totalDelta * 100.0;

            return Math.Clamp(
                usagePercent,
                0.0,
                100.0);
        }

        private static void ReadSystemTimes(
            out ulong idleTime,
            out ulong kernelTime,
            out ulong userTime)
        {
            if (!GetSystemTimes(
                    out long nativeIdleTime,
                    out long nativeKernelTime,
                    out long nativeUserTime))
            {
                throw new InvalidOperationException(
                    $"No fue posible obtener los tiempos del procesador. " +
                    $"Error de Windows: {Marshal.GetLastWin32Error()}.");
            }

            idleTime = unchecked((ulong)nativeIdleTime);
            kernelTime = unchecked((ulong)nativeKernelTime);
            userTime = unchecked((ulong)nativeUserTime);
        }

        [DllImport(
            "kernel32.dll",
            SetLastError = true,
            ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemTimes(
            out long lpIdleTime,
            out long lpKernelTime,
            out long lpUserTime);
    }
}