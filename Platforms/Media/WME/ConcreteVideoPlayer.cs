// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;
using DXRaw = SharpDX.Mathematics.Interop;
using MediaFoundation = SharpDX.MediaFoundation;


namespace Microsoft.Xna.Platform.Media
{
    internal sealed class ConcreteVideoPlayerStrategy : VideoPlayerStrategy
    {
        MediaFoundation.DXGIDeviceManager _devManager;
        private MediaFoundation.MediaEngine _mediaEngine;

        private Texture2D _lastFrame;

        public override MediaState State
        {
            get { return base.State; }
            protected set { base.State = value; }
        }

        public override bool IsLooped
        {
            get { return base.IsLooped; }
            set { base.IsLooped = value; }
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

        public override TimeSpan PlayPosition
        {
            get { return TimeSpan.FromSeconds(_mediaEngine.CurrentTime); }
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

        internal ConcreteVideoPlayerStrategy()
        {
            MediaFoundation.MediaManager.Startup();

            _devManager = new MediaFoundation.DXGIDeviceManager();
            _devManager.ResetDevice(((IPlatformGraphicsDevice)ConcreteGame.ConcreteGameInstance.GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice);

            using (MediaFoundation.MediaEngineClassFactory factory = new MediaFoundation.MediaEngineClassFactory())
            using (MediaFoundation.MediaEngineAttributes attributes = new MediaFoundation.MediaEngineAttributes
            {
                VideoOutputFormat = (int)DXGI.Format.B8G8R8A8_UNorm,
                DxgiManager = _devManager
            })
            {
                _mediaEngine = new MediaFoundation.MediaEngine(factory, attributes, MediaFoundation.MediaEngineCreateFlags.None, OnMediaEngineEvent);
            }
        }

        private void OnMediaEngineEvent(MediaFoundation.MediaEngineEvent mediaEvent, long param1, int param2)
        {
            if (!_mediaEngine.HasVideo())
                return;

            switch (mediaEvent)
            {
                case MediaFoundation.MediaEngineEvent.Play:
                    break;
                
                case MediaFoundation.MediaEngineEvent.Ended:
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
                _lastFrame = new RenderTarget2D(((IPlatformVideo)base.Video).Strategy.GraphicsDevice, base.Video.Width, base.Video.Height, false, SurfaceFormat.Bgra32, DepthFormat.None);


            if (base.State == MediaState.Playing)
            {
                long pts;
                if (_mediaEngine.HasVideo() && _mediaEngine.OnVideoStreamTick(out pts) && _mediaEngine.ReadyState >= 2)
                {
                    DXRaw.RawRectangle region = new DXRaw.RawRectangle(0, 0, base.Video.Width, base.Video.Height);
                    DX.ComObject dstSurfRef = (DX.ComObject)_lastFrame.GetD3D11Resource();
                    _mediaEngine.TransferVideoFrame(dstSurfRef, null, region, null);
                }
            }

            return _lastFrame;
        }

        protected override void PlatformUpdateState(ref MediaState state)
        {
        }
        
        public override void PlatformPlay(Video video)
        {
            base.Video = video;

            string assetsLocationFullPath = ((ITitleContainer)TitleContainer.Current).Location;
            _mediaEngine.Source = System.IO.Path.Combine(assetsLocationFullPath, ((IPlatformVideo)base.Video).Strategy.FileName);
            _mediaEngine.Play();

            State = MediaState.Playing;
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


        public override void PlatformStop()
        {
            _mediaEngine.Pause();
            _mediaEngine.CurrentTime = 0.0;

            State = MediaState.Stopped;
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
