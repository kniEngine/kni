// Copyright (C)2022-2024 Nick Kastellanos

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

        public XRDevice(IGraphicsDeviceService graphics,
            XRMode mode = XRMode.VR)
        {
            _strategy = new ConcreteXRDevice(null, graphics, mode);

        }

        public XRDevice(Game game, IGraphicsDeviceService graphics,
            XRMode mode = XRMode.VR)
        {
            _strategy = new ConcreteXRDevice(game, graphics, mode);

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

        public Matrix CreateProjection(int eye, float znear, float zfar)
        {
            XREye xrEye = (XREye)(eye+1);
            return _strategy.CreateProjection(xrEye, znear, zfar);
        }

        public RenderTarget2D GetEyeRenderTarget(int eye)
        {
            XREye xrEye = (XREye)(eye + 1);
            return _strategy.GetEyeRenderTarget(xrEye);
        }

        public int CommitRenderTarget(int eye, RenderTarget2D rt)
        {
            XREye xrEye = (XREye)(eye + 1);
            _strategy.CommitRenderTarget(xrEye, rt);
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

