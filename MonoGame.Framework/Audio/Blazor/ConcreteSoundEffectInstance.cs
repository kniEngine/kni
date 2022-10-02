﻿// MonoGame - Copyright (C) The MonoGame Team
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

        #region Initialization

        internal ConcreteSoundEffectInstance(AudioServiceStrategy audioServiceStrategy, SoundEffectStrategy sfxStrategy, float pan)
            : base(audioServiceStrategy, sfxStrategy, pan)
        {
            _audioServiceStrategy = audioServiceStrategy;
            _concreteSoundEffect = (ConcreteSoundEffect)sfxStrategy;

            var context = ConcreteAudioService.Context;

            _stereoPannerNode = ConcreteAudioService.Context.CreateStereoPanner();
            _gainNode = ConcreteAudioService.Context.CreateGain();

            _sourceTarget = context.Destination;
            //_stereoPannerNode.Connect(_sourceTarget); _sourceTarget = _stereoPannerNode;
            _gainNode.Connect(context.Destination); _sourceTarget = _gainNode;

        }

        #endregion // Initialization

        internal override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
        }

        internal override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformPlay(bool isLooped)
        {
            var context = ConcreteAudioService.Context;

            _bufferSource = context.CreateBufferSource();
            _bufferSource.Loop = isLooped;
            _bufferSource.Buffer = _concreteSoundEffect.GetAudioBuffer();
            _bufferSource.Connect(_sourceTarget);

            _gainNode.Gain.SetTargetAtTime(_volume, 0, 0);
            _stereoPannerNode.Pan.SetTargetAtTime(_pan, 0, 0);

            _bufferSource.OnEnded += _bufferSource_OnEnded;
            _bufferSource.Start();
        }

        internal override void PlatformResume(bool isLooped)
        {
            throw new NotImplementedException();
        }

        internal override void PlatformStop()
        {
            var context = ConcreteAudioService.Context;

            _bufferSource.OnEnded -= _bufferSource_OnEnded;
            _ended = false;

            _bufferSource.Stop();
            _bufferSource.Disconnect(_sourceTarget);
            _bufferSource.Dispose();
            _bufferSource = null;
        }

        internal override void PlatformRelease(bool isLooped)
        {
        }

        internal override bool PlatformUpdateState(ref SoundState state)
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

        internal override void PlatformSetIsLooped(bool isLooped, SoundState state)
        {
            if (_bufferSource != null)
            {
                _bufferSource.Loop = isLooped;
            }
        }

        internal override void PlatformSetPan(float pan)
        {
            _pan = pan;
            if (_bufferSource != null)
                _stereoPannerNode.Pan.SetTargetAtTime(pan, 0, 0.05f);
        }

        internal override void PlatformSetPitch(float pitch)
        {
        }

        internal override void PlatformSetVolume(float volume)
        {
            _volume = volume;
            if (_bufferSource != null)
                _gainNode.Gain.SetTargetAtTime(volume, 0, 0.05f);
        }

        internal override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
        }

        internal override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
        }

        internal override void PlatformClearFilter()
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
