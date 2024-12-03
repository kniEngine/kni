// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.XR;

namespace Microsoft.Xna.Platform.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {
        //Game _game;
        IGraphicsDeviceService _graphics;

        public override XRMode Mode
        {
            get { return base.Mode; }
            set { base.Mode = value; }
        }

        public override XRDeviceState State
        {
            get { return base.State; }
            set { base.State = value; }
        }

        public override bool IsConnected
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ConcreteXRDevice(Game game, IGraphicsDeviceService graphics, XRMode mode)
        {
            //this._game = game;
            this._graphics = graphics;
            this.Mode = mode;
        }

        public override int CreateDevice()
        {
            throw new NotImplementedException();
        }

        public override int BeginFrame()
        {
            throw new NotImplementedException();
        }

        public override HeadsetState GetHeadsetState()
        {
            throw new NotImplementedException();
        }

        public override IEnumerable<XREye> GetEyes()
        {
            throw new NotImplementedException();
        }

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            throw new NotImplementedException();
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            throw new NotImplementedException();
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            throw new NotImplementedException();
        }

        public override int EndFrame()
        {
            throw new NotImplementedException();
        }

        public override HandsState GetHandsState()
        {
            throw new NotImplementedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}