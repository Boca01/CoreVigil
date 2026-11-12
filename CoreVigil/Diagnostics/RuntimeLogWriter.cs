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
        private const int RetentionDays = 7;
        private const int MaximumBufferedSamples = 10_000;

        private const long MaximumLogsSizeBytes =
            25L * 1024L * 1024L;
        private readonly List<RuntimeSample> _buffer =
            new();
        private readonly object _bufferLock = new();
        private readonly SemaphoreSlim _flushSemaphore = new(1, 1);
        private long _droppedSampleCount;
        private readonly string _logsDirectory;

        private readonly string _logPath;

        public RuntimeLogWriter()
            : this(
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder.LocalApplicationData),
                    "CoreVigil",
                    "Logs"))
        {
        }

        public RuntimeLogWriter(
            string logsDirectory)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(
                logsDirectory);

            _logsDirectory =
                logsDirectory;

            Directory.CreateDirectory(
                _logsDirectory);

            CleanupOldLogs();

            string fileName =
                $"runtime-{DateTime.Now:yyyyMMdd-HHmmss}.csv";

            _logPath =
                Path.Combine(
                    _logsDirectory,
                    fileName);
        }

        private void CleanupOldLogs()
        {
            try
            {
                DateTime retentionLimitUtc =
                    DateTime.UtcNow.AddDays(
                        -RetentionDays);

                FileInfo[] logFiles =
                    new DirectoryInfo(
                        _logsDirectory)
                    .GetFiles(
                        "runtime-*.csv")
                    .OrderBy(
                        file => file.LastWriteTimeUtc)
                    .ToArray();

                foreach (FileInfo file in logFiles)
                {
                    if (file.LastWriteTimeUtc <
                        retentionLimitUtc)
                    {
                        file.Delete();
                    }
                }

                logFiles =
                    new DirectoryInfo(
                        _logsDirectory)
                    .GetFiles(
                        "runtime-*.csv")
                    .OrderBy(
                        file => file.LastWriteTimeUtc)
                    .ToArray();

                long totalSizeBytes =
                    logFiles.Sum(
                        file => file.Length);

                foreach (FileInfo file in logFiles)
                {
                    if (totalSizeBytes <=
                        MaximumLogsSizeBytes)
                    {
                        break;
                    }

                    long fileSize =
                        file.Length;

                    file.Delete();

                    totalSizeBytes -=
                        fileSize;
                }
            }
            catch (Exception ex)
            {
                // La limpieza de logs es una tarea secundaria.
                // Un fallo no debe impedir el inicio de CoreVigil.
                System.Diagnostics.Debug.WriteLine(
                    $"No fue posible limpiar los logs de CoreVigil: {ex}");
            }
        }

        public string LogPath =>
            _logPath;

        public int BufferedSampleCount
        {
            get
            {
                lock (_bufferLock)
                {
                    return _buffer.Count;
                }
            }
        }

        public long DroppedSampleCount
        {
            get
            {
                lock (_bufferLock)
                {
                    return _droppedSampleCount;
                }
            }
        }

        public bool Add(
            RuntimeSample sample)
        {
            lock (_bufferLock)
            {
                bool droppedOldestSample =
                    _buffer.Count == MaximumBufferedSamples;

                if (droppedOldestSample)
                {
                    _buffer.RemoveAt(0);
                    _droppedSampleCount++;
                }

                _buffer.Add(sample);
                return droppedOldestSample;
            }
        }

        public async Task FlushAsync()
        {
            await _flushSemaphore.WaitAsync();

            try
            {
                RuntimeSample[] samples;

                lock (_bufferLock)
                {
                    if (_buffer.Count == 0)
                    {
                        return;
                    }

                    samples = _buffer.ToArray();
                    _buffer.Clear();
                }

                try
                {
                    await WriteSamplesAsync(samples);
                }
                catch
                {
                    lock (_bufferLock)
                    {
                        _buffer.InsertRange(0, samples);

                        while (_buffer.Count > MaximumBufferedSamples)
                        {
                            _buffer.RemoveAt(0);
                            _droppedSampleCount++;
                        }
                    }

                    throw;
                }
            }
            finally
            {
                _flushSemaphore.Release();
            }
        }

        private async Task WriteSamplesAsync(
            RuntimeSample[] samples)
        {
            bool writeHeader =
                !File.Exists(_logPath);

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
                    "SampleIntervalMilliseconds," +
                    "SystemCpuPercent," +
                    "CpuFrequencyMhz," +
                    "SystemMemoryPercent," +
                    "CoreVigilCpuPercent," +
                    "WorkingSetBytes," +
                    "ManagedHeapBytes," +
                    "GcHeapSizeBytes," +
                    "GcIndex," +
                    "IsManagedHeapReadingValid," +
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
                            sample.SampleIntervalMilliseconds),
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
                        sample.GcHeapSizeBytes
                            .ToString(
                                CultureInfo.InvariantCulture),
                        sample.GcIndex
                            .ToString(
                                CultureInfo.InvariantCulture),
                        sample.IsManagedHeapReadingValid
                            ? "true"
                            : "false",
                        sample.IsHudVisible
                            ? "true"
                            : "false",
                        sample.IsTopmost
                            ? "true"
                            : "false",
                        EscapeCsv(sample.TrayState));

                await writer.WriteLineAsync(line);
            }

            await writer.FlushAsync();
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