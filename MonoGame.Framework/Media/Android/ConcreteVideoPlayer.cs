// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Android.Widget;

namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsMuted
        {
            get { return base.IsMuted; }
            set
            {
                base.IsMuted = value;
                throw new NotImplementedException();
            }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    PlatformSetVolume();
            }
        }

        public ConcreteVideoPlayerStrategy()
        {
            
        }

        public override Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPause()
        {
            base.Video.Player.Pause();
            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            base.Video.Player.Start();
            State = MediaState.Playing;
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            base.Video.Player.SetDisplay(((AndroidGameWindow)Game.Instance.Window).GameView.Holder);
            base.Video.Player.Start();

            ConcreteGame.IsPlayingVideo = true;

            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            base.Video.Player.Stop();

            ConcreteGame.IsPlayingVideo = false;
            base.Video.Player.SetDisplay(null);

            State = MediaState.Stopped;
        }

        public override TimeSpan PlatformGetPlayPosition()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetVolume()
        {
            throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
