// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        GameWindow _gameWindow;


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

        CardboardHeadsetState _headsetState;

        public override int BeginFrame()
        {
            var window = _gameWindow as AndroidGameWindow;
            window.UpdateHeadsetState(out _headsetState);

            return 0;
        }

        public override HeadsetState GetHeadsetState()
        {
            HeadsetState headsetState = default;

            //TODO: get HeadTransform from VrRendererOnDrawFrame(VRCardboard.HeadTransform headTransform, ...)
            headsetState.HeadTransform = Matrix.Identity;
            headsetState.LEyeTransform = Matrix.Invert(_headsetState.LeftEye.View);
            headsetState.REyeTransform = Matrix.Invert(_headsetState.RightEye.View);

            return headsetState;
        }

        public override IEnumerable<XREye> GetEyes()
        {
            yield return XREye.Left;
            yield return XREye.Right;
        }

        RenderTarget2D[] _rt = new RenderTarget2D[2];

        public override RenderTarget2D GetEyeRenderTarget(XREye xreye)
        {
            int eye = (int)xreye - 1;

            Viewport vp = default;
            switch (xreye)
            {
                case XREye.Left: vp = _headsetState.LeftEye.Viewport; break;
                case XREye.Right: vp = _headsetState.RightEye.Viewport; break;
            }
            int w = vp.Width;
            int h = vp.Height;


            var gd = _graphics.GraphicsDevice;

            if (_rt[eye] != null
            && (_rt[eye].Width != w || _rt[eye].Height != h)
            )
            {
                _rt[eye].Dispose();
                _rt[eye] = null;
            }

            if (_rt[eye] == null)
            {
                _rt[eye] = new RenderTarget2D(_graphics.GraphicsDevice, w, h, false,
                                              SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 4, RenderTargetUsage.PreserveContents);
            }

            return _rt[eye];
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            Matrix proj;

            switch (eye)
            {
                case XREye.None:
                    throw new NotImplementedException();
                case XREye.Left:
                    proj = _headsetState.LeftEye.Projection;
                    break;
                case XREye.Right:
                    proj = _headsetState.RightEye.Projection;
                    break;

                default:
                    throw new InvalidOperationException();
            }

            // extract FOV from the proj Matrix
            float tanAngleLeft   = (1 + proj.M31) / proj.M11;
            float tanAngleRight  = (1 - proj.M31) / proj.M11;
            float tanAngleBottom = (1 + proj.M32) / proj.M22;
            float tanAngleTop    = (1 - proj.M32) / proj.M22;

            //TODO: Get FOV from VRCardboard.EyeParams and create projection with znear/zfar
            //proj = CreateProjection(tanAngleLeft, tanAngleRight, tanAngleBottom, tanAngleTop, znear, zfar);

            return proj;
        }

        public override void CommitRenderTarget(XREye xreye, RenderTarget2D rt)
        {
            int eye = (int)xreye - 1;

            Debug.Assert(_rt[eye] == rt);
        }


        SpriteBatch _sb;
        public override int EndFrame()
        {
            var gd = _graphics.GraphicsDevice;

            if (_sb == null)
                _sb = new SpriteBatch(_graphics.GraphicsDevice);

            Viewport vp = default;

            _sb.Begin();
             vp = _headsetState.LeftEye.Viewport;
            _sb.Draw(_rt[0], new Rectangle(vp.X, vp.Y, vp.Width, vp.Height), Color.White);
            vp = _headsetState.RightEye.Viewport; 
            _sb.Draw(_rt[1], new Rectangle(vp.X, vp.Y, vp.Width, vp.Height), Color.White);
            _sb.End();

            return 0;
        }

        public override HandsState GetHandsState()
        {
            return default(HandsState);
        }

        static unsafe Matrix CreateProjection(   float tanAngleLeft, float tanAngleRight,
                                                 float tanAngleBottom, float tanAngleTop,
                                                 float nearZ, float farZ)
        {
            float tanAngleWidth = tanAngleLeft + tanAngleRight;
            float tanAngleHeight = tanAngleBottom + tanAngleTop;

            Matrix result;
            
            result.M11 = (2f) / tanAngleWidth;
            result.M12 = result.M13 = result.M14 = 0;
            result.M22 = (2f) / tanAngleHeight;
            result.M21 = result.M23 = result.M24 = 0;
            result.M31 = -(tanAngleRight - tanAngleLeft) / tanAngleWidth;
            result.M32 = -(tanAngleTop - tanAngleBottom) / tanAngleHeight;
            result.M33 = (float)((farZ) / ((double)nearZ - farZ));
            result.M34 = -1;
            result.M43 = (float)((farZ * ((double)nearZ)) / ((double)nearZ - farZ));
            result.M41 = result.M42 = result.M44 = 0;
            return result;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }
    }
}