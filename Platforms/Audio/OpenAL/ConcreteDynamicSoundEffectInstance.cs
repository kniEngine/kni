// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Platform.Audio.OpenAL;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {
        private int _sampleRate;
        private int _channels;
        private ALFormat _alFormat;
        private int _queuedBuffersCount;

        private readonly WeakReference _dynamicSoundEffectInstanceRef = new WeakReference(null);
        DynamicSoundEffectInstance IDynamicSoundEffectInstanceStrategy.DynamicSoundEffectInstance
        {
            get { return _dynamicSoundEffectInstanceRef.Target as DynamicSoundEffectInstance; }
            set { _dynamicSoundEffectInstanceRef.Target = value; }
        }

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels)
            : base(audioServiceStrategy, null)
        {
            _sampleRate = sampleRate;
            _channels = channels;

            _alFormat = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;

            _sourceId = ConcreteAudioService.ReserveSource();
        }

        public int BuffersNeeded { get; set; }

        public int DynamicPlatformGetPendingBufferCount()
        {
            return _queuedBuffersCount;
        }

        public override void PlatformPause()
        {
            ConcreteAudioService.OpenAL.SourcePause(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to pause the source.");
        }

        public override void PlatformPlay(bool isLooped)
        {
            // Ensure that the source is not looped (due to source recycling)
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourceb.Looping, false);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source loop state.");

            ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to play the source.");
        }

        public override void PlatformResume(bool isLooped)
        {
            ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to play the source.");
        }

        public override void PlatformStop()
        {
            ConcreteAudioService.OpenAL.SourceStop(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to stop the source.");

            DynamicPlatformClearBuffers();
        }

        public override void PlatformRelease(bool isLooped)
        {
            System.Diagnostics.Debug.Assert(isLooped == false);

            // TODO: remove queued buffers except for the current one that is still playing.
            throw new NotImplementedException();
        }

        public void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state)
        {
            // Get a buffer
            int alBuffer = ConcreteAudioService.OpenAL.GenBuffer();
            ConcreteAudioService.OpenAL.CheckError("Failed to generate OpenAL data buffer.");
            
            // Bind the data
            ConcreteAudioService.OpenAL.BufferData(alBuffer, _alFormat, buffer, offset, count, _sampleRate, 0);
            ConcreteAudioService.OpenAL.CheckError("Failed to fill buffer.");

            // Queue the buffer
            _queuedBuffersCount++;
            ConcreteAudioService.OpenAL.SourceQueueBuffer(_sourceId, alBuffer);
            ConcreteAudioService.OpenAL.CheckError("Failed to queue the buffer.");

            // If the source has run out of buffers, restart it
            ALSourceState sourceState = ConcreteAudioService.OpenAL.GetSourceState(_sourceId);
            if (state == SoundState.Playing && sourceState == ALSourceState.Stopped)
            {
                ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
                ConcreteAudioService.OpenAL.CheckError("Failed to resume source playback.");
            }
        }

        public unsafe void DynamicPlatformClearBuffers()
        {
            // Get the queued buffers
            int queuedBuffers;
            ConcreteAudioService.OpenAL.GetSource(_sourceId, ALGetSourcei.BuffersQueued, out queuedBuffers);
            ConcreteAudioService.OpenAL.CheckError("Failed to get processed buffer count.");

            // Remove all queued buffers
            if (queuedBuffers > 0)
            {
                int* pQueuedBuffers = stackalloc int[queuedBuffers];
                ConcreteAudioService.OpenAL.alSourceUnqueueBuffers(_sourceId, queuedBuffers, pQueuedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to unqueue buffers.");

                ConcreteAudioService.OpenAL.alDeleteBuffers(queuedBuffers, pQueuedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to delete buffers.");

                _queuedBuffersCount-= queuedBuffers;
            }
        }

        public unsafe void DynamicPlatformUpdateBuffers()
        {
            // Get the processed buffers
            int processedBuffers;
            ConcreteAudioService.OpenAL.GetSource(_sourceId, ALGetSourcei.BuffersProcessed, out processedBuffers);
            ConcreteAudioService.OpenAL.CheckError("Failed to get processed buffer count.");

            // Unqueue and release buffers
            if (processedBuffers > 0)
            {
                int* pProcessedBuffers = stackalloc int[processedBuffers];
                ConcreteAudioService.OpenAL.alSourceUnqueueBuffers(_sourceId, processedBuffers, pProcessedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to unqueue buffers.");

                ConcreteAudioService.OpenAL.alDeleteBuffers(processedBuffers, pProcessedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to delete buffers.");

                _queuedBuffersCount-= processedBuffers;

                for (int i = 0; i < processedBuffers; i++)
                {
                    int bufferId = pProcessedBuffers[i];
                }
            }

            // Raise the event for each removed buffer
            this.BuffersNeeded+= processedBuffers;
        }

        protected unsafe override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            ALSourceState sourceState = ConcreteAudioService.OpenAL.GetSourceState(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to get state.");
            if (sourceState != ALSourceState.Stopped)
            {
                ConcreteAudioService.OpenAL.SourceStop(_sourceId);
                ConcreteAudioService.OpenAL.CheckError("Failed to stop source.");
            }
            // Get the queued buffers
            int queuedBuffers;
            ConcreteAudioService.OpenAL.GetSource(_sourceId, ALGetSourcei.BuffersQueued, out queuedBuffers);
            ConcreteAudioService.OpenAL.CheckError("Failed to get processed buffer count.");

            // Remove all queued buffers
            if (queuedBuffers > 0)
            {
                int* pQueuedBuffers = stackalloc int[queuedBuffers];
                ConcreteAudioService.OpenAL.alSourceUnqueueBuffers(_sourceId, queuedBuffers, pQueuedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to unqueue buffers.");

                ConcreteAudioService.OpenAL.alDeleteBuffers(queuedBuffers, pQueuedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to delete buffers.");
                
                _queuedBuffersCount-= queuedBuffers;
            }

            ConcreteAudioService.RecycleSource(_sourceId);
            _sourceId = 0;

            //base.Dispose(disposing);
        }

    }
}
