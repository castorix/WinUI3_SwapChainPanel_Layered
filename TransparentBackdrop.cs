using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Media;

// Windows App SDK 1.3, 1.4, 1.5, 1.6

namespace WinUI3_SwapChainPanel_Layered
{
    internal class TransparentBackdrop : SystemBackdrop
    {
        static readonly Lazy<Windows.UI.Composition.Compositor> m_Compositor = new(() =>
        {
            WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();
            return new();
        });
        static Windows.UI.Composition.Compositor Compositor => m_Compositor.Value;
        protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, Microsoft.UI.Xaml.XamlRoot xamlRoot)
        {
            connectedTarget.SystemBackdrop = Compositor.CreateColorBrush(Windows.UI.Color.FromArgb(0, 255, 0, 255));
        }
        protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget)
        {
            disconnectedTarget.SystemBackdrop = null;
        }
    }

    //https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/system-backdrop-controller#example-use-mica-in-a-windows-appsdkwinui-3-app

    public static class WindowsSystemDispatcherQueueHelper
    {
        [StructLayout(LayoutKind.Sequential)]
        struct DispatcherQueueOptions
        {
            internal int dwSize;
            internal int threadType;
            internal int apartmentType;
        }

        [DllImport("CoreMessaging.dll")]
        private static extern int CreateDispatcherQueueController([In] DispatcherQueueOptions options, [In, Out, MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);

        static object? m_dispatcherQueueController = null;
        public static void EnsureWindowsSystemDispatcherQueueController()
        {
            if (Windows.System.DispatcherQueue.GetForCurrentThread() != null)
            {
                // one already exists, so we'll just use it.
                return;
            }

            if (m_dispatcherQueueController == null)
            {
                DispatcherQueueOptions options;
                options.dwSize = Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = 2;    // DQTYPE_THREAD_CURRENT
                options.apartmentType = 2; // DQTAT_COM_STA

                CreateDispatcherQueueController(options, ref m_dispatcherQueueController);
            }
        }
    }
}
