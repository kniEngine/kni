// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;

namespace Microsoft.Xna.Platform.Audio
{
    public class ConcreteSoundEffectInstance : SoundEffectInstanceStrategy
    {
        private AudioServiceStrategy _audioServiceStrategy;
        private ConcreteSoundEffect _concreteSoundEffect;
        internal ConcreteAudioService ConcreteAudioService { get { return (ConcreteAudioService)_audioServiceStrategy; } }

        protected int _sourceId;
        float reverb;
        bool applyFilter = false;
        EfxFilterType filterType;
        float filterQ;
        float frequency;

        float _volume = 1f;
        float _pitch = 0f;
        // emmiter's position/velocity relative to the listener
        Vector3 _relativePosition;
        Vector3 _relativeVelocity;

        #region Initialization

        internal ConcreteSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy, float pan)
            : base(audioServiceStrategy, sfxStrategy, pan)
        {
            _audioServiceStrategy = audioServiceStrategy;
            _concreteSoundEffect = (ConcreteSoundEffect)sfxStrategy;
        }

        #endregion // Initialization

        /// <summary>
        /// Converts the XNA [-1, 1] pitch range to OpenAL pitch (0, INF).
        /// <param name="xnaPitch">The pitch of the sound in the Microsoft XNA range.</param>
        /// </summary>
        private static float XnaPitchToAlPitch(float xnaPitch)
        {
            return (float)Math.Pow(2, xnaPitch);
        }

        internal override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            // set up matrix to transform world space coordinates to listener space coordinates
            Matrix worldSpaceToListenerSpace = Matrix.Transpose(Matrix.CreateWorld(listener.Position, listener.Forward, listener.Up));
            // set up our final position and velocity according to orientation of listener
            _relativePosition = emitter.Position;
            Vector3.Transform(ref _relativePosition, ref worldSpaceToListenerSpace, out _relativePosition);
            _relativeVelocity = emitter.Velocity - listener.Velocity;
            Vector3.TransformNormal(ref _relativeVelocity, ref worldSpaceToListenerSpace, out _relativeVelocity);

