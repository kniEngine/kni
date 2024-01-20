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
        private ALFormat _format;
        private Queue<int> _queuedBuffers = new Queue<int>();

        private readonly WeakReference _dynamicSoundEffectInstanceRef = new WeakReference(null);
        DynamicSoundEffectInstance IDynamicSoundEffectInstanceStrategy.DynamicSoundEffectInstance
        {
            get { return _dynamicSoundEffectInstanceRef.Target as DynamicSoundEffectInstance; }
            set { _dynamicSoundEffectInstanceRef.Target = value; }
        }

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels, float pan)
            : base(audioServiceStrategy, null, pan)
        {
            _sampleRate = sampleRate;
            _channels = channels;

            _format = channels == 1 ? ALFormat.Mono16 : ALFormat.Stereo16;

            _sourceId = ConcreteAudioService.ReserveSource();
        }

        public int DynamicPlatformGetPendingBufferCount()
        {
            return _queuedBuffers.Count;
        }

        internal override void PlatformPause()
        {
            ConcreteAudioService.OpenAL.SourcePause(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to pause the source.");
        }

        internal override void PlatformPlay(bool isLooped)
        {
            // Ensure that the source is not looped (due to source recycling)
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourceb.Looping, false);
            ConcreteAudioService.OpenAL.CheckError("Failed to set source loop state.");

            ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to play the source.");
        }

        internal override void PlatformResume(bool isLooped)
        {
            ConcreteAudioService.OpenAL.SourcePlay(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to play the source.");
        }

        internal override void PlatformStop()
        {
            ConcreteAudioService.OpenAL.SourceStop(_sourceId);
            ConcreteAudioService.OpenAL.CheckError("Failed to stop the source.");

            DynamicPlatformClearBuffers();
        }

        internal override void PlatformRelease(bool isLooped)
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
            ConcreteAudioService.OpenAL.BufferData(alBuffer, _format, buffer, offset, count, _sampleRate, 0);
            ConcreteAudioService.OpenAL.CheckError("Failed to fill buffer.");

            // Queue the buffer
            _queuedBuffers.Enqueue(alBuffer);
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

        public void DynamicPlatformClearBuffers()
        {
            // detach buffers
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcei.Buffer, 0);
            ConcreteAudioService.OpenAL.CheckError("Failed to unbind buffers from source.");

            // Remove all queued buffers
            while (_queuedBuffers.Count > 0)
            {
                int buffer = _queuedBuffers.Dequeue();
                ConcreteAudioService.OpenAL.DeleteBuffer(buffer);
                ConcreteAudioService.OpenAL.CheckError("Failed to delete buffer.");
            }
        }

        public void DynamicPlatformUpdateBuffers()
        {
            // Get the processed buffers
            int processedBuffers;
            ConcreteAudioService.OpenAL.GetSource(_sourceId, ALGetSourcei.BuffersProcessed, out processedBuffers);
            ConcreteAudioService.OpenAL.CheckError("Failed to get processed buffer count.");

            // Unqueue and release buffers
            if (processedBuffers > 0)
            {
                ConcreteAudioService.OpenAL.SourceUnqueueBuffers(_sourceId, processedBuffers);
                ConcreteAudioService.OpenAL.CheckError("Failed to unqueue buffers.");
                for (int i = 0; i < processedBuffers; i++)
                {
                    int buffer = _queuedBuffers.Dequeue();
                    ConcreteAudioService.OpenAL.DeleteBuffer(buffer);
                    ConcreteAudioService.OpenAL.CheckError("Failed to delete buffer.");
                }
            }

            // Raise the event for each removed buffer
            if (_dynamicSoundEffectInstanceRef.Target != null)
            {
                DynamicSoundEffectInstance instance = (DynamicSoundEffectInstance)_dynamicSoundEffectInstanceRef.Target;
                for (int i = 0; i < processedBuffers; i++)
                    instance._dstrategy_OnBufferNeeded();
            }
        }

        protected override void Dispose(bool disposing)
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

            // detach buffers
            ConcreteAudioService.OpenAL.Source(_sourceId, ALSourcei.Buffer, 0);
            ConcreteAudioService.OpenAL.CheckError("Failed to unbind buffer from source.");

            // Remove all queued buffers
            while (_queuedBuffers.Count > 0)
            {
                int buffer = _queuedBuffers.Dequeue();
                ConcreteAudioService.OpenAL.DeleteBuffer(buffer);
                ConcreteAudioService.OpenAL.CheckError("Failed to delete buffer.");
            }

            ConcreteAudioService.RecycleSource(_sourceId);
            _sourceId = 0;

            //base.Dispose(disposing);
        }

    }
}
