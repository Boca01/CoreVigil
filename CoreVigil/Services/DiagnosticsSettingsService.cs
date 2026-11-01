using System.IO;
using System.Text.Json;

namespace CoreVigil.Services
{
    /// <summary>
    /// Guarda y recupera las preferencias locales
    /// relacionadas con el diagnóstico interno.
    /// </summary>
    public sealed class DiagnosticsSettingsService
    {
        private readonly string _settingsPath;

        public DiagnosticsSettingsService()
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
                    "diagnostics-settings.json");
        }

        public bool IsEnabled()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    return false;
                }

                string json =
                    File.ReadAllText(
                        _settingsPath);

                DiagnosticsSettings? settings =
                    JsonSerializer.Deserialize<DiagnosticsSettings>(
                        json);

                return
                    settings?.Enabled
                    ?? false;
            }
            catch
            {
                return false;
            }
        }

        public void SetEnabled(
            bool enabled)
        {
            try
            {
                var settings =
                    new DiagnosticsSettings
                    {
                        Enabled = enabled
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
                // La configuración de diagnóstico es secundaria.
                // Un fallo no debe impedir el uso de CoreVigil.
                System.Diagnostics.Debug.WriteLine(
                    $"No fue posible guardar la configuración " +
                    $"de diagnóstico: {ex}");
            }
        }
    }

    public sealed class DiagnosticsSettings
    {
        public bool Enabled { get; init; }
    }
}