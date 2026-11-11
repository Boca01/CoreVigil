using CoreVigil.Diagnostics;

namespace CoreVigil.Tests.Diagnostics
{
    /// <summary>
    /// Verifica las garantias publicas y estables
    /// del diagnostico interno de CoreVigil.
    /// </summary>
    public sealed class RuntimeDiagnosticsServiceTests
    {
        [Fact]
        public void Capture_PrimeraMuestra_IniciaIntervaloYCpuPropiaEnCero()
        {
            using var service =
                new RuntimeDiagnosticsService();

            RuntimeSample sample =
                service.Capture(
                    applicationUptime:
                        TimeSpan.FromMinutes(5),
                    systemCpuPercent:
                        25.0,
                    cpuFrequencyMhz:
                        3500.0,
                    systemMemoryPercent:
                        60.0,
                    isHudVisible:
                        true,
                    isTopmost:
                        false,
                    trayState:
                        "Low");

            Assert.Equal(
                0.0,
                sample.SampleIntervalMilliseconds,
                precision: 6);

            Assert.Equal(
                0.0,
                sample.CoreVigilCpuPercent,
                precision: 6);
        }

        [Fact]
        public void Capture_ConDatosDelSistema_LosConservaEnLaMuestra()
        {
            using var service =
                new RuntimeDiagnosticsService();

            TimeSpan expectedUptime =
                TimeSpan.FromHours(2);

            RuntimeSample sample =
                service.Capture(
                    applicationUptime:
                        expectedUptime,
                    systemCpuPercent:
                        47.5,
                    cpuFrequencyMhz:
                        4200.0,
                    systemMemoryPercent:
                        68.25,
                    isHudVisible:
                        true,
                    isTopmost:
                        true,
                    trayState:
                        "Medium");

            Assert.Equal(
                expectedUptime,
                sample.ApplicationUptime);

            Assert.Equal(
                47.5,
                sample.SystemCpuPercent,
                precision: 6);

            Assert.Equal(
                4200.0,
                sample.CpuFrequencyMhz,
                precision: 6);

            Assert.Equal(
                68.25,
                sample.SystemMemoryPercent,
                precision: 6);

            Assert.True(
                sample.IsHudVisible);

            Assert.True(
                sample.IsTopmost);

            Assert.Equal(
                "Medium",
                sample.TrayState);
        }

        [Fact]
        public void Capture_DevuelveCpuPropiaDentroDelRangoValido()
        {
            using var service =
                new RuntimeDiagnosticsService();

            service.Capture(
                applicationUptime:
                    TimeSpan.Zero,
                systemCpuPercent:
                    0.0,
                cpuFrequencyMhz:
                    0.0,
                systemMemoryPercent:
                    0.0,
                isHudVisible:
                    false,
                isTopmost:
                    false,
                trayState:
                    "Unknown");

            RuntimeSample sample =
                service.Capture(
                    applicationUptime:
                        TimeSpan.FromSeconds(1),
                    systemCpuPercent:
                        10.0,
                    cpuFrequencyMhz:
                        3000.0,
                    systemMemoryPercent:
                        50.0,
                    isHudVisible:
                        false,
                    isTopmost:
                        false,
                    trayState:
                        "Low");

            Assert.InRange(
                sample.CoreVigilCpuPercent,
                0.0,
                100.0);
        }

        [Fact]
        public void Capture_LecturaDeHeapValida_EsCoherenteConElValorCapturado()
        {
            using var service =
                new RuntimeDiagnosticsService();

            RuntimeSample sample =
                service.Capture(
                    applicationUptime:
                        TimeSpan.Zero,
                    systemCpuPercent:
                        0.0,
                    cpuFrequencyMhz:
                        0.0,
                    systemMemoryPercent:
                        0.0,
                    isHudVisible:
                        false,
                    isTopmost:
                        false,
                    trayState:
                        "Unknown");

            Assert.Equal(
                sample.ManagedHeapBytes >= 0,
                sample.IsManagedHeapReadingValid);
        }
    }
}