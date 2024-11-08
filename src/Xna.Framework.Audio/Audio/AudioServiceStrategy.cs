// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    public interface IPlatformAudioService
    {
        AudioServiceStrategy Strategy { get; }
    }

    abstract public class AudioServiceStrategy : IDisposable
    {
        // factory methods
        public abstract SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy _strategy);
        public abstract IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, int channels);

        public abstract void Suspend();
        public abstract void Resume();
        public abstract int PlatformGetMaxPlayingInstances();
        public abstract void PlatformSetReverbSettings(ReverbSettings reverbSettings);

        public abstract void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone);

        public T ToConcrete<T>() where T : AudioServiceStrategy
        {
            return (T)this;
        }

        protected Microphone CreateMicrophone(string deviceIdentifier)
        {
            return new Microphone(deviceIdentifier);
        }

        #region IDisposable
        ~AudioServiceStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

    abstract public class MicrophoneStrategy
    {
        private int _sampleRate = 44100;
        private TimeSpan _bufferDuration = TimeSpan.FromMilliseconds(1000.0);

        public int SampleRate { get { return _sampleRate; } }

        public TimeSpan BufferDuration
        {
            get { return _bufferDuration; }
            set { _bufferDuration = value; }
        }


        public abstract void PlatformStart(string deviceName);
        public abstract void PlatformStop();
        public abstract bool PlatformUpdate();
        public abstract bool PlatformIsHeadset();
        public abstract int PlatformGetData(byte[] buffer, int offset, int count);


        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return AudioService.GetSampleDuration(sizeInBytes, _sampleRate, AudioChannels.Mono);
        }

        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            // this should be 10ms aligned
            // this assumes 16bit mono data
            return AudioService.GetSampleSizeInBytes(duration, _sampleRate, AudioChannels.Mono);
        }
    }

    abstract public class SoundEffectStrategy : IDisposable
    {
        public abstract void PlatformLoadAudioStream(Stream stream, out TimeSpan duration);

        public abstract void PlatformInitializeFormat(byte[] header, byte[] buffer, int index, int count, int loopStart, int loopLength);
        public abstract void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength);
        public abstract void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength);


        #region IDisposable
        ~SoundEffectStrategy()
        {
           Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);            
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

    abstract public class SoundEffectInstanceStrategy : IDisposable
    {
        protected SoundEffectInstanceStrategy(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy) { }

        public abstract void PlatformApply3D(AudioListener listener, AudioEmitter emitter);
        public abstract void PlatformSetIsLooped(bool isLooped, SoundState state);
        public abstract void PlatformSetPan(float pan);
        public abstract void PlatformSetPitch(float pitch);
        public abstract void PlatformSetVolume(float volume);
        public abstract bool PlatformUpdateState(ref SoundState state);

        public abstract void PlatformPause();
        public abstract void PlatformPlay(bool isLooped);
        public abstract void PlatformResume(bool isLooped);
        public abstract void PlatformStop();

        public abstract void PlatformRelease(bool isLooped);

        public abstract void PlatformSetReverbMix(SoundState state, float mix, float pan);
        public abstract void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency);
        public abstract void PlatformClearFilter();

        #region IDisposable
        ~SoundEffectInstanceStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }

    public interface IDynamicSoundEffectInstanceStrategy
    {
        DynamicSoundEffectInstance DynamicSoundEffectInstance { get; set; }

        int BuffersNeeded { get; set; }

        void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state);
        void DynamicPlatformUpdateBuffers();
        void DynamicPlatformClearBuffers();
        int DynamicPlatformGetPendingBufferCount();
    }
}
