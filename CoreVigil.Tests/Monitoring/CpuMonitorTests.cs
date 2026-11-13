using CoreVigil.Monitoring;

namespace CoreVigil.Tests.Monitoring
{
    public sealed class CpuMonitorTests
    {
        [Fact]
        public void CalculateUsagePercent_SinTiempoTranscurrido_DevuelveCero()
        {
            double usagePercent =
                CpuMonitor.CalculateUsagePercent(
                    idleDelta: 0,
                    kernelDelta: 0,
                    userDelta: 0);

            Assert.Equal(0.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_ConActividadParcial_CalculaPorcentaje()
        {
            double usagePercent =
                CpuMonitor.CalculateUsagePercent(
                    idleDelta: 50,
                    kernelDelta: 100,
                    userDelta: 100);

            Assert.Equal(75.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_SinTiempoIdle_DevuelveCien()
        {
            double usagePercent =
                CpuMonitor.CalculateUsagePercent(
                    idleDelta: 0,
                    kernelDelta: 100,
                    userDelta: 100);

            Assert.Equal(100.0, usagePercent);
        }

        [Fact]
        public void CalculateUsagePercent_IdleMayorAlTotal_NoDevuelveNegativo()
        {
            double usagePercent =
                CpuMonitor.CalculateUsagePercent(
                    idleDelta: 300,
                    kernelDelta: 100,
                    userDelta: 100);

            Assert.Equal(0.0, usagePercent);
        }
    }
}