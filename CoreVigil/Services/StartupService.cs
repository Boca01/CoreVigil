using Microsoft.Win32;
using System.IO;

namespace CoreVigil.Services
{
    /// <summary>
    /// Gestiona el inicio automatico de CoreVigil
    /// al iniciar sesion en Windows.
    /// </summary>
    public sealed class StartupService
    {
        private const string RunKeyPath =
            @"Software\Microsoft\Windows\CurrentVersion\Run";

        private const string ValueName =
            "CoreVigil";

        private const string ClickOnceShortcutName =
            "CoreVigil.appref-ms";

        public bool IsEnabled()
        {
            try
            {
                using RegistryKey? key =
                    Registry.CurrentUser.OpenSubKey(
                        RunKeyPath,
                        writable: false);

                string? registeredCommand =
                    key?.GetValue(ValueName) as string;

                if (string.IsNullOrWhiteSpace(registeredCommand))
                {
                    return false;
                }

                string? clickOnceShortcutPath =
                    FindClickOnceShortcutPath();

                if (clickOnceShortcutPath is not null)
                {
                    string expectedCommand =
                        BuildClickOnceCommand(
                            clickOnceShortcutPath);

                    return string.Equals(
                        registeredCommand,
                        expectedCommand,
                        StringComparison.OrdinalIgnoreCase);
                }

                string? executablePath =
                    Environment.ProcessPath;

                if (string.IsNullOrWhiteSpace(executablePath) ||
                    !File.Exists(executablePath))
                {
                    return false;
                }

                string expectedExecutableCommand =
                    $"\"{executablePath}\"";

                return string.Equals(
                    registeredCommand,
                    expectedExecutableCommand,
                    StringComparison.OrdinalIgnoreCase);
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
                    "No fue posible acceder a la configuracion de inicio de Windows.");
            }

            if (!enabled)
            {
                key.DeleteValue(
                    ValueName,
                    throwOnMissingValue: false);

                return;
            }

            string? clickOnceShortcutPath =
                FindClickOnceShortcutPath();

            string startupCommand;

            if (clickOnceShortcutPath is not null)
            {
                // ClickOnce instala el ejecutable en una ruta versionada.
                // El acceso appref-ms permanece estable entre actualizaciones.
                startupCommand =
                    BuildClickOnceCommand(
                        clickOnceShortcutPath);
            }
            else
            {
                // Durante el desarrollo puede no existir un acceso ClickOnce.
                // En ese caso se utiliza el ejecutable actual.
                string executablePath =
                    Environment.ProcessPath
                    ?? throw new InvalidOperationException(
                        "No fue posible determinar la ubicacion de CoreVigil.");

                if (!File.Exists(executablePath))
                {
                    throw new InvalidOperationException(
                        "El ejecutable actual de CoreVigil no existe.");
                }

                startupCommand =
                    $"\"{executablePath}\"";
            }

            key.SetValue(
                ValueName,
                startupCommand,
                RegistryValueKind.String);
        }

        private static string? FindClickOnceShortcutPath()
        {
            string startMenuProgramsPath =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.Programs);

            if (string.IsNullOrWhiteSpace(startMenuProgramsPath) ||
                !Directory.Exists(startMenuProgramsPath))
            {
                return null;
            }

            try
            {
                return Directory
                    .EnumerateFiles(
                        startMenuProgramsPath,
                        ClickOnceShortcutName,
                        SearchOption.AllDirectories)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string BuildClickOnceCommand(
            string clickOnceShortcutPath)
        {
            return
                $"explorer.exe \"{clickOnceShortcutPath}\"";
        }
    }
}