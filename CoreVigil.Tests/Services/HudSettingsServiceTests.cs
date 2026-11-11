using CoreVigil.Services;

namespace CoreVigil.Tests.Services
{
    /// <summary>
    /// Verifica la persistencia local de las preferencias del HUD
    /// utilizando archivos temporales aislados de la configuracion real.
    /// </summary>
    public sealed class HudSettingsServiceTests
    {
        [Fact]
        public void Load_SinArchivo_DevuelveNull()
        {
            string settingsPath =
                CreateTemporarySettingsPath();

            try
            {
                var service =
                    new HudSettingsService(
                        settingsPath);

                HudSettings? settings =
                    service.Load();

                Assert.Null(settings);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    settingsPath);
            }
        }

        [Fact]
        public void SaveLoad_GuardaYRecuperaEstadoCompleto()
        {
            string settingsPath =
                CreateTemporarySettingsPath();

            try
            {
                var service =
                    new HudSettingsService(
                        settingsPath);

                service.Save(
                    fullLeft: 120.5,
                    fullTop: 240.5,
                    miniLeft: 600.0,
                    miniTop: 700.0,
                    topmost: true,
                    isCpuActivityExpanded: false,
                    isMiniMode: true,
                    backgroundOpacity: 0.55,
                    opacityPopupHorizontalOffset: 35.0,
                    opacityPopupVerticalOffset: -18.0);

                HudSettings? settings =
                    service.Load();

                Assert.NotNull(settings);

                Assert.Equal(
                    120.5,
                    settings.FullLeft);

                Assert.Equal(
                    240.5,
                    settings.FullTop);

                Assert.Equal(
                    600.0,
                    settings.MiniLeft);

                Assert.Equal(
                    700.0,
                    settings.MiniTop);

                Assert.True(
                    settings.Topmost);

                Assert.False(
                    settings.IsCpuActivityExpanded);

                Assert.True(
                    settings.IsMiniMode);

                Assert.Equal(
                    0.55,
                    settings.BackgroundOpacity,
                    precision: 6);

                Assert.Equal(
                    35.0,
                    settings.OpacityPopupHorizontalOffset);

                Assert.Equal(
                    -18.0,
                    settings.OpacityPopupVerticalOffset);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    settingsPath);
            }
        }

        [Fact]
        public void SaveLoad_ConPosicionesMiniNulas_LasConservaNulas()
        {
            string settingsPath =
                CreateTemporarySettingsPath();

            try
            {
                var service =
                    new HudSettingsService(
                        settingsPath);

                service.Save(
                    fullLeft: 100.0,
                    fullTop: 200.0,
                    miniLeft: null,
                    miniTop: null,
                    topmost: false,
                    isCpuActivityExpanded: true,
                    isMiniMode: false,
                    backgroundOpacity: 0.82,
                    opacityPopupHorizontalOffset: null,
                    opacityPopupVerticalOffset: null);

                HudSettings? settings =
                    service.Load();

                Assert.NotNull(settings);

                Assert.Null(
                    settings.MiniLeft);

                Assert.Null(
                    settings.MiniTop);

                Assert.Null(
                    settings.OpacityPopupHorizontalOffset);

                Assert.Null(
                    settings.OpacityPopupVerticalOffset);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    settingsPath);
            }
        }

        [Fact]
        public void Load_ConJsonInvalido_DevuelveNull()
        {
            string settingsPath =
                CreateTemporarySettingsPath();

            try
            {
                string? directory =
                    Path.GetDirectoryName(
                        settingsPath);

                Assert.False(
                    string.IsNullOrWhiteSpace(directory));

                Directory.CreateDirectory(
                    directory!);

                File.WriteAllText(
                    settingsPath,
                    "{ json invalido");

                var service =
                    new HudSettingsService(
                        settingsPath);

                HudSettings? settings =
                    service.Load();

                Assert.Null(settings);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    settingsPath);
            }
        }

        [Fact]
        public void Load_ConConfiguracionHistorica_RecuperaLeftYTop()
        {
            string settingsPath =
                CreateTemporarySettingsPath();

            try
            {
                string? directory =
                    Path.GetDirectoryName(
                        settingsPath);

                Assert.False(
                    string.IsNullOrWhiteSpace(directory));

                Directory.CreateDirectory(
                    directory!);

                File.WriteAllText(
                    settingsPath,
                    """
            {
              "Left": 150.5,
              "Top": 275.25,
              "Topmost": true
            }
            """);

                var service =
                    new HudSettingsService(
                        settingsPath);

                HudSettings? settings =
                    service.Load();

                Assert.NotNull(settings);

                Assert.Equal(
                    150.5,
                    settings.Left);

                Assert.Equal(
                    275.25,
                    settings.Top);

                Assert.Null(
                    settings.FullLeft);

                Assert.Null(
                    settings.FullTop);

                Assert.True(
                    settings.Topmost);

                Assert.Equal(
                    0.82,
                    settings.BackgroundOpacity,
                    precision: 6);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    settingsPath);
            }
        }

        [Fact]
        public void SaveParcial_ConservaOpacidadYOffsetsExistentes()
        {
            string settingsPath =
                CreateTemporarySettingsPath();

            try
            {
                var service =
                    new HudSettingsService(
                        settingsPath);

                service.Save(
                    fullLeft: 100.0,
                    fullTop: 200.0,
                    miniLeft: 300.0,
                    miniTop: 400.0,
                    topmost: false,
                    isCpuActivityExpanded: true,
                    isMiniMode: true,
                    backgroundOpacity: 0.61,
                    opacityPopupHorizontalOffset: 42.0,
                    opacityPopupVerticalOffset: -15.0);

                service.Save(
                    left: 100.0,
                    top: 200.0,
                    topmost: true,
                    isCpuActivityExpanded: false);

                HudSettings? settings =
                    service.Load();

                Assert.NotNull(settings);

                Assert.Equal(
                    0.61,
                    settings.BackgroundOpacity,
                    precision: 6);

                Assert.Equal(
                    42.0,
                    settings.OpacityPopupHorizontalOffset);

                Assert.Equal(
                    -15.0,
                    settings.OpacityPopupVerticalOffset);

                Assert.True(
                    settings.Topmost);

                Assert.False(
                    settings.IsCpuActivityExpanded);

                Assert.True(
                    settings.IsMiniMode);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    settingsPath);
            }
        }

        private static string CreateTemporarySettingsPath()
        {
            string directory =
                Path.Combine(
                    Path.GetTempPath(),
                    "CoreVigil.Tests",
                    Guid.NewGuid().ToString("N"));

            return Path.Combine(
                directory,
                "hud-settings.json");
        }

        private static void DeleteTemporaryDirectory(
            string settingsPath)
        {
            string? directory =
                Path.GetDirectoryName(
                    settingsPath);

            if (string.IsNullOrWhiteSpace(directory) ||
                !Directory.Exists(directory))
            {
                return;
            }

            Directory.Delete(
                directory,
                recursive: true);
        }
    }
}