// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Audio;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a single instance of a playing, paused, or stopped sound.</summary>
    /// <remarks>
    /// <para>SoundEffectInstances are created through SoundEffect.CreateInstance() and used internally by SoundEffect.Play()</para>
    /// </remarks>
    public class SoundEffectInstance : IDisposable
    {
        internal AudioService _audioService { get; private set; }
        internal SoundEffectInstanceStrategy _strategy;

        internal LinkedListNode<SoundEffectInstance> PlayingInstancesNode { get; private set; }
        internal LinkedListNode<SoundEffectInstance> PooledInstancesNode { get; private set; }

        private SoundState _state = SoundState.Stopped;
        private bool _isLooped = false;
        private bool _isDisposed = false;
        internal bool _isXAct;
        internal SoundEffect _effect;
        private float _pan;
        private float _volume;
        private float _pitch;


        /// <summary>Enables or Disables whether the SoundEffectInstance should repeat after playback.</summary>
        /// <remarks>This value has no effect on an already playing sound.</remarks>
        public virtual bool IsLooped
        { 
            get { return _isLooped; }
            set
            {
                if (value == _isLooped) return;

                //   XNA will throw an InvalidOperationException if you change 'IsLooped'
                // after the first call to 'Play()'
                //   If the audio platform can't release or resume loop while a sound is playing
                // it's preferred to throw an exception in 'PlatformSetIsLooped()'

                _strategy.PlatformSetIsLooped(value, State);
                _isLooped = value;
            }
        }

        /// <summary>Gets or sets the pan, or speaker balance..</summary>
        /// <value>Pan value ranging from -1.0 (left speaker) to 0.0 (centered), 1.0 (right speaker). Values outside of this range will throw an exception.</value>
        /// <remarks>In OpenAL Panning/3D works only with mono sounds.</remarks>
        public float Pan
        {
            get { return _pan; } 
            set
            {
                if (value < -1.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                _pan = value;
                _strategy.PlatformSetPan(value);
            }
        }

        /// <summary>Gets or sets the pitch adjustment.</summary>
        /// <value>Pitch adjustment, ranging from -1.0 (down an octave) to 0.0 (no change) to 1.0 (up an octave). Values outside of this range will throw an Exception.</value>
        public float Pitch
        {
            get { return _pitch; }
            set
            {
                // XAct sounds effects don't have pitch limits
                if (!_isXAct && (value < -1.0f || value > 1.0f))
                    throw new ArgumentOutOfRangeException();

                _pitch = value;
                _strategy.PlatformSetPitch(_pitch);
            }
        }

        /// <summary>Gets or sets the volume of the SoundEffectInstance.</summary>
        /// <value>Volume, ranging from 0.0 (silence) to 1.0 (full volume). Volume during playback is scaled by SoundEffect.MasterVolume.</value>
        /// <remarks>
        /// This is the volume relative to SoundEffect.MasterVolume. Before playback, this Volume property is multiplied by SoundEffect.MasterVolume when determining the final mix volume.
        /// </remarks>
        public float Volume
        {
            get { return _volume; }
            set
            {
                // XAct sound effects don't have volume limits.
                if (!_isXAct && (value < 0.0f || value > 1.0f))
                    throw new ArgumentOutOfRangeException();

                _volume = value;

                // XAct sound effects are not tied to the SoundEffect master volume.
                if (_isXAct)
                    _strategy.PlatformSetVolume(value);
                else
                    _strategy.PlatformSetVolume(value * SoundEffect.MasterVolume);
            }
        }

        /// <summary>Gets the SoundEffectInstance's current playback state.</summary>
        public virtual SoundState State
        {
            get
            {
                lock (AudioService.SyncHandle)
                {
                    UpdateState();
                    return _state;
                }
            }
        }

        /// <summary>Indicates whether the object is disposed.</summary>
        public bool IsDisposed { get { return _isDisposed; } }

        internal SoundEffectInstance(AudioService audioService)
        {
            System.Diagnostics.Debug.Assert(audioService != null);

            _audioService = audioService;
            _effect = null;
            _isXAct = false;
            PlayingInstancesNode = new LinkedListNode<SoundEffectInstance>(this);
            PooledInstancesNode = null;

            _pan = 0.0f;
            _volume = 1.0f;
            _pitch = 0.0f;
        }

        internal SoundEffectInstance(AudioService audioService, SoundEffect effect, bool isPooled = false, bool isXAct = false)
        {
            System.Diagnostics.Debug.Assert(audioService != null);
            System.Diagnostics.Debug.Assert(effect != null);

            _audioService = audioService;
            _effect = effect;
            _isXAct = isXAct;
            PlayingInstancesNode = new LinkedListNode<SoundEffectInstance>(this);
            PooledInstancesNode = (isPooled) ? new LinkedListNode<SoundEffectInstance>(this) : null;

            _pan = 0.0f;
            _volume = 1.0f;
            _pitch = 0.0f;

            _strategy = ((IPlatformAudioService)audioService).Strategy.CreateSoundEffectInstanceStrategy(_effect._strategy);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="Microsoft.Xna.Framework.Audio.SoundEffectInstance"/> is reclaimed by garbage collection.
        /// </summary>
        ~SoundEffectInstance()
        {
            Dispose(false);
        }

        /// <summary>Applies 3D positioning to the SoundEffectInstance using a single listener.</summary>
        /// <param name="listener">Data about the listener.</param>
        /// <param name="emitter">Data about the source of emission.</param>
        public void Apply3D(AudioListener listener, AudioEmitter emitter)
        {
            _strategy.PlatformApply3D(listener, emitter);
        }

        /// <summary>Applies 3D positioning to the SoundEffectInstance using multiple listeners.</summary>
        /// <param name="listeners">Data about each listener.</param>
        /// <param name="emitter">Data about the source of emission.</param>
        /// <remarks>In OpenAL Panning/3D works only with mono sounds.</remarks>
        public void Apply3D(AudioListener[] listeners, AudioEmitter emitter)
        {
            foreach (AudioListener listener in listeners)
                _strategy.PlatformApply3D(listener, emitter);
        }

        /// <summary>Pauses playback of a SoundEffectInstance.</summary>
        /// <remarks>Paused instances can be resumed with SoundEffectInstance.Play() or SoundEffectInstance.Resume().</remarks>
        public virtual void Pause()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = State;
                switch (state)
                {
                    case SoundState.Paused:
                        return;
                    case SoundState.Stopped:
                        return;
                    case SoundState.Playing:
                        {
                            _strategy.PlatformPause();
                            _state = SoundState.Paused;
                        }
                        return;
                }
            }
        }

        /// <summary>Plays or resumes a SoundEffectInstance.</summary>
        /// <remarks>Throws an exception if more sounds are playing than the platform allows.</remarks>
        public virtual void Play()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = State;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Paused:
                        Resume();
                        return;
                    case SoundState.Stopped:
                        {
                            // For non-XAct sounds we need to be sure the latest
                            // master volume level is applied before playback.
                            if (!_isXAct)
                                _strategy.PlatformSetVolume(_volume * SoundEffect.MasterVolume);

                            _strategy.PlatformPlay(_isLooped);
                            _state = SoundState.Playing;
                            _audioService.AddPlayingInstance(this);
                        }
                        return;
                }
            }
        }

        /// <summary>Resumes playback for a SoundEffectInstance.</summary>
        // <remarks>Only has effect on a SoundEffectInstance in a paused state.</remarks>
        // In XNA 'Resume()' behaves exactly like 'Play()'.
        public virtual void Resume()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = State;
                switch (state)
                {
                    case SoundState.Playing:
                        return;
                    case SoundState.Stopped:
                        Play();
                        return;
                    case SoundState.Paused:
                        {
                            _strategy.PlatformResume(_isLooped);
                            _state = SoundState.Playing;
                        }
                        return;
                }
            }
        }

        /// <summary>Immediately stops playing a SoundEffectInstance.</summary>
        public virtual void Stop()
        {
            lock (AudioService.SyncHandle)
            {
                AssertNotDisposed();

                SoundState state = State;
                switch (state)
                {
                    case SoundState.Stopped:
                        return;
                    case SoundState.Paused:
                    case SoundState.Playing:
                        {
                            _strategy.PlatformStop();
                            _state = SoundState.Stopped;
                            _audioService.RemovePlayingInstance(this);
                        }
                        return;
                }
            }
        }

        /// <summary>Stops playing a SoundEffectInstance, either immediately or as authored.</summary>
        /// <param name="immediate">Determined whether the sound stops immediately, or after playing its release phase and/or transitions.</param>
        /// <remarks>Stopping a sound with the immediate argument set to false will allow it to play any release phases, such as fade, before coming to a stop.</remarks>
        public virtual void Stop(bool immediate)
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
                        return;
                    case SoundState.Paused:
                    case SoundState.Playing:
                        _strategy.PlatformRelease(_isLooped);
                        return;
                }
            }
        }

        internal void UpdateState()
        {
            if (_strategy.PlatformUpdateState(ref _state))
            {
                if (_state == SoundState.Stopped)
                {
                    _audioService.RemovePlayingInstance(this);
                    _audioService.AddPooledInstance(this);
                }
            }
        }

        internal void SetReverbMix(float reverbMix)
        {
            _strategy.PlatformSetReverbMix(State, reverbMix, _pan);
        }
        
        internal void SetFilter(FilterMode mode, float filterQ, float frequency)
        {
            _strategy.PlatformSetFilter(State, mode, filterQ, frequency);
        }

        /// <summary>
        /// Reset used instance to the "default" state.
        /// </summary>
        internal void Reset()
        {
            Volume = 1.0f;
            Pan = 0.0f;
            Pitch = 0.0f;
            IsLooped = false;
            SetReverbMix(0);
            _strategy.PlatformClearFilter();
        }

        /// <summary>Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffectInstance"/>.</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the resources held by this <see cref="Microsoft.Xna.Framework.Audio.SoundEffectInstance"/>.
        /// </summary>
        /// <param name="disposing">If set to <c>true</c>, Dispose was called explicitly.</param>
        /// <remarks>If the disposing parameter is true, the Dispose method was called explicitly. This
        /// means that managed objects referenced by this instance should be disposed or released as
        /// required.  If the disposing parameter is false, Dispose was called by the finalizer and
        /// no managed objects should be touched because we do not know if they are still valid or
        /// not at that time.  Unmanaged resources should always be released.</remarks>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (AudioService.SyncHandle)
                {
                    if (_isDisposed)
                        return;

                    if (_strategy != null)
                    {
                        Stop();
                        _strategy.Dispose();
                        _strategy = null;
                    }

                    _audioService = null;
                    _effect = null;
                    PlayingInstancesNode = null;
                    PooledInstancesNode = null;

                    _isDisposed = true;
                }
            }
            else
            {
                lock (AudioService.SyncHandle)
                {
                    if (_isDisposed)
                        return;

                    if (_strategy != null)
                    { 
                        Stop();
                        _strategy = null;
                    }

                    _audioService = null;
                    _effect = null;
                    PlayingInstancesNode = null;
                    PooledInstancesNode = null;

                    _isDisposed = true;
                }
            }
        }

        private void AssertNotDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException("SoundEffectInstance");
        }
    }
}
