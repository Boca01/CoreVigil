using System.IO;
using System.Text.Json;

namespace CoreVigil.Services
{
    /// <summary>
    /// Guarda y recupera la última posición conocida del HUD.
    /// </summary>
    public sealed class HudPositionService
    {
        private readonly string _settingsPath;

        public HudPositionService()
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
                    "hud-position.json");
        }

        public HudPosition? Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    return null;
                }

                string json =
                    File.ReadAllText(_settingsPath);

                return JsonSerializer.Deserialize<HudPosition>(json);
            }
            catch
            {
                return null;
            }
        }

        public void Save(
            double left,
            double top)
        {
            try
            {
                var position =
                    new HudPosition
                    {
                        Left = left,
                        Top = top
                    };

                string json =
                    JsonSerializer.Serialize(position);

                File.WriteAllText(
                    _settingsPath,
                    json);
            }
            catch (Exception ex)
            {
                // La posición del HUD es una preferencia secundaria.
                // Un fallo al guardarla no debe detener CoreVigil.

                System.Diagnostics.Debug.WriteLine(
                    $"No fue posible guardar la posición del HUD: {ex}");
            }
        }
    }

    public sealed class HudPosition
    {
        public double Left { get; init; }

        public double Top { get; init; }
    }
}