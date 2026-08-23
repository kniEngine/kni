// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Xna.Platform.Utilities;

internal partial class Sdl
{
    private static Sdl _current;

    public IntPtr NativeLibrary { get; private set; }
    public Window WINDOW { get; private set; }
    public Display DISPLAY { get; private set; }
    public GL OpenGL { get; private set; }
    public Mouse MOUSE { get; private set; }
    public Keyboard KEYBOARD { get; private set; }
    public Joystick JOYSTICK { get; private set; }
    public GameController GAMECONTROLLER { get; private set; }
    public Haptic HAPTIC { get; private set; }
    public Touch TOUCH { get; private set; }

    public readonly Version version;
    private int _SDLInitThreadId = -1;

    public static Sdl Current
    {
        get
        {
            if (_current != null)
                return _current;

            if (_current == null)
                _current = new Sdl();

            return _current;
        }
    }

    public Sdl()
    {
        NativeLibrary = GetNativeLibrary();
        LoadEntryPoints(NativeLibrary);

        GetVersion(out version);

        WINDOW = new Window(this, NativeLibrary);
        DISPLAY = new Display(this, NativeLibrary);
        OpenGL = new GL(this, NativeLibrary);
        MOUSE = new Mouse(this, NativeLibrary);
        KEYBOARD = new Keyboard(this, NativeLibrary);
        JOYSTICK = new Joystick(this, NativeLibrary);
        GAMECONTROLLER = new GameController(this, NativeLibrary);
        HAPTIC = new Haptic(this, NativeLibrary);
        TOUCH = new Touch(this, NativeLibrary);
    }

    private IntPtr GetNativeLibrary()
    {
        if (CurrentPlatform.OS == OS.Windows)
            return FuncLoader.LoadLibraryExt("SDL2.dll");
        else if (CurrentPlatform.OS == OS.Linux)
            return FuncLoader.LoadLibraryExt("libSDL2-2.0.so.0");
        else if (CurrentPlatform.OS == OS.MacOSX)
            return FuncLoader.LoadLibraryExt("libSDL2.dylib");
        else
            return FuncLoader.LoadLibraryExt("sdl2");
    }


    [Flags]
    public enum InitFlags : int
    {
        Video          = 0x00000020,
        Joystick       = 0x00000200,
        Haptic         = 0x00001000,
        GameController = 0x00002000,
    }

    public enum EventType : uint
    {
        First = 0,

        Quit = 0x100,

        WindowEvent = 0x200,
        SysWM       = 0x201,

        KeyDown     = 0x300,
        KeyUp       = 0x301,
        TextEditing = 0x302,
        TextInput   = 0x303,

        MouseMotion     = 0x400,
        MouseButtonDown = 0x401,
        MouseButtonUp   = 0x402,
        MouseWheel      = 0x403,

        JoyAxisMotion = 0x600,
        JoyBallMotion = 0x601,
        JoyHatMotion  = 0x602,
        JoyButtonDown = 0x603,
        JoyButtonUp   = 0x604,
        JoyDeviceAdded   = 0x605,
        JoyDeviceRemoved = 0x606,

        ControllerAxisMotion = 0x650,
        ControllerButtonDown = 0x651,
        ControllerButtonUp   = 0x652,
        ControllerDeviceAdded    = 0x653,
        ControllerDeviceRemoved  = 0x654,
        ControllerDeviceRemapped = 0x655,

        FingerDown   = 0x700,
        FingerUp     = 0x701,
        FingerMotion = 0x702,

        DollarGesture = 0x800,
        DollarRecord  = 0x801,
        MultiGesture  = 0x802,

        ClipboardUpdate = 0x900,

        DropFile = 0x1000,
        DropText = 0x1001,
        DropBegin = 0x1002,
        DropComplete = 0x1003,

        AudioDeviceAdded   = 0x1100,
        AudioDeviceRemoved = 0x1101,

        RenderTargetsReset = 0x2000,
        RenderDeviceReset  = 0x2001,

        UserEvent = 0x8000,

