using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace CoreVigil.UI
{
    public partial class VigilUI
    {
        private const int ExtendedWindowStyle = -20;
        private const long AppWindowStyle = 0x00040000L;
        private const long ToolWindowStyle = 0x00000080L;
        private const uint FrameChangedFlags =
            0x0001 | 0x0002 | 0x0004 | 0x0010 | 0x0020;

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            ApplyAltTabVisibility();
        }

        private void ApplyAltTabVisibility()
        {
            try
            {
                IntPtr handle =
                    new WindowInteropHelper(this).Handle;

                if (handle == IntPtr.Zero)
                {
                    return;
                }

                long extendedStyle =
                    GetWindowLongPointer(
                        handle,
                        ExtendedWindowStyle).ToInt64();

                if (IsMiniHudMode)
                {
                    extendedStyle &= ~AppWindowStyle;
                    extendedStyle |= ToolWindowStyle;
                }
                else
                {
                    extendedStyle &= ~ToolWindowStyle;
                    extendedStyle |= AppWindowStyle;
                }

                SetWindowLongPointer(
                    handle,
                    ExtendedWindowStyle,
                    new IntPtr(extendedStyle));

                if (!SetWindowPositionNative(
                        handle,
                        IntPtr.Zero,
                        0,
                        0,
                        0,
                        0,
                        FrameChangedFlags))
                {
                    throw new Win32Exception(
                        Marshal.GetLastPInvokeError());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    $"No fue posible aplicar el estilo Alt+Tab: {ex}");
            }
        }

        private static IntPtr GetWindowLongPointer(
            IntPtr handle,
            int index)
        {
            Marshal.SetLastPInvokeError(0);

            IntPtr value = IntPtr.Size == 8
                ? GetWindowLongPtr64(handle, index)
                : new IntPtr(GetWindowLong32(handle, index));

            int error =
                Marshal.GetLastPInvokeError();

            if (value == IntPtr.Zero && error != 0)
            {
                throw new Win32Exception(error);
            }

            return value;
        }

        private static void SetWindowLongPointer(
            IntPtr handle,
            int index,
            IntPtr value)
        {
            Marshal.SetLastPInvokeError(0);

            IntPtr previousValue;

            if (IntPtr.Size == 8)
            {
                previousValue =
                    SetWindowLongPtr64(
                        handle,
                        index,
                        value);
            }
            else
            {
                previousValue =
                    new IntPtr(
                        SetWindowLong32(
                            handle,
                            index,
                            value.ToInt32()));
            }

            int error =
                Marshal.GetLastPInvokeError();

            if (previousValue == IntPtr.Zero && error != 0)
            {
                throw new Win32Exception(error);
            }
        }

        [DllImport(
            "user32.dll",
            EntryPoint = "GetWindowLongPtrW",
            SetLastError = true)]
        private static extern IntPtr GetWindowLongPtr64(
            IntPtr handle,
            int index);

        [DllImport(
            "user32.dll",
            EntryPoint = "GetWindowLongW",
            SetLastError = true)]
        private static extern int GetWindowLong32(
            IntPtr handle,
            int index);

        [DllImport(
            "user32.dll",
            EntryPoint = "SetWindowLongPtrW",
            SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr64(
            IntPtr handle,
            int index,
            IntPtr value);

        [DllImport(
            "user32.dll",
            EntryPoint = "SetWindowLongW",
            SetLastError = true)]
        private static extern int SetWindowLong32(
            IntPtr handle,
            int index,
            int value);

        [DllImport(
            "user32.dll",
            EntryPoint = "SetWindowPos",
            SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPositionNative(
            IntPtr handle,
            IntPtr insertAfter,
            int x,
            int y,
            int width,
            int height,
            uint flags);
    }
}
