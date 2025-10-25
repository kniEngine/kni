// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Audio;
using Android.Media;
using Android.Runtime;

namespace Microsoft.Xna.Platform.Audio
{
    /// <summary>
    /// Provides microphones capture features.
    /// </summary>
    public sealed class ConcreteMicrophoneDroid : MicrophoneStrategy
    {
        private AudioRecord _audioRecord;
        private int _bufferSize;

        private const ChannelIn ChannelConfig = ChannelIn.Mono;
        private const Encoding AudioFormat = Encoding.Pcm16bit;
        private const int AUDIORECORD_ERROR_BAD_VALUE = -2;
        private const int AUDIORECORD_ERROR = -1;


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

        public ConcreteMicrophoneDroid() : base()
        {
        }

        public override void PlatformStart(string deviceName)
        {
#if !XAMARIN
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
            {
                // Calculate the buffer size
                int minBufferSize = AudioRecord.GetMinBufferSize(SampleRate, ChannelConfig, AudioFormat);
                if (minBufferSize == AUDIORECORD_ERROR_BAD_VALUE || minBufferSize == AUDIORECORD_ERROR)
                    throw new InvalidOperationException("GetMinBufferSize failed with error "+ minBufferSize + ".");
                
                _bufferSize = base.GetSampleSizeInBytes(base.BufferDuration);
                if (_bufferSize <= minBufferSize)
                    throw new Exception("Invalid buffer size for AudioRecord!");

                if (deviceName == "DefaultMicrophone")
                {
                    // Create AudioRecord instance
                    _audioRecord = new AudioRecord(
                        AudioSource.Mic, // Microphone as the audio source
                        SampleRate,
                        ChannelConfig,
                        AudioFormat,
                        _bufferSize * 2
                    );
                }
                else
                {
                    throw new NotImplementedException();
                }

                if (_audioRecord.State != Android.Media.State.Initialized)
                    throw new Exception("AudioRecord initialization failed!");

                _audioRecord.SetNotificationMarkerPosition(_bufferSize);
                _audioRecord.MarkerReached += _audioRecord_MarkerReached;

                _audioRecord.StartRecording();
            }
            else
            {
                throw new InvalidOperationException();
            }
#endif
        }

        public override void PlatformStop()
        {
            _audioRecord.MarkerReached -= _audioRecord_MarkerReached;
            _audioRecord.Stop();
            _audioRecord.Release();
        }

        public override bool PlatformIsHeadset()
        {
            throw new NotImplementedException();
        }

        int _notifications = 0;
        private void _audioRecord_MarkerReached(object sender, AudioRecord.MarkerReachedEventArgs e)
        {
            AudioRecord audioRecord = (AudioRecord)sender;

            _audioRecord.SetNotificationMarkerPosition(_bufferSize);

            _notifications++;
            return;
        }

        public override bool PlatformUpdate()
        {
            int notifications = _notifications;
            _notifications = Math.Max(0, _notifications-1);

            return (notifications > 0);
        }

        public override int PlatformGetData(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, _bufferSize);
            int bytesRead = _audioRecord.Read(buffer, offset, count);
            return bytesRead;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
