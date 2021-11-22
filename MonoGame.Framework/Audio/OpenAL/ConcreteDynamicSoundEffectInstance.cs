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

        public event EventHandler<EventArgs> OnBufferNeeded;

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
            AL.SourcePause(_sourceId);
            ALHelper.CheckError("Failed to pause the source.");
        }

        internal override void PlatformPlay(bool isLooped)
        {
            // Ensure that the source is not looped (due to source recycling)
            AL.Source(_sourceId, ALSourceb.Looping, false);
            ALHelper.CheckError("Failed to set source loop state.");

            AL.SourcePlay(_sourceId);
            ALHelper.CheckError("Failed to play the source.");
        }

        internal override void PlatformResume(bool isLooped)
        {
            AL.SourcePlay(_sourceId);
            ALHelper.CheckError("Failed to play the source.");
        }

        internal override void PlatformStop()
        {
            AL.SourceStop(_sourceId);
            ALHelper.CheckError("Failed to stop the source.");

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
            int alBuffer = AL.GenBuffer();
            ALHelper.CheckError("Failed to generate OpenAL data buffer.");
            
            // Bind the data
            AL.BufferData(alBuffer, _format, buffer, offset, count, _sampleRate, 0);
            ALHelper.CheckError("Failed to fill buffer.");

            // Queue the buffer
            _queuedBuffers.Enqueue(alBuffer);
            AL.SourceQueueBuffer(_sourceId, alBuffer);
            ALHelper.CheckError("Failed to queue the buffer.");

            // If the source has run out of buffers, restart it
            var sourceState = AL.GetSourceState(_sourceId);
            if (state == SoundState.Playing && sourceState == ALSourceState.Stopped)
            {
                AL.SourcePlay(_sourceId);
                ALHelper.CheckError("Failed to resume source playback.");
            }
        }

        public void DynamicPlatformClearBuffers()
        {
            // detach buffers
            AL.Source(_sourceId, ALSourcei.Buffer, 0);
            ALHelper.CheckError("Failed to unbind buffers from source.");

            // Remove all queued buffers
            while (_queuedBuffers.Count > 0)
            {
                int buffer = _queuedBuffers.Dequeue();
                AL.DeleteBuffer(buffer);
                ALHelper.CheckError("Failed to delete buffer.");
            }
        }

        public void DynamicPlatformUpdateBuffers()
        {
            // Get the processed buffers
            int processedBuffers;
            AL.GetSource(_sourceId, ALGetSourcei.BuffersProcessed, out processedBuffers);
            ALHelper.CheckError("Failed to get processed buffer count.");

            // Unqueue and release buffers
            if (processedBuffers > 0)
            {
                AL.SourceUnqueueBuffers(_sourceId, processedBuffers);
                ALHelper.CheckError("Failed to unqueue buffers.");
                for (int i = 0; i < processedBuffers; i++)
                {
                    var buffer = _queuedBuffers.Dequeue();
                    AL.DeleteBuffer(buffer);
                    ALHelper.CheckError("Failed to delete buffer.");
                }
            }

            // Raise the event for each removed buffer
            var handler = OnBufferNeeded;
            if (handler != null)
            {
               for (int i = 0; i < processedBuffers; i++)
                   handler(this, EventArgs.Empty);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            var sourceState = AL.GetSourceState(_sourceId);
            ALHelper.CheckError("Failed to get state.");
            if (sourceState != ALSourceState.Stopped)
            {
                AL.SourceStop(_sourceId);
                ALHelper.CheckError("Failed to stop source.");
            }

            // detach buffers
            AL.Source(_sourceId, ALSourcei.Buffer, 0);
            ALHelper.CheckError("Failed to unbind buffer from source.");

            // Remove all queued buffers
            while (_queuedBuffers.Count > 0)
            {
                var buffer = _queuedBuffers.Dequeue();
                AL.DeleteBuffer(buffer);
                ALHelper.CheckError("Failed to delete buffer.");
            }

            ConcreteAudioService.RecycleSource(_sourceId);
            _sourceId = 0;

            //base.Dispose(disposing);
        }

    }
}
