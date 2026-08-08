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
    }

}
