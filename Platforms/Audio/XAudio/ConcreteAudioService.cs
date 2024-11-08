// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using SharpDX;
using SharpDX.XAudio2;
using SharpDX.Multimedia;
using SharpDX.X3DAudio;

namespace Microsoft.Xna.Platform.Audio
{
    internal class ConcreteAudioService : AudioServiceStrategy
    {
        internal XAudio2 Device { get; private set; }
        internal MasteringVoice MasterVoice { get; private set; }

        private X3DAudio _device3D;
        private bool _device3DDirty = true;
        private Speakers _speakers = Speakers.Stereo;


        // XNA does not expose this, but it exists in X3DAudio.
        // TODO: move to SoundEngine.
        //[CLSCompliant(false)]
        public Speakers Speakers
        {
            get { return _speakers; }

            set
            {
                if (_speakers == value)
                    return;

                _speakers = value;
                _device3DDirty = true;
            }
        }

        internal X3DAudio Device3D
        {
            get
            {
                if (_device3DDirty)
                {
                    _device3DDirty = false;
                    _device3D = new X3DAudio(_speakers);
                }

                return _device3D;
            }
        }

        private SubmixVoice _reverbVoice;


        internal ConcreteAudioService()
        {
            try
            {
                if (Device == null)
                {
#if WINDOWSDX
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) => { AudioService.Shutdown(); };
#endif

#if DEBUG && !(UAP || WINUI)
                    try
                    {
                        //Fails if the XAudio2 SDK is not installed
                        Device = new XAudio2(XAudio2Flags.DebugEngine, ProcessorSpecifier.DefaultProcessor);
                        Device.StartEngine();
                    }
                    catch
#endif
                    {
                        Device = new XAudio2(XAudio2Flags.None, ProcessorSpecifier.DefaultProcessor);
                        Device.StartEngine();
                    }
                }

                // Just use the default device.
#if (UAP || WINUI)
                string deviceId = null;
#else
                const int deviceId = 0;
#endif

                if (MasterVoice == null)
                {
                    // Let windows autodetect number of channels and sample rate.
                    MasterVoice = new MasteringVoice(Device, XAudio2.DefaultChannels, XAudio2.DefaultSampleRate);
                }

                // The autodetected value of MasterVoice.ChannelMask corresponds to the speaker layout.
#if (UAP || WINUI)
                Speakers = (Speakers)MasterVoice.ChannelMask;
#else
                Speakers = Device.Version == XAudio2Version.Version27
                    ? Device.GetDeviceDetails(deviceId).OutputFormat.ChannelMask
                    : (Speakers)MasterVoice.ChannelMask;
#endif
            }
            catch
            {
                // Release the device and null it as
                // we have no audio support.
                if (Device != null)
                {
                    Device.Dispose();
                    Device = null;
                }

                MasterVoice = null;
            }
        }

        public override SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy sfxStrategy)
        {
            return new ConcreteSoundEffectInstance(this, sfxStrategy);
        }

        public override IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, int channels)
        {
            return new ConcreteDynamicSoundEffectInstance(this, sampleRate, channels);
        }

        public override void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone)
        {
        }

        internal SubmixVoice ReverbVoice
        {
            get
            {
                if (_reverbVoice == null)
                {
                    VoiceDetails details = MasterVoice.VoiceDetails;
                    _reverbVoice = new SubmixVoice(Device, details.InputChannelCount, details.InputSampleRate);

#if SHARPDX263
                    var reverb = new SharpDX.XAudio2.Fx.Reverb();
#else
                    var reverb = new SharpDX.XAudio2.Fx.Reverb(Device);
#endif
                    EffectDescriptor desc = new EffectDescriptor(reverb);
                    desc.InitialState = true;
                    desc.OutputChannelCount = details.InputChannelCount;
                    _reverbVoice.SetEffectChain(desc);
                }

                return _reverbVoice;
            }
        }

        public override int PlatformGetMaxPlayingInstances()
        {
            // These platforms are only limited by memory.
            return int.MaxValue;
        }

        public override void PlatformSetReverbSettings(ReverbSettings reverbSettings)
        {
             // All parameters related to sampling rate or time are relative to a 48kHz 
            // voice and must be scaled for use with other sampling rates.
            float timeScale = 48000.0f / ReverbVoice.VoiceDetails.InputSampleRate;

            var settings = new SharpDX.XAudio2.Fx.ReverbParameters
            {
                ReflectionsGain = reverbSettings.ReflectionsGainDb,
                ReverbGain = reverbSettings.ReverbGainDb,
                DecayTime = reverbSettings.DecayTimeSec,
                ReflectionsDelay = (byte)(reverbSettings.ReflectionsDelayMs * timeScale),
                ReverbDelay = (byte)(reverbSettings.ReverbDelayMs * timeScale),
                RearDelay = (byte)(reverbSettings.RearDelayMs * timeScale),
                RoomSize = reverbSettings.RoomSizeFeet,
                Density = reverbSettings.DensityPct,
                LowEQGain = (byte)reverbSettings.LowEqGain,
                LowEQCutoff = (byte)reverbSettings.LowEqCutoff,
                HighEQGain = (byte)reverbSettings.HighEqGain,
                HighEQCutoff = (byte)reverbSettings.HighEqCutoff,
                PositionLeft = (byte)reverbSettings.PositionLeft,
                PositionRight = (byte)reverbSettings.PositionRight,
                PositionMatrixLeft = (byte)reverbSettings.PositionLeftMatrix,
                PositionMatrixRight = (byte)reverbSettings.PositionRightMatrix,
                EarlyDiffusion = (byte)reverbSettings.EarlyDiffusion,
                LateDiffusion = (byte)reverbSettings.LateDiffusion,
                RoomFilterMain = reverbSettings.RoomFilterMainDb,
                RoomFilterFreq = reverbSettings.RoomFilterFrequencyHz * timeScale,
                RoomFilterHF = reverbSettings.RoomFilterHighFrequencyDb,
                WetDryMix = reverbSettings.WetDryMixPct
            };

            ReverbVoice.SetEffectParameters(0, settings);
        }

        public override void Suspend()
        {
        }

        public override void Resume()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_reverbVoice != null)
                {
                    _reverbVoice.DestroyVoice();
                    _reverbVoice.Dispose();
                    _reverbVoice = null;
                }

                if (MasterVoice != null)
                {
                    MasterVoice.Dispose();
                    MasterVoice = null;
                }

                if (Device != null)
                {
                    Device.StopEngine();
                    Device.Dispose();
                    Device = null;
                }
            }

            _device3DDirty = true;
            _speakers = Speakers.Stereo;
        }
    }
}

