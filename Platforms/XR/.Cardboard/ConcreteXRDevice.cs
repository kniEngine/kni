// Copyright (C)2024 Nick Kastellanos

using System;
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

        internal GameWindow _gameWindow;


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
            get { return (_deviceState == XRDeviceState.Ready); }
        }

        public override bool TrackFloorLevelOrigin
        {
            get { throw new System.NotImplementedException(); }
            set { throw new System.NotImplementedException(); }
        }

        public ConcreteXRDevice(Game game, IGraphicsDeviceService graphics, XRMode mode)
        {
            if (game == null)
                throw new ArgumentNullException("game");
            if (mode != XRMode.VR)
                throw new ArgumentException("mode");

            this._game = game;
            this._graphics = graphics;
            this._xrMode = mode;

            this._deviceState = XRDeviceState.Disabled;
        }

        public override int CreateDevice()
        {
            _gameWindow = _game.Window;

            _deviceState = XRDeviceState.Ready;
            return 0;
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
            if (disposing)
            {
            }

        }
    }
}