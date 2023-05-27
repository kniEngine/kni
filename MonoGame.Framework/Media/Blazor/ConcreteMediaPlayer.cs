// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Media;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteMediaPlayerStrategy : MediaPlayerStrategy
    {

        internal ConcreteMediaPlayerStrategy()
        {
        }

        #region Properties

        internal override bool PlatformIsMuted
        {
            get { return base.PlatformIsMuted; }
            set
            {
                base.PlatformIsMuted = value;

                if (Queue.Count > 0)
                    SetChannelVolumes();
            }
        }

        internal override bool PlatformIsRepeating
        {
            get { return base.PlatformIsRepeating; }
            set { base.PlatformIsRepeating = value; }
        }

        internal override bool PlatformIsShuffled
        {
            get { return base.PlatformIsShuffled; }
            set { base.PlatformIsShuffled = value; }
        }

        internal override TimeSpan PlatformPlayPosition
        {
            get { throw new NotImplementedException(); }
        }

        protected override bool PlatformUpdateState(ref MediaState state)
        {
            return false;
        }

        internal override float PlatformVolume
        {
            get { return base.PlatformVolume; }
            set
            {
                base.PlatformVolume = value;

                if (Queue.ActiveSong != null)
                    SetChannelVolumes();
            }
        }

        internal override bool PlatformGameHasControl
        {
            get { return true; }
        }

        #endregion

        private void SetChannelVolumes()
        {
            float innerVolume = base.PlatformIsMuted ? 0.0f : base.PlatformVolume;
            
            foreach (Song queuedSong in Queue.Songs)
            {
                ((ConcreteSongStrategy)queuedSong.Strategy).Volume = innerVolume;
            }
        }

        internal override void PlatformPlaySong(Song song)
        {
            throw new NotImplementedException();
        }

        internal override void PlatformPause()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformResume()
        {
            throw new NotImplementedException();
        }

        internal override void PlatformStop()
        {
            throw new NotImplementedException();
        }

    }
}
