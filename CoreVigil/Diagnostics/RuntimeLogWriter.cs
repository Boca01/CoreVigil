using System.Globalization;
using System.IO;
using System.Text;

namespace CoreVigil.Diagnostics
{
    /// <summary>
    /// Acumula muestras de diagnóstico en memoria
    /// y las escribe periódicamente en un archivo CSV.
    /// </summary>
    public sealed class RuntimeLogWriter
    {
        private readonly List<RuntimeSample> _buffer =
            new();

        private readonly string _logPath;

        public RuntimeLogWriter()
        {
            string directory =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "CoreVigil",
                    "Logs");

            Directory.CreateDirectory(directory);

            string fileName =
                $"runtime-{DateTime.Now:yyyyMMdd-HHmmss}.csv";

            _logPath =
                Path.Combine(
                    directory,
                    fileName);
        }

        public string LogPath =>
            _logPath;

        public void Add(
            RuntimeSample sample)
        {
            _buffer.Add(sample);
        }

        public async Task FlushAsync()
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            bool writeHeader =
                !File.Exists(_logPath);

            RuntimeSample[] samples =
                _buffer.ToArray();

            _buffer.Clear();

            await using var writer =
                new StreamWriter(
                    _logPath,
                    append: true,
                    Encoding.UTF8);

            if (writeHeader)
            {
                await writer.WriteLineAsync(
                    "Timestamp," +
                    "ApplicationUptimeSeconds," +
                    "SystemCpuPercent," +
                    "CpuFrequencyMhz," +
                    "SystemMemoryPercent," +
                    "CoreVigilCpuPercent," +
                    "WorkingSetBytes," +
                    "ManagedHeapBytes," +
                    "IsHudVisible," +
                    "IsTopmost," +
                    "TrayState");
            }

            foreach (RuntimeSample sample in samples)
            {
                string line =
                    string.Join(
                        ",",
                        FormatDateTime(sample.Timestamp),
                        FormatDouble(
                            sample.ApplicationUptime.TotalSeconds),
                        FormatDouble(
                            sample.SystemCpuPercent),
                        FormatDouble(
                            sample.CpuFrequencyMhz),
                        FormatDouble(
                            sample.SystemMemoryPercent),
                        FormatDouble(
                            sample.CoreVigilCpuPercent),
                        sample.WorkingSetBytes
                            .ToString(
                                CultureInfo.InvariantCulture),
                        sample.ManagedHeapBytes
                            .ToString(
                                CultureInfo.InvariantCulture),
                        sample.IsHudVisible
                            ? "true"
                            : "false",
                        sample.IsTopmost
                            ? "true"
                            : "false",
                        EscapeCsv(sample.TrayState));

                await writer.WriteLineAsync(line);
            }
        }

        private static string FormatDateTime(
            DateTime value)
        {
            return value.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        private static string FormatDouble(
            double value)
        {
            return value.ToString(
                "F3",
                CultureInfo.InvariantCulture);
        }

        private static string EscapeCsv(
            string value)
        {
            if (!value.Contains(',') &&
                !value.Contains('"') &&
                !value.Contains('\n') &&
                !value.Contains('\r'))
            {
                return value;
            }

            return
                "\"" +
                value.Replace(
                    "\"",
                    "\"\"") +
                "\"";
        }
    }
}