// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;
using MonoGame.Framework.Utilities;

#if ANDROID
using Microsoft.Xna.Framework;
using System.Globalization;
using Android.Content.PM;
using Android.Content;
using Android.Media;
#endif

#if IOS || TVOS
using Microsoft.Xna.Framework;
using AudioToolbox;
using AudioUnit;
using AVFoundation;
#endif


namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioService : AudioServiceStrategy
    {
        private IntPtr _device;
        private IntPtr _context;
        IntPtr NullContext = IntPtr.Zero;

#if ANDROID
        private const int DEFAULT_FREQUENCY = 48000;
        private const int DEFAULT_UPDATE_SIZE = 512;
        private const int DEFAULT_UPDATE_BUFFER_COUNT = 2;
#endif
        private Stack<int> _alSourcesPool = new Stack<int>(32);
        bool _isDisposed;
        public bool SupportsIma4 { get; private set; }
        public bool SupportsAdpcm { get; private set; }
        public bool SupportsEfx { get; private set; }
        public bool SupportsIeee { get; private set; }
        
        internal int ReverbSlot = 0;
        internal int ReverbEffect = 0;
        
        public int Filter { get; private set; }

        internal AL OpenAL { get { return AL.Current; } }

        internal ConcreteAudioService()
        {
            if (OpenAL.NativeLibrary == IntPtr.Zero)
                throw new DllNotFoundException("Couldn't initialize OpenAL because the native binaries couldn't be found.");

            if (!OpenSoundDevice())
                throw new NoAudioHardwareException("OpenAL device could not be initialized, see console output for details.");

            // We have hardware here and it is ready
            Filter = 0;
            if (OpenAL.Efx.IsInitialized)
            {
                Filter = OpenAL.Efx.GenFilter();
            }
        }


        public override SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy sfxStrategy, float pan)
        {
            return new ConcreteSoundEffectInstance(this, sfxStrategy, pan);
        }

        public override IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, int channels, float pan)
        {
            return new ConcreteDynamicSoundEffectInstance(this, sampleRate, channels, pan);
        }


        /// <summary>
        /// Open the sound device, sets up an audio context, and makes the new context
        /// the current context. Note that this method will stop the playback of
        /// music that was running prior to the game start. If any error occurs, then
        /// the state of the controller is reset.
        /// </summary>
        /// <returns>True if the sound device was setup, and false if not.</returns>
        private bool OpenSoundDevice()
        {
            try
            {
                _device = OpenAL.ALC.OpenDevice(string.Empty);
                OpenAL.Efx.Initialize(_device);
            }
            catch (Exception ex)
            {
                throw new NoAudioHardwareException("OpenAL device could not be initialized.", ex);
            }

            OpenAL.ALC.CheckError("Could not open OpenAL device");

            if (_device != IntPtr.Zero)
            {
#if ANDROID
                // Attach activity event handlers so we can pause and resume all playing sounds
                AndroidSurfaceView.OnPauseGameThread += Activity_Paused;
                AndroidSurfaceView.OnResumeGameThread += Activity_Resumed;

                // Query the device for the ideal frequency and update buffer size so
                // we can get the low latency sound path.

                /*
                The recommended sequence is:

                Check for feature "android.hardware.audio.low_latency" using code such as this:
                import android.content.pm.PackageManager;
                ...
                PackageManager pm = getContext().getPackageManager();
                boolean claimsFeature = pm.hasSystemFeature(PackageManager.FEATURE_AUDIO_LOW_LATENCY);
                Check for API level 17 or higher, to confirm use of android.media.AudioManager.getProperty().
                Get the native or optimal output sample rate and buffer size for this device's primary output stream, using code such as this:
                import android.media.AudioManager;
                ...
                AudioManager am = (AudioManager) getSystemService(Context.AUDIO_SERVICE);
                String sampleRate = am.getProperty(AudioManager.PROPERTY_OUTPUT_SAMPLE_RATE));
                String framesPerBuffer = am.getProperty(AudioManager.PROPERTY_OUTPUT_FRAMES_PER_BUFFER));
                Note that sampleRate and framesPerBuffer are Strings. First check for null and then convert to int using Integer.parseInt().
                Now use OpenSL ES to create an AudioPlayer with PCM buffer queue data locator.

                See http://stackoverflow.com/questions/14842803/low-latency-audio-playback-on-android
                */

                int frequency = DEFAULT_FREQUENCY;
                int updateSize = DEFAULT_UPDATE_SIZE;
                int updateBuffers = DEFAULT_UPDATE_BUFFER_COUNT;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.JellyBeanMr1)
                {
                    var appContext = Android.App.Application.Context;
                    Android.Util.Log.Debug("OAL", appContext.PackageManager.HasSystemFeature(PackageManager.FeatureAudioLowLatency) ? "Supports low latency audio playback." : "Does not support low latency audio playback.");

                    AudioManager audioManager = appContext.GetSystemService(Context.AudioService) as AudioManager;
                    if (audioManager != null)
                    {
                        string frequencyStr = audioManager.GetProperty(AudioManager.PropertyOutputSampleRate);
                        if (!string.IsNullOrEmpty(frequencyStr))
                            frequency = int.Parse(frequencyStr, CultureInfo.InvariantCulture);
                        string updateSizeStr = audioManager.GetProperty(AudioManager.PropertyOutputFramesPerBuffer);
                        if (!string.IsNullOrEmpty(updateSizeStr))
                            updateSize = int.Parse(updateSizeStr, CultureInfo.InvariantCulture);
                    }

                    // If 4.4 or higher, then we don't need to double buffer on the application side.
                    // See http://stackoverflow.com/a/15006327
                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Kitkat)
                    {
                        updateBuffers = 1;
                    }
                }
                else
                {
                    Android.Util.Log.Debug("OAL", "Android 4.2 or higher required for low latency audio playback.");
                }
                Android.Util.Log.Debug("OAL", "Using sample rate " + frequency + "Hz and " + updateBuffers + " buffers of " + updateSize + " frames.");

                // These are missing and non-standard ALC constants
                const int AlcFrequency = 0x1007;
                const int AlcUpdateSize = 0x1014;
                const int AlcUpdateBuffers = 0x1015;

                int[] attribute = new[]
                {
                    AlcFrequency, frequency,
                    AlcUpdateSize, updateSize,
                    AlcUpdateBuffers, updateBuffers,
                    0
                };
