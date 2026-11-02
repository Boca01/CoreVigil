namespace CoreVigil.Diagnostics
{
    /// <summary>
    /// Representa una muestra interna del comportamiento
    /// de CoreVigil durante una sesión de diagnóstico.
    /// </summary>
    public sealed class RuntimeSample
    {
        public DateTime Timestamp { get; init; }

        public TimeSpan ApplicationUptime { get; init; }

        public double SampleIntervalMilliseconds { get; init; }

        public double SystemCpuPercent { get; init; }

        public double CpuFrequencyMhz { get; init; }

        public double SystemMemoryPercent { get; init; }    

        public double CoreVigilCpuPercent { get; init; }

        public long WorkingSetBytes { get; init; }

        public long ManagedHeapBytes { get; init; }

        public long GcHeapSizeBytes { get; init; }

        public long GcIndex { get; init; }

        public bool IsManagedHeapReadingValid { get; init; }

        public bool IsHudVisible { get; init; }

        public bool IsTopmost { get; init; }

        public string TrayState { get; init; } =
            string.Empty;
    }
}