        Last = 0xFFFF
    }

    public enum EventAction
    {
        AddEvent = 0x0,
        PeekEvent = 0x1,
        GetEvent = 0x2,
    }

    public enum PixelType : byte
    {
        Unknown = 0,
        Index1 = 1,
        Index4 = 2,
        Index8 = 3,
        Packed8 = 4,
        Packed16 = 5,
        Packed32 = 6,
        ArrayU8 = 7,
        ArrayU16 = 8,
        ArrayU32 = 9,
        ArrayF16 = 10,
        ArrayF32 = 11,
    }

    public enum BitmapOrder : byte
    {
        None = 0,
        _4321 = 1,
        _1234 = 2,
    }

    public enum PackedOrder : byte
    {
        None = 0,
        XRGB = 1,
        RGBX = 2,
        ARGB = 3,
        RGBA = 4,
        XBGR = 5,
        BGRX = 6,
        ABGR = 7,
        BGRA = 8,
    }

    public enum ArrayOrder : byte
    {
        None = 0,
        RGB = 1,
        RGBA = 2,
        ARGB = 3,
        BGR = 4,
        BGRA = 5,
        ABGR = 6,
    }

    public enum PackedLayout : byte
    {
        None = 0,
        _332 = 1,
        _4444 = 2,
        _1555 = 3,
        _5551 = 4,
        _565 = 5,
        _8888 = 6,
        _2101010 = 7,
        _1010102 = 8,
    }

    public enum PixelFormat : uint
    {
        Unknown = 0,

        Index1LSB = (1u << 28) | (1u << 24) | (1u << 20) | (1u << 8),
        Index1MSB = (1u << 28) | (1u << 24) | (2u << 20) | (1u << 8),
        Index4LSB = (1u << 28) | (2u << 24) | (1u << 20) | (4u << 8),
        Index4MSB = (1u << 28) | (2u << 24) | (2u << 20) | (4u << 8),
        Index8 = (1u << 28) | (3u << 24) | (8u << 8) | 1u,

        RGB332 = (1u << 28) | (4u << 24) | (1u << 20) | (1u << 16) | (8u << 8) | 1u,
        RGB444 = (1u << 28) | (5u << 24) | (1u << 20) | (2u << 16) | (12u << 8) | 2u,
        RGB555 = (1u << 28) | (5u << 24) | (2u << 20) | (3u << 16) | (15u << 8) | 2u,
        BGR555 = (1u << 28) | (5u << 24) | (6u << 20) | (3u << 16) | (15u << 8) | 2u,
        ARGB4444 = (1u << 28) | (5u << 24) | (3u << 20) | (2u << 16) | (16u << 8) | 2u,
        RGBA4444 = (1u << 28) | (5u << 24) | (4u << 20) | (2u << 16) | (16u << 8) | 2u,
        ABGR4444 = (1u << 28) | (5u << 24) | (7u << 20) | (2u << 16) | (16u << 8) | 2u,
        BGRA4444 = (1u << 28) | (5u << 24) | (8u << 20) | (2u << 16) | (16u << 8) | 2u,
        ARGB1555 = (1u << 28) | (5u << 24) | (3u << 20) | (3u << 16) | (16u << 8) | 2u,
        RGBA5551 = (1u << 28) | (5u << 24) | (4u << 20) | (4u << 16) | (16u << 8) | 2u,
        ABGR1555 = (1u << 28) | (5u << 24) | (7u << 20) | (3u << 16) | (16u << 8) | 2u,
        BGRA5551 = (1u << 28) | (5u << 24) | (8u << 20) | (4u << 16) | (16u << 8) | 2u,
        RGB565 = (1u << 28) | (5u << 24) | (2u << 20) | (5u << 16) | (16u << 8) | 2u,
        BGR565 = (1u << 28) | (5u << 24) | (6u << 20) | (5u << 16) | (16u << 8) | 2u,

