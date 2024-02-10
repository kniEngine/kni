// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Platform.Audio
{   
    public sealed partial class AudioService : IDisposable
        , IPlatformAudioService
    {
        private volatile static AudioService _current;
        private AudioServiceStrategy _strategy;

        private LinkedList<SoundEffectInstance> _playingInstances = new LinkedList<SoundEffectInstance>();
        private readonly LinkedList<DynamicSoundEffectInstance> _dynamicPlayingInstances = new LinkedList<DynamicSoundEffectInstance>();

        public readonly static object SyncHandle = new object();

        AudioServiceStrategy IPlatformAudioService.Strategy { get { return _strategy; } }

        public static AudioService Current
        {
            get
            {
                AudioService current = _current;
                if (current != null)
                    return current;

                // Create instance
                lock(SyncHandle)
                {
                    if (_current == null)
                    {   
                        try
                        {
                            _current = new AudioService();
                        }
                        catch (Exception ex)
                        {
                            throw new NoAudioHardwareException("Audio has failed to initialize.", ex);
                        }
                    }
                    return _current;
                }
            }
        }


        internal int MAX_PLAYING_INSTANCES
        {
            get { return _strategy.PlatformGetMaxPlayingInstances(); }
        }

        private AudioService()
        {
            PreserveAudioContentTypeReaders();

            _strategy = AudioFactory.Current.CreateAudioServiceStrategy();
            _strategy.PlatformPopulateCaptureDevices(_microphones, ref _defaultMicrophone);

            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += AudioService.Update;
        }

        // Trick to prevent the linker removing the code, but not actually execute the code
        static bool _trimmingFalseFlag = false;
        private static void PreserveAudioContentTypeReaders()
        {
#pragma warning disable 0219, 0649
            // Trick to prevent the linker removing the code, but not actually execute the code
            if (_trimmingFalseFlag)
            {
                // Dummy variables required for it to work with trimming ** DO NOT DELETE **
                // This forces the classes not to be optimized out when deploying with trimming

                // Framework.Audio types
                SoundEffectReader hSoundEffectReader = new SoundEffectReader();
            }
#pragma warning restore 0219, 0649
        }

        internal static void UpdateMasterVolume()
        {
            if (_current == null) return;

            lock (SyncHandle)
            {
                if (_current != null)
                    _current._UpdateMasterVolume();
            }
        }

        internal static void OnEffectDisposed(SoundEffect effect, bool disposing)
        {
            if (_current == null) return;

            lock (SyncHandle)
            {
                if (_current != null)
                    _current._OnEffectDisposed(effect, disposing);
            }
        }

        internal static void Update()
        {
            if (_current == null) return;

            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current.UpdatePlayingInstances();
                    _current.UpdateMicrophones();
                }
            }

        }

        public static void Suspend()
        {
            if (_current == null) return;

            // Shutdown
            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current._strategy.Suspend();
                }
            }
        }

        public static void Resume()
        {
            if (_current == null) return;

            // Shutdown
            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current._strategy.Resume();
                }
            }
        }

        public static void Shutdown()
        {
            if (_current == null) return;

            // Shutdown
            lock (SyncHandle)
            {
                if (_current != null)
                {
                    _current.Dispose();
                    _current = null;
                }
            }
        }

        private void _UpdateMasterVolume()
        {
            for (var node = _playingInstances.First; node != null; node = node.Next)
            {
                SoundEffectInstance inst = node.Value;

                // XAct sounds are not controlled by the SoundEffect
                // master volume, so we can skip them completely.
                if (inst._isXAct)
                    continue;

                // Re-applying the volume to itself will update
                // the sound with the current master volume.
                inst.Volume = inst.Volume;
            }
        }

        public void SetReverbSettings(ReverbSettings reverbSettings)
        {
            _strategy.PlatformSetReverbSettings(reverbSettings);
        }

        /// <summary>
        /// Iterates the list of playing instances, stop them and return them to the pool if they are instances of the given SoundEffect.
        /// </summary>
        /// <param name="effect">The SoundEffect</param>
        /// <param name="disposing">true if the Effect was disposed. false if it was collected.</param>
        private void _OnEffectDisposed(SoundEffect effect, bool disposing)
        {
            // stop playing instances of the disposed effect
            for (var node = _playingInstances.First; node != null; )
            {
                SoundEffectInstance inst = node.Value;
                node = node.Next;

                if (inst._effect == effect)
                {
                    inst.Stop();
                }
            }

            // remove instances of the disposed effect from _pooledInstances
            for (var node = _pooledInstances.First; node != null;)
            {
                SoundEffectInstance inst = node.Value;
                node = node.Next;

                if (inst._effect == effect)
                {
                    _pooledInstances.Remove(inst.PooledInstancesNode);

                    if (disposing)
                    {
                        inst.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Iterates the list of playing instances, returning them to the pool if they
        /// have stopped playing.
        /// </summary>
        private void UpdatePlayingInstances()
        {
            // Updates buffer queues of the currently playing dynamic instances.
            // XNA posts "DynamicSoundEffectInstance.BufferNeeded" events always on the main thread.
            for (var node = _dynamicPlayingInstances.First; node != null;)
            {
                DynamicSoundEffectInstance inst = node.Value;
                node = node.Next;

                if (!inst.IsDisposed)
                    inst.Update();
            }

            // Cleanup instances which have finished playing.
            for (var node = _playingInstances.First; node != null; )
            {
                SoundEffectInstance inst = node.Value;
                node = node.Next;

                // Don't consume XACT instances... XACT will
                // clear this flag when it is done with the wave.
                if (inst._isXAct)
                    continue;

                System.Diagnostics.Debug.Assert(!inst.IsDisposed);

                inst.UpdateState();
            }
        }

        internal bool Play(SoundEffect effect)
        {
            lock (SyncHandle)
            {
                // is Sounds Available?
                if (!(_playingInstances.Count < MAX_PLAYING_INSTANCES))
                    return false;

                SoundEffectInstance inst = GetInstance(effect);

                inst.Play();
            }

            return true;
        }

        internal bool Play(SoundEffect effect, float volume, float pitch, float pan)
        {
            lock (SyncHandle)
            {
                // is Sounds Available?
                if (!(_playingInstances.Count < MAX_PLAYING_INSTANCES))
                    return false;

                SoundEffectInstance inst = GetInstance(effect);

                inst.Volume = volume;
                inst.Pitch = pitch;
                inst.Pan = pan;

                inst.Play();
            }

            return true;
        }

        internal SoundEffectInstance GetInstance(SoundEffect effect, bool isXAct = false)
        {
            SoundEffectInstance inst = GetPooledInstance(effect, isXAct);
            if (inst == null)
                inst = new SoundEffectInstance(this, effect, true, isXAct);

            return inst;
        }

        internal void AddPlayingInstance(SoundEffectInstance inst)
        {
            if (_playingInstances.Count >= MAX_PLAYING_INSTANCES) // is Sounds Available?
                throw new InstancePlayLimitException();

            _playingInstances.AddLast(inst.PlayingInstancesNode);
        }

        internal void RemovePlayingInstance(SoundEffectInstance inst)
        {
            _playingInstances.Remove(inst.PlayingInstancesNode);
        }

        public void AddDynamicPlayingInstance(DynamicSoundEffectInstance instance)
        {
            _dynamicPlayingInstances.AddLast(instance.DynamicPlayingInstancesNode);
        }

        public void RemoveDynamicPlayingInstance(DynamicSoundEffectInstance instance)
        {
            _dynamicPlayingInstances.Remove(instance.DynamicPlayingInstancesNode);
        }


        /// <summary>
        /// Returns the duration for 16-bit PCM audio.
        /// </summary>
        /// <param name="sizeInBytes">The length of the audio data in bytes.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz). Must be between 8000 Hz and 48000 Hz</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns>The duration of the audio data.</returns>
        internal static TimeSpan GetSampleDuration(int sizeInBytes, int sampleRate, AudioChannels channels)
        {
            if (sizeInBytes < 0)
                throw new ArgumentException("Buffer size cannot be negative.", "sizeInBytes");
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");

            int numChannels = (int)channels;
            if (numChannels != 1 && numChannels != 2)
                throw new ArgumentOutOfRangeException("channels");

            float dur = sizeInBytes / (sampleRate * numChannels * 16f / 8f);

            return TimeSpan.FromSeconds(dur);
        }

        /// <summary>
        /// Returns the data size in bytes for 16bit PCM audio.
        /// </summary>
        /// <param name="duration">The total duration of the audio data.</param>
        /// <param name="sampleRate">Sample rate, in Hertz (Hz), of audio data. Must be between 8,000 and 48,000 Hz.</param>
        /// <param name="channels">Number of channels in the audio data.</param>
        /// <returns>The size in bytes of a single sample of audio data.</returns>
        internal static int GetSampleSizeInBytes(TimeSpan duration, int sampleRate, AudioChannels channels)
        {
            if (duration < TimeSpan.Zero || duration > TimeSpan.FromMilliseconds(0x7FFFFFF))
                throw new ArgumentOutOfRangeException("duration");
            if (sampleRate < 8000 || sampleRate > 48000)
                throw new ArgumentOutOfRangeException("sampleRate");

            int numChannels = (int)channels;
            if (numChannels != 1 && numChannels != 2)
                throw new ArgumentOutOfRangeException("channels");

            return (int)(duration.TotalSeconds * (sampleRate * numChannels * 16f / 8f));
        }


        #region IDisposable

        private bool isDisposed = false;
        public event EventHandler Disposing;

        ~AudioService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (isDisposed)
                    return;

                // stop playing instances of the disposed AudioSystem
                for (var node = _playingInstances.First; node != null;)
                {
                    SoundEffectInstance inst = node.Value;
                    node = node.Next;

                    inst.Stop();
                }

                var handler = Disposing;
                if (handler != null)
                    handler(this, EventArgs.Empty);

                // dispose pooled instances
                for (var node = _pooledInstances.First; node != null;)
                {
                    SoundEffectInstance inst = node.Value;
                    node = node.Next;

                    inst.Dispose();
                }

                // stop all running microphones
                StopMicrophones();

                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= AudioService.Update;

                _strategy.Dispose();
                _strategy = null;

                // free unmanaged resources (unmanaged objects)
                _playingInstances.Clear();
                _pooledInstances.Clear();

                // set large fields to null.
                _playingInstances = null;
                _pooledInstances = null;

                isDisposed = true;
            }
            else
            {
                if (isDisposed)
                    return;

                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= AudioService.Update;

                // stop all running microphones
                StopMicrophones();
                
                _strategy = null;

                // free unmanaged resources (unmanaged objects)
                _playingInstances.Clear();
                _pooledInstances.Clear();
                _microphones.Clear();

                // set large fields to null.
                _playingInstances = null;
                _pooledInstances = null;
                _microphones = null;
                _defaultMicrophone = null;

                isDisposed = true;
            }
        }


        #endregion // IDisposable
    }
}

