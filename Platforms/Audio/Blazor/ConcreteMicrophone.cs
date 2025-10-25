// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using nkast.Wasm.Audio;
using nkast.Wasm.ChannelMessaging;
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

        byte[] _dataBuffer;
        private int _dataBufferBegin = 0;
        private int _dataBufferEnd = 0;


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
            using (AudioContext ac = new AudioContext())
            {
                base.SampleRate = ac.SampleRate;

                int bufferSize = base.SampleRate * 1 * sizeof(short) * 2;
                _dataBuffer = new byte[bufferSize];
            }
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
            if (_dataBufferEnd != _dataBufferBegin)
                return true;

            return false;
        }

        public override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            int availableDataSize = (_dataBufferEnd - _dataBufferBegin);
            if (availableDataSize < 0)
                availableDataSize = availableDataSize + _dataBuffer.Length;

            count = Math.Min(count, availableDataSize);
            if (count > 0)
            {
                int firstBlockSize = Math.Min(count, _dataBuffer.Length - _dataBufferBegin);
                Buffer.BlockCopy(_dataBuffer, _dataBufferBegin, buffer, offset, firstBlockSize);
                if (count > firstBlockSize)
                    Buffer.BlockCopy(_dataBuffer, 0, buffer, offset + firstBlockSize, (count - firstBlockSize));
                _dataBufferBegin = (_dataBufferBegin + count) % _dataBuffer.Length;
            }

            return count;
        }

        private void OnMicMessage(object sender, MessageEventArgs e)
        {
            if (e.DataByteArray != null)
            {
                JSUInt8Array data = e.DataByteArray;

                int count = data.Count;

                if (count > _dataBuffer.Length / 2)
                    throw new InvalidOperationException("_dataBuffer must be at least twice as large as data length.");

                if (count <= (_dataBuffer.Length - _dataBufferEnd))
                {
                    data.CopyTo(0, _dataBuffer, _dataBufferEnd, count);
                }
                else
                {
                    int firstBlockSize = (_dataBuffer.Length - _dataBufferEnd);
                    data.CopyTo(0, _dataBuffer, _dataBufferEnd, firstBlockSize);
                    data.CopyTo(firstBlockSize, _dataBuffer, 0, count - firstBlockSize);
                }
                _dataBufferEnd = (_dataBufferEnd + count) % _dataBuffer.Length;

                data.Dispose();
            }
            else
            {
                double msg = e.DataFloat64;

            }
        }

        private async Task InitMicrophoneDeviceAsync()
        {
            _micInitCts = new CancellationTokenSource();
            CancellationToken token = _micInitCts.Token;

            try
            {
                _ac = new AudioContext();

                // init micProcessor AudioWorklet
                await _ac.AudioWorklet.AddModuleAsync("js/micProcessor.js");
                if (token.IsCancellationRequested) return;

                _micWorkletNode = _ac.CreateWorklet("mic-processor");
                _micWorkletNode.Port.Message += OnMicMessage;

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
                _micWorkletNode.Port.Message-= OnMicMessage;
                _micWorkletNode.Dispose();
                _micWorkletNode = null;
            }

            if (_ac != null)
            {
                _ac.Dispose();
                _ac = null;
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
