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
        internal float[] _sampleBuffer;
        internal byte[] _dataBuffer;


        internal MediaPlatformStream()
        {
        }

        event ConcreteMediaPlayerStrategy.FinishedPlayingHandler DonePlaying;

        /// <summary>
        /// Set the event handler for "Finished Playing". Done this way to prevent multiple bindings.
        /// </summary>
        internal void SetEventHandler(ConcreteMediaPlayerStrategy.FinishedPlayingHandler handler)
        {
            if (DonePlaying == null)
                DonePlaying += handler;
        }

        internal void sfxi_BufferNeeded(object sender, EventArgs e)
        {
            DynamicSoundEffectInstance sfxi = (DynamicSoundEffectInstance)sender;
            int count = ConcreteMediaPlayerStrategy.SubmitBuffer(this, sfxi, _reader);

            if (count == 0 && sfxi.PendingBufferCount <= 0)
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += Song_OnUpdate;
            }
        }

        private void Song_OnUpdate()
        {
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate -= Song_OnUpdate;

            ConcreteMediaPlayerStrategy.FinishedPlayingHandler handler = DonePlaying;
            if (handler != null)
                handler();
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

            //base.Dispose(disposing);

        }
        #endregion
    }
}

