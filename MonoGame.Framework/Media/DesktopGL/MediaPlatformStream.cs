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
    internal sealed class MediaPlatformStream
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

    }
}

