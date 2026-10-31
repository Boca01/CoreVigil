using CoreVigil.Core.Abstractions;

namespace CoreVigil.Monitoring
{
    /// <summary>
    /// Obtiene el tiempo de actividad transcurrido desde
    /// el último inicio del sistema.
    /// </summary>
    public sealed class SystemUptimeProvider : ISystemUptimeProvider
    {
        public TimeSpan GetUptime()
        {
            return TimeSpan.FromMilliseconds(
                Environment.TickCount64);
        }
    }
}