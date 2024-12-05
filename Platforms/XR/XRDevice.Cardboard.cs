// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.XR;

namespace Microsoft.Xna.Framework.XR
{
    public class XRDevice : IPlatformXRDevice, IDisposable
    {
        private XRDeviceStrategy _strategy;

        T IPlatformXRDevice.GetStrategy<T>()
        {
            return (T)_strategy;
        }


        public XRDeviceState State
        {
            get { return _strategy.State; }
        }

        public XRMode Mode
        {
            get { return _strategy.Mode; }
        }

        public bool IsConnected
        {
            get { return _strategy.IsConnected; }
        }

        public bool TrackFloorLevelOrigin
        {
            get { return _strategy.TrackFloorLevelOrigin; }
            set { _strategy.TrackFloorLevelOrigin = value; }
        }

        public XRDevice(Game game, IGraphicsDeviceService graphics,
            XRMode mode = XRMode.VR)
        {
            _strategy = new ConcreteXRDevice(game, graphics, mode);

        }

        public HeadsetState GetState()
        {
            HeadsetState state;

            var window = ((IPlatformXRDevice)this).GetStrategy<ConcreteXRDevice>()._gameWindow as AndroidGameWindow;

            window.UpdateHeadsetState(out state);

            return state;
        }


        // the following functions should be called in order
        public int CreateDevice()
        {
            return _strategy.CreateDevice();
        }

        public int BeginFrame()
        {
            return _strategy.BeginFrame();
        }

        public HeadsetState GetHeadsetState()
        {
            return _strategy.GetHeadsetState();
        }

        public IEnumerable<XREye> GetEyes()
        {
            return _strategy.GetEyes();
        }

        public RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            return _strategy.GetEyeRenderTarget(eye);
        }

        public Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            return _strategy.CreateProjection(eye, znear, zfar);
        }

        public int CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            _strategy.CommitRenderTarget(eye, rt);
            return 0;
        }

        public int EndFrame()
        {
            return _strategy.EndFrame();
        }

        public HandsState GetHandsState()
        {
            return _strategy.GetHandsState();
        }


        #region IDisposable
        ~XRDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _strategy.Dispose();
                _strategy = null;
            }

        }
        #endregion
    }
}

