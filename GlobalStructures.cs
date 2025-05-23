using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

namespace GlobalStructures
{
    public enum HRESULT : int
    {
        S_OK = 0,
        S_FALSE = 1,
        E_NOTIMPL = unchecked((int)0x80004001),
        E_NOINTERFACE = unchecked((int)0x80004002),
        E_POINTER = unchecked((int)0x80004003),
        E_FAIL = unchecked((int)0x80004005),
        E_UNEXPECTED = unchecked((int)0x8000FFFF),
        E_OUTOFMEMORY = unchecked((int)0x8007000E),
        E_INVALIDARG = unchecked((int)0x80070057),
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
        public RECT(int Left, int Top, int Right, int Bottom)
        {
            left = Left;
            top = Top;
            right = Right;
            bottom = Bottom;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x;
        public int y;

        public POINT(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SIZE
    {
        public int cx;
        public int cy;
        public SIZE(int cx, int cy)
        {
            this.cx = cx;
            this.cy = cy;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct LARGE_INTEGER
    {
        [FieldOffset(0)]
        public int LowPart;
        [FieldOffset(4)]
        public int HighPart;
        [FieldOffset(0)]
        public long QuadPart;
    }

    public enum VARENUM
    {
        VT_EMPTY = 0,
        VT_NULL = 1,
        VT_I2 = 2,
        VT_I4 = 3,
        VT_R4 = 4,
        VT_R8 = 5,
        VT_CY = 6,
        VT_DATE = 7,
        VT_BSTR = 8,
        VT_DISPATCH = 9,
        VT_ERROR = 10,
        VT_BOOL = 11,
        VT_VARIANT = 12,
        VT_UNKNOWN = 13,
        VT_DECIMAL = 14,
        VT_I1 = 16,
        VT_UI1 = 17,
        VT_UI2 = 18,
        VT_UI4 = 19,
        VT_I8 = 20,
        VT_UI8 = 21,
        VT_INT = 22,
        VT_UINT = 23,
        VT_VOID = 24,
        VT_HRESULT = 25,
        VT_PTR = 26,
        VT_SAFEARRAY = 27,
        VT_CARRAY = 28,
        VT_USERDEFINED = 29,
        VT_LPSTR = 30,
        VT_LPWSTR = 31,
        VT_RECORD = 36,
        VT_INT_PTR = 37,
        VT_UINT_PTR = 38,
        VT_FILETIME = 64,
        VT_BLOB = 65,
        VT_STREAM = 66,
        VT_STORAGE = 67,
        VT_STREAMED_OBJECT = 68,
        VT_STORED_OBJECT = 69,
        VT_BLOB_OBJECT = 70,
        VT_CF = 71,
        VT_CLSID = 72,
        VT_VERSIONED_STREAM = 73,
        VT_BSTR_BLOB = 0xfff,
        VT_VECTOR = 0x1000,
        VT_ARRAY = 0x2000,
        VT_BYREF = 0x4000,
        VT_RESERVED = 0x8000,
        VT_ILLEGAL = 0xffff,
        VT_ILLEGALMASKED = 0xfff,
        VT_TYPEMASK = 0xfff
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct PROPARRAY
    {
        public uint cElems;
        public IntPtr pElems;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct PROPVARIANT
    {
        [FieldOffset(0)]
        public ushort varType;
        [FieldOffset(2)]
        public ushort wReserved1;
        [FieldOffset(4)]
        public ushort wReserved2;
        [FieldOffset(6)]
        public ushort wReserved3;

        [FieldOffset(8)]
        public byte bVal;
        [FieldOffset(8)]
        public sbyte cVal;
        [FieldOffset(8)]
        public ushort uiVal;
        [FieldOffset(8)]
        public short iVal;
        [FieldOffset(8)]
        public UInt32 uintVal;
        [FieldOffset(8)]
        public Int32 intVal;
        [FieldOffset(8)]
        public UInt64 ulVal;
        [FieldOffset(8)]
        public Int64 lVal;
        [FieldOffset(8)]
        public float fltVal;
        [FieldOffset(8)]
        public double dblVal;
        [FieldOffset(8)]
        public short boolVal;
        [FieldOffset(8)]
        public IntPtr pclsidVal; // GUID ID pointer
        [FieldOffset(8)]
        public IntPtr pszVal; // Ansi string pointer
        [FieldOffset(8)]
        public IntPtr pwszVal; // Unicode string pointer
        [FieldOffset(8)]
        public IntPtr punkVal; // punkVal (interface pointer)
        [FieldOffset(8)]
        public PROPARRAY ca;
        [FieldOffset(8)]
        public System.Runtime.InteropServices.ComTypes.FILETIME filetime;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PROPERTYKEY
    {
        private readonly Guid _fmtid;
        private readonly uint _pid;

        public PROPERTYKEY(Guid fmtid, uint pid)
        {
            _fmtid = fmtid;
            _pid = pid;
        }

        public static readonly PROPERTYKEY PKEY_ItemNameDisplay = new PROPERTYKEY(new Guid("B725F130-47EF-101A-A5F1-02608C9EEBAC"), 10);
        public static readonly PROPERTYKEY PKEY_FileVersion = new PROPERTYKEY(new Guid("0CEF7D53-FA64-11D1-A203-0000F81FEDEE"), 4);
    }  

    internal class GlobalTools
    {
#nullable enable
        public static void SafeRelease<T>(ref T? comObject) where T : class
        {
            T? t = comObject;
            comObject = default;
            if (t != null)
            {
                if (Marshal.IsComObject(t))
                    Marshal.ReleaseComObject(t);
            }
        }
#nullable disable

        public const long DELETE = (0x00010000L);
        public const long READ_CONTROL = (0x00020000L);
        public const long WRITE_DAC = (0x00040000L);
        public const long WRITE_OWNER = (0x00080000L);
        public const long SYNCHRONIZE = (0x00100000L);

        public const long GENERIC_READ = (0x80000000L);
        public const long GENERIC_WRITE = (0x40000000L);
        public const long GENERIC_EXECUTE = (0x20000000L);
        public const long GENERIC_ALL = (0x10000000L);

        public enum STGC :int
        {
            STGC_DEFAULT = 0,
            STGC_OVERWRITE = 1,
            STGC_ONLYIFCURRENT = 2,
            STGC_DANGEROUSLYCOMMITMERELYTODISKCACHE = 4,
            STGC_CONSOLIDATE = 8
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint GetDpiForWindow(IntPtr hwnd);
    }
}