#elif IOS || TVOS
                AVAudioSession.SharedInstance().Init();

                // NOTE: Do not override AVAudioSessionCategory set by the game developer:
                //       see https://github.com/MonoGame/MonoGame/issues/6595

                EventHandler<AVAudioSessionInterruptionEventArgs> handler = delegate(object sender, AVAudioSessionInterruptionEventArgs e) {
                    switch (e.InterruptionType)
                    {
                        case AVAudioSessionInterruptionType.Began:
                            AVAudioSession.SharedInstance().SetActive(false);
                            OpenAL.ALC.MakeContextCurrent(IntPtr.Zero);
                            OpenAL.ALC.SuspendContext(_context);
                            break;
                        case AVAudioSessionInterruptionType.Ended:
                            AVAudioSession.SharedInstance().SetActive(true);
                            OpenAL.ALC.MakeContextCurrent(_context);
                            OpenAL.ALC.ProcessContext(_context);
                            break;
                    }
                };

                AVAudioSession.Notifications.ObserveInterruption(handler);

                // Activate the instance or else the interruption handler will not be called.
                AVAudioSession.SharedInstance().SetActive(true);

                int[] attribute = new int[0];
#else
                int[] attribute = new int[0];
#endif

                _context = OpenAL.ALC.CreateContext(_device, attribute);

                OpenAL.ALC.CheckError("Could not create OpenAL context");

                if (_context != NullContext)
                {
                    OpenAL.ALC.MakeContextCurrent(_context);
                    OpenAL.ALC.CheckError("Could not make OpenAL context current");
                    SupportsIma4 = OpenAL.IsExtensionPresent("AL_EXT_IMA4");
                    SupportsAdpcm = OpenAL.IsExtensionPresent("AL_SOFT_MSADPCM");
                    SupportsEfx = OpenAL.IsExtensionPresent("AL_EXT_EFX");
                    SupportsIeee = OpenAL.IsExtensionPresent("AL_EXT_float32");
                    return true;
                }
            }
            return false;
        }

        public override void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone)
        {
            if (!OpenAL.ALC.IsExtensionPresent(_device, "ALC_EXT_CAPTURE"))
                return;

            // default device
            string defaultDevice = OpenAL.ALC.GetString(IntPtr.Zero, AlcGetString.CaptureDefaultDeviceSpecifier);

#if true //DESKTOPGL
            // enumerating capture devices
            IntPtr deviceList = OpenAL.ALC.alcGetString(IntPtr.Zero, (int)AlcGetString.CaptureDeviceSpecifier);

            // Marshal native UTF-8 character array to .NET string
            // The native string is a null-char separated list of known capture device specifiers ending with an empty string

            while (true)
            {
                string deviceIdentifier = InteropHelpers.Utf8ToString(deviceList);

                if (string.IsNullOrEmpty(deviceIdentifier))
                    break;

                Microphone microphone = new Microphone(deviceIdentifier);
                microphones.Add(microphone);
                if (deviceIdentifier == defaultDevice)
                    defaultMicrophone = microphone;

                // increase the offset, add one extra for the terminator
                deviceList += deviceIdentifier.Length + 1;
            }
#else
            // Xamarin platforms don't provide an handle to alGetString that allow to marshal string arrays
            // so we're basically only adding the default microphone
            Microphone microphone = new Microphone(defaultDevice);
            microphones.Add(microphone);
            defaultMicrophone = microphone;
#endif
        }

        public override int PlatformGetMaxPlayingInstances()
        {
#if DESKTOPGL
            // MacOS & Linux shares a limit of 256.
            return 256;
#elif IOS || TVOS
            // Reference: http://stackoverflow.com/questions/3894044/maximum-number-of-openal-sound-buffers-on-iphone
            return 32;
#elif ANDROID
            // Set to the same as OpenAL on iOS
            return 32;
#endif
        }

        public override void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
            if (!OpenAL.Efx.IsInitialized)
                return;

            if (ReverbEffect != 0)
                return;

            EffectsExtension efx = OpenAL.Efx;
            efx.GenAuxiliaryEffectSlots(1, out ReverbSlot);
            efx.GenEffect(out ReverbEffect);
            efx.Effect(ReverbEffect, EfxEffecti.EffectType, (int)EfxEffectType.Reverb);
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbReflectionsDelay, reverbSettings.ReflectionsDelayMs / 1000.0f);
            efx.Effect(ReverbEffect, EfxEffectf.LateReverbDelay, reverbSettings.ReverbDelayMs / 1000.0f);
            // map these from range 0-15 to 0-1
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbDiffusion, reverbSettings.EarlyDiffusion / 15f);
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbDiffusion, reverbSettings.LateDiffusion / 15f);
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbGainLF, Math.Min(XactHelpers.ParseVolumeFromDecibels(reverbSettings.LowEqGain - 8f), 1.0f));
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbLFReference, (reverbSettings.LowEqCutoff * 50f) + 50f);
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbGainHF, XactHelpers.ParseVolumeFromDecibels(reverbSettings.HighEqGain - 8f));
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbHFReference, (reverbSettings.HighEqCutoff * 500f) + 1000f);
            // According to Xamarin docs EaxReverbReflectionsGain Unit: Linear gain Range [0.0f .. 3.16f] Default: 0.05f
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbReflectionsGain, Math.Min(XactHelpers.ParseVolumeFromDecibels(reverbSettings.ReflectionsGainDb), 3.16f));
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbGain, Math.Min(XactHelpers.ParseVolumeFromDecibels(reverbSettings.ReverbGainDb), 1.0f));
            // map these from 0-100 down to 0-1
            efx.Effect(ReverbEffect, EfxEffectf.EaxReverbDensity, reverbSettings.DensityPct / 100f);
            efx.AuxiliaryEffectSlot(ReverbSlot, EfxEffectSlotf.EffectSlotGain, reverbSettings.WetDryMixPct / 200f);

            // Dont know what to do with these EFX has no mapping for them. Just ignore for now
            // we can enable them as we go. 
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.PositionLeft, reverbSettings.PositionLeft);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.PositionRight, reverbSettings.PositionRight);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.PositionLeftMatrix, reverbSettings.PositionLeftMatrix);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.PositionRightMatrix, reverbSettings.PositionRightMatrix);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RearDelayMs);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomFilterFrequencyHz);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomFilterMainDb);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomFilterHighFrequencyDb);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.DecayTimeSec);
            //efx.SetEffectParam(ReverbEffect, EfxEffectf.LowFrequencyReference, reverbSettings.RoomSizeFeet);

            efx.BindEffectToAuxiliarySlot(ReverbSlot, ReverbEffect);
        }


        /// <summary>
        /// Reserves a sound buffer and return its identifier. If there are no available sources
        /// or the controller was not able to setup the hardware then an
        /// <see cref="InstancePlayLimitException"/> is thrown.
        /// </summary>
        /// <returns>The source number of the reserved sound buffer.</returns>
        public int ReserveSource()
        {
            if (_alSourcesPool.Count > 0)
                return _alSourcesPool.Pop();

            int src = OpenAL.GenSource();
            OpenAL.CheckError("Failed to generate source.");
            return src;
        }

        public void RecycleSource(int sourceId)
        {
            OpenAL.Source(sourceId, ALSourcei.Buffer, 0);
            OpenAL.CheckError("Failed to free source from buffers.");

            _alSourcesPool.Push(sourceId);
        }

        public double SourceCurrentPosition(int sourceId)
        {
            int pos;
            OpenAL.GetSource(sourceId, ALGetSourcei.SampleOffset, out pos);
            OpenAL.CheckError("Failed to set source offset.");
            return pos;
        }

