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

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _contextMenu.Dispose();
        }
    }
}