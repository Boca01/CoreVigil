using Microsoft.Win32;

namespace CoreVigil.Services
{
    /// <summary>
    /// Gestiona el inicio automático de CoreVigil
    /// al iniciar sesión en Windows.
    /// </summary>
    public sealed class StartupService
    {
        private const string RunKeyPath =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        private const string ValueName =
            "CoreVigil";

        public bool IsEnabled()
        {
            try
            {
                using RegistryKey? key =
                    Registry.CurrentUser.OpenSubKey(
                        RunKeyPath,
                        writable: false);

                return key?.GetValue(ValueName) is string;
            }
            catch
            {
                return false;
            }
        }

        public void SetEnabled(
            bool enabled)
        {
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    RunKeyPath,
                    writable: true);

            if (key is null)
            {
                throw new InvalidOperationException(
                    "No fue posible acceder a la configuración de inicio de Windows.");
            }

            if (!enabled)
            {
                key.DeleteValue(
                    ValueName,
                    throwOnMissingValue: false);

                return;
            }

            string executablePath =
                Environment.ProcessPath
                ?? throw new InvalidOperationException(
                    "No fue posible determinar la ubicación de CoreVigil.");

            key.SetValue(
                ValueName,
                $"\"{executablePath}\"",
                RegistryValueKind.String);
        }
    }
}