        RGB24 = (1u << 28) | (7u << 24) | (1u << 20) | (24u << 8) | 3u,
        BGR24 = (1u << 28) | (7u << 24) | (4u << 20) | (24u << 8) | 3u,

        RGB888 = (1u << 28) | (6u << 24) | (1u << 20) | (6u << 16) | (24u << 8) | 4u,
        RGBX8888 = (1u << 28) | (6u << 24) | (2u << 20) | (6u << 16) | (32u << 8) | 4u,
        BGR888 = (1u << 28) | (6u << 24) | (5u << 20) | (6u << 16) | (24u << 8) | 4u,
        BGRX8888 = (1u << 28) | (6u << 24) | (6u << 20) | (6u << 16) | (32u << 8) | 4u,
        ARGB8888 = (1u << 28) | (6u << 24) | (3u << 20) | (6u << 16) | (32u << 8) | 4u,
        RGBA8888 = (1u << 28) | (6u << 24) | (4u << 20) | (6u << 16) | (32u << 8) | 4u,
        ABGR8888 = (1u << 28) | (6u << 24) | (7u << 20) | (6u << 16) | (32u << 8) | 4u,
        BGRA8888 = (1u << 28) | (6u << 24) | (8u << 20) | (6u << 16) | (32u << 8) | 4u,
        ARGB2101010 = (1u << 28) | (6u << 24) | (3u << 20) | (7u << 16) | (32u << 8) | 4u,
    }

    [StructLayout(LayoutKind.Explicit, Size = 56)]
    public struct Event
    {
        [FieldOffset(0)]
        public EventType Type;

        [FieldOffset(4)]
        public Window.Event Window;
        [FieldOffset(4)]
        public Keyboard.Event Key;
        [FieldOffset(4)]
        public Mouse.MotionEvent Motion;
        [FieldOffset(4)]
        public Keyboard.TextEditingEvent Edit;
        [FieldOffset(4)]
        public Keyboard.TextInputEvent Text;
        [FieldOffset(4)]
        public Mouse.WheelEvent Wheel;
        [FieldOffset(4)]
        public Touch.FingerEvent Finger;
        [FieldOffset(4)]
        public Joystick.DeviceEvent JoystickDevice;
        [FieldOffset(4)]
        public GameController.DeviceEvent ControllerDevice;
        [FieldOffset(4)]
        public Drop.Event Drop;
    }

    [DebuggerDisplay("X: {X}, Y: {Y}, Width: {Width}, Height: {Height}")]
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public struct Version
    {
        public byte Major;
        public byte Minor;
        public byte Patch;

        public int PackedValue { get { return (Major << 16 | Minor << 8 | Patch); } }

        public Version(byte major, byte minor, byte patch) : this()
        {
            this.Major = major;
            this.Minor = minor;
            this.Patch = patch;
        }

        public static bool operator <(Version l, Version r)
        {
            return l.PackedValue < r.PackedValue;
        }

        public static bool operator >(Version l, Version r)
        {
            return l.PackedValue > r.PackedValue;
        }

        public static bool operator <=(Version l, Version r)
        {
            return l.PackedValue <= r.PackedValue;
        }

        public static bool operator >=(Version l, Version r)
        {
            return l.PackedValue >= r.PackedValue;
        }

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}",Major, Minor, Patch);
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_init(InitFlags flags);
    public d_sdl_init SDL_Init;

    public int SDLInitThreadId { get { return _SDLInitThreadId; } }
    public int GetManagedThreadId()
    {
#if NET6_0_OR_GREATER || NETSTANDARD2_0
            return Environment.CurrentManagedThreadId;
#else
        return System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
    }

