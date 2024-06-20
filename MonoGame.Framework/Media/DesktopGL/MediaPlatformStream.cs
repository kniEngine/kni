// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using NVorbis;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class MediaPlatformStream : IDisposable
    {
        internal DynamicSoundEffectInstance _player; // TODO: Move _player to MediaPlayer
        internal VorbisReader _reader;
        float[] _sampleBuffer;
        byte[] _dataBuffer;


        internal MediaPlatformStream()
        {
        }

        internal delegate void FinishedPlayingHandler();
        event FinishedPlayingHandler DonePlaying;

        /// <summary>
        /// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
        /// </summary>
        internal void SetEventHandler(FinishedPlayingHandler handler)
        {
            if (DonePlaying == null)
                DonePlaying += handler;
        }

        internal static void CreatePlayer(MediaPlatformStream mediaPlatformStream, SongStrategy strategy)
        {
            if (mediaPlatformStream._player == null)
            {
                mediaPlatformStream._reader = new VorbisReader(strategy.Filename);
                strategy.Duration = mediaPlatformStream._reader.TotalTime;

                int samples = (mediaPlatformStream._reader.SampleRate * mediaPlatformStream._reader.Channels) / 2;
                mediaPlatformStream._sampleBuffer = new float[samples];
                mediaPlatformStream._dataBuffer = new byte[samples * sizeof(short)];

                mediaPlatformStream._player = new DynamicSoundEffectInstance(mediaPlatformStream._reader.SampleRate, (AudioChannels)mediaPlatformStream._reader.Channels);
                mediaPlatformStream._player.BufferNeeded += mediaPlatformStream.sfxi_BufferNeeded;
            }
        }

        private void sfxi_BufferNeeded(object sender, EventArgs e)
        {
            DynamicSoundEffectInstance sfxi = (DynamicSoundEffectInstance)sender;
            int count = SubmitBuffer(sfxi, _reader);

            if (count == 0 && sfxi.PendingBufferCount <= 0)
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += Song_OnUpdate;
            }
        }

        private void Song_OnUpdate()
        {
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= Song_OnUpdate;

            FinishedPlayingHandler handler = DonePlaying;
            if (handler != null)
                handler();
        }

        private unsafe int SubmitBuffer(DynamicSoundEffectInstance sfxi, VorbisReader reader)
        {
            int count = _reader.ReadSamples(_sampleBuffer, 0, _sampleBuffer.Length);
            if (count > 0)
            {
                fixed (float* pSampleBuffer = _sampleBuffer)
                fixed (byte* pDataBuffer = _dataBuffer)
                {
                    ConvertFloat32ToInt16(pSampleBuffer, (short*)pDataBuffer, count);
                }
                sfxi.SubmitBuffer(_dataBuffer, 0, count * sizeof(short));
            }

            return count;
        }

        static unsafe void ConvertFloat32ToInt16(float* fbuffer, short* outBuffer, int samples)
        {
            for (int i = 0; i < samples; i++)
            {
                int val = (int)(fbuffer[i] * short.MaxValue);
                val = Math.Min(val, +short.MaxValue);
                val = Math.Max(val, -short.MaxValue);
                outBuffer[i] = (short)val;
            }
        }

        internal void DestroyPlayer()
        {
            if (_player != null)
            {
                _player.BufferNeeded -= sfxi_BufferNeeded;
                _player.Dispose();
            }
            _player = null;

            if (_reader != null)
                _reader.Dispose();
            _reader = null;

            _sampleBuffer = null;
            _dataBuffer = null;
        }

        #region IDisposable
        ~MediaPlatformStream()
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
                DestroyPlayer();

            }

            //base.Dispose(disposing);

        }
        #endregion
    }
}

