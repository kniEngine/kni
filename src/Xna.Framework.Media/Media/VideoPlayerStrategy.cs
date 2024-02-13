// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Microsoft.Xna.Platform.Media
{
    abstract public class VideoPlayerStrategy : IDisposable
    {
        private MediaState _state = MediaState.Stopped;

        private Video _currentVideo;
        private float _volume = 1.0f;
        private bool _isLooped = false;
        private bool _isMuted = false;

        public virtual MediaState State
        {
            get
            {
                PlatformUpdateState(ref _state);
                return _state;
            }
            protected set { _state = value; }
        }

        public virtual Video Video
        {
            get { return _currentVideo; }
            protected set { _currentVideo = value; }
        }

        public virtual float Volume
        {
            get { return _volume; }
            set
            {
                if (value < 0.0f || value > 1.0f)
                    throw new ArgumentOutOfRangeException();

                _volume = value;
            }
        }

        public virtual bool IsLooped
        {
            get { return _isLooped; }
            set { _isLooped = value; }
        }

        public virtual bool IsMuted
        {
            get { return _isMuted; }
            set { _isMuted = value; }
        }

        virtual public TimeSpan PlayPosition
        {
            get { throw new NotImplementedException(); }
        }

        ~VideoPlayerStrategy()
        {
            Dispose(false);
        }

        protected abstract void PlatformUpdateState(ref MediaState state);


        virtual public Texture2D PlatformGetTexture()
        {
            throw new NotImplementedException();
        }

        virtual public void PlatformPlay(Video video)
        {
            throw new NotImplementedException();
        }

        virtual public void PlatformPause()
        {
            throw new NotImplementedException();
        }

        virtual public void PlatformResume()
        {
            throw new NotImplementedException();
        }

        virtual public void PlatformStop()
        {
            throw new NotImplementedException();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _currentVideo = null;
            }

        }

        #endregion
    }
}
