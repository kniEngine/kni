// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using MonoGame.Framework.Utilities;

internal class Sdl
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

    public readonly Version version;

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
        SysWM = 0x201,

        KeyDown = 0x300,
        KeyUp = 0x301,
        TextEditing = 0x302,
        TextInput = 0x303,

        MouseMotion = 0x400,
        MouseButtonDown = 0x401,
        MouseButtonup = 0x402,
        MouseWheel = 0x403,

        JoyAxisMotion = 0x600,
        JoyBallMotion = 0x601,
        JoyHatMotion = 0x602,
        JoyButtonDown = 0x603,
        JoyButtonUp = 0x604,
        JoyDeviceAdded = 0x605,
        JoyDeviceRemoved = 0x606,

        ControllerAxisMotion = 0x650,
        ControllerButtonDown = 0x651,
        ControllerButtonUp = 0x652,
        ControllerDeviceAdded = 0x653,
        ControllerDeviceRemoved = 0x654,
        ControllerDeviceRemapped = 0x654,

        FingerDown = 0x700,
        FingerUp = 0x701,
        FingerMotion = 0x702,

        DollarGesture = 0x800,
        DollarRecord = 0x801,
        MultiGesture = 0x802,

        ClipboardUpdate = 0x900,

        DropFile = 0x1000,
        DropText = 0x1001,
        DropBegin = 0x1002,
        DropComplete = 0x1003,

        AudioDeviceAdded = 0x1100,
        AudioDeviceRemoved = 0x1101,

        RenderTargetsReset = 0x2000,
        RenderDeviceReset = 0x2001,

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
        [FieldOffset(0)]
        public Window.Event Window;
        [FieldOffset(0)]
        public Keyboard.Event Key;
        [FieldOffset(0)]
        public Mouse.MotionEvent Motion;
        [FieldOffset(0)]
        public Keyboard.TextEditingEvent Edit;
        [FieldOffset(0)]
        public Keyboard.TextInputEvent Text;
        [FieldOffset(0)]
        public Mouse.WheelEvent Wheel;
        [FieldOffset(0)]
        public Joystick.DeviceEvent JoystickDevice;
        [FieldOffset(0)]
        public GameController.DeviceEvent ControllerDevice;
        [FieldOffset(0)]
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

    public void Init(InitFlags flags)
    {
        int res = SDL_Init(flags);
        GetError(res);
    }

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
    public delegate int d_sdl_pumpevents();
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

    public int GetError(int value)
    {
        if (value < 0)
            Debug.WriteLine(GetError());

        return value;
    }

    public IntPtr GetError(IntPtr pointer)
    {
        if (pointer == IntPtr.Zero)
            Debug.WriteLine(GetError());

        return pointer;
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
        return GetError(SDL_LoadBMP_RW(src, freesrc));
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void d_sdl_quit();
    public d_sdl_quit Quit;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate IntPtr d_sdl_rwfrommem(byte[] mem, int size);
    private d_sdl_rwfrommem SDL_RWFromMem;

    public IntPtr RwFromMem(byte[] mem, int size)
    {
        return GetError(SDL_RWFromMem(mem, size));
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
        Quit = FuncLoader.LoadFunctionOrNull<d_sdl_quit>(library, "SDL_Quit");
        SDL_RWFromMem = FuncLoader.LoadFunctionOrNull<d_sdl_rwfrommem>(library, "SDL_RWFromMem");
        SetHint = FuncLoader.LoadFunctionOrNull<d_sdl_sethint>(library, "SDL_SetHint");
        SDL_Free = FuncLoader.LoadFunctionOrNull<d_sdl_free>(library, "SDL_free");
    }


    public class Window
    {
        private Sdl _sdl;

        public const int PosUndefined = 0x1FFF0000;
        public const int PosCentered = 0x2FFF0000;

        public enum EventId : byte
        {
            None,
            Shown,
            Hidden,
            Exposed,
            Moved,
            Resized,
            SizeChanged,
            Minimized,
            Maximized,
            Restored,
            Enter,
            Leave,
            FocusGained,
            FocusLost,
            Close,
        }


        public Window(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        public enum State : int
        {
            Fullscreen      = 0x00000001,
            OpenGL          = 0x00000002,
            Shown           = 0x00000004,
            Hidden          = 0x00000008,
            Borderless      = 0x00000010,
            Resizable       = 0x00000020,
            Minimized       = 0x00000040,
            Maximized       = 0x00000080,
            Grabbed         = 0x00000100,
            InputFocus      = 0x00000200,
            MouseFocus      = 0x00000400,
            Foreign         = 0x00000800,
            FullscreenDesktop = 0x00001001,
            AllowHighDPI    = 0x00002000,
            MouseCapture    = 0x00004000,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowID;
            public EventId EventID;
            private byte padding1;
            private byte padding2;
            private byte padding3;
            public int Data1;
            public int Data2;
        }

        public enum SysWMType
        {
            Unknow,
            Windows,
            X11,
            Directfb,
            Cocoa,
            UiKit,
            Wayland,
            Mir,
            WinRt,
            Android
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_SysWMinfo
        {
            public Version version;
            public SysWMType subsystem;
            public IntPtr window;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createwindow(string title, int x, int y, int w, int h, int flags);
        private d_sdl_createwindow SDL_CreateWindow;

        public IntPtr Create(string title, int x, int y, int w, int h, Sdl.Window.State flags)
        {
            return _sdl.GetError(SDL_CreateWindow(title, x, y, w, h, (int)flags));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_destroywindow(IntPtr window);
        public d_sdl_destroywindow Destroy;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate uint d_sdl_getwindowid(IntPtr window);
        public d_sdl_getwindowid GetWindowId;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getwindowdisplayindex(IntPtr window);
        private d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex;

        public int GetDisplayIndex(IntPtr window)
        {
            return _sdl.GetError(SDL_GetWindowDisplayIndex(window));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getwindowflags(IntPtr window);
        public d_sdl_getwindowflags GetWindowFlags;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowicon(IntPtr window, IntPtr icon);
        public d_sdl_setwindowicon SetIcon;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_getwindowposition(IntPtr window, out int x, out int y);
        public d_sdl_getwindowposition GetPosition;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_getwindowsize(IntPtr window, out int w, out int h);
        public d_sdl_getwindowsize GetSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowbordered(IntPtr window, int bordered);
        public d_sdl_setwindowbordered SetBordered;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_setwindowfullscreen(IntPtr window, int flags);
        private d_sdl_setwindowfullscreen SDL_SetWindowFullscreen;

        public void SetFullscreen(IntPtr window, Sdl.Window.State flags)
        {
            _sdl.GetError(SDL_SetWindowFullscreen(window, (int)flags));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowposition(IntPtr window, int x, int y);
        public d_sdl_setwindowposition SetPosition;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowresizable(IntPtr window, bool resizable);
        public d_sdl_setwindowresizable SetResizable;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setwindowsize(IntPtr window, int w, int h);
        public d_sdl_setwindowsize SetSize;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void d_sdl_setwindowtitle(IntPtr window, byte* value);
        private d_sdl_setwindowtitle SDL_SetWindowTitle;

        public unsafe void SetTitle(IntPtr handle, string title)
        {
            byte[] str = Encoding.UTF8.GetBytes(title+'\0');

            fixed (byte* pStr = str)
            {
                SDL_SetWindowTitle(handle, pStr);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_showwindow(IntPtr window);
        public d_sdl_showwindow Show;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool d_sdl_getwindowwminfo(IntPtr window, ref SDL_SysWMinfo sysWMinfo);
        public d_sdl_getwindowwminfo GetWindowWMInfo;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_getwindowborderssize(IntPtr window, out int top, out int left, out int right, out int bottom);
        public d_sdl_getwindowborderssize GetBorderSize;


        private void LoadEntryPoints(IntPtr library)
        {
            SDL_CreateWindow = FuncLoader.LoadFunctionOrNull<d_sdl_createwindow>(library, "SDL_CreateWindow");
            Destroy = FuncLoader.LoadFunctionOrNull<d_sdl_destroywindow>(library, "SDL_DestroyWindow");
            GetWindowId = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowid>(library, "SDL_GetWindowID");
            SDL_GetWindowDisplayIndex = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowdisplayindex>(library, "SDL_GetWindowDisplayIndex");
            GetWindowFlags = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowflags>(library, "SDL_GetWindowFlags");
            SetIcon = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowicon>(library, "SDL_SetWindowIcon");
            GetPosition = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowposition>(library, "SDL_GetWindowPosition");
            GetSize = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowsize>(library, "SDL_GetWindowSize");
            SetBordered = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowbordered>(library, "SDL_SetWindowBordered");
            SDL_SetWindowFullscreen = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowfullscreen>(library, "SDL_SetWindowFullscreen");
            SetPosition = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowposition>(library, "SDL_SetWindowPosition");
            SetResizable = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowresizable>(library, "SDL_SetWindowResizable");
            SetSize = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowsize>(library, "SDL_SetWindowSize");
            SDL_SetWindowTitle = FuncLoader.LoadFunctionOrNull<d_sdl_setwindowtitle>(library, "SDL_SetWindowTitle");
            Show = FuncLoader.LoadFunctionOrNull<d_sdl_showwindow>(library, "SDL_ShowWindow");
            GetWindowWMInfo = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowwminfo>(library, "SDL_GetWindowWMInfo");
            GetBorderSize = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowborderssize>(library, "SDL_GetWindowBordersSize");
        }
    }

    public class Display
    {
        private Sdl _sdl;

        public struct Mode
        {
            public uint Format;
            public int Width;
            public int Height;
            public int RefreshRate;
            public IntPtr DriverData;
        }

        public Display(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getdisplaybounds(int displayIndex, out Rectangle rect);
        private d_sdl_getdisplaybounds SDL_GetDisplayBounds;

        public void GetBounds(int displayIndex, out Rectangle rect)
        {
            _sdl.GetError(SDL_GetDisplayBounds(displayIndex, out rect));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getcurrentdisplaymode(int displayIndex, out Mode mode);
        private d_sdl_getcurrentdisplaymode SDL_GetCurrentDisplayMode;

        public void GetCurrentDisplayMode(int displayIndex, out Mode mode)
        {
            _sdl.GetError(SDL_GetCurrentDisplayMode(displayIndex, out mode));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getdisplaymode(int displayIndex, int modeIndex, out Mode mode);
        private d_sdl_getdisplaymode SDL_GetDisplayMode;

        public void GetDisplayMode(int displayIndex, int modeIndex, out Mode mode)
        {
            _sdl.GetError(SDL_GetDisplayMode(displayIndex, modeIndex, out mode));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getclosestdisplaymode(int displayIndex, Mode mode, out Mode closest);
        private d_sdl_getclosestdisplaymode SDL_GetClosestDisplayMode;

        public void GetClosestDisplayMode(int displayIndex, Mode mode, out Mode closest)
        {
            _sdl.GetError(SDL_GetClosestDisplayMode(displayIndex, mode, out closest));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_getdisplayname(int index);
        private d_sdl_getdisplayname SDL_GetDisplayName;

        public string GetDisplayName(int index)
        {
            return InteropHelpers.Utf8ToString(_sdl.GetError(SDL_GetDisplayName(index)));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getnumdisplaymodes(int displayIndex);
        private d_sdl_getnumdisplaymodes SDL_GetNumDisplayModes;

        public int GetNumDisplayModes(int displayIndex)
        {
            return _sdl.GetError(SDL_GetNumDisplayModes(displayIndex));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getnumvideodisplays();
        private d_sdl_getnumvideodisplays SDL_GetNumVideoDisplays;

        public int GetNumVideoDisplays()
        {
            return _sdl.GetError(SDL_GetNumVideoDisplays());
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_getwindowdisplayindex(IntPtr window);
        private d_sdl_getwindowdisplayindex SDL_GetWindowDisplayIndex;

        public int GetWindowDisplayIndex(IntPtr window)
        {
            return _sdl.GetError(SDL_GetWindowDisplayIndex(window));
        }


        private void LoadEntryPoints(IntPtr library)
        {
            SDL_GetDisplayBounds = FuncLoader.LoadFunctionOrNull<d_sdl_getdisplaybounds>(library, "SDL_GetDisplayBounds");
            SDL_GetCurrentDisplayMode = FuncLoader.LoadFunctionOrNull<d_sdl_getcurrentdisplaymode>(library, "SDL_GetCurrentDisplayMode");
            SDL_GetDisplayMode = FuncLoader.LoadFunctionOrNull<d_sdl_getdisplaymode>(library, "SDL_GetDisplayMode");
            SDL_GetClosestDisplayMode = FuncLoader.LoadFunctionOrNull<d_sdl_getclosestdisplaymode>(library, "SDL_GetClosestDisplayMode");
            SDL_GetDisplayName = FuncLoader.LoadFunctionOrNull<d_sdl_getdisplayname>(library, "SDL_GetDisplayName");
            SDL_GetNumDisplayModes = FuncLoader.LoadFunctionOrNull<d_sdl_getnumdisplaymodes>(library, "SDL_GetNumDisplayModes");
            SDL_GetNumVideoDisplays = FuncLoader.LoadFunctionOrNull<d_sdl_getnumvideodisplays>(library, "SDL_GetNumVideoDisplays");
            SDL_GetWindowDisplayIndex = FuncLoader.LoadFunctionOrNull<d_sdl_getwindowdisplayindex>(library, "SDL_GetWindowDisplayIndex");
        }
    }

    public class GL
    {
        private Sdl _sdl;

        public enum Attribute
        {
            RedSize,
            GreenSize,
            BlueSize,
            AlphaSize,
            BufferSize,
            DoubleBuffer,
            DepthSize,
            StencilSize,
            AccumRedSize,
            AccumGreenSize,
            AccumBlueSize,
            AccumAlphaSize,
            Stereo,
            MultiSampleBuffers,
            MultiSampleSamples,
            AcceleratedVisual,
            RetainedBacking,
            ContextMajorVersion,
            ContextMinorVersion,
            ContextEgl,
            ContextFlags,
            ContextProfileMask,
            ShareWithCurrentContext,
            FramebufferSRGBCapable,
            ContextReleaseBehaviour,
        }


        public GL(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gl_createcontext(IntPtr window);
        private d_sdl_gl_createcontext SDL_GL_CreateContext;

        public IntPtr CreateGLContext(IntPtr window)
        {
            return _sdl.GetError(SDL_GL_CreateContext(window));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gl_deletecontext(IntPtr context);
        public d_sdl_gl_deletecontext DeleteContext;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gl_getcurrentcontext();
        private d_sdl_gl_getcurrentcontext SDL_GL_GetCurrentContext;

        public IntPtr GetCurrentContext()
        {
            return _sdl.GetError(SDL_GL_GetCurrentContext());
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_gl_getprocaddress(string proc);
        public d_sdl_gl_getprocaddress GetProcAddress;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_getswapinterval();
        public d_sdl_gl_getswapinterval GetSwapInterval;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_makecurrent(IntPtr window, IntPtr context);
        public d_sdl_gl_makecurrent MakeCurrent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_gl_setattribute(Attribute attr, int value);
        private d_sdl_gl_setattribute SDL_GL_SetAttribute;

        public int SetAttribute(Attribute attr, int value)
        {
            return _sdl.GetError(SDL_GL_SetAttribute(attr, value));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gl_setswapinterval(int interval);
        public d_sdl_gl_setswapinterval SetSwapInterval;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gl_swapwindow(IntPtr window);
        public d_sdl_gl_swapwindow SwapWindow;

        private void LoadEntryPoints(IntPtr library)
        {
            SDL_GL_CreateContext = FuncLoader.LoadFunctionOrNull<d_sdl_gl_createcontext>(library, "SDL_GL_CreateContext");
            DeleteContext = FuncLoader.LoadFunctionOrNull<d_sdl_gl_deletecontext>(library, "SDL_GL_DeleteContext");
            SDL_GL_GetCurrentContext = FuncLoader.LoadFunctionOrNull<d_sdl_gl_getcurrentcontext>(library, "SDL_GL_GetCurrentContext");
            GetProcAddress = FuncLoader.LoadFunctionOrNull<d_sdl_gl_getprocaddress>(library, "SDL_GL_GetProcAddress");
            GetSwapInterval = FuncLoader.LoadFunctionOrNull<d_sdl_gl_getswapinterval>(library, "SDL_GL_GetSwapInterval");
            MakeCurrent = FuncLoader.LoadFunctionOrNull<d_sdl_gl_makecurrent>(library, "SDL_GL_MakeCurrent");
            SDL_GL_SetAttribute = FuncLoader.LoadFunctionOrNull<d_sdl_gl_setattribute>(library, "SDL_GL_SetAttribute");
            SetSwapInterval = FuncLoader.LoadFunctionOrNull<d_sdl_gl_setswapinterval>(library, "SDL_GL_SetSwapInterval");
            SwapWindow = FuncLoader.LoadFunctionOrNull<d_sdl_gl_swapwindow>(library, "SDL_GL_SwapWindow");
        }
    }

    public class Mouse
    {
        private Sdl _sdl;

        [Flags]
        public enum Button
        {
            Left = 1 << 0,
            Middle = 1 << 1,
            Right = 1 << 2,
            X1Mask = 1 << 3,
            X2Mask = 1 << 4
        }

        public enum SystemCursor
        {
            Arrow,
            IBeam,
            Wait,
            Crosshair,
            WaitArrow,
            SizeNWSE,
            SizeNESW,
            SizeWE,
            SizeNS,
            SizeAll,
            No,
            Hand
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MotionEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowID;
            public uint Which;
            public byte State;
            private byte _padding1;
            private byte _padding2;
            private byte _padding3;
            public int X;
            public int Y;
            public int Xrel;
            public int Yrel;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WheelEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowId;
            public uint Which;
            public int X;
            public int Y;
            public uint Direction;
        }

        public Mouse(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createcolorcursor(IntPtr surface, int x, int y);
        private d_sdl_createcolorcursor SDL_CreateColorCursor;

        public IntPtr CreateColorCursor(IntPtr surface, int x, int y)
        {
            return _sdl.GetError(SDL_CreateColorCursor(surface, x, y));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_createsystemcursor(SystemCursor id);
        private d_sdl_createsystemcursor SDL_CreateSystemCursor;

        public IntPtr CreateSystemCursor(SystemCursor id)
        {
            return _sdl.GetError(SDL_CreateSystemCursor(id));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_freecursor(IntPtr cursor);
        public d_sdl_freecursor FreeCursor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getglobalmousestate(out int x, out int y);
        public d_sdl_getglobalmousestate GetGlobalState;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Button d_sdl_getmousestate(out int x, out int y);
        public d_sdl_getmousestate GetState;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_setcursor(IntPtr cursor);
        public d_sdl_setcursor SetCursor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_showcursor(int toggle);
        public d_sdl_showcursor ShowCursor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_warpmouseinwindow(IntPtr window, int x, int y);
        public d_sdl_warpmouseinwindow WarpInWindow;

        private void LoadEntryPoints(IntPtr library)
        {
            SDL_CreateColorCursor = FuncLoader.LoadFunctionOrNull<d_sdl_createcolorcursor>(library, "SDL_CreateColorCursor");
            SDL_CreateSystemCursor = FuncLoader.LoadFunctionOrNull<d_sdl_createsystemcursor>(library, "SDL_CreateSystemCursor");
            FreeCursor = FuncLoader.LoadFunctionOrNull<d_sdl_freecursor>(library, "SDL_FreeCursor");
            GetGlobalState = FuncLoader.LoadFunctionOrNull<d_sdl_getglobalmousestate>(library, "SDL_GetGlobalMouseState");
            GetState = FuncLoader.LoadFunctionOrNull<d_sdl_getmousestate>(library, "SDL_GetMouseState");
            SetCursor = FuncLoader.LoadFunctionOrNull<d_sdl_setcursor>(library, "SDL_SetCursor");
            ShowCursor = FuncLoader.LoadFunctionOrNull<d_sdl_showcursor>(library, "SDL_ShowCursor");
            WarpInWindow = FuncLoader.LoadFunctionOrNull<d_sdl_warpmouseinwindow>(library, "SDL_WarpMouseInWindow");
        }
    }

    public class Keyboard
    {
        private Sdl _sdl;

        public struct Keysym
        {
            public int Scancode;
            public int Sym;
            public Keymod Mod;
            public uint Unicode;
        }

        [Flags]
        public enum Keymod : ushort
        {
            None = 0x0000,
            LeftShift = 0x0001,
            RightShift = 0x0002,
            LeftCtrl = 0x0040,
            RightCtrl = 0x0080,
            LeftAlt = 0x0100,
            RightAlt = 0x0200,
            LeftGui = 0x0400,
            RightGui = 0x0800,
            NumLock = 0x1000,
            CapsLock = 0x2000,
            AltGr = 0x4000,
            Reserved = 0x8000,
            Ctrl = (LeftCtrl | RightCtrl),
            Shift = (LeftShift | RightShift),
            Alt = (LeftAlt | RightAlt),
            Gui = (LeftGui | RightGui)
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public uint WindowId;
            public byte State;
            public byte Repeat;
            private byte padding2;
            private byte padding3;
            public Keysym Keysym;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct TextEditingEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowId;
            public fixed byte Text[32];
            public int Start;
            public int Length;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct TextInputEvent
        {
            public EventType Type;
            public uint Timestamp;
            public uint WindowId;
            public fixed byte Text[32];
        }


        public Keyboard(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Keymod d_sdl_getmodstate();
        public d_sdl_getmodstate GetModState;

        private void LoadEntryPoints(IntPtr library)
        {
            GetModState = FuncLoader.LoadFunctionOrNull<d_sdl_getmodstate>(library, "SDL_GetModState");
        }
    }

    public class Joystick
    {
        private Sdl _sdl;

        [Flags]
        public enum Hat : byte
        {
            Centered = 0,

            Up    = 1 << 0,
            Right = 1 << 1,
            Down  = 1 << 2,
            Left  = 1 << 3
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public int Which;
        }

        public Joystick(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_joystickclose(IntPtr joystick);
        public d_sdl_joystickclose Close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);
        private d_sdl_joystickfrominstanceid SDL_JoystickFromInstanceID;

        public IntPtr FromInstanceID(int joyid)
        {
            return _sdl.GetError(SDL_JoystickFromInstanceID(joyid));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short d_sdl_joystickgetaxis(IntPtr joystick, int axis);
        public d_sdl_joystickgetaxis GetAxis;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_joystickgetbutton(IntPtr joystick, int button);
        public d_sdl_joystickgetbutton GetButton;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickname(IntPtr joystick);
        private d_sdl_joystickname JoystickName;

        public string GetJoystickName(IntPtr joystick)
        {
            return InteropHelpers.Utf8ToString(JoystickName(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Guid d_sdl_joystickgetguid(IntPtr joystick);
        public d_sdl_joystickgetguid GetGUID;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate Hat d_sdl_joystickgethat(IntPtr joystick, int hat);
        public d_sdl_joystickgethat GetHat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_joystickinstanceid(IntPtr joystick);
        public d_sdl_joystickinstanceid InstanceID;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickopen(int deviceIndex);
        private d_sdl_joystickopen SDL_JoystickOpen;

        public IntPtr Open(int deviceIndex)
        {
            return _sdl.GetError(SDL_JoystickOpen(deviceIndex));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumaxes(IntPtr joystick);
        private d_sdl_joysticknumaxes SDL_JoystickNumAxes;

        public int NumAxes(IntPtr joystick)
        {
            return _sdl.GetError(SDL_JoystickNumAxes(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumbuttons(IntPtr joystick);
        private d_sdl_joysticknumbuttons SDL_JoystickNumButtons;

        public int NumButtons(IntPtr joystick)
        {
            return _sdl.GetError(SDL_JoystickNumButtons(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_joysticknumhats(IntPtr joystick);
        private d_sdl_joysticknumhats SDL_JoystickNumHats;

        public int NumHats(IntPtr joystick)
        {
            return _sdl.GetError(SDL_JoystickNumHats(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_numjoysticks();
        private d_sdl_numjoysticks SDL_NumJoysticks;

        public int NumJoysticks()
        {
            return _sdl.GetError(SDL_NumJoysticks());
        }

        private void LoadEntryPoints(IntPtr library)
        {
            Close = FuncLoader.LoadFunctionOrNull<d_sdl_joystickclose>(library, "SDL_JoystickClose");
            SDL_JoystickFromInstanceID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickfrominstanceid>(library, "SDL_JoystickFromInstanceID");
            GetAxis = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgetaxis>(library, "SDL_JoystickGetAxis");
            GetButton = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgetbutton>(library, "SDL_JoystickGetButton");
            JoystickName = FuncLoader.LoadFunctionOrNull<d_sdl_joystickname>(library, "SDL_JoystickName");
            GetGUID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgetguid>(library, "SDL_JoystickGetGUID");
            GetHat = FuncLoader.LoadFunctionOrNull<d_sdl_joystickgethat>(library, "SDL_JoystickGetHat");
            InstanceID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickinstanceid>(library, "SDL_JoystickInstanceID");
            SDL_JoystickOpen = FuncLoader.LoadFunctionOrNull<d_sdl_joystickopen>(library, "SDL_JoystickOpen");
            SDL_JoystickNumAxes = FuncLoader.LoadFunctionOrNull<d_sdl_joysticknumaxes>(library, "SDL_JoystickNumAxes");
            SDL_JoystickNumButtons = FuncLoader.LoadFunctionOrNull<d_sdl_joysticknumbuttons>(library, "SDL_JoystickNumButtons");
            SDL_JoystickNumHats = FuncLoader.LoadFunctionOrNull<d_sdl_joysticknumhats>(library, "SDL_JoystickNumHats");
            SDL_NumJoysticks = FuncLoader.LoadFunctionOrNull<d_sdl_numjoysticks>(library, "SDL_NumJoysticks");
        }
    }

    public class GameController
    {
        private Sdl _sdl;

        public enum Axis
        {
            Invalid = -1,
            LeftX,
            LeftY,
            RightX,
            RightY,
            TriggerLeft,
            TriggerRight,
            Max,
        }

        public enum Button
        {
            Invalid = -1,
            A,
            B,
            X,
            Y,
            Back,
            Guide,
            Start,
            LeftStick,
            RightStick,
            LeftShoulder,
            RightShoulder,
            DpadUp,
            DpadDown,
            DpadLeft,
            DpadRight,
            Max,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DeviceEvent
        {
            public EventType Type;
            public uint TimeStamp;
            public int Which;
        }

        public GameController(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrolleraddmapping(string mappingString);
        public d_sdl_gamecontrolleraddmapping AddMapping;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrolleraddmappingsfromrw(IntPtr rw, int freew);
        public d_sdl_gamecontrolleraddmappingsfromrw AddMappingFromRw;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_gamecontrollerclose(IntPtr gamecontroller);
        public d_sdl_gamecontrollerclose Close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_joystickfrominstanceid(int joyid);
        private d_sdl_joystickfrominstanceid SDL_GameControllerFromInstanceID;

        public IntPtr FromInstanceID(int joyid)
        {
            return _sdl.GetError(SDL_GameControllerFromInstanceID(joyid));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate short d_sdl_gamecontrollergetaxis(IntPtr gamecontroller, Axis axis);
        public d_sdl_gamecontrollergetaxis GetAxis;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_gamecontrollergetbutton(IntPtr gamecontroller, Button button);
        public d_sdl_gamecontrollergetbutton GetButton;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollergetjoystick(IntPtr gamecontroller);
        private d_sdl_gamecontrollergetjoystick SDL_GameControllerGetJoystick;

        public IntPtr GetJoystick(IntPtr gamecontroller)
        {
            return _sdl.GetError(SDL_GameControllerGetJoystick(gamecontroller));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_isgamecontroller(int joystickIndex);
        public d_sdl_isgamecontroller IsGameController;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_gamecontrollermapping(IntPtr gamecontroller);
        public d_sdl_gamecontrollermapping SDL_GameControllerMapping;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrolleropen(int joystickIndex);
        private d_sdl_gamecontrolleropen SDL_GameControllerOpen;

        public IntPtr Open(int joystickIndex)
        {
            return _sdl.GetError(SDL_GameControllerOpen(joystickIndex));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_gamecontrollername(IntPtr gamecontroller);
        private d_sdl_gamecontrollername SDL_GameControllerName;

        public string GetName(IntPtr gamecontroller)
        {
            return InteropHelpers.Utf8ToString(SDL_GameControllerName(gamecontroller));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_gamecontrollerrumble(IntPtr gamecontroller, ushort left, ushort right, uint duration);
        public d_sdl_gamecontrollerrumble Rumble;
        public d_sdl_gamecontrollerrumble RumbleTriggers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate byte d_sdl_gamecontrollerhasrumble(IntPtr gamecontroller);
        public d_sdl_gamecontrollerhasrumble HasRumble;
        public d_sdl_gamecontrollerhasrumble HasRumbleTriggers;

        private void LoadEntryPoints(IntPtr library)
        {            
            AddMapping = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrolleraddmapping>(library, "SDL_GameControllerAddMapping");
            AddMappingFromRw = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrolleraddmappingsfromrw>(library, "SDL_GameControllerAddMappingsFromRW");
            Close = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerclose>(library, "SDL_GameControllerClose");
            SDL_GameControllerFromInstanceID = FuncLoader.LoadFunctionOrNull<d_sdl_joystickfrominstanceid>(library, "SDL_JoystickFromInstanceID");
            GetAxis = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollergetaxis>(library, "SDL_GameControllerGetAxis");
            GetButton = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollergetbutton>(library, "SDL_GameControllerGetButton");
            SDL_GameControllerGetJoystick = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollergetjoystick>(library, "SDL_GameControllerGetJoystick");
            IsGameController = FuncLoader.LoadFunctionOrNull<d_sdl_isgamecontroller>(library, "SDL_IsGameController");
            SDL_GameControllerMapping = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollermapping>(library, "SDL_GameControllerMapping");
            SDL_GameControllerOpen = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrolleropen>(library, "SDL_GameControllerOpen");
            SDL_GameControllerName = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollername>(library, "SDL_GameControllerName");
            Rumble = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerrumble>(library, "SDL_GameControllerRumble");
            RumbleTriggers = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerrumble>(library, "SDL_GameControllerRumbleTriggers");
            HasRumble = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerhasrumble>(library, "SDL_GameControllerHasRumble");
            HasRumbleTriggers = FuncLoader.LoadFunctionOrNull<d_sdl_gamecontrollerhasrumble>(library, "SDL_GameControllerHasRumbleTriggers");
        }
    }

    public class Haptic
    {
        private Sdl _sdl;

        // For some reason, different game controllers support different maximum values
        // Also, the closer a given value is to the maximum, the more likely the value will be ignored
        // Hence, we're setting an arbitrary safe value as a maximum
        public const uint Infinity = 1000000U;

        public enum EffectId : ushort
        {
            LeftRight = (1 << 2),
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct LeftRight
        {
            public EffectId Type;
            public uint Length;
            public ushort LargeMagnitude;
            public ushort SmallMagnitude;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct Effect
        {
            [FieldOffset(0)] public EffectId type;
            [FieldOffset(0)] public LeftRight leftright;
        }

        public Haptic(Sdl sdl, IntPtr library)
        {
            _sdl = sdl;
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void d_sdl_hapticclose(IntPtr haptic);
        public d_sdl_hapticclose Close;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_hapticeffectsupported(IntPtr haptic, ref Effect effect);
        public d_sdl_hapticeffectsupported EffectSupported;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int d_sdl_joystickishaptic(IntPtr joystick);
        public d_sdl_joystickishaptic IsHaptic;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticneweffect(IntPtr haptic, ref Effect effect);
        private d_sdl_hapticneweffect SDL_HapticNewEffect;

        public void NewEffect(IntPtr haptic, ref Effect effect)
        {
            _sdl.GetError(SDL_HapticNewEffect(haptic, ref effect));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr d_sdl_hapticopen(int device_index);
        public d_sdl_hapticopen Open;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_sdl_hapticopenfromjoystick(IntPtr joystick);
        private d_sdl_hapticopenfromjoystick SDL_HapticOpenFromJoystick;

        public IntPtr OpenFromJoystick(IntPtr joystick)
        {
            return _sdl.GetError(SDL_HapticOpenFromJoystick(joystick));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumbleinit(IntPtr haptic);
        private d_sdl_hapticrumbleinit SDL_HapticRumbleInit;

        public void RumbleInit(IntPtr haptic)
        {
            _sdl.GetError(SDL_HapticRumbleInit(haptic));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumbleplay(IntPtr haptic, float strength, uint length);
        private d_sdl_hapticrumbleplay SDL_HapticRumblePlay;

        public void RumblePlay(IntPtr haptic, float strength, uint length)
        {
            _sdl.GetError(SDL_HapticRumblePlay(haptic, strength, length));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticrumblesupported(IntPtr haptic);
        private d_sdl_hapticrumblesupported SDL_HapticRumbleSupported;

        public int RumbleSupported(IntPtr haptic)
        {
            return _sdl.GetError(SDL_HapticRumbleSupported(haptic));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticruneffect(IntPtr haptic, int effect, uint iterations);
        private d_sdl_hapticruneffect SDL_HapticRunEffect;

        public void RunEffect(IntPtr haptic, int effect, uint iterations)
        {
            _sdl.GetError(SDL_HapticRunEffect(haptic, effect, iterations));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticstopall(IntPtr haptic);
        private d_sdl_hapticstopall SDL_HapticStopAll;

        public void StopAll(IntPtr haptic)
        {
            _sdl.GetError(SDL_HapticStopAll(haptic));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int d_sdl_hapticupdateeffect(IntPtr haptic, int effect, ref Effect data);
        private d_sdl_hapticupdateeffect SDL_HapticUpdateEffect;

        public void UpdateEffect(IntPtr haptic, int effect, ref Effect data)
        {
            _sdl.GetError(SDL_HapticUpdateEffect(haptic, effect, ref data));
        }

        private void LoadEntryPoints(IntPtr library)
        {
            Close = FuncLoader.LoadFunctionOrNull<d_sdl_hapticclose>(library, "SDL_HapticClose");
            EffectSupported = FuncLoader.LoadFunctionOrNull<d_sdl_hapticeffectsupported>(library, "SDL_HapticEffectSupported");
            IsHaptic = FuncLoader.LoadFunctionOrNull<d_sdl_joystickishaptic>(library, "SDL_JoystickIsHaptic");
            SDL_HapticNewEffect = FuncLoader.LoadFunctionOrNull<d_sdl_hapticneweffect>(library, "SDL_HapticNewEffect");
            Open = FuncLoader.LoadFunctionOrNull<d_sdl_hapticopen>(library, "SDL_HapticOpen");
            SDL_HapticOpenFromJoystick = FuncLoader.LoadFunctionOrNull<d_sdl_hapticopenfromjoystick>(library, "SDL_HapticOpenFromJoystick");
            SDL_HapticRumbleInit = FuncLoader.LoadFunctionOrNull<d_sdl_hapticrumbleinit>(library, "SDL_HapticRumbleInit");
            SDL_HapticRumblePlay = FuncLoader.LoadFunctionOrNull<d_sdl_hapticrumbleplay>(library, "SDL_HapticRumblePlay");
            SDL_HapticRumbleSupported = FuncLoader.LoadFunctionOrNull<d_sdl_hapticrumblesupported>(library, "SDL_HapticRumbleSupported");
            SDL_HapticRunEffect = FuncLoader.LoadFunctionOrNull<d_sdl_hapticruneffect>(library, "SDL_HapticRunEffect");
            SDL_HapticStopAll = FuncLoader.LoadFunctionOrNull<d_sdl_hapticstopall>(library, "SDL_HapticStopAll");
            SDL_HapticUpdateEffect = FuncLoader.LoadFunctionOrNull<d_sdl_hapticupdateeffect>(library, "SDL_HapticUpdateEffect");
        }
    }

    public class Drop
    {
        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct Event
        {
            public EventType Type;
            public uint TimeStamp;
            public IntPtr File;
            public uint WindowId;
        }
    }
}
