using System.Drawing;
using System.Windows.Forms;

namespace CoreVigil.Services
{
    /// <summary>
    /// Gestiona el icono de CoreVigil en el área
    /// de notificación de Windows.
    /// </summary>
    public sealed class TrayIconService : IDisposable
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private CpuTrayState _cpuState = CpuTrayState.Unknown;
        public TrayIconService()
        {
            _contextMenu = new ContextMenuStrip();

            var showItem =
                new ToolStripMenuItem("Mostrar CoreVigil");

            var exitItem =
                new ToolStripMenuItem("Salir");

            showItem.Click +=
                (_, _) => ShowRequested?.Invoke(
                    this,
                    EventArgs.Empty);

            exitItem.Click +=
                (_, _) => ExitRequested?.Invoke(
                    this,
                    EventArgs.Empty);

            _contextMenu.Items.Add(showItem);
            _contextMenu.Items.Add(
                new ToolStripSeparator());
            _contextMenu.Items.Add(exitItem);

            _notifyIcon =
                new NotifyIcon
                {
                    Text = "CoreVigil",
                    Icon = SystemIcons.Application,
                    ContextMenuStrip = _contextMenu,
                    Visible = true
                };

            _notifyIcon.DoubleClick +=
                (_, _) => ShowRequested?.Invoke(
                    this,
                    EventArgs.Empty);
        }


        public event EventHandler? ShowRequested;

        public event EventHandler? ExitRequested;

        public void UpdateCpuUsage(
            double usagePercent)
        {
            CpuTrayState newState =
                usagePercent switch
                {
                    < 40.0 => CpuTrayState.Low,
                    < 70.0 => CpuTrayState.Medium,
                    < 90.0 => CpuTrayState.High,
                    _ => CpuTrayState.Critical
                };

            if (_cpuState == newState)
            {
                return;
            }

            _cpuState = newState;

            _notifyIcon.Icon =
                newState switch
                {
                    CpuTrayState.Low =>
                        SystemIcons.Information,

                    CpuTrayState.Medium =>
                        SystemIcons.Application,

                    CpuTrayState.High =>
                        SystemIcons.Warning,

                    CpuTrayState.Critical =>
                        SystemIcons.Error,

                    _ =>
                        SystemIcons.Application
                };

            _notifyIcon.Text =
                $"CoreVigil - CPU {usagePercent:F0}%";
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }

    public enum CpuTrayState
    {
        Unknown,
        Low,
        Medium,
        High,
        Critical
    }

}