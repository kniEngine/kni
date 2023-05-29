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
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
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

        internal ConcreteVideoPlayerStrategy()
        {
            
        }

        public override Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            VideoPlatformStream videoPlatformStream = ((ConcreteVideoStrategy)base.Video.Strategy).GetVideoPlatformStream();
            videoPlatformStream.Player.SetDisplay(((AndroidGameWindow)Game.Instance.Window).GameView.Holder);
            videoPlatformStream.Player.Start();

            State = MediaState.Playing;
        }

        public override void PlatformPause()
        {
            VideoPlatformStream videoPlatformStream = ((ConcreteVideoStrategy)base.Video.Strategy).GetVideoPlatformStream();
            videoPlatformStream.Player.Pause();
            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            VideoPlatformStream videoPlatformStream = ((ConcreteVideoStrategy)base.Video.Strategy).GetVideoPlatformStream();
            videoPlatformStream.Player.Start();
            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            VideoPlatformStream videoPlatformStream = ((ConcreteVideoStrategy)base.Video.Strategy).GetVideoPlatformStream();
            videoPlatformStream.Player.Stop();
            videoPlatformStream.Player.SetDisplay(null);

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
    
    internal sealed class VideoPlatformStream : IDisposable
    {
        private Android.Media.MediaPlayer _player;

        internal Android.Media.MediaPlayer Player { get { return _player; } }


        internal VideoPlatformStream(string filename)
        {
            _player = new Android.Media.MediaPlayer();

            var afd = AndroidGameWindow.Activity.Assets.OpenFd(filename);
            if (afd != null)
            {
                _player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                _player.Prepare();
            }

        }


        #region IDisposable
        ~VideoPlatformStream()
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
                    _player.Dispose();
                    _player = null;
                }

            }

            //base.Dispose(disposing);

        }
        #endregion
    }
}
