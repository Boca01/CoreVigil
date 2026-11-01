using System.Drawing;
using System.Windows.Forms;
using System.IO;

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
        private readonly Icon _lowIcon;
        private readonly Icon _mediumIcon;
        private readonly Icon _highIcon;
        private readonly Icon _criticalIcon;
        public TrayIconService()
        {
            string basePath =
    AppContext.BaseDirectory;

            _lowIcon =
                new Icon(
                    Path.Combine(
                        basePath,
                        "Assets",
                        "Icons",
                        "Tray",
                        "CoreVigil_Low.ico"));

            _mediumIcon =
                new Icon(
                    Path.Combine(
                        basePath,
                        "Assets",
                        "Icons",
                        "Tray",
                        "CoreVigil_Medium.ico"));

            _highIcon =
                new Icon(
                    Path.Combine(
                        basePath,
                        "Assets",
                        "Icons",
                        "Tray",
                        "CoreVigil_High.ico"));

            _criticalIcon =
                new Icon(
                    Path.Combine(
                        basePath,
                        "Assets",
                        "Icons",
                        "Tray",
                        "CoreVigil_Critical.ico"));

            _contextMenu = new ContextMenuStrip();

            var showItem =
                new ToolStripMenuItem("Mostrar CoreVigil");

            var startupItem =
                 new ToolStripMenuItem("Iniciar con Windows")
    {
        CheckOnClick = false
    };

            var exitItem =
                new ToolStripMenuItem("Salir");

            showItem.Click +=
                (_, _) => ShowRequested?.Invoke(
                    this,
                    EventArgs.Empty);

            startupItem.Click +=
                (_, _) => StartupToggleRequested?.Invoke(
                    this,
                    EventArgs.Empty);

            exitItem.Click +=
                (_, _) => ExitRequested?.Invoke(
                    this,
                    EventArgs.Empty);

            _contextMenu.Items.Add(showItem);

            _contextMenu.Items.Add(
                new ToolStripSeparator());

            _contextMenu.Items.Add(startupItem);

            _contextMenu.Items.Add(
                new ToolStripSeparator());

            _contextMenu.Items.Add(exitItem);

            _notifyIcon =
                new NotifyIcon
                {
                    Text = "CoreVigil",
                    Icon = _lowIcon,
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
        public event EventHandler? StartupToggleRequested;
        public CpuTrayState CurrentState => _cpuState;

        public void SetStartupEnabled(
        bool enabled)
        {
            foreach (ToolStripItem item in _contextMenu.Items)
            {
                if (item is ToolStripMenuItem menuItem &&
                    menuItem.Text == "Iniciar con Windows")
                {
                    menuItem.Checked = enabled;
                    return;
                }
            }
        }

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
                        _lowIcon,

                    CpuTrayState.Medium =>
                        _mediumIcon,

                    CpuTrayState.High =>
                        _highIcon,

                    CpuTrayState.Critical =>
                        _criticalIcon,

                    _ =>
                        _lowIcon
                };

            _notifyIcon.Text =
                $"CoreVigil - CPU {usagePercent:F0}%";
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
            _lowIcon.Dispose();
            _mediumIcon.Dispose();
            _highIcon.Dispose();
            _criticalIcon.Dispose();
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