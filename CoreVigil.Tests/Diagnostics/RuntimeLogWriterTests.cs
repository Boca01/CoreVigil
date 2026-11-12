using CoreVigil.Diagnostics;

namespace CoreVigil.Tests.Diagnostics
{
    /// <summary>
    /// Verifica la escritura CSV del diagnostico interno
    /// utilizando una carpeta temporal aislada.
    /// </summary>
    public sealed class RuntimeLogWriterTests
    {
        [Fact]
        public async Task FlushAsync_SinMuestras_NoCreaArchivo()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                await writer.FlushAsync();

                Assert.False(
                    File.Exists(
                        writer.LogPath));
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public async Task FlushAsync_ConUnaMuestra_EscribeEncabezadoYDatos()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                writer.Add(
                    CreateSample(
                        trayState: "Low"));

                await writer.FlushAsync();

                Assert.True(
                    File.Exists(
                        writer.LogPath));

                string[] lines =
                    await File.ReadAllLinesAsync(
                        writer.LogPath);

                Assert.Equal(
                    2,
                    lines.Length);

                Assert.Equal(
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
                    "TrayState",
                    lines[0]);

                Assert.EndsWith(
                    ",Low",
                    lines[1]);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public async Task FlushAsync_SiFallaConservaMuestrasParaReintento()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                writer.Add(
                    CreateSample(
                        trayState: "Low"));

                Directory.Delete(directory);
                File.WriteAllText(
                    directory,
                    "bloqueo");

                await Assert.ThrowsAnyAsync<IOException>(
                    () => writer.FlushAsync());

                File.Delete(directory);
                Directory.CreateDirectory(directory);

                await writer.FlushAsync();

                string[] lines =
                    await File.ReadAllLinesAsync(
                        writer.LogPath);

                Assert.Equal(
                    2,
                    lines.Length);

