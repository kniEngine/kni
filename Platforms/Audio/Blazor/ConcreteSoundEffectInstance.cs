// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;
using AudioListener = Microsoft.Xna.Framework.Audio.AudioListener;
using WebAudioListener = nkast.Wasm.Audio.AudioListener;

namespace Microsoft.Xna.Platform.Audio
{
    public class ConcreteSoundEffectInstance : SoundEffectInstanceStrategy
    {
        private AudioServiceStrategy _audioServiceStrategy;
        private ConcreteSoundEffect _concreteSoundEffect;
        internal ConcreteAudioService ConcreteAudioService { get { return (ConcreteAudioService)_audioServiceStrategy; } }

        AudioBufferSourceNode _bufferSource;
        bool _ended;
        StereoPannerNode _stereoPannerNode;
        GainNode _gainNode;
        AudioNode _sourceTarget;

        float _pan = 1f;
        float _volume = 1f;

        public override bool IsXAct
        {
            get { return base.IsXAct; }
            set { base.IsXAct = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override float Pan
        {
            get { return base.Pan; }
            set { base.Pan = value; }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set { base.Volume = value; }
        }

        public override float Pitch
        {
            get { return base.Pitch; }
            set { base.Pitch = value; }
        }

        #region Initialization

        internal ConcreteSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy)
            : base(audioServiceStrategy, sfxStrategy)
        {
            _audioServiceStrategy = audioServiceStrategy;
            _concreteSoundEffect = (ConcreteSoundEffect)sfxStrategy;

            AudioContext context = ConcreteAudioService.Context;

            _stereoPannerNode = ConcreteAudioService.Context.CreateStereoPanner();
            _gainNode = ConcreteAudioService.Context.CreateGain();

            _sourceTarget = context.Destination;
            //_stereoPannerNode.Connect(_sourceTarget); _sourceTarget = _stereoPannerNode;
            _gainNode.Connect(context.Destination); _sourceTarget = _gainNode;

        }

        #endregion // Initialization

        public override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
        }

        public override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        public override void PlatformPlay(bool isLooped)
        {
            AudioContext context = ConcreteAudioService.Context;

            _bufferSource = context.CreateBufferSource();
            _bufferSource.Loop = isLooped;
            _bufferSource.Buffer = _concreteSoundEffect.GetAudioBuffer();
            _bufferSource.Connect(_sourceTarget);

            _gainNode.Gain.SetTargetAtTime(_volume, 0, 0);
            _stereoPannerNode.Pan.SetTargetAtTime(_pan, 0, 0);

            _bufferSource.OnEnded += _bufferSource_OnEnded;
            _bufferSource.Start();
        }

        public override void PlatformResume(bool isLooped)
        {
            throw new NotImplementedException();
        }

        public override void PlatformStop()
        {
            AudioContext context = ConcreteAudioService.Context;

            _bufferSource.OnEnded -= _bufferSource_OnEnded;
            _ended = false;

            _bufferSource.Stop();
            _bufferSource.Disconnect(_sourceTarget);
            _bufferSource.Dispose();
            _bufferSource = null;
        }

        public override void PlatformRelease(bool isLooped)
        {
        }

        public override bool PlatformUpdateState(ref SoundState state)
        {
            if (state != SoundState.Stopped && _ended)
            {
                _ended = false;
                state = SoundState.Stopped;

                _bufferSource.Disconnect(_sourceTarget);
                _bufferSource.Dispose();
                _bufferSource = null;

                return true;
            }

            return false;
        }

        public override void PlatformSetIsLooped(bool isLooped, SoundState state)
        {
            if (_bufferSource != null)
            {
                _bufferSource.Loop = isLooped;
            }
        }

        public override void PlatformSetPan(float pan)
        {
            _pan = pan;
            if (_bufferSource != null)
                _stereoPannerNode.Pan.SetTargetAtTime(pan, 0, 0.05f);
        }

        public override void PlatformSetPitch(float pitch)
        {
        }

        public override void PlatformSetVolume(float volume)
        {
            _volume = volume;
            if (_bufferSource != null)
                _gainNode.Gain.SetTargetAtTime(volume, 0, 0.05f);
        }

        public override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
        }

        public override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
        }

        public override void PlatformClearFilter()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bufferSource != null)
                    _bufferSource.Dispose();

                _gainNode.Dispose();
                _stereoPannerNode.Dispose();
            }

            _bufferSource = null;
            _gainNode = null;
            _stereoPannerNode = null;
        }

        private void _bufferSource_OnEnded(object sender, EventArgs e)
        {
            _ended = true;
        }
    }
}
