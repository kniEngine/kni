// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


using SharpDX.MediaFoundation;

namespace Microsoft.Xna.Framework.Media
{
    public sealed partial class VideoPlayer : IDisposable
    {
        DXGIDeviceManager _devManager;
        private MediaEngine _mediaEngine;

        private Texture2D _lastFrame;

        private void PlatformInitialize()
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
                        PlatformPlay(Strategy.Video);
                        return;
                    }                    
                    Strategy.State = MediaState.Stopped;
                    break;
            }
        }

        private Texture2D PlatformGetTexture()
        {
            if (_lastFrame != null)
            {
                if (_lastFrame.Width != Strategy.Video.Width || _lastFrame.Height != Strategy.Video.Height)
                {
                    _lastFrame.Dispose();
                    _lastFrame = null;
                }
            }
            if (_lastFrame == null)
                _lastFrame = new RenderTarget2D(Strategy.Video.GraphicsDevice, Strategy.Video.Width, Strategy.Video.Height, false, SurfaceFormat.Bgra32, DepthFormat.None);


            if (Strategy.State == MediaState.Playing)
            {
                long pts;
                if (_mediaEngine.HasVideo() && _mediaEngine.OnVideoStreamTick(out pts) && _mediaEngine.ReadyState >= 2)
                {
                    var region = new SharpDX.Mathematics.Interop.RawRectangle(0, 0, Strategy.Video.Width, Strategy.Video.Height);
                    SharpDX.ComObject dstSurfRef = (SharpDX.ComObject)_lastFrame.Handle;
                    _mediaEngine.TransferVideoFrame(dstSurfRef, null, region, null);
                }
            }

            return _lastFrame;
        }

        private MediaState PlatformUpdateState(MediaState currentState)
        {
            return currentState;
        }

        private void PlatformPause()
        {
            _mediaEngine.Pause();
        }

        private void PlatformResume()
        {
            _mediaEngine.Play();
        }

        private void PlatformPlay(Video video)
        {
            _mediaEngine.Source = System.IO.Path.Combine(TitleContainer.Location, Strategy.Video.FileName);
            _mediaEngine.Play();
        }

        private void PlatformStop()
        {
            _mediaEngine.Pause();
            _mediaEngine.CurrentTime = 0.0;
        }

        private void PlatformSetIsLooped()
        {
            throw new NotImplementedException();
        }

        private void PlatformSetIsMuted()
        {
            throw new NotImplementedException();
        }

        private TimeSpan PlatformGetPlayPosition()
        {
            return TimeSpan.FromSeconds(_mediaEngine.CurrentTime);
        }

        private void PlatformSetVolume()
        {
            _mediaEngine.Volume = Strategy.Volume;
        }

        private void PlatformDispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}