                Assert.EndsWith(
                    ",Low",
                    lines[1]);
            }
            finally
            {
                if (File.Exists(directory))
                {
                    File.Delete(directory);
                }

                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public void Add_AlSuperarElLimite_DescartaMuestrasAntiguasYLasCuenta()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                int droppedDuringTest = 0;

                for (int i = 0; i < 10_002; i++)
                {
                    if (writer.Add(
                            CreateSample(
                                trayState: i.ToString())))
                    {
                        droppedDuringTest++;
                    }
                }

                Assert.Equal(
                    10_000,
                    writer.BufferedSampleCount);

                Assert.Equal(
                    2,
                    writer.DroppedSampleCount);

                Assert.Equal(
                    2,
                    droppedDuringTest);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public async Task FlushAsync_EnVariasLlamadas_NoRepiteEncabezado()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                writer.Add(
                    CreateSample(
                        trayState: "Low"));

                await writer.FlushAsync();

                writer.Add(
                    CreateSample(
                        trayState: "Medium"));

                await writer.FlushAsync();

                string[] lines =
                    await File.ReadAllLinesAsync(
                        writer.LogPath);

                Assert.Equal(
                    3,
                    lines.Length);

                Assert.Equal(
                    1,
                    lines.Count(
                        line => line.StartsWith(
                            "Timestamp,")));
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public async Task FlushAsync_UsaFormatoNumericoInvariante()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                writer.Add(
                    CreateSample(
                        trayState: "Low"));

                await writer.FlushAsync();

                string[] lines =
                    await File.ReadAllLinesAsync(
                        writer.LogPath);

                string dataLine =
                    lines[1];

                Assert.Contains(
                    ",5.500,",
                    dataLine);

                Assert.Contains(
                    ",12.250,",
                    dataLine);

                Assert.Contains(
                    ",3500.750,",
                    dataLine);

                Assert.Contains(
                    ",63.125,",
                    dataLine);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public async Task FlushAsync_TrayStateConCaracteresCsv_LoEscapa()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                var writer =
                    new RuntimeLogWriter(
                        directory);

                writer.Add(
                    CreateSample(
                        trayState:
                            "Estado,\"Especial\""));

                await writer.FlushAsync();

                string[] lines =
                    await File.ReadAllLinesAsync(
                        writer.LogPath);

                Assert.EndsWith(
                    ",\"Estado,\"\"Especial\"\"\"",
                    lines[1]);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public void Constructor_ConLogMayorA7Dias_LoElimina()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                Directory.CreateDirectory(
                    directory);

                string oldLogPath =
                    Path.Combine(
                        directory,
                        "runtime-old.csv");

                File.WriteAllText(
                    oldLogPath,
                    "log antiguo");

                File.SetLastWriteTimeUtc(
                    oldLogPath,
                    DateTime.UtcNow.AddDays(-8));

                _ =
                    new RuntimeLogWriter(
                        directory);

                Assert.False(
                    File.Exists(
                        oldLogPath));
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public void Constructor_ConLogReciente_LoConserva()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                Directory.CreateDirectory(
                    directory);

                string recentLogPath =
                    Path.Combine(
                        directory,
                        "runtime-recent.csv");

                File.WriteAllText(
                    recentLogPath,
                    "log reciente");

                File.SetLastWriteTimeUtc(
                    recentLogPath,
                    DateTime.UtcNow.AddDays(-1));

                _ =
                    new RuntimeLogWriter(
                        directory);

                Assert.True(
                    File.Exists(
                        recentLogPath));
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        [Fact]
        public void Constructor_CuandoLogsSuperanLimite_EliminaPrimeroElMasAntiguo()
        {
            string directory =
                CreateTemporaryDirectory();

            try
            {
                Directory.CreateDirectory(
                    directory);

                string oldestLogPath =
                    Path.Combine(
                        directory,
                        "runtime-oldest.csv");

                string middleLogPath =
                    Path.Combine(
                        directory,
                        "runtime-middle.csv");

                string newestLogPath =
                    Path.Combine(
                        directory,
                        "runtime-newest.csv");

                CreateFileWithSize(
                    oldestLogPath,
                    10L * 1024L * 1024L);

                CreateFileWithSize(
                    middleLogPath,
                    10L * 1024L * 1024L);

                CreateFileWithSize(
                    newestLogPath,
                    10L * 1024L * 1024L);

                DateTime nowUtc =
                    DateTime.UtcNow;

                File.SetLastWriteTimeUtc(
                    oldestLogPath,
                    nowUtc.AddHours(-3));

                File.SetLastWriteTimeUtc(
                    middleLogPath,
                    nowUtc.AddHours(-2));

                File.SetLastWriteTimeUtc(
                    newestLogPath,
                    nowUtc.AddHours(-1));

                _ =
                    new RuntimeLogWriter(
                        directory);

                Assert.False(
                    File.Exists(
                        oldestLogPath));

                Assert.True(
                    File.Exists(
                        middleLogPath));

                Assert.True(
                    File.Exists(
                        newestLogPath));

                long remainingSize =
                    new DirectoryInfo(
                        directory)
                    .GetFiles(
                        "runtime-*.csv")
                    .Sum(
                        file => file.Length);

                Assert.True(
                    remainingSize <=
                    25L * 1024L * 1024L);
            }
            finally
            {
                DeleteTemporaryDirectory(
                    directory);
            }
        }

        private static RuntimeSample CreateSample(
            string trayState)
        {
            return new RuntimeSample
            {
                Timestamp =
                    new DateTime(
                        2026,
                        1,
                        2,
                        3,
                        4,
                        5,
                        DateTimeKind.Utc),

                ApplicationUptime =
                    TimeSpan.FromSeconds(
                        5.5),

                SampleIntervalMilliseconds =
                    1000.25,

                SystemCpuPercent =
                    12.25,

                CpuFrequencyMhz =
                    3500.75,

                SystemMemoryPercent =
                    63.125,

                CoreVigilCpuPercent =
                    0.5,

                WorkingSetBytes =
                    123456789,

                ManagedHeapBytes =
                    9876543,

                GcHeapSizeBytes =
                    8765432,

                GcIndex =
                    42,

                IsManagedHeapReadingValid =
                    true,

                IsHudVisible =
                    true,

                IsTopmost =
                    false,

                TrayState =
                    trayState
            };
        }

        private static string CreateTemporaryDirectory()
        {
            return Path.Combine(
                Path.GetTempPath(),
                "CoreVigil.Tests",
                Guid.NewGuid().ToString("N"));
        }

        private static void CreateFileWithSize(
    string path,
    long sizeBytes)
        {
            using FileStream stream =
                new(
                    path,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None);

            stream.SetLength(
                sizeBytes);
        }

        private static void DeleteTemporaryDirectory(
            string directory)
        {
            if (!Directory.Exists(directory))
            {
                return;
            }

            Directory.Delete(
                directory,
                recursive: true);
        }
    }
}