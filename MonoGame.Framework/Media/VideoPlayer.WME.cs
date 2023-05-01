// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Media;


using SharpDX.MediaFoundation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        DXGIDeviceManager _devManager;
        private MediaEngine _mediaEngine;

        private Texture2D _lastFrame;

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
            set
            {
                base.IsLooped = value;
                throw new NotImplementedException();
            }
        }

        public override float Volume
        {
            get { return base.Volume; }
            set
            {
                base.Volume = value;
                if (base.Video != null)
                    _mediaEngine.Volume = value;
            }
        }

        public ConcreteVideoPlayerStrategy()
        {
            MediaManager.Startup();

            _devManager = new DXGIDeviceManager();
            _devManager.ResetDevice(Game.Instance.Strategy.GraphicsDevice.D3DDevice);

            using (var factory = new MediaEngineClassFactory())
            using (var attributes = new MediaEngineAttributes
            {
                VideoOutputFormat = (int)SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                DxgiManager = _devManager
            })
            {
                _mediaEngine = new MediaEngine(factory, attributes, MediaEngineCreateFlags.None, OnMediaEngineEvent);
            }
        }

        private void OnMediaEngineEvent(MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (!_mediaEngine.HasVideo())
                return;

            switch (mediaEvent)
            {
                case MediaEngineEvent.Play:
                    break;
                
                case MediaEngineEvent.Ended:
                    if (IsLooped)
                    {
                        PlatformPlay(base.Video);
                        return;
                    }
                    State = MediaState.Stopped;
                    break;
            }
        }

        public override Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != base.Video.Width || _lastFrame.Height != base.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new RenderTarget2D(base.Video.GraphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Bgra32, DepthFormat.None);


            if (base.State == MediaState.Playing)
            {
                long pts;
                if (_mediaEngine.HasVideo() && _mediaEngine.OnVideoStreamTick(out pts) && _mediaEngine.ReadyState >= 2)
                {
                    var region = new SharpDX.Mathematics.Interop.RawRectangle(0, 0, base.Video.Width, base.Video.Height);
                    SharpDX.ComObject dstSurfRef = (SharpDX.ComObject)_lastFrame.Handle;
                    _mediaEngine.TransferVideoFrame(dstSurfRef, null, region, null);
                }
            }

            return _lastFrame;
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }

        public override void PlatformPause()
        {
            _mediaEngine.Pause();
            State = MediaState.Paused;
        }

        public override void PlatformResume()
        {
            _mediaEngine.Play();
            State = MediaState.Playing;
        }

        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            _mediaEngine.Source = System.IO.Path.Combine(TitleContainer.Location, base.Video.FileName);
            _mediaEngine.Play();

            State = MediaState.Playing;
        }

        public override void PlatformStop()
        {
            _mediaEngine.Pause();
            _mediaEngine.CurrentTime = 0.0;

            State = MediaState.Stopped;
        }

        public override TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromSeconds(_mediaEngine.CurrentTime);
        }

        private void PlatformSetVolume()
        {
            _mediaEngine.Volume = base.Volume;
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
