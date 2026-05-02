// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;
using nkast.Wasm.ChannelMessaging;

namespace Microsoft.Xna.Platform.Audio
{
    public sealed class ConcreteDynamicSoundEffectInstance : ConcreteSoundEffectInstance
        , IDynamicSoundEffectInstanceStrategy
    {
        private int _sampleRate;
        private int _channels;

        AudioWorkletNode _streamSource;
        private bool _isStreamSourceInitialized;
        private int _pendingBuffers;

        private enum PortMessageType
        {
            None = 0,
            BufferProcessed = 1,
            ClearBuffers = 2,
            Pause = 3,
            Resume = 4
        }

        private readonly WeakReference _dynamicSoundEffectInstanceRef = new WeakReference(null);
        DynamicSoundEffectInstance IDynamicSoundEffectInstanceStrategy.DynamicSoundEffectInstance
        {
            get { return _dynamicSoundEffectInstanceRef.Target as DynamicSoundEffectInstance; }
            set { _dynamicSoundEffectInstanceRef.Target = value; }
        }

        public override float Pitch
        {
            get { return base.Pitch; }
            set
            {
                if (value != 0)
                    throw new NotSupportedException("DynamicSoundEffectInstance does not support Pitch.");

                base.Pitch = value;
            }
        }

        internal ConcreteDynamicSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, int sampleRate, int channels)
            : base(audioServiceStrategy, null)
        {
            _sampleRate = sampleRate;
            _channels = channels;

            AudioContext context = ConcreteAudioService.Context;

            // TODO: implement resampling.
            if (_sampleRate != context.SampleRate)
                throw new NotImplementedException($"Sample rate {_sampleRate} does not match AudioContext sample rate {context.SampleRate}.");
            // TODO: implement Stereo.
            if (_channels != 1)
                throw new NotImplementedException($"Channels {_channels} is not implemented.");
        }

        public int BuffersNeeded { get; set; }

        public int DynamicPlatformGetPendingBufferCount()
        {
            return _tmpBuffers.Count + _pendingBuffers;
        }

        public override void PlatformPause()
        {
            _streamSource.Port.PostMessage((int)PortMessageType.Pause);
        }

        public override void PlatformPlay(bool isLooped)
        {
            AudioContext context = ConcreteAudioService.Context;

            float masterVolume = SoundEffect.MasterVolume;
            _gainNode.Gain.SetTargetAtTime(this.Volume * masterVolume, 0, 0);
            _stereoPannerNode.Pan.SetTargetAtTime(this.Pan, 0, 0);

            InitStreamSourceAsync();
        }

        private async Task InitStreamSourceAsync()
        {
            AudioContext context = ConcreteAudioService.Context;

            if (context.State == ContextState.Suspended)
                await context.ResumeAsync();

            if (!ConcreteAudioService.IsDynamicSoundModuleInitialized)
            {
            await context.AudioWorklet.AddModuleAsync("js/streamProcessor.js");
                ConcreteAudioService.IsDynamicSoundModuleInitialized = true;
            }

            _streamSource.Port.Message += StreamSource_OnMessage;
            _streamSource.Connect(_sourceTarget);

            _isStreamSourceInitialized = true;

            // Submit any pending buffers that were added before the AudioWorklet was initialized.
            while (_tmpBuffers.Count > 0)
            {
                byte[] tmpBuffer = _tmpBuffers.Dequeue();
                _streamSource.Port.PostMessage(tmpBuffer, 0, tmpBuffer.Length);
                _pendingBuffers++;
            }
        }

        private void StreamSource_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.DataByteArray != null)
            {
                
            }
            else
            {
                var msg = e.DataFloat64;
                
                if (msg == (int)PortMessageType.BufferProcessed)
                {
                    _pendingBuffers--;
                }
            }
        }

        public override void PlatformResume(bool isLooped)
        {
            _streamSource.Port.PostMessage((int)PortMessageType.Resume);
        }

        public override void PlatformStop()
        {
            if (_streamSource != null)
            {
                _streamSource.Disconnect(_sourceTarget);
                _streamSource.Dispose();
                _streamSource = null;
        }
            _isStreamSourceInitialized = false;
            _pendingBuffers = 0;
            _tmpBuffers.Clear();
        }

        public override void PlatformRelease(bool isLooped)
        {
            System.Diagnostics.Debug.Assert(isLooped == false);

            throw new NotImplementedException();
        }

        private readonly Queue<byte[]> _tmpBuffers = new Queue<byte[]>();

        public void DynamicPlatformSubmitBuffer(byte[] buffer, int offset, int count, SoundState state)
        {
            if (_isStreamSourceInitialized == false)
            {
                // store pending buffers until the AudioWorklet is initialized
                byte[] tmpBuffer = new byte[count];
                Buffer.BlockCopy(buffer, offset, tmpBuffer, 0, count);
                _tmpBuffers.Enqueue(tmpBuffer);
            }
            else
            {
                _streamSource.Port.PostMessage(buffer, offset, count);
                _pendingBuffers++;
            }
        }

        public void DynamicPlatformUpdateBuffers()
        {
        }

        public void DynamicPlatformClearBuffers()
        {
            _pendingBuffers = 0;
            _tmpBuffers.Clear();

            if (_streamSource != null)
                _streamSource.Port.PostMessage((int)PortMessageType.ClearBuffers);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
