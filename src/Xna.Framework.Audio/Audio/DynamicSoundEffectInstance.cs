// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// A <see cref="SoundEffectInstance"/> for which the audio buffer is provided by the game at run time.
    /// </summary>
    public sealed class DynamicSoundEffectInstance : SoundEffectInstance
    {
        private IDynamicSoundEffectInstanceStrategy _dynamicStrategy;

        private const int TargetPendingBufferCount = 3;
        private bool _initialBuffersNeeded;
        
        private int _sampleRate;
        private AudioChannels _channels;
        private SoundState _dynamicState = SoundState.Stopped;

        internal LinkedListNode<DynamicSoundEffectInstance> DynamicPlayingInstancesNode { get; private set; }

        #region Public Properties

        /// <summary>
        /// This value has no effect on DynamicSoundEffectInstance.
        /// It may not be set.
        /// </summary>
        public override bool IsLooped
        {
            get { return false; }
            set
            {
                AssertNotDisposed();
                if (value == true)
                    throw new InvalidOperationException("IsLooped cannot be set true. Submit looped audio data to implement looping.");
            }
        }

        public override SoundState State
        {
            get
            {
                AssertNotDisposed();
                return _dynamicState;
            }
        }

        /// <summary>
        /// Returns the number of audio buffers queued for playback.
        /// </summary>
        public int PendingBufferCount
        {
            get
            {
                AssertNotDisposed();
                return _dynamicStrategy.DynamicPlatformGetPendingBufferCount();
            }
        }

        /// <summary>
        /// The event that occurs when the number of queued audio buffers is less than or equal to 2.
        /// </summary>
        /// <remarks>
        /// This event may occur when <see cref="Play()"/> is called or during playback when a buffer is completed.
        /// </remarks>
        public event EventHandler<EventArgs> BufferNeeded;

        #endregion

        #region Public Constructor

        /// <param name="sampleRate">Sample rate, in Hertz (Hz).</param>
        /// <param name="channels">Number of channels (mono or stereo).</param>
        public DynamicSoundEffectInstance(int sampleRate, AudioChannels channels) 
            : base(AudioService.Current)
        {
            if ((sampleRate < 8000) || (sampleRate > 48000))
                throw new ArgumentOutOfRangeException("sampleRate");
            if ((channels != AudioChannels.Mono) && (channels != AudioChannels.Stereo))
                throw new ArgumentOutOfRangeException("channels");
            
            _sampleRate = sampleRate;
            _channels = channels;

            // This instance is added to the pool so that its volume reflects master volume changes
            // and it contributes to the playing instances limit, but the source/voice is not owned by the pool.
            DynamicPlayingInstancesNode = new LinkedListNode<DynamicSoundEffectInstance>(this);

            _dynamicStrategy = ((IPlatformAudioService)_audioService).Strategy.CreateDynamicSoundEffectInstanceStrategy(_sampleRate, (int)_channels);
            _strategy = (SoundEffectInstanceStrategy)_dynamicStrategy;
            _dynamicStrategy.DynamicSoundEffectInstance = this;
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Returns the duration of an audio buffer of the specified size, based on the settings of this instance.
        /// </summary>
        /// <param name="sizeInBytes">Size of the buffer, in bytes.</param>
        /// <returns>The playback length of the buffer.</returns>
        public TimeSpan GetSampleDuration(int sizeInBytes)
        {
            AssertNotDisposed();
            return AudioService.GetSampleDuration(sizeInBytes, _sampleRate, _channels);
        }

        /// <summary>
        /// Returns the size, in bytes, of a buffer of the specified duration, based on the settings of this instance.
        /// </summary>
        /// <param name="duration">The playback length of the buffer.</param>
        /// <returns>The data size of the buffer, in bytes.</returns>
        public int GetSampleSizeInBytes(TimeSpan duration)
        {
            AssertNotDisposed();
            return AudioService.GetSampleSizeInBytes(duration, _sampleRate, _channels);
        }

        /// <summary>
        /// Pauses playback of the DynamicSoundEffectInstance.
        /// </summary>
        public override void Pause()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = _dynamicState;
                switch (state)
                {
                    case SoundState.Paused:
                        return;
                    case SoundState.Stopped:
                        return;
                    case SoundState.Playing:
                        {
                            _strategy.PlatformPause();
                            _dynamicState = SoundState.Paused;
                            _initialBuffersNeeded = false;
                        }
                        return;
                }
            }
        }

        /// <summary>
        /// Plays or resumes the DynamicSoundEffectInstance.
        /// </summary>
        public override void Play()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = _dynamicState;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Paused:
                        Resume();
                        return;
                    case SoundState.Stopped:
                        {
                            // Ensure that the volume reflects master volume, which is done by the setter.
                            Volume = Volume;

                            _strategy.PlatformPlay(IsLooped);
                            _dynamicState = SoundState.Playing;
                            _initialBuffersNeeded = true;

                            _audioService.AddPlayingInstance(this);
                            _audioService.AddDynamicPlayingInstance(this);
                        }
                        return;
                }               
            }
        }

        /// <summary>
        /// Resumes playback of the DynamicSoundEffectInstance.
        /// </summary>
        public override void Resume()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = _dynamicState;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Stopped:
                        Play();
                        return;
                    case SoundState.Paused:
                        {
                            Volume = Volume;

                            _strategy.PlatformResume(IsLooped);
                            _dynamicState = SoundState.Playing;
                        }
                        return;
                }
            }
        }

        /// <summary>
        /// Immediately stops playing the DynamicSoundEffectInstance.
        /// </summary>
        /// <remarks>
        /// Calling this also releases all queued buffers.
        /// </remarks>
        public override void Stop()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = _dynamicState;
                switch (state)
                {
                    case SoundState.Stopped:
                        {
                            _dynamicStrategy.DynamicPlatformClearBuffers();
                        }
                        return;
                    case SoundState.Paused:
                    case SoundState.Playing:
                        {
                            _strategy.PlatformStop();
                            _dynamicState = SoundState.Stopped;

                            _dynamicStrategy.DynamicPlatformClearBuffers();
                            _initialBuffersNeeded = false;

                            _audioService.RemovePlayingInstance(this);
                            _audioService.RemoveDynamicPlayingInstance(this);
                        }
                        return;
                }
            }
        }

        /// <summary>
        /// Stops playing the DynamicSoundEffectInstance.
        /// </summary>
        /// <remarks>
        /// Calling this also releases all queued buffers.
        /// </remarks>
        public override void Stop(bool immediate)
        {
            if (immediate)
            {
                Stop();
                return;
            }
            
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = State;
                switch (state)
                {
                    case SoundState.Stopped:
                        {
                            _dynamicStrategy.DynamicPlatformClearBuffers();
                        }
                        return;
                    case SoundState.Paused:
                    case SoundState.Playing:
                        {
                            System.Diagnostics.Debug.Assert(IsLooped == false);
                            _strategy.PlatformRelease(IsLooped);
                        }
                        return;
                }
            }
        }

        /// <summary>
        /// Queues an audio buffer for playback.
        /// </summary>
        /// <remarks>
        /// The buffer length must conform to alignment requirements for the audio format.
        /// </remarks>
        /// <param name="buffer">The buffer containing PCM audio data.</param>
        public void SubmitBuffer(byte[] buffer)
        {
            SubmitBuffer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Queues an audio buffer for playback.
        /// </summary>
        /// <remarks>
        /// The buffer length must conform to alignment requirements for the audio format.
        /// </remarks>
        /// <param name="buffer">The buffer containing PCM audio data.</param>
        /// <param name="offset">The starting position of audio data.</param>
        /// <param name="count">The amount of bytes to use.</param>
        public void SubmitBuffer(byte[] buffer, int offset, int count)
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                if ((buffer == null) || (buffer.Length == 0))
                    throw new ArgumentException("Buffer may not be null or empty.");
                if (count <= 0)
                    throw new ArgumentException("Number of bytes must be greater than zero.");
                if ((offset + count) > buffer.Length)
                    throw new ArgumentException("Buffer is shorter than the specified number of bytes from the offset.");

                // Ensure that the buffer length and start position match alignment.
                int sampleSize = 2 * (int)_channels;
                if (count % sampleSize != 0)
                    throw new ArgumentException("Number of bytes does not match format alignment.");
                if (offset % sampleSize != 0)
                    throw new ArgumentException("Offset into the buffer does not match format alignment.");

                if (PendingBufferCount >= 64)
                    throw new InvalidOperationException("Buffers Limit Exceeded.");

                _dynamicStrategy.DynamicPlatformSubmitBuffer(buffer, offset, count, _dynamicState);
            }
        }

        #endregion

        #region Nonpublic Functions

        internal void Update()
        {
            lock (AudioService.SyncHandle)
            {
                // Update the buffers
                _dynamicStrategy.DynamicPlatformUpdateBuffers();

                if (_initialBuffersNeeded)
                    _dynamicStrategy.BuffersNeeded = Math.Max(_dynamicStrategy.BuffersNeeded, TargetPendingBufferCount - 1 - PendingBufferCount);

                // Raise the event
                var bufferNeededHandler = BufferNeeded;
                if (bufferNeededHandler != null)
                {
                    // raise the event for each processed buffer
                    while(_dynamicStrategy.BuffersNeeded-- != 0)
                    {
                        bufferNeededHandler(this, EventArgs.Empty);
                        if (this.IsDisposed)
                            return;
                    }

                    if (State == SoundState.Playing && PendingBufferCount < TargetPendingBufferCount)
                        bufferNeededHandler(this, EventArgs.Empty);
                }

                _initialBuffersNeeded = true;
                _dynamicStrategy.BuffersNeeded = 0;
            }
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("DynamicSoundEffectInstance");
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                if (_dynamicStrategy !=null)
                    _dynamicStrategy.DynamicSoundEffectInstance = null;
                base.Dispose(disposing);
                _dynamicStrategy = null;
                DynamicPlayingInstancesNode = null;
            }
            else
            {
                if (_dynamicStrategy != null)
                    _dynamicStrategy.DynamicSoundEffectInstance = null;
                base.Dispose(disposing);
                _dynamicStrategy = null;
                DynamicPlayingInstancesNode = null;
            }
        }

        #endregion
    }
}
