// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;
using AudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

namespace Microsoft.Xna.Platform.Audio
{
    public class ConcreteSoundEffectInstance : SoundEffectInstanceStrategy
    {
        private AudioServiceStrategy _audioServiceStrategy;
        private ConcreteSoundEffect _concreteSoundEffect;
        internal ConcreteAudioService ConcreteAudioService { get { return (ConcreteAudioService)_audioServiceStrategy; } }

        private AudioBufferSourceNode _bufferSource;
        protected PannerNode _pannerNode;
        protected StereoPannerNode _stereoPannerNode;
        protected GainNode _gainNode;
        protected AudioNode _sourceTarget;

        private bool _started;
        private bool _paused;
        private bool _stopping;
        private bool _ended;
        private double _offset;
        private double _lastCurrentTime;
        protected float _dopplerEffect;

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
                SetPlaybackRate();
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

            _dopplerEffect = 1.0f;

            this.Volume = 1.0f;
            this.Pan = 0.0f;
            this.Pitch = 0.0f;
        }

        #endregion // Initialization

        public override void PlatformApply3D(AudioListener listener, AudioEmitter emitter)
        {
            if (_pannerNode == null)
            {
                AudioContext context = ConcreteAudioService.Context;

                _stereoPannerNode.Disconnect(context.Destination);

                _pannerNode = context.CreatePanner();
                _pannerNode.Connect(context.Destination);

                _gainNode.Disconnect(_stereoPannerNode);
                _gainNode.Connect(_pannerNode);

                _pannerNode.PanningModel = PanningModelType.EqualPower;
                _pannerNode.DistanceModel = DistanceModelType.Inverse;
                _pannerNode.RolloffFactor = 1f;
                _pannerNode.MaxDistance = double.MaxValue;
            }

            Matrix worldSpaceToListenerSpace = Matrix.Invert(Matrix.CreateWorld(listener.Position, listener.Forward, listener.Up));
            Vector3 relativePosition = Vector3.Transform(emitter.Position, worldSpaceToListenerSpace);

            _pannerNode.RefDistance = SoundEffect.DistanceScale;

            _pannerNode.PositionX.SetValueAtTime(relativePosition.X, 0f);
            _pannerNode.PositionY.SetValueAtTime(relativePosition.Y, 0f);
            _pannerNode.PositionZ.SetValueAtTime(relativePosition.Z, 0f);

            Vector3 direction = (relativePosition != Vector3.Zero) ? Vector3.Normalize(relativePosition) : Vector3.Zero;
            float listenerRadialVelocity = listener.Velocity.X * direction.X + listener.Velocity.Y * direction.Y + listener.Velocity.Z * direction.Z;
            float emitterRadialVelocity = emitter.Velocity.X * direction.X + emitter.Velocity.Y * direction.Y + emitter.Velocity.Z * direction.Z;

            float unscaledDopplerEffect = Math.Max((SoundEffect.SpeedOfSound + listenerRadialVelocity), 0.0001f) /
                Math.Max((SoundEffect.SpeedOfSound + emitterRadialVelocity), 0.0001f);
            _dopplerEffect = 1f + ((unscaledDopplerEffect - 1f) * SoundEffect.DopplerScale * emitter.DopplerScale);

            SetPlaybackRate();
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
            _stopping = true;
            _bufferSource.Loop = false;
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
            if (_bufferSource != null && !_stopping)
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

            SetPlaybackRate();

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
                _stopping = false;
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

        protected virtual void SetPlaybackRate()
        {
            if (_bufferSource == null)
                return;

            UpdateOffset();

            float playbackRate = (float)Math.Pow(2, base.Pitch) * _dopplerEffect;
            playbackRate = MathHelper.Clamp(playbackRate, 0.5f, 2.0f);
            _bufferSource.PlaybackRate.SetValueAtTime(playbackRate, 0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_bufferSource != null)
                    _bufferSource.Dispose();

                _gainNode.Dispose();
                _stereoPannerNode.Dispose();

                if (_pannerNode != null)
                    _pannerNode.Dispose();
            }

            _bufferSource = null;
            _gainNode = null;
            _stereoPannerNode = null;
            _pannerNode = null;
        }

        private void _bufferSource_OnEnded(object sender, EventArgs e)
        {
            _ended = true;
        }
    }
}
