// Copyright (C)2024 Nick Kastellanos

using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.XR;

namespace Microsoft.Xna.Framework.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {
        Game _game;
        IGraphicsDeviceService _graphics;
        XRMode _xrMode;
        XRDeviceState _deviceState;

        public override XRMode Mode
        {
            get { return _xrMode; }
        }

        public override XRDeviceState State
        {
            get { return _deviceState; }
        }

        public override bool IsConnected
        {
            get { throw new System.NotImplementedException(); }
        }

        public override bool TrackFloorLevelOrigin
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public ConcreteXRDevice(Game game, IGraphicsDeviceService graphics, XRMode mode)
        {
            this._game = game;
            this._graphics = graphics;
            this._xrMode = mode;
        }

        public override int CreateDevice()
        {
            throw new System.NotImplementedException();
        }

        public override int BeginFrame()
        {
            throw new System.NotImplementedException();
        }

        public override HeadsetState GetHeadsetState()
        {
            throw new System.NotImplementedException();
        }

        public override IEnumerable<XREye> GetEyes()
        {
            throw new System.NotImplementedException();
        }

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            throw new System.NotImplementedException();
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            throw new System.NotImplementedException();
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            throw new System.NotImplementedException();
        }

        public override int EndFrame()
        {
            throw new System.NotImplementedException();
        }

        public override HandsState GetHandsState()
        {
            throw new System.NotImplementedException();
        }


        protected override void Dispose(bool disposing)
        {
        }
    }
}