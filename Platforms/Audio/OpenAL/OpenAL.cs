// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos
using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Utilities;

#if ANDROID
using System.IO;
#endif


namespace Microsoft.Xna.Platform.Audio.OpenAL
{
    internal enum ALFormat
    {
        Mono8 = 0x1100,
        Mono16 = 0x1101,
        Stereo8 = 0x1102,
        Stereo16 = 0x1103,
        MonoIma4 = 0x1300,
        StereoIma4 = 0x1301,
        MonoMSAdpcm = 0x1302,
        StereoMSAdpcm = 0x1303,
        MonoFloat32 = 0x10010,
        StereoFloat32 = 0x10011,
    }

    internal enum ALError
    {
        NoError = 0,
        InvalidName = 0xA001,
        InvalidEnum = 0xA002,
        InvalidValue = 0xA003,
        InvalidOperation = 0xA004,
        OutOfMemory = 0xA005,
    }

    internal enum ALGetString
    {
        Extensions = 0xB004,
    }

    internal enum ALBufferi
    {
        UnpackBlockAlignmentSoft = 0x200C,
        LoopSoftPointsExt = 0x2015,
    }

    internal enum ALGetBufferi
    {
        Bits = 0x2002,
        Channels = 0x2003,
        Size = 0x2004,
    }

    internal enum ALSourceb
    {
        Looping = 0x1007,
    }

    internal enum ALSourcei
    {
        SourceRelative = 0x202,
        Buffer = 0x1009,
        EfxDirectFilter = 0x20005,
        EfxAuxilarySendFilter = 0x20006,
    }

    internal enum ALSourcef
    {
        Pitch = 0x1003,
        Gain = 0x100A,
        ReferenceDistance = 0x1020,
    }

    internal enum ALGetSourcei
    {
        SampleOffset = 0x1025,
        SourceState = 0x1010,
        BuffersQueued = 0x1015,
        BuffersProcessed = 0x1016,
    }

    internal enum ALSourceState
    {
        Initial = 0x1011,
        Playing = 0x1012,
        Paused = 0x1013,
        Stopped = 0x1014,
    }

    internal enum ALListener3f
    {
        Position = 0x1004,
    }

    internal enum ALSource3f
    {
        Position = 0x1004,
        Velocity = 0x1006,
    }

    internal enum ALDistanceModel
    {
        None = 0,
        InverseDistanceClamped = 0xD002,
    }

    internal enum AlcError
    {
        NoError = 0,
    }

    internal enum AlcGetString
    {
        CaptureDeviceSpecifier = 0x0310,
        CaptureDefaultDeviceSpecifier = 0x0311,
        Extensions = 0x1006,
    }

    internal enum AlcGetInteger
    {
        CaptureSamples = 0x0312,
    }

    internal enum EfxFilteri
    {
        FilterType = 0x8001,
    }

    internal enum EfxFilterf
    {
        LowpassGain = 0x0001,
        LowpassGainHF = 0x0002,
        HighpassGain = 0x0001,
        HighpassGainLF = 0x0002,
        BandpassGain = 0x0001,
        BandpassGainLF = 0x0002,
        BandpassGainHF = 0x0003,
    }

    internal enum EfxFilterType
    {
        None = 0x0000,
        Lowpass = 0x0001,
        Highpass = 0x0002,
        Bandpass = 0x0003,
    }

    internal enum EfxEffecti
    {
        EffectType = 0x8001,
        SlotEffect = 0x0001,
    }

    internal enum EfxEffectSlotf
    {
        EffectSlotGain = 0x0002,
    }

    internal enum EfxEffectf
    {
        EaxReverbDensity = 0x0001,
        EaxReverbDiffusion = 0x0002,
        EaxReverbGain = 0x0003,
        EaxReverbGainHF = 0x0004,
        EaxReverbGainLF = 0x0005,
        DecayTime = 0x0006,
        DecayHighFrequencyRatio = 0x0007,
        DecayLowFrequencyRation = 0x0008,
        EaxReverbReflectionsGain = 0x0009,
        EaxReverbReflectionsDelay = 0x000A,
        ReflectionsPain = 0x000B,
        LateReverbGain = 0x000C,
        LateReverbDelay = 0x000D,
        LateRevertPain = 0x000E,
        EchoTime = 0x000F,
        EchoDepth = 0x0010,
        ModulationTime = 0x0011,
        ModulationDepth = 0x0012,
        AirAbsorbsionHighFrequency = 0x0013,
        EaxReverbHFReference = 0x0014,
        EaxReverbLFReference = 0x0015,
        RoomRolloffFactor = 0x0016,
        DecayHighFrequencyLimit = 0x0017,
    }

