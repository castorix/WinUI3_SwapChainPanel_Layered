// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

using DXGI;
using GlobalStructures;
using Direct2D;
using static DXGI.DXGITools;
using GDIPlus;
using static GDIPlus.GDIPlusTools;
using static WinUI3_SwapChainPanel_Layered.MainWindow;
using Microsoft.Win32;
using System.Text;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WinUI3_SwapChainPanel_Layered
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        [ComImport, Guid("63aad0b8-7c24-40ff-85a8-640d944cc325"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface ISwapChainPanelNative
        {
            [PreserveSig]
            HRESULT SetSwapChain(IDXGISwapChain swapChain);
        }

        public const uint LWA_COLORKEY = 0x00000001;
        public const uint LWA_ALPHA = 0x00000002;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, IntPtr pptDst, IntPtr psize, IntPtr hdcSrc, IntPtr pprSrc, int crKey, ref BLENDFUNCTION pblend, int dwFlags);

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        public const byte AC_SRC_OVER = 0x00;
        public const byte AC_SRC_ALPHA = 0x01;

        public const int ULW_COLORKEY = 0x00000001;
        public const int ULW_ALPHA = 0x00000002;
        public const int ULW_OPAQUE = 0x00000004;

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetObject(IntPtr hFont, int nSize, out BITMAP bm);

        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct BITMAP
        {
            public int bmType;
            public int bmWidth;
            public int bmHeight;
            public int bmWidthBytes;
            public short bmPlanes;
            public short bmBitsPixel;
            public IntPtr bmBits;
        }

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        const int GWL_STYLE = (-16);
        const int GWL_EXSTYLE = (-20);
        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLongPtr32(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        public static long GetWindowLong(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 4)
            {
                return GetWindowLong32(hWnd, nIndex);
            }
            return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("User32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern long GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("User32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern long GetWindowLongPtr64(IntPtr hWnd, int nIndex);

        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TRANSPARENT =0x00000020;
        //public const int WS_POPUP = unchecked((int)0x80000000L);
        //public const int WS_VISIBLE = 0x10000000;
        //public const int WS_SYSMENU = 0x00080000;
        //public const int WS_THICKFRAME = 0x00040000;
        //public const int WS_BORDER = 0x00800000;
        //public const int WS_CAPTION = 0x00C00000;
        //public const int WS_MINIMIZEBOX = 0x00020000;
        //public const int WS_MAXIMIZEBOX = 0x00010000;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetCursorPos(out Windows.Graphics.PointInt32 lpPoint);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        public const int RDW_INVALIDATE = 0x0001;
        public const int RDW_INTERNALPAINT = 0x0002;
        public const int RDW_ERASE = 0x0004;

        public const int RDW_VALIDATE = 0x0008;
        public const int RDW_NOINTERNALPAINT = 0x0010;
        public const int RDW_NOERASE = 0x0020;

        public const int RDW_NOCHILDREN = 0x0040;
        public const int RDW_ALLCHILDREN = 0x0080;

        public const int RDW_UPDATENOW = 0x0100;
        public const int RDW_ERASENOW = 0x0200;

        public const int RDW_FRAME = 0x0400;
        public const int RDW_NOFRAME = 0x0800;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public const int SWP_NOSIZE = 0x0001;
        public const int SWP_NOMOVE = 0x0002;
        public const int SWP_NOZORDER = 0x0004;
        public const int SWP_NOREDRAW = 0x0008;
        public const int SWP_NOACTIVATE = 0x0010;
        public const int SWP_FRAMECHANGED = 0x0020;  /* The frame changed: send WM_NCCALCSIZE */
        public const int SWP_SHOWWINDOW = 0x0040;
        public const int SWP_HIDEWINDOW = 0x0080;
        public const int SWP_NOCOPYBITS = 0x0100;
        public const int SWP_NOOWNERZORDER = 0x0200;  /* Don't do owner Z ordering */
        public const int SWP_NOSENDCHANGING = 0x0400;  /* Don't send WM_WINDOWPOSCHANGING */
        public const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
        public const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
        public const int SWP_DEFERERASE = 0x2000;
        public const int SWP_ASYNCWINDOWPOS = 0x4000;

        //[DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#132")]
        //public static extern bool ShouldAppsUseDarkMode();

        //[DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        //public static extern bool ShouldSystemUseDarkMode();

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);


        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_PASSIVE_UPDATE_MODE,
            DWMWA_USE_HOSTBACKDROPBRUSH,
            DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
            DWMWA_WINDOW_CORNER_PREFERENCE = 33,
            DWMWA_BORDER_COLOR,
            DWMWA_CAPTION_COLOR,
            DWMWA_TEXT_COLOR,
            DWMWA_VISIBLE_FRAME_BORDER_THICKNESS,
            DWMWA_SYSTEMBACKDROP_TYPE,
            DWMWA_LAST
        };

        public enum DWMNCRENDERINGPOLICY
        {
            DWMNCRP_USEWINDOWSTYLE, // Enable/disable non-client rendering based on window style
            DWMNCRP_DISABLED,       // Disabled non-client rendering; window style is ignored
            DWMNCRP_ENABLED,        // Enabled non-client rendering; window style is ignored
            DWMNCRP_LAST
        };

        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        [DllImport("Dwmapi.dll", SetLastError = true, CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern HRESULT DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute, uint cbAttribute);

        [DllImport("Dwmapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFOHEADER
        {
            [MarshalAs(UnmanagedType.I4)]
            public int biSize;
            [MarshalAs(UnmanagedType.I4)]
            public int biWidth;
            [MarshalAs(UnmanagedType.I4)]
            public int biHeight;
            [MarshalAs(UnmanagedType.I2)]
            public short biPlanes;
            [MarshalAs(UnmanagedType.I2)]
            public short biBitCount;
            [MarshalAs(UnmanagedType.I4)]
            public int biCompression;
            [MarshalAs(UnmanagedType.I4)]
            public int biSizeImage;
            [MarshalAs(UnmanagedType.I4)]
            public int biXPelsPerMeter;
            [MarshalAs(UnmanagedType.I4)]
            public int biYPelsPerMeter;
            [MarshalAs(UnmanagedType.I4)]
            public int biClrUsed;
            [MarshalAs(UnmanagedType.I4)]
            public int biClrImportant;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BITMAPINFO
        {
            [MarshalAs(UnmanagedType.Struct, SizeConst = 40)]
            public BITMAPINFOHEADER bmiHeader;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public int[] bmiColors;
        }

        public const int BI_RGB = 0;
        public const int BI_RLE8 = 1;
        public const int BI_RLE4 = 2;
        public const int BI_BITFIELDS = 3;
        public const int BI_JPEG = 4;
        public const int BI_PNG = 5;

        public const int DIB_RGB_COLORS = 0;
        public const int DIB_PAL_COLORS = 1;

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO pbmi, uint usage, ref IntPtr ppvBits, IntPtr hSection, int offset);

        public const int SRCCOPY = 0x00CC0020; /* dest = source                   */
        public const int SRCPAINT = 0x00EE0086; /* dest = source OR dest           */
        public const int SRCAND = 0x008800C6; /* dest = source AND dest          */
        public const int SRCINVERT = 0x00660046; /* dest = source XOR dest          */
        public const int SRCERASE = 0x00440328; /* dest = source AND (NOT dest )   */
        public const int NOTSRCCOPY = 0x00330008; /* dest = (NOT source)             */
        public const int NOTSRCERASE = 0x001100A6; /* dest = (NOT src) AND (NOT dest) */
        public const int MERGECOPY = 0x00C000CA; /* dest = (source AND pattern)     */
        public const int MERGEPAINT = 0x00BB0226; /* dest = (NOT source) OR dest     */
        public const int PATCOPY = 0x00F00021; /* dest = pattern                  */
        public const int PATPAINT = 0x00FB0A09; /* dest = DPSnoo                   */
        public const int PATINVERT = 0x005A0049; /* dest = pattern XOR dest         */
        public const int DSTINVERT = 0x00550009; /* dest = (NOT dest)               */
        public const int BLACKNESS = 0x00000042; /* dest = BLACK                    */
        public const int WHITENESS = 0x00FF0062; /* dest = WHITE                    */
        public const int NOMIRRORBITMAP = unchecked((int)0x80000000); /* Do not Mirror the bitmap in this call */
        public const int CAPTUREBLT = 0x40000000; /* Include layered windows */

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool BitBlt(IntPtr hDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int StretchDIBits(IntPtr hdc, int XDest, int YDest, int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth, int nSrcHeight, IntPtr lpBits, ref BITMAPINFO lpBitsInfo, int iUsage, int dwRop);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int StretchDIBits(IntPtr hdc, int XDest, int YDest, int nDestWidth, int nDestHeight, int XSrc, int YSrc, int nSrcWidth, int nSrcHeight, byte[] lpBits, ref BITMAPINFO lpBitsInfo, int iUsage, int dwRop);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr WindowFromPoint(Windows.Graphics.PointInt32 Point);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetPixel(IntPtr hdc, int x, int y);

        [DllImport("User32.dll", SetLastError = true)]
        public static extern IntPtr CreateWindowEx(int dwExStyle, string lpClassName, string lpWindowName, int dwStyle, int x, int y, int nWidth, int nHeight, IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

        public const int WS_OVERLAPPED = 0x00000000,
            WS_POPUP = unchecked((int)0x80000000),
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_CAPTION = 0x00C00000,
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_TABSTOP = 0x00010000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED |
                             WS_CAPTION |
                             WS_SYSMENU |
                             WS_THICKFRAME |
                             WS_MINIMIZEBOX |
                             WS_MAXIMIZEBOX;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint SetClassLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public const int GCL_HBRBACKGROUND = -10;

        [DllImport("Gdi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr CreateSolidBrush(int crColor);

        public int RGB(byte r, byte g, byte b)
        {
            return (r) | ((g) << 8) | ((b) << 16);
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool ShowWindow(IntPtr hWnd, int nShowCmd);

        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;

        public delegate int SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, uint dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        public static extern int DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool FillRect(IntPtr hdc, [In] ref RECT rect, IntPtr hbrush);

        public const int WM_ERASEBKGND = 0x0014;

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int SendInput(int nInputs, [MarshalAs(UnmanagedType.LPArray)] INPUT[] pInput, int cbSize);

        public const int INPUT_MOUSE = 0;
        public const int INPUT_KEYBOARD = 1;
        public const int INPUT_HARDWARE = 2;

        public const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        public const int KEYEVENTF_KEYUP = 0x0002;
        public const int KEYEVENTF_UNICODE = 0x0004;

        public const int MOUSEEVENTF_MOVE = 0x0001;
        public const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        public const int MOUSEEVENTF_LEFTUP = 0x0004;
        public const int MOUSEEVENTF_ABSOLUTE = 0x8000;

        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;

        public const int WM_MOUSEMOVE = 0x0200;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;

        [StructLayout(LayoutKind.Sequential)]
        public struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public int mouseData;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KEYBDINPUT
        {
            public short wVk;
            public short wScan;
            public int dwFlags;
            public int time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HARDWAREINPUT
        {
            public int uMsg;
            public short wParamL;
            public short wParamH;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct INPUT
        {
            public int type;
            public INPUTUNION inputUnion;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct INPUTUNION
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public int wParam;
            public IntPtr lParam;
            public uint time;
            public System.Drawing.Point pt;
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        public const int PM_NOREMOVE = 0x0000;
        public const int PM_REMOVE = 0x0001;
        public const int PM_NOYIELD = 0x0002;

        private SUBCLASSPROC SubClassDelegate;

        private IntPtr hWndMain = IntPtr.Zero;
        private IntPtr hWndDesktopChildSiteBridge = IntPtr.Zero;
        private Microsoft.UI.Windowing.AppWindow _apw;
        private Microsoft.UI.Windowing.OverlappedPresenter _presenter;

        IntPtr m_initToken = IntPtr.Zero;
        IntPtr m_hBitmap = IntPtr.Zero;

        ID2D1Factory m_pD2DFactory = null;
        ID2D1Factory1 m_pD2DFactory1 = null;

        IntPtr m_pD3D11DevicePtr = IntPtr.Zero;
        ID3D11DeviceContext m_pD3D11DeviceContext = null;
        IDXGIDevice1 m_pDXGIDevice = null;

        ID2D1DeviceContext m_pD2DDeviceContext = null;

        //ID2D1Bitmap1 m_pD2DTargetBitmap = null;
        IDXGISwapChain1 m_pDXGISwapChain1 = null;

        public MainWindow()
        {
            this.InitializeComponent();

            HRESULT hr = HRESULT.S_OK;

            //Application.Current.Resources["ButtonBackground"] = new SolidColorBrush(Microsoft.UI.Colors.Blue);
            Application.Current.Resources["ButtonBackgroundPressed"] = new SolidColorBrush(Microsoft.UI.Colors.LightSteelBlue);
            Application.Current.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(Microsoft.UI.Colors.RoyalBlue);    

            hWndMain = WinRT.Interop.WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId myWndId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWndMain);
            _apw = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(myWndId);

            hWndDesktopChildSiteBridge = FindWindowEx(hWndMain, IntPtr.Zero, "Microsoft.UI.Content.ContentWindowSiteBridge", null);

            _presenter = _apw.Presenter as Microsoft.UI.Windowing.OverlappedPresenter;
            _presenter.IsResizable = false;
            //_presenter.IsResizable = true;
            _presenter.SetBorderAndTitleBar(false, false);
            //_presenter.SetBorderAndTitleBar(true, false);
            //
            //this.ExtendsContentIntoTitleBar = true;
            //_presenter.IsAlwaysOnTop = true;

            _apw.Resize(new Windows.Graphics.SizeInt32(800, 500));
            _apw.Move(new Windows.Graphics.PointInt32(500, 300));

            // Update for Windows 11 from michalleptuch comment : https://github.com/microsoft/microsoft-ui-xaml/issues/1247#issuecomment-1374474960
            // otherwise there are borders + shadow from his test
            // Returns logically 0x80070057 (E_INVALIDARG) on Windows 10
            int nValue = (int)DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_DEFAULT;
            hr = DwmSetWindowAttribute(hWndMain, (int)DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE, ref nValue, Marshal.SizeOf(typeof(int)));

            //nValue = (int)DWMNCRENDERINGPOLICY.DWMNCRP_DISABLED;
            //hr = DwmSetWindowAttribute(hWndMain, (int)DWMWINDOWATTRIBUTE.DWMWA_NCRENDERING_POLICY, ref nValue, Marshal.SizeOf(typeof(int)));

            this.Closed += MainWindow_Closed;

            StartupInput input = StartupInput.GetDefault();
            StartupOutput output;           
            GpStatus nStatus = GdiplusStartup(out m_initToken, ref input, out output);
 
            IntPtr pImage = IntPtr.Zero;
            nStatus = GdipCreateBitmapFromFile(@".\Assets\Frame_Blue_center_transp.png", out pImage);
            if (nStatus == GpStatus.Ok)
            {
                GdipCreateHBITMAPFromBitmap(pImage, out m_hBitmap, RGB(Microsoft.UI.Colors.Black.R, Microsoft.UI.Colors.Black.G, Microsoft.UI.Colors.Black.B));
                GdipDisposeImage(pImage);
            }

            hr = CreateD2D1Factory();
            if (hr == HRESULT.S_OK)
            {
                hr = CreateDeviceContext();
                // hr = CreateDeviceResources();
                // hr = CreateSwapChain(hWndMain);
                hr = CreateSwapChain(IntPtr.Zero);
                if (hr == HRESULT.S_OK)
                {
                    //hr = ConfigureSwapChain();
                    ISwapChainPanelNative panelNative = WinRT.CastExtensions.As<ISwapChainPanelNative>(swapChainPanel1);
                    hr = panelNative.SetSwapChain(m_pDXGISwapChain1);
                    //swapChainPanel1.SizeChanged += SwapChainPanel1_SizeChanged;
                }
                //CompositionTarget.Rendering += CompositionTarget_Rendering;
            }

            long nExStyle = GetWindowLong(hWndMain, GWL_EXSTYLE);
            if ((nExStyle & WS_EX_LAYERED) == 0)
            {
                SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(nExStyle | WS_EX_LAYERED));
                //SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(nExStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT));

                // Test Light Mode
                int nAppsUseLightTheme = 0;
                int nSystemUsesLightTheme = 0;
                string sPathKey = @"Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
                using (RegistryKey rkLocal = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64))
                {
                    using (RegistryKey rk = rkLocal.OpenSubKey(sPathKey, false))
                    {
                        nAppsUseLightTheme = (int)rk.GetValue("AppsUseLightTheme", 0);
                        nSystemUsesLightTheme = (int)rk.GetValue("SystemUsesLightTheme", 0);
                    }
                }
                uint nColorBackground = (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Black);
                //if (nAppsUseLightTheme == 1 || nSystemUsesLightTheme == 1)
                if (nAppsUseLightTheme == 1)
                {
                    nColorBackground = (uint)System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.White);
                    // not refreshed when mouse over...
                    // myButton.Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
                }
                //bool bReturn = SetLayeredWindowAttributes(hWndMain, nColorBackground, 55, LWA_COLORKEY | LWA_ALPHA);
                bool bReturn = SetLayeredWindowAttributes(hWndMain, nColorBackground, 255, LWA_COLORKEY );
            }

            UIElement root = (UIElement)this.Content;         
            root.PointerMoved += Root_PointerMoved;
            root.PointerPressed += Root_PointerPressed;
            root.PointerReleased += Root_PointerReleased;

            //SubClassDelegate = new SUBCLASSPROC(WindowSubClass);
            //bool bRet = SetWindowSubclass(hWndMain, SubClassDelegate, 0, 0);
            
            // Test TopMost
            //_presenter.IsAlwaysOnTop = true;
        }

        private void tsClickThrough_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch ts = sender as ToggleSwitch;
            if (ts.IsOn)
            {
                tb2.Visibility = Visibility.Visible;
                tb3.Visibility = Visibility.Visible;
            }
            else
            {
                tb2.Visibility = Visibility.Collapsed;
                tb3.Visibility = Visibility.Collapsed;
            }
        }

        public void SetOpacity(IntPtr hWnd, int nOpacity)
        {
            SetLayeredWindowAttributes(hWnd, 0, (byte)(255 * nOpacity / 100), LWA_ALPHA);
        }

        bool bSet = false;
        private void myButton_Click(object sender, RoutedEventArgs e)
        {           
            if (m_hBitmap != IntPtr.Zero && !bSet)
            {
                SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(GetWindowLong(hWndMain, GWL_EXSTYLE) & ~WS_EX_LAYERED));
                RedrawWindow(hWndMain, IntPtr.Zero, IntPtr.Zero, RDW_ERASE | RDW_INVALIDATE | RDW_FRAME | RDW_ALLCHILDREN);
                SetWindowLong(hWndMain, GWL_EXSTYLE, (IntPtr)(GetWindowLong(hWndMain, GWL_EXSTYLE) | WS_EX_LAYERED));
                if (SetPictureToLayeredWindow(hWndMain, m_hBitmap))
                {                    
                    mainBorder.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                    tb1.Margin = new Thickness(5, 100, 5, 5);
                    myButton.Margin = new Thickness(10, 10, 10, 10);
                    myButton.Content = "Bitmap set";
                    //RedrawWindow(hWndMain, IntPtr.Zero, IntPtr.Zero, RDW_ERASE | RDW_INVALIDATE | RDW_FRAME | RDW_ALLCHILDREN | RDW_UPDATENOW | RDW_ERASENOW);
                    RECT rectWnd;
                    GetWindowRect(hWndMain, out rectWnd);
                    SetWindowPos(hWndMain, IntPtr.Zero, rectWnd.left, rectWnd.top - 1, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
                    SetWindowPos(hWndMain, IntPtr.Zero, rectWnd.left, rectWnd.top, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW | SWP_FRAMECHANGED);
                    bSet = true;
                }
            }
        }

        private int nX = 0, nY = 0, nXWindow = 0, nYWindow = 0;
        private bool bMoving = false;

        private void Root_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ((UIElement)sender).ReleasePointerCaptures();
            bMoving = false;
        }

        private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
                ((UIElement)sender).CapturePointer(e.Pointer);
                nXWindow = _apw.Position.X;
                nYWindow = _apw.Position.Y;
                Windows.Graphics.PointInt32 pt;
                GetCursorPos(out pt);
                nX = pt.X;
                nY = pt.Y;

                //IntPtr hDCScreen = GetDC(IntPtr.Zero);
                //uint nColor = GetPixel(hDCScreen, nX, nY);
                //ReleaseDC(IntPtr.Zero, hDCScreen);

                IntPtr hWnd = WindowFromPoint(pt);

                //StringBuilder sbClass = new StringBuilder(260);
                //GetClassName(hWnd, sbClass, (int)(sbClass.Capacity));
                //System.Diagnostics.Debug.WriteLine(string.Format("Window = 0x{0:X8} - {1}", hWnd, sbClass.ToString()));

                Microsoft.UI.Input.PointerPoint pp = e.GetCurrentPoint((UIElement)sender);
                Point ptElement = new Point(pp.Position.X, pp.Position.Y);
                IEnumerable<UIElement> elementStack = VisualTreeHelper.FindElementsInHostCoordinates(ptElement, (UIElement)sender);
                int nCpt = 0;
                bool bOK = true;
                foreach (UIElement element in elementStack)
                {
                    if (nCpt == 0)
                    {
                        if (!(element.GetType() == typeof(Border)))
                        {
                            bOK = false;
                            break;                       
                        }
                    }
                    if (nCpt == 1)
                    {
                        if (!(element.GetType() == typeof(SwapChainPanel)))
                        {
                            bOK = false;
                            break;
                        }
                    }
                    nCpt++;
                }                

                if (bOK && tsClickThrough.IsOn)
                {
                    bool bAlwaysOnTop = false;
                    if (_presenter.IsAlwaysOnTop)
                    {
                        bAlwaysOnTop = true;
                        _presenter.IsAlwaysOnTop = false;                       
                    }
                    System.Threading.Thread.Sleep(100);
                    SwitchToThisWindow(hWnd, true);
                    System.Threading.Thread.Sleep(100);
                    INPUT[] mi = new INPUT[1];
                    mi[0].type = INPUT_MOUSE;                   
                    mi[0].inputUnion.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
                    SendInput(1, mi, Marshal.SizeOf(mi[0]));
                    //System.Threading.Thread.Sleep(100);
                    mi[0].inputUnion.mi.dwFlags = MOUSEEVENTF_LEFTUP;
                    SendInput(1, mi, Marshal.SizeOf(mi[0]));
                    //Console.Beep(5000, 10);
                    if (bAlwaysOnTop)
                        _presenter.IsAlwaysOnTop = true;
                }
                else
                    bMoving = true;
                //((UIElement)sender).ReleasePointerCapture(e.Pointer);

                //MSG msg = new MSG();
                //while (PeekMessage(out msg, IntPtr.Zero, WM_LBUTTONDOWN, WM_LBUTTONUP, PM_REMOVE));
                
            }
            else if (properties.IsRightButtonPressed)
            {
                System.Threading.Thread.Sleep(200);
                Application.Current.Exit();
            }
        }

        private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
        {  
            var properties = e.GetCurrentPoint((UIElement)sender).Properties;
            if (properties.IsLeftButtonPressed)
            {
                Windows.Graphics.PointInt32 pt;
                GetCursorPos(out pt);
                if (bMoving)
                    _apw.Move(new Windows.Graphics.PointInt32(nXWindow + (pt.X - nX), nYWindow + (pt.Y - nY)));               
                e.Handled = true;
            }
        }

        public HRESULT CreateD2D1Factory()
        {
            HRESULT hr = HRESULT.S_OK;
            D2D1_FACTORY_OPTIONS options = new D2D1_FACTORY_OPTIONS();

            // Needs "Enable native code Debugging"
            options.debugLevel = D2D1_DEBUG_LEVEL.D2D1_DEBUG_LEVEL_INFORMATION;

            hr = D2DTools.D2D1CreateFactory(D2D1_FACTORY_TYPE.D2D1_FACTORY_TYPE_SINGLE_THREADED, ref D2DTools.CLSID_D2D1Factory, ref options, out m_pD2DFactory);
            m_pD2DFactory1 = (ID2D1Factory1)m_pD2DFactory;
            return hr;
        }

        public HRESULT CreateDeviceContext()
        {
            HRESULT hr = HRESULT.S_OK;
            uint creationFlags = (uint)D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_BGRA_SUPPORT;

            // Needs "Enable native code Debugging"
            creationFlags |= (uint)D3D11_CREATE_DEVICE_FLAG.D3D11_CREATE_DEVICE_DEBUG;

            int[] aD3D_FEATURE_LEVEL = new int[] { (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_1, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_11_0,
                (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_1, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_10_0, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_3,
                (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_2, (int)D3D_FEATURE_LEVEL.D3D_FEATURE_LEVEL_9_1};

            D3D_FEATURE_LEVEL featureLevel;
            hr = D2DTools.D3D11CreateDevice(null,    // specify null to use the default adapter
                D3D_DRIVER_TYPE.D3D_DRIVER_TYPE_HARDWARE,
                IntPtr.Zero,
                creationFlags,      // optionally set debug and Direct2D compatibility flags
                aD3D_FEATURE_LEVEL, // list of feature levels this app can support
                // (uint)Marshal.SizeOf(aD3D_FEATURE_LEVEL),   // number of possible feature levels
                (uint)aD3D_FEATURE_LEVEL.Length, // number of possible feature levels
                D2DTools.D3D11_SDK_VERSION,
                out m_pD3D11DevicePtr,    // returns the Direct3D device created
                out featureLevel,         // returns feature level of device created            
                out m_pD3D11DeviceContext // returns the device immediate context
            );
            if (hr == HRESULT.S_OK)
            {
                m_pDXGIDevice = Marshal.GetObjectForIUnknown(m_pD3D11DevicePtr) as IDXGIDevice1;
                if (m_pD2DFactory1 != null)
                {
                    ID2D1Device pD2DDevice = null;
                    hr = m_pD2DFactory1.CreateDevice(m_pDXGIDevice, out pD2DDevice);
                    if (hr == HRESULT.S_OK)
                    {
                        hr = pD2DDevice.CreateDeviceContext(D2D1_DEVICE_CONTEXT_OPTIONS.D2D1_DEVICE_CONTEXT_OPTIONS_NONE, out m_pD2DDeviceContext);
                        GlobalTools.SafeRelease(ref pD2DDevice);
                    }
                }
                //Marshal.ReleaseComObject(m_pDXGIDevice);
                //Marshal.Release(m_pD3D11DevicePtr);
            }
            return hr;
        }

        HRESULT CreateSwapChain(IntPtr hWnd)
        {
            HRESULT hr = HRESULT.S_OK;
            DXGI_SWAP_CHAIN_DESC1 swapChainDesc = new DXGI_SWAP_CHAIN_DESC1();
            swapChainDesc.Width = 1;
            swapChainDesc.Height = 1;
            swapChainDesc.Format = DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM; // this is the most common swapchain format
            swapChainDesc.Stereo = false;
            swapChainDesc.SampleDesc.Count = 1;                // don't use multi-sampling
            swapChainDesc.SampleDesc.Quality = 0;
            swapChainDesc.BufferUsage = D2DTools.DXGI_USAGE_RENDER_TARGET_OUTPUT;
            swapChainDesc.BufferCount = 2;                     // use double buffering to enable flip
            swapChainDesc.Scaling = (hWnd != IntPtr.Zero) ? DXGI_SCALING.DXGI_SCALING_NONE : DXGI_SCALING.DXGI_SCALING_STRETCH;
            swapChainDesc.SwapEffect = DXGI_SWAP_EFFECT.DXGI_SWAP_EFFECT_FLIP_SEQUENTIAL; // all apps must use this SwapEffect       
            swapChainDesc.Flags = 0;

            IDXGIAdapter pDXGIAdapter;
            hr = m_pDXGIDevice.GetAdapter(out pDXGIAdapter);
            if (hr == HRESULT.S_OK)
            {
                IntPtr pDXGIFactory2Ptr;
                hr = pDXGIAdapter.GetParent(typeof(IDXGIFactory2).GUID, out pDXGIFactory2Ptr);
                if (hr == HRESULT.S_OK)
                {
                    IDXGIFactory2 pDXGIFactory2 = Marshal.GetObjectForIUnknown(pDXGIFactory2Ptr) as IDXGIFactory2;
                    if (hWnd != IntPtr.Zero)
                        hr = pDXGIFactory2.CreateSwapChainForHwnd(m_pD3D11DevicePtr, hWnd, ref swapChainDesc, IntPtr.Zero, null, out m_pDXGISwapChain1);
                    else
                        hr = pDXGIFactory2.CreateSwapChainForComposition(m_pD3D11DevicePtr, ref swapChainDesc, null, out m_pDXGISwapChain1);

                    hr = m_pDXGIDevice.SetMaximumFrameLatency(1);
                    GlobalTools.SafeRelease(ref pDXGIFactory2);
                    Marshal.Release(pDXGIFactory2Ptr);
                }
                GlobalTools.SafeRelease(ref pDXGIAdapter);
            }
            return hr;
        }

        private bool SetPictureToLayeredWindow(IntPtr hWnd, IntPtr hBitmap)
        {
            BITMAP bm;
            GetObject(hBitmap, Marshal.SizeOf(typeof(BITMAP)), out bm);
            System.Drawing.Size sizeBitmap = new System.Drawing.Size(bm.bmWidth, bm.bmHeight);

            IntPtr hDCScreen = GetDC(IntPtr.Zero);
            IntPtr hDCMem = CreateCompatibleDC(hDCScreen);
            IntPtr hBitmapOld = SelectObject(hDCMem, hBitmap);

            BLENDFUNCTION bf = new BLENDFUNCTION();
            bf.BlendOp = AC_SRC_OVER;
            bf.SourceConstantAlpha = 255;
            bf.AlphaFormat = AC_SRC_ALPHA;

            RECT rectWnd;
            GetWindowRect(hWnd, out rectWnd);

            System.Drawing.Point ptSrc = new System.Drawing.Point();
            System.Drawing.Point ptDest = new System.Drawing.Point(rectWnd.left, rectWnd.top);

            IntPtr pptSrc = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Drawing.Point)));
            Marshal.StructureToPtr(ptSrc, pptSrc, false);

            IntPtr pptDest = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Drawing.Point)));
            Marshal.StructureToPtr(ptDest, pptDest, false);

            IntPtr psizeBitmap = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(System.Drawing.Size)));
            Marshal.StructureToPtr(sizeBitmap, psizeBitmap, false);

            bool bRet = UpdateLayeredWindow(hWnd, hDCScreen, pptDest, psizeBitmap, hDCMem, pptSrc, 0, ref bf, ULW_ALPHA);
            //int nErr = Marshal.GetLastWin32Error();

            Marshal.FreeHGlobal(pptSrc);
            Marshal.FreeHGlobal(pptDest);
            Marshal.FreeHGlobal(psizeBitmap);

            SelectObject(hDCMem, hBitmapOld);
            DeleteDC(hDCMem);
            ReleaseDC(IntPtr.Zero, hDCScreen);

            return bRet;
        }

        void Clean()
        {
            GlobalTools.SafeRelease(ref m_pD2DDeviceContext);
            //GlobalTools.SafeRelease(ref m_pD2DDeviceContext3);

            //CleanDeviceResources();

            //GlobalTools.SafeRelease(ref m_pD2DTargetBitmap);
            GlobalTools.SafeRelease(ref m_pDXGISwapChain1);

            GlobalTools.SafeRelease(ref m_pDXGIDevice);
            GlobalTools.SafeRelease(ref m_pD3D11DeviceContext);
            Marshal.Release(m_pD3D11DevicePtr);

            //   GlobalTools.SafeRelease(ref m_pWICImagingFactory);
            GlobalTools.SafeRelease(ref m_pD2DFactory1);
            GlobalTools.SafeRelease(ref m_pD2DFactory);

            GdiplusShutdown(m_initToken);
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            Clean();
        }

        private int WindowSubClass(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, IntPtr uIdSubclass, uint dwRefData)
        {
            switch (uMsg)
            {
                case WM_ERASEBKGND:
                    {
                        RECT rect;
                        GetClientRect(hWnd, out rect);
                        //int nRet = ExcludeClipRect(wParam, 0, 0, rect.right, 35);
                        IntPtr hBrush = CreateSolidBrush(System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.Magenta));
                        //IntPtr hBrush = CreateSolidBrush((int)MakeArgb(255, 255, 0, 0));                       
                        //IntPtr hBrush = CreateSolidBrush(System.Drawing.ColorTranslator.ToWin32(System.Drawing.Color.FromArgb(255, 32, 32, 32)));
                        FillRect(wParam, ref rect, hBrush);
                        DeleteObject(hBrush);
                        return 1;
                    }
                    break;
            }
            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
    }
}
