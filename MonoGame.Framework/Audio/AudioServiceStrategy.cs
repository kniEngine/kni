// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{
    abstract public class AudioServiceStrategy : IDisposable
    {
        // factory methods
        internal abstract SoundEffectInstanceStrategy CreateSoundEffectInstanceStrategy(SoundEffectStrategy _strategy, float _pan);
        internal abstract IDynamicSoundEffectInstanceStrategy CreateDynamicSoundEffectInstanceStrategy(int sampleRate, int channels, float pan);

        internal abstract int PlatformGetMaxPlayingInstances();
        internal abstract void PlatformSetReverbSettings(ReverbSettings reverbSettings);

        internal abstract void PlatformPopulateCaptureDevices(List<Microphone> microphones, ref Microphone defaultMicrophone);
        

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


        internal abstract void PlatformStart(string deviceName);
        internal abstract void PlatformStop();
        internal abstract bool PlatformUpdate();
        internal abstract bool PlatformIsHeadset();
        internal abstract int PlatformGetData(byte[] buffer, int offset, int count);


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
        internal abstract void PlatformLoadAudioStream(Stream stream, out TimeSpan duration);
        internal abstract void PlatformInitializePcm(byte[] buffer, int index, int count, int sampleBits, int sampleRate, int channels, int loopStart, int loopLength);
        internal abstract void PlatformInitializeXactAdpcm(byte[] buffer, int index, int count, int channels, int sampleRate, int blockAlignment, int loopStart, int loopLength);
        internal abstract void PlatformInitializeFormat(byte[] header, byte[] buffer, int index, int count, int loopStart, int loopLength);

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
        internal SoundEffectInstanceStrategy(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy, float pan) { }

        internal abstract void PlatformApply3D(AudioListener listener, AudioEmitter emitter);
        internal abstract void PlatformSetIsLooped(bool isLooped, SoundState state);
        internal abstract void PlatformSetPan(float pan);
        internal abstract void PlatformSetPitch(float pitch);
        internal abstract void PlatformSetVolume(float volume);
        internal abstract bool PlatformUpdateState(ref SoundState state);
        
        internal abstract void PlatformPause();
        internal abstract void PlatformPlay(bool isLooped);
        internal abstract void PlatformResume(bool isLooped);
        internal abstract void PlatformStop();

        internal abstract void PlatformRelease(bool isLooped);

        internal abstract void PlatformSetReverbMix(SoundState state, float mix, float pan);
        internal abstract void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency);
        internal abstract void PlatformClearFilter();

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

        void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state);
        void DynamicPlatformUpdateBuffers();
        void DynamicPlatformClearBuffers();
        int DynamicPlatformGetPendingBufferCount();
    }
}
