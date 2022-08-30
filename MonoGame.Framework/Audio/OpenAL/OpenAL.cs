// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Framework.Utilities;

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
        public static IntPtr NativeLibrary = GetNativeLibrary();

        private static IntPtr GetNativeLibrary()
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
            var ret = FuncLoader.LoadLibrary("libopenal32.so");

            if (ret == IntPtr.Zero)
            {
                var appFilesDir = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                var appDir = Path.GetDirectoryName(appFilesDir);
                var lib = Path.Combine(appDir, "lib", "libopenal32.so");

                ret = FuncLoader.LoadLibrary(lib);
            }

            return ret;
#else
            return FuncLoader.LoadLibrary("/System/Library/Frameworks/OpenAL.framework/OpenAL");
#endif
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alenable(int cap);
        internal static d_alenable Enable = FuncLoader.LoadFunction<d_alenable>(NativeLibrary, "alEnable");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_albufferdata(int buffer, int format, byte* data, int size, int freq);
        internal static d_albufferdata alBufferData = FuncLoader.LoadFunction<d_albufferdata>(NativeLibrary, "alBufferData");

        internal unsafe static void BufferData(int buffer, ALFormat format, byte[] data, int index, int count, int freq, int sampleAlignment)
        {
            if (sampleAlignment > 0)
            {
                AL.Bufferi(buffer, ALBufferi.UnpackBlockAlignmentSoft, sampleAlignment);
                ALHelper.CheckError("Failed to fill buffer.");
            }

            fixed(byte* pData = data)
            {
                alBufferData(buffer, (int)format, (pData + index), count, freq);
            }
        }

        internal unsafe static void BufferData(int buffer, ALFormat format, short[] data, int count, int freq)
        {
            fixed (void* pData = data)
            {
                alBufferData(buffer, (int)format, (byte*)pData, count, freq);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_aldeletebuffers(int n, int* pbuffers);
        internal static d_aldeletebuffers alDeleteBuffers = FuncLoader.LoadFunction<d_aldeletebuffers>(NativeLibrary, "alDeleteBuffers");

        internal unsafe static void DeleteBuffers(int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alDeleteBuffers(buffers.Length, pbuffers);
            }
        }

        internal unsafe static void DeleteBuffer(int buffer)
        {
            alDeleteBuffers(1, &buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferi(int buffer, ALBufferi param, int value);
        internal static d_albufferi Bufferi = FuncLoader.LoadFunction<d_albufferi>(NativeLibrary, "alBufferi");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetbufferi(int buffer, ALGetBufferi param, out int value);
        internal static d_algetbufferi GetBufferi = FuncLoader.LoadFunction<d_algetbufferi>(NativeLibrary, "alGetBufferi");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_albufferiv(int buffer, ALBufferi param, int[] values);
        internal static d_albufferiv Bufferiv = FuncLoader.LoadFunction<d_albufferiv>(NativeLibrary, "alBufferiv");

        internal static void GetBuffer(int buffer, ALGetBufferi param, out int value)
        {
            GetBufferi(buffer, param, out value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_algenbuffers(int count, int* pbuffers);
        internal static d_algenbuffers alGenBuffers = FuncLoader.LoadFunction<d_algenbuffers>(NativeLibrary, "alGenBuffers");

        internal unsafe static void GenBuffers(int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alGenBuffers(buffers.Length, pbuffers);
            }
        }

        internal unsafe static int GenBuffer()
        {
            int buffer;
            alGenBuffers(1, &buffer);
            return buffer;
        }

        internal static float GetDuration(int buffer, int sampleRate)
        {
            int samples = GetSamples(buffer);
            return (float)samples / (float)sampleRate;
        }

        internal static int GetSamples(int buffer)
        {
            int bits, channels, unpackedSize;
            AL.GetBuffer(buffer, ALGetBufferi.Bits, out bits);
            ALHelper.CheckError("Failed to get buffer bits");
            AL.GetBuffer(buffer, ALGetBufferi.Channels, out channels);
            ALHelper.CheckError("Failed to get buffer channels");
            AL.GetBuffer(buffer, ALGetBufferi.Size, out unpackedSize);
            ALHelper.CheckError("Failed to get buffer size");
            return (unpackedSize / ((bits / 8) * channels));
        }


        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_algensources(int n, int* sources);
        internal static d_algensources alGenSources = FuncLoader.LoadFunction<d_algensources>(NativeLibrary, "alGenSources");


        internal unsafe static void GenSources(int[] sources)
        {
            fixed (int* psources = sources)
            {
                alGenSources(sources.Length, psources);
            }
        }

        internal unsafe static int GenSource()
        {
            int source;
            alGenSources(1, &source);
            return source;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate ALError d_algeterror();
        internal static d_algeterror GetError = FuncLoader.LoadFunction<d_algeterror>(NativeLibrary, "alGetError");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alisbuffer(int buffer);
        internal static d_alisbuffer alIsBuffer = FuncLoader.LoadFunction<d_alisbuffer>(NativeLibrary, "alIsBuffer");

        internal static bool IsBuffer(int buffer)
        {
            return alIsBuffer(buffer);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcepause(int source);
        internal static d_alsourcepause alSourcePause = FuncLoader.LoadFunction<d_alsourcepause>(NativeLibrary, "alSourcePause");

        internal static void SourcePause(int source)
        {
            alSourcePause(source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourceplay(int source);
        internal static d_alsourceplay alSourcePlay = FuncLoader.LoadFunction<d_alsourceplay>(NativeLibrary, "alSourcePlay");

        internal static void SourcePlay(int source)
        {
            alSourcePlay(source);
        }

        internal static string GetErrorString(ALError errorCode)
        {
            return errorCode.ToString();
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alissource(int source);
        internal static d_alissource IsSource = FuncLoader.LoadFunction<d_alissource>(NativeLibrary, "alIsSource");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_aldeletesources(int n, int* psources);
        internal static d_aldeletesources alDeleteSources = FuncLoader.LoadFunction<d_aldeletesources>(NativeLibrary, "alDeleteSources");

        internal unsafe static void DeleteSource(int source)
        {
            alDeleteSources(1, &source);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcestop(int source);
        internal static d_alsourcestop SourceStop = FuncLoader.LoadFunction<d_alsourcestop>(NativeLibrary, "alSourceStop");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcei(int source, int param, int value);
        internal static d_alsourcei alSourcei = FuncLoader.LoadFunction<d_alsourcei>(NativeLibrary, "alSourcei");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsource3i(int source, ALSourcei param, int a, int b, int c);
        internal static d_alsource3i alSource3i = FuncLoader.LoadFunction<d_alsource3i>(NativeLibrary, "alSource3i");

        internal static void Source(int source, ALSourcei param, int value)
        {
            alSourcei(source, (int)param, value);
        }

        internal static void Source(int source, ALSourceb param, bool value)
        {
            alSourcei(source, (int)param, value ? 1 : 0);
        }

        internal static void Source(int source, ALSource3f param, float x, float y, float z)
        {
            alSource3f(source, param, x, y, z);
        }

        internal static void Source(int source, ALSource3f param, ref Vector3 value)
        {
            alSource3f(source, param, value.X, value.Y, value.Z);
        }

        internal static void Source(int source, ALSourcef param, float value)
        {
            alSourcef(source, param, value);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsourcef(int source, ALSourcef param, float value);
        internal static d_alsourcef alSourcef = FuncLoader.LoadFunction<d_alsourcef>(NativeLibrary, "alSourcef");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alsource3f(int source, ALSource3f param, float x, float y, float z);
        internal static d_alsource3f alSource3f = FuncLoader.LoadFunction<d_alsource3f>(NativeLibrary, "alSource3f");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetsourcei(int source, ALGetSourcei param, out int value);
        internal static d_algetsourcei GetSource = FuncLoader.LoadFunction<d_algetsourcei>(NativeLibrary, "alGetSourcei");

        internal static ALSourceState GetSourceState(int source)
        {
            int state;
            GetSource(source, ALGetSourcei.SourceState, out state);
            return (ALSourceState)state;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_algetlistener3f(ALListener3f param, out float value1, out float value2, out float value3);
        internal static d_algetlistener3f GetListener = FuncLoader.LoadFunction<d_algetlistener3f>(NativeLibrary, "alGetListener3f");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldistancemodel(ALDistanceModel model);
        internal static d_aldistancemodel DistanceModel = FuncLoader.LoadFunction<d_aldistancemodel>(NativeLibrary, "alDistanceModel");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_aldopplerfactor(float value);
        internal static d_aldopplerfactor DopplerFactor = FuncLoader.LoadFunction<d_aldopplerfactor>(NativeLibrary, "alDopplerFactor");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alsourcequeuebuffers(int source, int numEntries, int* pbuffers);
        internal static d_alsourcequeuebuffers alSourceQueueBuffers = FuncLoader.LoadFunction<d_alsourcequeuebuffers>(NativeLibrary, "alSourceQueueBuffers");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alsourceunqueuebuffers(int source, int numEntries, int* pbuffers);
        internal static d_alsourceunqueuebuffers alSourceUnqueueBuffers = FuncLoader.LoadFunction<d_alsourceunqueuebuffers>(NativeLibrary, "alSourceUnqueueBuffers");

        internal static unsafe void SourceQueueBuffers(int source, int numEntries, int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                AL.alSourceQueueBuffers(source, numEntries, pbuffers);
            }
        }

        internal unsafe static void SourceQueueBuffer(int source, int buffer)
        {
            AL.alSourceQueueBuffers(source, 1, &buffer);
        }

        internal static unsafe int[] SourceUnqueueBuffers(int source, int numEntries)
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

        internal unsafe static void SourceUnqueueBuffers(int source, int numEntries, int[] buffers)
        {
            fixed (int* pbuffers = buffers)
            {
                alSourceUnqueueBuffers(source, numEntries, pbuffers);
            }
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int d_algetenumvalue(string enumName);
        internal static d_algetenumvalue alGetEnumValue = FuncLoader.LoadFunction<d_algetenumvalue>(NativeLibrary, "alGetEnumValue");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alisextensionpresent(string extensionName);
        internal static d_alisextensionpresent IsExtensionPresent = FuncLoader.LoadFunction<d_alisextensionpresent>(NativeLibrary, "alIsExtensionPresent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_algetprocaddress(string functionName);
        internal static d_algetprocaddress alGetProcAddress = FuncLoader.LoadFunction<d_algetprocaddress>(NativeLibrary, "alGetProcAddress");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr d_algetstring(int p);
        private static d_algetstring alGetString = FuncLoader.LoadFunction<d_algetstring>(NativeLibrary, "alGetString");

        internal static string GetString(int p)
        {
            return Marshal.PtrToStringAnsi(alGetString(p));
        }

        internal static string Get(ALGetString p)
        {
            return GetString((int)p);
        }
    }

    internal partial class Alc
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccreatecontext(IntPtr device, int[] attributes);
        internal static d_alccreatecontext CreateContext = FuncLoader.LoadFunction<d_alccreatecontext>(AL.NativeLibrary, "alcCreateContext");

        internal static AlcError GetError()
        {
            return GetErrorForDevice(IntPtr.Zero);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate AlcError d_alcgeterror(IntPtr device);
        internal static d_alcgeterror GetErrorForDevice = FuncLoader.LoadFunction<d_alcgeterror>(AL.NativeLibrary, "alcGetError");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal unsafe delegate void d_alcgetintegerv(IntPtr device, int param, int size, int* value);
        internal static d_alcgetintegerv alcGetIntegerv = FuncLoader.LoadFunction<d_alcgetintegerv>(AL.NativeLibrary, "alcGetIntegerv");

        internal unsafe static int GetInteger(IntPtr device, AlcGetInteger param)
        {
            int value;
            alcGetIntegerv(device, (int)param, 1, &value);
            return value;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcgetcurrentcontext();
        internal static d_alcgetcurrentcontext GetCurrentContext = FuncLoader.LoadFunction<d_alcgetcurrentcontext>(AL.NativeLibrary, "alcGetCurrentContext");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcmakecontextcurrent(IntPtr context);
        internal static d_alcmakecontextcurrent MakeContextCurrent = FuncLoader.LoadFunction<d_alcmakecontextcurrent>(AL.NativeLibrary, "alcMakeContextCurrent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdestroycontext(IntPtr context);
        internal static d_alcdestroycontext DestroyContext = FuncLoader.LoadFunction<d_alcdestroycontext>(AL.NativeLibrary, "alcDestroyContext");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcclosedevice(IntPtr device);
        internal static d_alcclosedevice CloseDevice = FuncLoader.LoadFunction<d_alcclosedevice>(AL.NativeLibrary, "alcCloseDevice");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcopendevice(string device);
        internal static d_alcopendevice OpenDevice = FuncLoader.LoadFunction<d_alcopendevice>(AL.NativeLibrary, "alcOpenDevice");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccaptureopendevice(string device, uint sampleRate, int format, int sampleSize);
        internal static d_alccaptureopendevice alcCaptureOpenDevice = FuncLoader.LoadFunction<d_alccaptureopendevice>(AL.NativeLibrary, "alcCaptureOpenDevice");

        internal static IntPtr CaptureOpenDevice(string device, uint sampleRate, ALFormat format, int sampleSize)
        {
            return alcCaptureOpenDevice(device, sampleRate, (int)format, sampleSize);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccapturestart(IntPtr device);
        internal static d_alccapturestart CaptureStart = FuncLoader.LoadFunction<d_alccapturestart>(AL.NativeLibrary, "alcCaptureStart");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alccapturesamples(IntPtr device, IntPtr buffer, int samples);
        internal static d_alccapturesamples CaptureSamples = FuncLoader.LoadFunction<d_alccapturesamples>(AL.NativeLibrary, "alcCaptureSamples");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccapturestop(IntPtr device);
        internal static d_alccapturestop CaptureStop = FuncLoader.LoadFunction<d_alccapturestop>(AL.NativeLibrary, "alcCaptureStop");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alccaptureclosedevice(IntPtr device);
        internal static d_alccaptureclosedevice CaptureCloseDevice = FuncLoader.LoadFunction<d_alccaptureclosedevice>(AL.NativeLibrary, "alcCaptureCloseDevice");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate bool d_alcisextensionpresent(IntPtr device, string extensionName);
        internal static d_alcisextensionpresent IsExtensionPresent = FuncLoader.LoadFunction<d_alcisextensionpresent>(AL.NativeLibrary, "alcIsExtensionPresent");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr d_alcgetstring(IntPtr device, int p);
        internal static d_alcgetstring alcGetString = FuncLoader.LoadFunction<d_alcgetstring>(AL.NativeLibrary, "alcGetString");

        internal static string GetString(IntPtr device, int p)
        {
            return Marshal.PtrToStringAnsi(alcGetString(device, p));
        }

        internal static string GetString(IntPtr device, AlcGetString p)
        {
            return GetString(device, (int)p);
        }

#if (IOS || TVOS)
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcsuspendcontext(IntPtr context);
        internal static d_alcsuspendcontext SuspendContext = FuncLoader.LoadFunction<d_alcsuspendcontext>(AL.NativeLibrary, "alcSuspendContext");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcprocesscontext(IntPtr context);
        internal static d_alcprocesscontext ProcessContext = FuncLoader.LoadFunction<d_alcprocesscontext>(AL.NativeLibrary, "alcProcessContext");
#endif

#if ANDROID
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdevicepausesoft(IntPtr device);
        internal static d_alcdevicepausesoft DevicePause = FuncLoader.LoadFunction<d_alcdevicepausesoft>(AL.NativeLibrary, "alcDevicePauseSOFT");

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void d_alcdeviceresumesoft(IntPtr device);
        internal static d_alcdeviceresumesoft DeviceResume = FuncLoader.LoadFunction<d_alcdeviceresumesoft>(AL.NativeLibrary, "alcDeviceResumeSOFT");
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

        internal XRamExtension()
        {
            IsInitialized = false;
            if (!AL.IsExtensionPresent("EAX-RAM"))
            {
                return;
            }
            RamSize = AL.alGetEnumValue("AL_EAX_RAM_SIZE");
            RamFree = AL.alGetEnumValue("AL_EAX_RAM_FREE");
            StorageAuto = AL.alGetEnumValue("AL_STORAGE_AUTOMATIC");
            StorageHardware = AL.alGetEnumValue("AL_STORAGE_HARDWARE");
            StorageAccessible = AL.alGetEnumValue("AL_STORAGE_ACCESSIBLE");
            if (RamSize == 0 || RamFree == 0 || StorageAuto == 0 || StorageHardware == 0 || StorageAccessible == 0)
            {
                return;
            }
            try
            {
                setBufferMode = (XRamExtension.SetBufferModeDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("EAXSetBufferMode"), typeof(XRamExtension.SetBufferModeDelegate));
            }
            catch (Exception)
            {
                return;
            }
            IsInitialized = true;
        }

        internal bool IsInitialized { get; private set; }

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
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alDeleteEffectsDelegate(int n, ref int effect);
        //[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
        //private delegate bool alIsEffectDelegate (int effect);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectfDelegate(int effect, EfxEffectf param, float value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alEffectiDelegate(int effect, EfxEffecti param, int value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alGenAuxiliaryEffectSlotsDelegate(int n, out int effectslots);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alDeleteAuxiliaryEffectSlotsDelegate(int n, ref int effectslots);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotiDelegate(int slot, EfxEffecti type, int effect);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alAuxiliaryEffectSlotfDelegate(int slot, EfxEffectSlotf param, float value);

        /* Filter API */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alGenFiltersDelegate(int n, [Out] int* pfilters);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilteriDelegate(int filter, EfxFilteri param, int value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void alFilterfDelegate(int filter, EfxFilterf param, float value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private unsafe delegate void alDeleteFiltersDelegate(int n, [In] int* pfilters);


        private alGenEffectsDelegate alGenEffects;
        private alDeleteEffectsDelegate alDeleteEffects;
        //private alIsEffectDelegate alIsEffect;
        private alEffectfDelegate alEffectf;
        private alEffectiDelegate alEffecti;
        private alGenAuxiliaryEffectSlotsDelegate alGenAuxiliaryEffectSlots;
        private alDeleteAuxiliaryEffectSlotsDelegate alDeleteAuxiliaryEffectSlots;
        private alAuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;
        private alAuxiliaryEffectSlotfDelegate alAuxiliaryEffectSlotf;
        private alGenFiltersDelegate alGenFilters;
        private alFilteriDelegate alFilteri;
        private alFilterfDelegate alFilterf;
        private alDeleteFiltersDelegate alDeleteFilters;

        internal static IntPtr device;
        static EffectsExtension _instance;
        internal static EffectsExtension Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EffectsExtension();
                return _instance;
            }
        }

        internal EffectsExtension()
        {
            IsInitialized = false;
            if (!Alc.IsExtensionPresent(device, "ALC_EXT_EFX"))
            {
                return;
            }

            alGenEffects = (alGenEffectsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alGenEffects"), typeof(alGenEffectsDelegate));
            alDeleteEffects = (alDeleteEffectsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alDeleteEffects"), typeof(alDeleteEffectsDelegate));
            alEffectf = (alEffectfDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alEffectf"), typeof(alEffectfDelegate));
            alEffecti = (alEffectiDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alEffecti"), typeof(alEffectiDelegate));
            alGenAuxiliaryEffectSlots = (alGenAuxiliaryEffectSlotsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alGenAuxiliaryEffectSlots"), typeof(alGenAuxiliaryEffectSlotsDelegate));
            alDeleteAuxiliaryEffectSlots = (alDeleteAuxiliaryEffectSlotsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alDeleteAuxiliaryEffectSlots"), typeof(alDeleteAuxiliaryEffectSlotsDelegate));
            alAuxiliaryEffectSloti = (alAuxiliaryEffectSlotiDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alAuxiliaryEffectSloti"), typeof(alAuxiliaryEffectSlotiDelegate));
            alAuxiliaryEffectSlotf = (alAuxiliaryEffectSlotfDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alAuxiliaryEffectSlotf"), typeof(alAuxiliaryEffectSlotfDelegate));

            alGenFilters = (alGenFiltersDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alGenFilters"), typeof(alGenFiltersDelegate));
            alFilteri = (alFilteriDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alFilteri"), typeof(alFilteriDelegate));
            alFilterf = (alFilterfDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alFilterf"), typeof(alFilterfDelegate));
            alDeleteFilters = (alDeleteFiltersDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alDeleteFilters"), typeof(alDeleteFiltersDelegate));

            IsInitialized = true;
        }

        internal bool IsInitialized { get; private set; }

        /*

alEffecti (effect, EfxEffecti.FilterType, (int)EfxEffectType.Reverb);
            ALHelper.CheckError ("Failed to set Filter Type.");

        */

        internal void GenAuxiliaryEffectSlots(int count, out int slot)
        {
            this.alGenAuxiliaryEffectSlots(count, out slot);
            ALHelper.CheckError("Failed to Genereate Aux slot");
        }

        internal void GenEffect(out int effect)
        {
            this.alGenEffects(1, out effect);
            ALHelper.CheckError("Failed to Generate Effect.");
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
            ALHelper.CheckError("Failed to bind Effect");
        }

        internal void AuxiliaryEffectSlot(int slot, EfxEffectSlotf param, float value)
        {
            alAuxiliaryEffectSlotf(slot, param, value);
            ALHelper.CheckError("Failes to set " + param + " " + value);
        }

        internal void BindSourceToAuxiliarySlot(int source, int slot, int slotnumber, int filter)
        {
            AL.alSource3i(source, ALSourcei.EfxAuxilarySendFilter, slot, slotnumber, filter);
        }

        internal void Effect(int effect, EfxEffectf param, float value)
        {
            alEffectf(effect, param, value);
            ALHelper.CheckError("Failed to set " + param + " " + value);
        }

        internal void Effect(int effect, EfxEffecti param, int value)
        {
            alEffecti(effect, param, value);
            ALHelper.CheckError("Failed to set " + param + " " + value);
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
            AL.Source(source, ALSourcei.EfxDirectFilter, filter);
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
        internal static void CheckError(string message)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(string message, params object[] args)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                message = String.Format(message, args);
                throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
            }
        }

        public static bool IsStereoFormat(ALFormat format)
        {
            return (format == ALFormat.Stereo8
                || format == ALFormat.Stereo16
                || format == ALFormat.StereoFloat32
                || format == ALFormat.StereoIma4
                || format == ALFormat.StereoMSAdpcm);
        }
    }

    internal static class AlcHelper
    {
        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(string message)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            AlcError error = Alc.GetError();
            if (error != AlcError.NoError)
            {
                throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        [System.Diagnostics.DebuggerHidden]
        internal static void CheckError(string message, params object[] args)
        {
            System.Diagnostics.Debug.Assert(!String.IsNullOrEmpty(message));

            AlcError error = Alc.GetError();
            if (error != AlcError.NoError)
            {
                message = String.Format(message, args);
                throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
            }
        }
    }
}