    public void Init(InitFlags flags)
    {
        int res = SDL_Init(flags);
        GetError(res);

        _SDLInitThreadId = this.GetManagedThreadId();
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_InitSubSystem(InitFlags flags);
    public d_sdl_InitSubSystem SDL_InitSubSystem;

    public void InitSubSystem(InitFlags flags)
    {
        int res = SDL_InitSubSystem(flags);
        GetError(res);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_QuitSubSystem(InitFlags flags);
    public d_sdl_QuitSubSystem QuitSubSystem;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_WasInit(InitFlags flags);
    public d_sdl_init WasInit;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_quit();
    public d_sdl_quit Quit;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_disablescreensaver();
    public d_sdl_disablescreensaver DisableScreenSaver;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_getversion(out Version version);
    public d_sdl_getversion GetVersion;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_pollevent([Out] out Event _event);
    public d_sdl_pollevent PollEvent;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_pumpevents();
    public d_sdl_pumpevents PumpEvents;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_creatergbsurfacefrom(IntPtr pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask);
    private d_sdl_creatergbsurfacefrom SDL_CreateRGBSurfaceFrom;

    public IntPtr CreateRGBSurfaceFrom(byte[] pixels, int width, int height, int depth, int pitch, uint rMask, uint gMask, uint bMask, uint aMask)
    {
        var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
        try
        {
            return SDL_CreateRGBSurfaceFrom(handle.AddrOfPinnedObject(), width, height, depth, pitch, rMask, gMask, bMask, aMask);
        }
        finally
        {
            handle.Free();
        }
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_freesurface(IntPtr surface);
    public d_sdl_freesurface FreeSurface;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_geterror();
    private d_sdl_geterror SDL_GetError;

    public string GetError()
    {
        return InteropHelpers.Utf8ToString(SDL_GetError());
    }

    public void GetError(int res)
    {
        if (res < 0)
            Debug.WriteLine(GetError());
    }

    public void GetError(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
            Debug.WriteLine(GetError());
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_getpixelformatname(uint format);
    private d_sdl_getpixelformatname SDL_GetPixelFormatName;

    public string GetPixelFormatName(uint format)
    {
        return InteropHelpers.Utf8ToString(SDL_GetPixelFormatName(format));
    }

    public string GetPixelFormatName(PixelFormat format)
    {
        return GetPixelFormatName((uint)format);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int d_sdl_pixelformatenumtomasks(uint format, out int bpp, out uint rmask, out uint gmask, out uint bmask, out uint amask);
    private d_sdl_pixelformatenumtomasks SDL_PixelFormatEnumToMasks;

    public bool PixelFormatEnumToMasks(uint format, out int bpp, out uint rmask, out uint gmask, out uint bmask, out uint amask)
    {
        return SDL_PixelFormatEnumToMasks(format, out bpp, out rmask, out gmask, out bmask, out amask) != 0;
    }

    public bool PixelFormatEnumToMasks(PixelFormat format, out int bpp, out uint rmask, out uint gmask, out uint bmask, out uint amask)
    {
        return PixelFormatEnumToMasks((uint)format, out bpp, out rmask, out gmask, out bmask, out amask);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate uint d_sdl_maskstopixelformatenum(int bpp, uint rmask, uint gmask, uint bmask, uint amask);
    private d_sdl_maskstopixelformatenum SDL_MasksToPixelFormatEnum;

    public uint MasksToPixelFormatEnum(int bpp, uint rmask, uint gmask, uint bmask, uint amask)
    {
        return SDL_MasksToPixelFormatEnum(bpp, rmask, gmask, bmask, amask);
    }

    public PixelFormat MasksToPixelFormat(int bpp, uint rmask, uint gmask, uint bmask, uint amask)
    {
        return (PixelFormat)MasksToPixelFormatEnum(bpp, rmask, gmask, bmask, amask);
    }

    public static PixelType GetPixelType(uint format)
    {
        return (PixelType)((format >> 24) & 0x0F);
    }

    public static int GetBitsPerPixel(uint format)
    {
        return (int)((format >> 8) & 0xFF);
    }

    public static int GetBytesPerPixel(uint format)
    {
        return (int)(format & 0xFF);
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_clearerror();
    public d_sdl_clearerror ClearError;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate IntPtr d_sdl_gethint(string name);
    public d_sdl_gethint SDL_GetHint;

    public string GetHint(string name)
    {
        return InteropHelpers.Utf8ToString(SDL_GetHint(name));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_loadbmp_rw(IntPtr src, int freesrc);
    private d_sdl_loadbmp_rw SDL_LoadBMP_RW;

    public IntPtr LoadBMP_RW(IntPtr src, int freesrc)
    {
        IntPtr pointer = SDL_LoadBMP_RW(src, freesrc);
        GetError(pointer);
        return pointer;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_rwfrommem(byte[] mem, int size);
    private d_sdl_rwfrommem SDL_RWFromMem;

    public IntPtr RwFromMem(byte[] mem, int size)
    {
        IntPtr pointer = SDL_RWFromMem(mem, size);
        GetError(pointer);
        return pointer;
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int d_sdl_sethint(string name, string value);
    public d_sdl_sethint SetHint;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_free(IntPtr ptr);
    public d_sdl_free SDL_Free;

    private void LoadEntryPoints(IntPtr library)
    {
        SDL_Init = FuncLoader.LoadFunctionOrNull<d_sdl_init>(library, "SDL_Init");
        SDL_InitSubSystem = FuncLoader.LoadFunctionOrNull<d_sdl_InitSubSystem>(library, "SDL_InitSubSystem");
        QuitSubSystem = FuncLoader.LoadFunctionOrNull<d_sdl_QuitSubSystem>(library, "SDL_QuitSubSystem");
        WasInit = FuncLoader.LoadFunctionOrNull<d_sdl_init>(library, "SDL_WasInit");
        Quit = FuncLoader.LoadFunctionOrNull<d_sdl_quit>(library, "SDL_Quit");
        DisableScreenSaver = FuncLoader.LoadFunctionOrNull<d_sdl_disablescreensaver>(library, "SDL_DisableScreenSaver");
        GetVersion = FuncLoader.LoadFunctionOrNull<d_sdl_getversion>(library, "SDL_GetVersion");
        PollEvent = FuncLoader.LoadFunctionOrNull<d_sdl_pollevent>(library, "SDL_PollEvent");
        PumpEvents = FuncLoader.LoadFunctionOrNull<d_sdl_pumpevents>(library, "SDL_PumpEvents");
        SDL_CreateRGBSurfaceFrom = FuncLoader.LoadFunctionOrNull<d_sdl_creatergbsurfacefrom>(library, "SDL_CreateRGBSurfaceFrom");
        FreeSurface = FuncLoader.LoadFunctionOrNull<d_sdl_freesurface>(library, "SDL_FreeSurface");
        SDL_GetError = FuncLoader.LoadFunctionOrNull<d_sdl_geterror>(library, "SDL_GetError");
        ClearError = FuncLoader.LoadFunctionOrNull<d_sdl_clearerror>(library, "SDL_ClearError");
        SDL_GetHint = FuncLoader.LoadFunctionOrNull<d_sdl_gethint>(library, "SDL_GetHint");
        SDL_LoadBMP_RW = FuncLoader.LoadFunctionOrNull<d_sdl_loadbmp_rw>(library, "SDL_LoadBMP_RW");
        SDL_RWFromMem = FuncLoader.LoadFunctionOrNull<d_sdl_rwfrommem>(library, "SDL_RWFromMem");
        SetHint = FuncLoader.LoadFunctionOrNull<d_sdl_sethint>(library, "SDL_SetHint");
        SDL_Free = FuncLoader.LoadFunctionOrNull<d_sdl_free>(library, "SDL_free");
        SDL_GetPixelFormatName = FuncLoader.LoadFunctionOrNull<d_sdl_getpixelformatname>(library, "SDL_GetPixelFormatName");
        SDL_PixelFormatEnumToMasks = FuncLoader.LoadFunctionOrNull<d_sdl_pixelformatenumtomasks>(library, "SDL_PixelFormatEnumToMasks");
        SDL_MasksToPixelFormatEnum = FuncLoader.LoadFunctionOrNull<d_sdl_maskstopixelformatenum>(library, "SDL_MasksToPixelFormatEnum");
    }

}
