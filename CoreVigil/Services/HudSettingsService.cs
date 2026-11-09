using System.IO;
using System.Text.Json;

namespace CoreVigil.Services
{
    /// <summary>
    /// Guarda y recupera las preferencias locales del HUD.
    /// </summary>
    public sealed class HudSettingsService
    {
        private readonly string _settingsPath;

        public HudSettingsService()
        {
            string directory = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "CoreVigil");

            Directory.CreateDirectory(directory);

            _settingsPath = Path.Combine(
                directory,
                "hud-settings.json");
        }

        public HudSettings? Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    return null;
                }

                string json = File.ReadAllText(_settingsPath);
                return JsonSerializer.Deserialize<HudSettings>(json);
            }
            catch
            {
                return null;
            }
        }

        public void Save(
            double left,
            double top,
            bool topmost)
        {
            bool isCpuActivityExpanded =
                Load()?.IsCpuActivityExpanded ?? true;

            Save(
                left,
                top,
                topmost,
                isCpuActivityExpanded);
        }

        public void Save(
            double left,
            double top,
            bool topmost,
            bool isCpuActivityExpanded)
        {
            HudSettings? currentSettings =
                Load();

            Save(
                fullLeft:
                    currentSettings?.FullLeft ??
                    currentSettings?.Left ??
                    left,
                fullTop:
                    currentSettings?.FullTop ??
                    currentSettings?.Top ??
                    top,
                miniLeft:
                    currentSettings?.MiniLeft,
                miniTop:
                    currentSettings?.MiniTop,
                topmost:
                    topmost,
                isCpuActivityExpanded:
                    isCpuActivityExpanded,
                isMiniMode:
                    currentSettings?.IsMiniMode ??
                    false);
        }
        public void Save(
    double fullLeft,
    double fullTop,
    double? miniLeft,
    double? miniTop,
    bool topmost,
    bool isCpuActivityExpanded,
    bool isMiniMode)
        {
            try
            {
                var settings =
                    new HudSettings
                    {
                        // Se conservan para migrar configuraciones
                        // creadas antes de introducir el modo Mini.
                        Left =
                            fullLeft,

                        Top =
                            fullTop,

                        FullLeft =
                            fullLeft,

                        FullTop =
                            fullTop,

                        MiniLeft =
                            miniLeft,

                        MiniTop =
                            miniTop,

                        Topmost =
                            topmost,

                        IsCpuActivityExpanded =
                            isCpuActivityExpanded,

                        IsMiniMode =
                            isMiniMode
                    };

                string json =
                    JsonSerializer.Serialize(
                        settings);

                File.WriteAllText(
                    _settingsPath,
                    json);
            }
            catch (Exception ex)
            {
                // Las preferencias del HUD son secundarias.
                // Un fallo al guardarlas no debe detener CoreVigil.
                System.Diagnostics.Debug.WriteLine(
                    $"No fue posible guardar las preferencias del HUD: {ex}");
            }
        }
    }

    public sealed class HudSettings
    {
        // Compatibilidad con hud-settings.json anteriores.
        public double? Left { get; init; }

        public double? Top { get; init; }

        public double? FullLeft { get; init; }

        public double? FullTop { get; init; }

        public double? MiniLeft { get; init; }

        public double? MiniTop { get; init; }

        public bool Topmost { get; init; }

        public bool IsCpuActivityExpanded { get; init; } =
            true;

        public bool IsMiniMode { get; init; }
    }
}
