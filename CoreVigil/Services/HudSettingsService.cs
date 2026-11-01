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
            string directory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "CoreVigil");

            Directory.CreateDirectory(directory);

            _settingsPath =
                Path.Combine(
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

                string json =
                    File.ReadAllText(_settingsPath);

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
            try
            {
                var settings =
                    new HudSettings
                    {
                        Left = left,
                        Top = top,
                        Topmost = topmost
                    };

                string json =
                    JsonSerializer.Serialize(settings);

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
        public double Left { get; init; }

        public double Top { get; init; }

        public bool Topmost { get; init; }
    }
}