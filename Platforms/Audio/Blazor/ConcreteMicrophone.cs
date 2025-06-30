// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;
using nkast.Wasm.Dom;
using nkast.Wasm.Media;

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Provides microphones capture features.
    /// </summary>
    public sealed class ConcreteMicrophone : MicrophoneStrategy
    {
        AudioContext _ac;
        private CancellationTokenSource _micInitCts;
        MediaStream _micStream;
        MediaStreamSourceNode _micNode;
        AudioWorkletNode _micWorkletNode;

        public override TimeSpan BufferDuration
        {
            get { return base.BufferDuration; }
            set { base.BufferDuration = value; }
        }

        public override MicrophoneState State
        {
            get { return base.State; }
            set { base.State = value; }
        }


        internal ConcreteMicrophone()
            : base()
        {
            _ac = new AudioContext();
            base.SampleRate = _ac.SampleRate;
        }

        public override void PlatformStart(string deviceName)
        {
            InitMicrophoneDeviceAsync();
        }

        public override void PlatformStop()
        {
            ReleaseMicrophoneDevice();
        }

        public override bool PlatformIsHeadset()
        {
            throw new NotImplementedException();
        }

        public override bool PlatformUpdate()
        {
            throw new NotImplementedException();
        }

        public override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        private async Task InitMicrophoneDeviceAsync()
        {
            _micInitCts = new CancellationTokenSource();
            CancellationToken token = _micInitCts.Token;

            try
            {
                // init micProcessor AudioWorklet
                await _ac.AudioWorklet.AddModuleAsync("js/micProcessor.js");
                if (token.IsCancellationRequested) return;

                _micWorkletNode = _ac.CreateWorklet("mic-processor");

                // init and start Microphone
                MediaDevices md = MediaDevices.FromNavigator(Window.Current.Navigator);
                _micStream = await md.GetUserMediaAsync(new UserMediaConstraints() { Audio = true });
                if (token.IsCancellationRequested) return;
                _micNode = _ac.CreateMediaStreamSource(_micStream);
                _micNode.Connect(_micWorkletNode);
            }
            catch (Exception ex)
            {
            }
        }

        private void ReleaseMicrophoneDevice()
        {
            if (_micInitCts != null)
            {
                _micInitCts.Cancel();
                _micInitCts = null;
            }

            if (_micNode != null)
            {
                _micNode.Disconnect(_micWorkletNode);
                _micNode.Dispose();
                _micNode = null;
            }

            if (_micStream != null)
            {
                _micStream.Dispose();
                _micStream = null;
            }

            if (_micWorkletNode != null)
            {
                _micWorkletNode.Dispose();
                _micWorkletNode = null;
            }
        }
    }
}
