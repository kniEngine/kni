// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;
using System;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {
        private int _sampleRate;
        private int _channels;

        private StreamingAudioWorkletNode _dynamicSoundEffectNode;

        float _volume = 1f;

        public override float Pan
        {
            get { return base.Pan; }
            set
            {
                base.Pan = value;

                if (_dynamicSoundEffectNode != null)
                {
                    _stereoPannerNode.Pan.SetTargetAtTime(value, 0, 0.05f);
                }
            }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;

                // XAct sound effects are not tied to the SoundEffect master volume.
                float masterVolume = (!this.IsXAct) ? SoundEffect.MasterVolume : 1f;
                _volume = value * masterVolume;

                if (_dynamicSoundEffectNode != null)
                {
                    _gainNode.Gain.SetTargetAtTime(value * masterVolume, 0, 0.05f);
                }
            }
        }

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
        }

        public int BuffersNeeded { get; set; }

        public int DynamicPlatformGetPendingBufferCount()
        {
            return _dynamicSoundEffectNode?.QueuedBufferCount ?? int.MaxValue;
        }

        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformPlay(bool isLooped)
        {
            AudioContext context = ConcreteAudioService.Context;

            if (!context.IsInitialized)
            {
                throw new InvalidOperationException("AudioContext is not initialized yet for DynamicSoundEffect playback.");
            }

            _dynamicSoundEffectNode = context.CreateStreamingAudioWorkletNode(_channels);
            _dynamicSoundEffectNode.Connect(_sourceTarget);

            _gainNode.Gain.SetTargetAtTime(_volume, 0, 0);
            _stereoPannerNode.Pan.SetTargetAtTime(base.Pan, 0, 0);
        }

        public override void PlatformResume(bool isLooped)
        {
            throw new NotImplementedException();
        }

        public override void PlatformStop()
        {
            _dynamicSoundEffectNode.Stop();
            _dynamicSoundEffectNode.Disconnect(_sourceTarget);
            _dynamicSoundEffectNode.Dispose();
            _dynamicSoundEffectNode = null;
        }

        public override void PlatformRelease(bool isLooped)
        {
            System.Diagnostics.Debug.Assert(isLooped == false);

            throw new NotImplementedException();
        }

        public void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state)
        {
            if (!ConcreteAudioService.Context.IsInitialized || _dynamicSoundEffectNode == null)
            {
                return;
            }

            unsafe
            {
                fixed (void* pBuffer = buffer)
                {
                    short* pBuffer16 = (short*)pBuffer;
                    int destLength = count / 2;
                    var dest = new float[destLength];
                    
                    for (int i = 0; i < destLength; i++)
                    {
                        dest[i] = ((float)pBuffer16[i] / (float)short.MaxValue) * 2 - 1;
                    }

                    _dynamicSoundEffectNode.SubmitBuffer(dest);
                }
            }
        }

        public void DynamicPlatformClearBuffers()
        {
            if (!ConcreteAudioService.Context.IsInitialized || _dynamicSoundEffectNode == null)
            {
                return;
            }

            _dynamicSoundEffectNode.ClearBuffers();
        }

        public void DynamicPlatformUpdateBuffers()
        {
            this.BuffersNeeded = Math.Max(0, 2 - DynamicPlatformGetPendingBufferCount());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_dynamicSoundEffectNode != null)
                    _dynamicSoundEffectNode.Dispose();
            }

            _dynamicSoundEffectNode = null;
            base.Dispose(disposing);
        }
    }
}
