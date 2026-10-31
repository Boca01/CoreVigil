namespace CoreVigil.Core.Abstractions
{
    /// <summary>
    /// Define el contrato para obtener el tiempo de actividad del sistema.
    /// </summary>
    public interface ISystemUptimeProvider
    {
        TimeSpan GetUptime();
    }
}