            if (_sourceId != 0)
            {
                // set the position based on relative position
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSource3f.Position, ref _relativePosition);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source position.");
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSource3f.Velocity, ref _relativeVelocity);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source velocity.");
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcef.ReferenceDistance, SoundEffect.DistanceScale);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source distance scale.");
                ConcreteAudioService.OpenAL.DopplerFactor(SoundEffect.DopplerScale);
                ConcreteAudioService.OpenAL.CheckError("Failed to set Doppler scale.");
            }
        }

        internal override void PlatformPause()
        {
            ConcreteAudioService.OpenAL.SourcePause(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to pause source.");
        }

        internal override void PlatformPlay(bool isLooped)
        {
            _sourceId = ConcreteAudioService.ReserveSource();

            // bind buffer to source
            int bufferId = _concreteSoundEffect.GetALSoundBuffer().Buffer;
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcei.Buffer, bufferId);
            ConcreteAudioService.OpenAL.CheckError("Failed to bind buffer to source.");

            // Send the position, gain, looping, pitch, and distance model to the OpenAL driver.

            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcei.SourceRelative, 1);
            ConcreteAudioService.OpenAL.CheckError("Failed set source relative.");
            // Distance Model
            ConcreteAudioService.OpenAL.DistanceModel(ALDistanceModel.InverseDistanceClamped);
            ConcreteAudioService.OpenAL.CheckError("Failed set source distance.");
            // Position/Pan
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSource3f.Position, ref _relativePosition);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source position/pan.");
            // Velocity
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSource3f.Velocity, ref _relativeVelocity);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source pan.");
            // Distance Scale
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcef.ReferenceDistance, SoundEffect.DistanceScale);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source distance scale.");
            // Doppler Scale
            ConcreteAudioService.OpenAL.DopplerFactor(SoundEffect.DopplerScale);
            ConcreteAudioService.OpenAL.CheckError("Failed to set Doppler scale.");
            // Volume
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcef.Gain, _volume);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source volume.");
            // Looping
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourceb.Looping, isLooped);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source loop state.");
            // Pitch
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
            ConcreteAudioService.OpenAL.CheckError("Failed to set source pitch.");

            ApplyReverb();
            ApplyFilter();

            ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to play source.");
        }

        internal override void PlatformResume(bool isLooped)
        {
            ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to play source.");
        }

        internal override void PlatformStop()
        {
            ConcreteAudioService.OpenAL.SourceStop(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to stop source.");

            // Reset the SendFilter to 0 if we are NOT using reverb since
            // sources are recycled
            if (ConcreteAudioService.SupportsEfx)
            {
                ConcreteAudioService.OpenAL.Efx.BindSourceToAuxiliarySlot(_sourceId, 0, 0, 0);
                ConcreteAudioService.OpenAL.CheckError("Failed to unset reverb.");
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcei.EfxDirectFilter, 0);
                ConcreteAudioService.OpenAL.CheckError("Failed to unset filter.");
            }

            ConcreteAudioService.RecycleSource(_sourceId);
            _sourceId = 0;
        }

        internal override void PlatformRelease(bool isLooped)
        {
            if (isLooped)
            {
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourceb.Looping, false);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source loop state.");
            }
        }

        internal override bool PlatformUpdateState(ref SoundState state)
        {
            // check if the sound has stopped
            if (state == SoundState.Playing)
            {
                ALSourceState alState = ConcreteAudioService.OpenAL.GetSourceState(_sourceId);
                ConcreteAudioService.OpenAL.CheckError("Failed to get source state.");

                if (alState == ALSourceState.Stopped)
                {
                    // update instance
                    PlatformStop();
                    state = SoundState.Stopped;
                    return true;
                }
            }

            return false;
        }

        internal override void PlatformSetIsLooped(bool isLooped, SoundState state)
        {
            if (_sourceId != 0)
            {
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourceb.Looping, isLooped);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source loop state.");
            }
        }

        internal override void PlatformSetPan(float pan)
        {
            // OpenAL doesn't support Panning. We emulate it using 3D audio.
            // If the user set both Pan and Apply3D(), only the last call takes effect.
            _relativePosition.X = (float)Math.Sin(pan * MathHelper.PiOver2) * SoundEffect.DistanceScale;
            _relativePosition.Y = (float)Math.Cos(pan * MathHelper.PiOver2) * SoundEffect.DistanceScale;
            _relativePosition.Z = 0f;

            if (_sourceId != 0)
            {
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSource3f.Position, ref _relativePosition);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source pan.");
            }
        }

        internal override void PlatformSetPitch(float pitch)
        {
            _pitch = pitch;

            if (_sourceId != 0)
            {
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcef.Pitch, XnaPitchToAlPitch(_pitch));
                ConcreteAudioService.OpenAL.CheckError("Failed to set source pitch.");
            }
        }

        internal override void PlatformSetVolume(float volume)
        {
            _volume = volume;

            if (_sourceId != 0)
            {
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcef.Gain, _volume);
                ConcreteAudioService.OpenAL.CheckError("Failed to set source volume.");
            }
        }

        internal override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
            if (!ConcreteAudioService.OpenAL.Efx.IsInitialized)
                return;

            reverb = mix;

            if (state == SoundState.Playing)
            {
                ApplyReverb();
                reverb = 0f;
            }
        }

        void ApplyReverb()
        {
            if (reverb > 0f && ConcreteAudioService.ReverbSlot != 0)
            {
                ConcreteAudioService.OpenAL.Efx.BindSourceToAuxiliarySlot(_sourceId, ConcreteAudioService.ReverbSlot, 0, 0);
                ConcreteAudioService.OpenAL.CheckError("Failed to set reverb.");
            }
        }

        void ApplyFilter()
        {
            if (applyFilter && ConcreteAudioService.Filter > 0)
            {
                float freq = frequency / 20000f;
                float lf = 1.0f - freq;
                EffectsExtension efx = ConcreteAudioService.OpenAL.Efx;
                efx.Filter(ConcreteAudioService.Filter, EfxFilteri.FilterType, (int)filterType);
                ConcreteAudioService.OpenAL.CheckError("Failed to set filter.");
                switch (filterType)
                {
                case EfxFilterType.Lowpass:
                    efx.Filter(ConcreteAudioService.Filter, EfxFilterf.LowpassGainHF, freq);
                        ConcreteAudioService.OpenAL.CheckError("Failed to set LowpassGainHF.");
                    break;
                case EfxFilterType.Highpass:
                    efx.Filter(ConcreteAudioService.Filter, EfxFilterf.HighpassGainLF, freq);
                        ConcreteAudioService.OpenAL.CheckError("Failed to set HighpassGainLF.");
                    break;
                case EfxFilterType.Bandpass:
                    efx.Filter(ConcreteAudioService.Filter, EfxFilterf.BandpassGainHF, freq);
                        ConcreteAudioService.OpenAL.CheckError("Failed to set BandpassGainHF.");
                    efx.Filter(ConcreteAudioService.Filter, EfxFilterf.BandpassGainLF, lf);
                        ConcreteAudioService.OpenAL.CheckError("Failed to set BandpassGainLF.");
                    break;
                }
                ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcei.EfxDirectFilter, ConcreteAudioService.Filter);
                ConcreteAudioService.OpenAL.CheckError("Failed to set DirectFilter.");
            }
        }

        internal override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
            if (!ConcreteAudioService.OpenAL.Efx.IsInitialized)
                return;

            applyFilter = true;
            switch (mode)
            {
            case FilterMode.BandPass:
                filterType = EfxFilterType.Bandpass;
                break;
                case FilterMode.LowPass:
                filterType = EfxFilterType.Lowpass;
                break;
                case FilterMode.HighPass:
                filterType = EfxFilterType.Highpass;
                break;
            }

            this.filterQ = filterQ;
            this.frequency = frequency;

            if (state == SoundState.Playing)
            {
                ApplyFilter();
                applyFilter = false;
            }
        }

        internal override void PlatformClearFilter()
        {
            if (!ConcreteAudioService.OpenAL.Efx.IsInitialized)
                return;

            applyFilter = false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
