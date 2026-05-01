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
        protected StereoPannerNode _stereoPannerNode;
        protected GainNode _gainNode;
        protected AudioNode _sourceTarget;

        private bool _started;
        private bool _paused;
        private bool _ended;
        private double _offset;
        private double _lastCurrentTime;

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
            set
            {
                base.Pan = value;

                if (_stereoPannerNode != null)
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

                if (_gainNode != null)
                {
                    _gainNode.Gain.SetTargetAtTime(value * masterVolume, 0, 0.05f);
                }
            }
        }

        public override float Pitch
        {
            get { return base.Pitch; }
            set
            {
                base.Pitch = value;

                if (_bufferSource != null)
                {
                    float wapitch = (float)Math.Pow(2, value);
                    //TODO: implement Pitch
                    //_bufferSource.PlaybackRate.SetTargetAtTime(wapitch, 0, 0.05f);
                }
            }
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
            _stereoPannerNode.Connect(_sourceTarget); _sourceTarget = _stereoPannerNode;
            _gainNode.Connect(_sourceTarget); _sourceTarget = _gainNode;

            this.Volume = 1.0f;
            this.Pan = 0.0f;
            this.Pitch = 0.0f;
        }

        #endregion // Initialization

        public override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
        }

        public override void PlatformPause()
        {
            SourceNodeStop(pause: true);
        }

        public override void PlatformPlay(bool isLooped)
        {
            SourceNodeStart(offset: 0d);
        }

        public override void PlatformResume(bool isLooped)
        {
            SourceNodeStart(offset: _offset);
        }

        public override void PlatformStop()
        {
            SourceNodeStop(pause: false);
        }

        public override void PlatformRelease(bool isLooped)
        {
        }

        public override bool PlatformUpdateState(ref SoundState state)
        {
            UpdateOffset();

            if (state == SoundState.Playing && _ended)
            {
                SourceNodeStop(pause: false);
                state = SoundState.Stopped;
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

        public override void PlatformSetReverbMix(SoundState state, float mix, float pan)
        {
        }

        public override void PlatformSetFilter(SoundState state, FilterMode mode, float filterQ, float frequency)
        {
        }

        public override void PlatformClearFilter()
        {
        }

        private void SourceNodeStart(double offset)
        {
            AudioContext context = ConcreteAudioService.Context;
            AudioBuffer audioBuffer = _concreteSoundEffect.GetAudioBuffer();

            _bufferSource = context.CreateBufferSource();
            _bufferSource.Loop = IsLooped;
            _bufferSource.Buffer = audioBuffer;
            _bufferSource.OnEnded += _bufferSource_OnEnded;
            _bufferSource.Connect(_sourceTarget);

            _offset = offset;
            if (IsLooped)
                _offset %= audioBuffer.Duration;

            if (_offset == 0)
                _bufferSource.Start();
            else
                _bufferSource.Start(0, _offset);

            _started = true;
            _paused = false;
            _lastCurrentTime = context.CurrentTime;
        }

        private void SourceNodeStop(bool pause)
        {
            if (pause)
                UpdateOffset();

            _bufferSource.OnEnded -= _bufferSource_OnEnded;

            if (!_ended)
                _bufferSource.Stop();

            _bufferSource.Disconnect(_sourceTarget);
            _bufferSource.Dispose();
            _bufferSource = null;

            if (pause)
            {
                _paused = true;
            }
            else
            {
                _started = false;
                _paused = false;
            }
            _ended = false;
        }

        private void UpdateOffset()
        {
            if (!_started || _paused || _ended)
                return;

            AudioContext context = ConcreteAudioService.Context;

            double currentTime = context.CurrentTime;
            if (currentTime == _lastCurrentTime)
                return;

            double elapsedRealTime = currentTime - _lastCurrentTime;
            double elapsedBufferTime = elapsedRealTime * _bufferSource.PlaybackRate.Value;
            _offset += elapsedBufferTime;
            _lastCurrentTime = currentTime;
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
