using CoreVigil.Monitoring;

namespace CoreVigil.Tests.Monitoring
{
    public sealed class CpuFrequencyMonitorTests
    {
        [Fact]
        public void CalculateCurrentFrequencyMhz_SinMuestraPdh_UsaFrecuenciaActualDeWindows()
        {
            double currentMhz =
                CpuFrequencyMonitor.CalculateCurrentFrequencyMhz(
                    nativeCurrentMhz: 2475.0,
                    maximumMhz: 3500.0,
                    performancePercent: null);

            Assert.Equal(2475.0, currentMhz);
        }

        [Fact]
        public void CalculateCurrentFrequencyMhz_ConMuestraPdh_UsaRendimientoRelativo()
        {
            double currentMhz =
                CpuFrequencyMonitor.CalculateCurrentFrequencyMhz(
                    nativeCurrentMhz: 2475.0,
                    maximumMhz: 3500.0,
                    performancePercent: 80.0);

            Assert.Equal(2800.0, currentMhz);
        }

        [Fact]
        public void CalculateCurrentFrequencyMhz_ConTurbo_NoLimitaAlMaximoNominal()
        {
            double currentMhz =
                CpuFrequencyMonitor.CalculateCurrentFrequencyMhz(
                    nativeCurrentMhz: 2475.0,
                    maximumMhz: 3500.0,
                    performancePercent: 110.0);

            Assert.Equal(3850.0, currentMhz);
        }

        [Fact]
        public void CalculateCurrentFrequencyMhz_ConMuestraNoFinita_UsaFrecuenciaActualDeWindows()
        {
            double currentMhz =
                CpuFrequencyMonitor.CalculateCurrentFrequencyMhz(
                    nativeCurrentMhz: 2475.0,
                    maximumMhz: 3500.0,
                    performancePercent: double.NaN);

            Assert.Equal(2475.0, currentMhz);
        }
    }
}