#if ANDROID
        void Activity_Paused(object sender, EventArgs e)
        {
            // Pause all currently playing sounds by pausing the mixer
            OpenAL.ALC.DevicePause(_device);
        }

        void Activity_Resumed(object sender, EventArgs e)
        {
            // Resume all sounds that were playing when the activity was paused
            OpenAL.ALC.DeviceResume(_device);
        }
#endif

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (ReverbEffect != 0)
            {
                OpenAL.Efx.DeleteAuxiliaryEffectSlot(ReverbSlot);
                OpenAL.Efx.DeleteEffect((int)ReverbEffect);
            }

            while (_alSourcesPool.Count > 0)
            {
                OpenAL.DeleteSource(_alSourcesPool.Pop());
                OpenAL.CheckError("Failed to delete source.");
            }

            if (Filter != 0 && OpenAL.Efx.IsInitialized)
            {
                OpenAL.Efx.DeleteFilter(Filter);
            }
            
            // CleanUpOpenAL
            OpenAL.ALC.MakeContextCurrent(NullContext);

            if (_context != NullContext)
            {
                OpenAL.ALC.DestroyContext(_context);
            }

            if (_device != IntPtr.Zero)
            {
                OpenAL.ALC.CloseDevice(_device);
            }

            _context = NullContext;
            _device = IntPtr.Zero;
        }
    }
}