    internal enum EfxEffectType
    {
        Reverb = 0x8000,
    }

    internal class AL
    {
        public IntPtr NativeLibrary { get; private set; }
        public Alc ALC { get; private set; }
        public EffectsExtension Efx { get; private set; }

    private static AL _current;

        public static AL Current
        {
            get
            {
                if (_current != null)
                    return _current;

                if (_current == null)
                    _current = new AL();

                return _current;
            }
        }

        public AL()
        {
            NativeLibrary = GetNativeLibrary();
            LoadEntryPoints(NativeLibrary);

            ALC = new Alc(NativeLibrary);
            Efx = new EffectsExtension(this);
        }

        private IntPtr GetNativeLibrary()
        {
#if DESKTOPGL
            if (CurrentPlatform.OS == OS.Windows)
                return FuncLoader.LoadLibraryExt("soft_oal.dll");
            else if (CurrentPlatform.OS == OS.Linux)
                return FuncLoader.LoadLibraryExt("libopenal.so.1");
            else if (CurrentPlatform.OS == OS.MacOSX)
                return FuncLoader.LoadLibraryExt("libopenal.1.dylib");
            else
                return FuncLoader.LoadLibraryExt("openal");
#elif ANDROID
            IntPtr ret = FuncLoader.LoadLibrary("libopenal32.so");

            if (ret == IntPtr.Zero)
            {
                string appFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                string appDir = Path.GetDirectoryName(appFilesDir);
                string lib = Path.Combine(appDir, "lib", "libopenal32.so");

                ret = FuncLoader.LoadLibrary(lib);
            }

            return ret;
#else
            return FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenAL.framework/OpenAL");
#endif
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alenable(int cap);
        internal d_alenable Enable;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_albufferdata(int buffer, int format, byte* data, int size, int freq);
        internal d_albufferdata alBufferData;

        internal unsafe void BufferData(int buffer, ALFormat alFormat, byte[] data, int index, int count, int freq, int sampleAlignment)
        {
            if (sampleAlignment > 0)
            {
                Bufferi(buffer, ALBufferi.UnpackBlockAlignmentSoft, sampleAlignment);
                this.CheckError("Failed to fill buffer.");
            }

            fixed(byte* pData = data)
            {
                alBufferData(buffer, (int)alFormat, (pData + index), count, freq);
            }
        }

        internal unsafe void BufferData(int buffer, ALFormat alFormat, short[] data, int count, int freq)
        {
            fixed (void* pData = data)
            {
                alBufferData(buffer, (int)alFormat, (byte*)pData, count, freq);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_aldeletebuffers(int n, int* pbuffers);
        internal d_aldeletebuffers alDeleteBuffers;

        internal unsafe void DeleteBuffers(int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alDeleteBuffers(buffers.Length, pbuffers);
            }
        }

        internal unsafe void DeleteBuffer(int buffer)
        {
            alDeleteBuffers(1, &buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferi(int buffer, ALBufferi param, int value);
        internal d_albufferi Bufferi;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetbufferi(int buffer, ALGetBufferi param, out int value);
        internal d_algetbufferi GetBufferi;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferiv(int buffer, ALBufferi param, int[] values);
        internal d_albufferiv Bufferiv;

        internal void GetBuffer(int buffer, ALGetBufferi param, out int value)
        {
            GetBufferi(buffer, param, out value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_algenbuffers(int count, int* pbuffers);
        internal d_algenbuffers alGenBuffers;

        internal unsafe void GenBuffers(int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alGenBuffers(buffers.Length, pbuffers);
            }
        }

        internal unsafe int GenBuffer()
        {
            int buffer;
            alGenBuffers(1, &buffer);
            return buffer;
        }

        internal float GetDuration(int buffer, int sampleRate)
        {
            int samples = GetSamples(buffer);
            return (float)samples / (float)sampleRate;
        }

        internal int GetSamples(int buffer)
        {
            int bits, channels, unpackedSize;
            GetBuffer(buffer, ALGetBufferi.Bits, out bits);
            this.CheckError("Failed to get buffer bits");
            GetBuffer(buffer, ALGetBufferi.Channels, out channels);
            this.CheckError("Failed to get buffer channels");
            GetBuffer(buffer, ALGetBufferi.Size, out unpackedSize);
            this.CheckError("Failed to get buffer size");
            return (unpackedSize / ((bits / 8) * channels));
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_algensources(int n, int* sources);
        internal d_algensources alGenSources;

        internal unsafe void GenSources(int[] sources)
        {
            fixed (int* psources = sources)
            {
                alGenSources(sources.Length, psources);
            }
        }

        internal unsafe int GenSource()
        {
            int source;
            alGenSources(1, &source);
            return source;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ALError d_algeterror();
        internal d_algeterror GetError;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alisbuffer(int buffer);
        internal d_alisbuffer alIsBuffer;

        internal bool IsBuffer(int buffer)
        {
            return alIsBuffer(buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcepause(int source);
        internal d_alsourcepause alSourcePause;

        internal void SourcePause(int source)
        {
            alSourcePause(source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourceplay(int source);
        internal d_alsourceplay alSourcePlay;

        internal void SourcePlay(int source)
        {
            alSourcePlay(source);
        }

        internal static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alissource(int source);
        internal d_alissource IsSource;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_aldeletesources(int n, int* psources);
        internal d_aldeletesources alDeleteSources;

        internal unsafe void DeleteSource(int source)
        {
            alDeleteSources(1, &source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcestop(int source);
        internal d_alsourcestop SourceStop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcei(int source, int param, int value);
        internal d_alsourcei alSourcei;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsource3i(int source, ALSourcei param, int a, int b, int c);
        internal d_alsource3i alSource3i;

        internal void Source(int source, ALSourcei param, int value)
        {
            alSourcei(source, (int)param, value);
        }

        internal void Source(int source, ALSourceb param, bool value)
        {
            alSourcei(source, (int)param, value ? 1 : 0);
        }

        internal void Source(int source, ALSource3f param, float x, float y, float z)
        {
            alSource3f(source, param, x, y, z);
        }

        internal void Source(int source, ALSource3f param, ref Vector3 value)
        {
            alSource3f(source, param, value.X, value.Y, value.Z);
        }

        internal void Source(int source, ALSourcef param, float value)
        {
            alSourcef(source, param, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcef(int source, ALSourcef param, float value);
        internal d_alsourcef alSourcef;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsource3f(int source, ALSource3f param, float x, float y, float z);
        internal d_alsource3f alSource3f;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetsourcei(int source, ALGetSourcei param, out int value);
        internal d_algetsourcei GetSource;

        internal ALSourceState GetSourceState(int source)
        {
            int state;
            GetSource(source, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetlistener3f(ALListener3f param, out float value1, out float value2, out float value3);
        internal d_algetlistener3f GetListener;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldistancemodel(ALDistanceModel model);
        internal d_aldistancemodel DistanceModel;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldopplerfactor(float value);
        internal d_aldopplerfactor DopplerFactor;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alsourcequeuebuffers(int source, int numEntries, int* pbuffers);
        internal d_alsourcequeuebuffers alSourceQueueBuffers;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alsourceunqueuebuffers(int source, int numEntries, int* pbuffers);
        internal d_alsourceunqueuebuffers alSourceUnqueueBuffers;

        internal unsafe void SourceQueueBuffers(int source, int numEntries, int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alSourceQueueBuffers(source, numEntries, pbuffers);
            }
        }

        internal unsafe void SourceQueueBuffer(int source, int buffer)
        {
            alSourceQueueBuffers(source, 1, &buffer);
        }

        internal unsafe int[] SourceUnqueueBuffers(int source, int numEntries)
        {
            if (numEntries <= 0)
                throw new ArgumentOutOfRangeException("numEntries", "Must be greater than zero.");

            int[] buffers = new int[numEntries];
            fixed (int* pbuffers = buffers)
            {
                alSourceUnqueueBuffers(source, numEntries, pbuffers);
            }
            return buffers;
        }

        internal unsafe void SourceUnqueueBuffers(int source, int numEntries, int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alSourceUnqueueBuffers(source, numEntries, pbuffers);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int d_algetenumvalue(string enumName);
        internal d_algetenumvalue alGetEnumValue;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alisextensionpresent(string extensionName);
        internal d_alisextensionpresent IsExtensionPresent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_algetprocaddress(string functionName);
        internal d_algetprocaddress alGetProcAddress;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_algetstring(int p);
        private d_algetstring alGetString;

        internal string GetString(int p)
        {
            return Marshal.PtrToStringAnsi(alGetString(p));
        }

        internal string Get(ALGetString p)
        {
            return GetString((int)p);
        }


        private void LoadEntryPoints(IntPtr library)
        {
            Enable = FuncLoader.LoadFunctionOrNull<d_alenable>(library, "alEnable");
            alBufferData = FuncLoader.LoadFunctionOrNull<d_albufferdata>(library, "alBufferData");
            alDeleteBuffers = FuncLoader.LoadFunctionOrNull<d_aldeletebuffers>(library, "alDeleteBuffers");
            Bufferi = FuncLoader.LoadFunctionOrNull<d_albufferi>(library, "alBufferi");
            GetBufferi = FuncLoader.LoadFunctionOrNull<d_algetbufferi>(library, "alGetBufferi");
            Bufferiv = FuncLoader.LoadFunctionOrNull<d_albufferiv>(library, "alBufferiv");
            alGenBuffers = FuncLoader.LoadFunctionOrNull<d_algenbuffers>(library, "alGenBuffers");
            alGenSources = FuncLoader.LoadFunctionOrNull<d_algensources>(library, "alGenSources");
            GetError = FuncLoader.LoadFunctionOrNull<d_algeterror>(library, "alGetError");
            alIsBuffer = FuncLoader.LoadFunctionOrNull<d_alisbuffer>(library, "alIsBuffer");
            alSourcePause = FuncLoader.LoadFunctionOrNull<d_alsourcepause>(library, "alSourcePause");
            alSourcePlay = FuncLoader.LoadFunctionOrNull<d_alsourceplay>(library, "alSourcePlay");
            IsSource = FuncLoader.LoadFunctionOrNull<d_alissource>(library, "alIsSource");
            alDeleteSources = FuncLoader.LoadFunctionOrNull<d_aldeletesources>(library, "alDeleteSources");
            SourceStop = FuncLoader.LoadFunctionOrNull<d_alsourcestop>(library, "alSourceStop");
            alSourcei = FuncLoader.LoadFunctionOrNull<d_alsourcei>(library, "alSourcei");
            alSource3i = FuncLoader.LoadFunctionOrNull<d_alsource3i>(library, "alSource3i");
            alSourcef = FuncLoader.LoadFunctionOrNull<d_alsourcef>(library, "alSourcef");
            alSource3f = FuncLoader.LoadFunctionOrNull<d_alsource3f>(library, "alSource3f");
            GetSource = FuncLoader.LoadFunctionOrNull<d_algetsourcei>(library, "alGetSourcei");
            GetListener = FuncLoader.LoadFunctionOrNull<d_algetlistener3f>(library, "alGetListener3f");
            DistanceModel = FuncLoader.LoadFunctionOrNull<d_aldistancemodel>(library, "alDistanceModel");
            DopplerFactor = FuncLoader.LoadFunctionOrNull<d_aldopplerfactor>(library, "alDopplerFactor");
            alSourceQueueBuffers = FuncLoader.LoadFunctionOrNull<d_alsourcequeuebuffers>(library, "alSourceQueueBuffers");
            alSourceUnqueueBuffers = FuncLoader.LoadFunctionOrNull<d_alsourceunqueuebuffers>(library, "alSourceUnqueueBuffers");
            alGetEnumValue = FuncLoader.LoadFunctionOrNull<d_algetenumvalue>(library, "alGetEnumValue");
            IsExtensionPresent = FuncLoader.LoadFunctionOrNull<d_alisextensionpresent>(library, "alIsExtensionPresent");
            alGetProcAddress = FuncLoader.LoadFunctionOrNull<d_algetprocaddress>(library, "alGetProcAddress");
            alGetString = FuncLoader.LoadFunctionOrNull<d_algetstring>(library, "alGetString");
        }
    }

    internal class Alc
    {
        public Alc(IntPtr library)
        {
            LoadEntryPoints(library);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccreatecontext(IntPtr device, int[] attributes);
        internal d_alccreatecontext CreateContext;

        internal AlcError GetError()
        {
            return GetErrorForDevice(IntPtr.Zero);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AlcError d_alcgeterror(IntPtr device);
        internal d_alcgeterror GetErrorForDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alcgetintegerv(IntPtr device, int param, int size, int* value);
        internal d_alcgetintegerv alcGetIntegerv;

        internal unsafe int GetInteger(IntPtr device, AlcGetInteger param)
        {
            int value;
            alcGetIntegerv(device, (int)param, 1, &value);
            return value;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcgetcurrentcontext();
        internal d_alcgetcurrentcontext GetCurrentContext;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcmakecontextcurrent(IntPtr context);
        internal d_alcmakecontextcurrent MakeContextCurrent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdestroycontext(IntPtr context);
        internal d_alcdestroycontext DestroyContext;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcclosedevice(IntPtr device);
        internal d_alcclosedevice CloseDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcopendevice(string device);
        internal d_alcopendevice OpenDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccaptureopendevice(string device, uint sampleRate, int format, int sampleSize);
        internal d_alccaptureopendevice alcCaptureOpenDevice;

        internal IntPtr CaptureOpenDevice(string device, uint sampleRate, ALFormat alFormat, int sampleSize)
        {
            return alcCaptureOpenDevice(device, sampleRate, (int)alFormat, sampleSize);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccapturestart(IntPtr device);
        internal d_alccapturestart CaptureStart;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alccapturesamples(IntPtr device, IntPtr buffer, int samples);
        internal d_alccapturesamples CaptureSamples;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccapturestop(IntPtr device);
        internal d_alccapturestop CaptureStop;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccaptureclosedevice(IntPtr device);
        internal d_alccaptureclosedevice CaptureCloseDevice;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alcisextensionpresent(IntPtr device, string extensionName);
        internal d_alcisextensionpresent IsExtensionPresent;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcgetstring(IntPtr device, int p);
        internal d_alcgetstring alcGetString;

        private void LoadEntryPoints(IntPtr library)
        {
            CreateContext = FuncLoader.LoadFunctionOrNull<d_alccreatecontext>(library, "alcCreateContext");
            GetErrorForDevice = FuncLoader.LoadFunctionOrNull<d_alcgeterror>(library, "alcGetError");
            alcGetIntegerv = FuncLoader.LoadFunctionOrNull<d_alcgetintegerv>(library, "alcGetIntegerv");
            GetCurrentContext = FuncLoader.LoadFunctionOrNull<d_alcgetcurrentcontext>(library, "alcGetCurrentContext");
            MakeContextCurrent = FuncLoader.LoadFunctionOrNull<d_alcmakecontextcurrent>(library, "alcMakeContextCurrent");
            DestroyContext = FuncLoader.LoadFunctionOrNull<d_alcdestroycontext>(library, "alcDestroyContext");
            CloseDevice = FuncLoader.LoadFunctionOrNull<d_alcclosedevice>(library, "alcCloseDevice");
            OpenDevice = FuncLoader.LoadFunctionOrNull<d_alcopendevice>(library, "alcOpenDevice");
            alcCaptureOpenDevice = FuncLoader.LoadFunctionOrNull<d_alccaptureopendevice>(library, "alcCaptureOpenDevice");
            CaptureStart = FuncLoader.LoadFunctionOrNull<d_alccapturestart>(library, "alcCaptureStart");
            CaptureSamples = FuncLoader.LoadFunctionOrNull<d_alccapturesamples>(library, "alcCaptureSamples");
            CaptureStop = FuncLoader.LoadFunctionOrNull<d_alccapturestop>(library, "alcCaptureStop");
            CaptureCloseDevice = FuncLoader.LoadFunctionOrNull<d_alccaptureclosedevice>(library, "alcCaptureCloseDevice");
            IsExtensionPresent = FuncLoader.LoadFunctionOrNull<d_alcisextensionpresent>(library, "alcIsExtensionPresent");
            alcGetString = FuncLoader.LoadFunctionOrNull<d_alcgetstring>(library, "alcGetString");

#if IOS || TVOS
            SuspendContext = FuncLoader.LoadFunctionOrNull<d_alcsuspendcontext>(library, "alcSuspendContext");
            ProcessContext = FuncLoader.LoadFunctionOrNull<d_alcprocesscontext>(library, "alcProcessContext");
#endif

#if ANDROID
            DevicePause = FuncLoader.LoadFunctionOrNull<d_alcdevicepausesoft>(library, "alcDevicePauseSOFT");
            DeviceResume = FuncLoader.LoadFunctionOrNull<d_alcdeviceresumesoft>(library, "alcDeviceResumeSOFT");
#endif
        }

        internal string GetString(IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi(alcGetString(device, p));
        }

        internal string GetString(IntPtr device, AlcGetString p)
        {
            return GetString(device, (int)p);
        }

#if IOS || TVOS
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcsuspendcontext(IntPtr context);
        internal d_alcsuspendcontext SuspendContext;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcprocesscontext(IntPtr context);
        internal d_alcprocesscontext ProcessContext;
#endif

#if ANDROID
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdevicepausesoft(IntPtr device);
        internal d_alcdevicepausesoft DevicePause;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdeviceresumesoft(IntPtr device);
        internal d_alcdeviceresumesoft DeviceResume;
#endif
    }

    internal class XRamExtension
    {
        internal enum XRamStorage
        {
            Automatic,
            Hardware,
            Accessible
        }

        private int RamSize;
        private int RamFree;
        private int StorageAuto;
        private int StorageHardware;
        private int StorageAccessible;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate bool SetBufferModeDelegate(int n, ref int buffers, int value);
        private SetBufferModeDelegate setBufferMode;

        internal bool IsInitialized { get; private set; }
        private AL _openAL;

        internal XRamExtension(AL openAL)
        {
            _openAL = openAL;
            Initialize();
        }

        private void Initialize()
        {
            IsInitialized = false;

            if (_openAL.IsExtensionPresent("EAX-RAM"))
            {
                RamSize = _openAL.alGetEnumValue("AL_EAX_RAM_SIZE");
                RamFree = _openAL.alGetEnumValue("AL_EAX_RAM_FREE");
                StorageAuto = _openAL.alGetEnumValue("AL_STORAGE_AUTOMATIC");
                StorageHardware = _openAL.alGetEnumValue("AL_STORAGE_HARDWARE");
                StorageAccessible = _openAL.alGetEnumValue("AL_STORAGE_ACCESSIBLE");

                if (RamSize == 0 || RamFree == 0 || StorageAuto == 0 || StorageHardware == 0 || StorageAccessible == 0)
                {
                    return;
                }

                try
                {
                    LoadEntryPoints();
                }
                catch (Exception)
                {
                    return;
                }

                IsInitialized = true;
            }
        }

        private void LoadEntryPoints()
        {
            setBufferMode = InteropHelpers.GetDelegateForFunctionPointer<XRamExtension.SetBufferModeDelegate>(_openAL.alGetProcAddress("EAXSetBufferMode"));
        }

        internal bool SetBufferMode(int n, ref int buffer, XRamStorage storage)
        {
            if (storage == XRamExtension.XRamStorage.Accessible)
            {
                return setBufferMode(n, ref buffer, StorageAccessible);
            }
            if (storage != XRamExtension.XRamStorage.Hardware)
            {
                return setBufferMode(n, ref buffer, StorageAuto);
            }
            return setBufferMode(n, ref buffer, StorageHardware);
        }
    }

    internal class EffectsExtension
    {
        /* Effect API */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alGenEffectsDelegate(int n, out int effect);
        private alGenEffectsDelegate alGenEffects;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alDeleteEffectsDelegate(int n, ref int effect);
        private alDeleteEffectsDelegate alDeleteEffects;

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //private delegate bool alIsEffectDelegate(int effect);
        //private alIsEffectDelegate alIsEffect;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectfDelegate(int effect, EfxEffectf param, float value);
        private alEffectfDelegate alEffectf;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectiDelegate(int effect, EfxEffecti param, int value);
        private alEffectiDelegate alEffecti;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alGenAuxiliaryEffectSlotsDelegate(int n, out int effectslots);
        private alGenAuxiliaryEffectSlotsDelegate alGenAuxiliaryEffectSlots;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alDeleteAuxiliaryEffectSlotsDelegate(int n, ref int effectslots);
        private alDeleteAuxiliaryEffectSlotsDelegate alDeleteAuxiliaryEffectSlots;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotiDelegate(int slot, EfxEffecti type, int effect);
        private alAuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotfDelegate(int slot, EfxEffectSlotf param, float value);
        private alAuxiliaryEffectSlotfDelegate alAuxiliaryEffectSlotf;

        /* Filter API */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alGenFiltersDelegate(int n, [Out] int* pfilters);
        private alGenFiltersDelegate alGenFilters;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilteriDelegate(int filter, EfxFilteri param, int value);
        private alFilteriDelegate alFilteri;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilterfDelegate(int filter, EfxFilterf param, float value);
        private alFilterfDelegate alFilterf;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alDeleteFiltersDelegate(int n, [In] int* pfilters);
        private alDeleteFiltersDelegate alDeleteFilters;


        internal bool IsInitialized { get; private set; }
        private AL _openAL;


        public EffectsExtension(AL openAL)
        {
            _openAL = openAL;
        }

        internal void Initialize(IntPtr device)
        {
            IsInitialized = false;

            if (_openAL.ALC.IsExtensionPresent(device, "ALC_EXT_EFX"))
            {
                LoadEntryPoints();
                IsInitialized = true;
            }
        }

        private void LoadEntryPoints()
        {
            alGenEffects = InteropHelpers.GetDelegateForFunctionPointer<alGenEffectsDelegate>(_openAL.alGetProcAddress("alGenEffects"));
            alDeleteEffects = InteropHelpers.GetDelegateForFunctionPointer<alDeleteEffectsDelegate>(_openAL.alGetProcAddress("alDeleteEffects"));
            alEffectf = InteropHelpers.GetDelegateForFunctionPointer<alEffectfDelegate>(_openAL.alGetProcAddress("alEffectf"));
            alEffecti = InteropHelpers.GetDelegateForFunctionPointer<alEffectiDelegate>(_openAL.alGetProcAddress("alEffecti"));
            alGenAuxiliaryEffectSlots = InteropHelpers.GetDelegateForFunctionPointer<alGenAuxiliaryEffectSlotsDelegate>(_openAL.alGetProcAddress("alGenAuxiliaryEffectSlots"));
            alDeleteAuxiliaryEffectSlots = InteropHelpers.GetDelegateForFunctionPointer<alDeleteAuxiliaryEffectSlotsDelegate>(_openAL.alGetProcAddress("alDeleteAuxiliaryEffectSlots"));
            alAuxiliaryEffectSloti = InteropHelpers.GetDelegateForFunctionPointer<alAuxiliaryEffectSlotiDelegate>(_openAL.alGetProcAddress("alAuxiliaryEffectSloti"));
            alAuxiliaryEffectSlotf = InteropHelpers.GetDelegateForFunctionPointer<alAuxiliaryEffectSlotfDelegate>(_openAL.alGetProcAddress("alAuxiliaryEffectSlotf"));

            alGenFilters = InteropHelpers.GetDelegateForFunctionPointer<alGenFiltersDelegate>(_openAL.alGetProcAddress("alGenFilters"));
            alFilteri = InteropHelpers.GetDelegateForFunctionPointer<alFilteriDelegate>(_openAL.alGetProcAddress("alFilteri"));
            alFilterf = InteropHelpers.GetDelegateForFunctionPointer<alFilterfDelegate>(_openAL.alGetProcAddress("alFilterf"));
            alDeleteFilters = InteropHelpers.GetDelegateForFunctionPointer<alDeleteFiltersDelegate>(_openAL.alGetProcAddress("alDeleteFilters"));
        }

        /*
            alEffecti(effect, EfxEffecti.FilterType, (int)EfxEffectType.Reverb);
            ALHelper.CheckError("Failed to set Filter Type.");
        */

        internal void GenAuxiliaryEffectSlots(int count, out int slot)
        {
            this.alGenAuxiliaryEffectSlots(count, out slot);
            _openAL.CheckError("Failed to Genereate Aux slot");
        }

        internal void GenEffect(out int effect)
        {
            this.alGenEffects(1, out effect);
            _openAL.CheckError("Failed to Generate Effect.");
        }

        internal void DeleteAuxiliaryEffectSlot(int slot)
        {
            alDeleteAuxiliaryEffectSlots(1, ref slot);
        }

        internal void DeleteEffect(int effect)
        {
            alDeleteEffects(1, ref effect);
        }

        internal void BindEffectToAuxiliarySlot(int slot, int effect)
        {
            alAuxiliaryEffectSloti(slot, EfxEffecti.SlotEffect, effect);
            _openAL.CheckError("Failed to bind Effect");
        }

        internal void AuxiliaryEffectSlot(int slot, EfxEffectSlotf param, float value)
        {
            alAuxiliaryEffectSlotf(slot, param, value);
            _openAL.CheckError("Failes to set " + param + " " + value);
        }

        internal void BindSourceToAuxiliarySlot(int source, int slot, int slotnumber, int filter)
        {
            _openAL.alSource3i(source, ALSourcei.EfxAuxilarySendFilter, slot, slotnumber, filter);
        }

        internal void Effect(int effect, EfxEffectf param, float value)
        {
            alEffectf(effect, param, value);
            _openAL.CheckError("Failed to set " + param + " " + value);
        }

        internal void Effect(int effect, EfxEffecti param, int value)
        {
            alEffecti(effect, param, value);
            _openAL.CheckError("Failed to set " + param + " " + value);
        }

        internal unsafe int GenFilter()
        {
            int filter = 0;
            this.alGenFilters(1, &filter);
            return filter;
        }
        internal void Filter(int filter, EfxFilteri param, int EfxFilterType)
        {
            this.alFilteri(filter, param, EfxFilterType);
        }
        internal void Filter(int filter, EfxFilterf param, float EfxFilterType)
        {
            this.alFilterf(filter, param, EfxFilterType);
        }
        internal void BindFilterToSource(int source, int filter)
        {
            _openAL.Source(source, ALSourcei.EfxDirectFilter, filter);
        }
        internal unsafe void DeleteFilter(int filter)
        {
            alDeleteFilters(1, &filter);
        }
    }

    internal static class ALHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(this AL openAL, string message)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            ALError error = openAL.GetError();
            if (error != ALError.NoError)
            {
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(this AL openAL, string message, params object[] args)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            ALError error = openAL.GetError();
            if (error != ALError.NoError)
            {
                message = String.Format(message, args);
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }

        public static bool IsStereoFormat(ALFormat alFormat)
        {
            return (alFormat == ALFormat.Stereo8
                ||  alFormat == ALFormat.Stereo16
                ||  alFormat == ALFormat.StereoFloat32
                ||  alFormat == ALFormat.StereoIma4
                ||  alFormat == ALFormat.StereoMSAdpcm);
        }
    }

    internal static class AlcHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(this Alc alc, string message)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            AlcError error = alc.GetError();
            if (error != AlcError.NoError)
            {
                throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(this Alc alc, string message, params object[] args)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            AlcError error = alc.GetError();
            if (error != AlcError.NoError)
            {
                message = String.Format(message, args);
                throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
            }
        }
    }

}
