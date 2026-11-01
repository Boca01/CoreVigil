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

        public TrayIconService()
        {
            _notifyIcon =
                new NotifyIcon
                {
                    Text = "CoreVigil",
                    Icon = SystemIcons.Application,
                    Visible = true
                };
        }

        public void Dispose()
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }
}