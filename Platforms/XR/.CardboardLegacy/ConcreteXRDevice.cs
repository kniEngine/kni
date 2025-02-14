﻿// Copyright (C)2024 Nick Kastellanos

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
        XRSessionMode _sessionMode;
        XRDeviceState _deviceState;
        bool _isTrackFloorLevelEnabled = false;

        GameWindow _gameWindow;



        public override bool IsVRSupported
        {
            get { return true; }
        }

        public override bool IsARSupported
        {
            get { return false; }
        }

        public override XRSessionMode SessionMode
        {
            get { return _sessionMode; }
        }

        public override XRDeviceState DeviceState
        {
            get { return _deviceState; }
        }

        public override bool IsTrackFloorLevelEnabled
        {
            get { return _isTrackFloorLevelEnabled; }
        }


        public ConcreteXRDevice(string applicationName, Game game)
        {
            if (game == null)
                throw new ArgumentNullException("game");

            IGraphicsDeviceService graphics = game.Services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;

            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this._game = game;
            this._graphics = graphics;

            this._deviceState = XRDeviceState.Disabled;
        }

        public ConcreteXRDevice(string applicationName, IServiceProvider services)
        {
            throw new PlatformNotSupportedException("Cardboard requires a Game reference.");
        }

        public override int BeginSessionAsync(XRSessionMode sessionMode)
        {
            if (sessionMode != XRSessionMode.VR)
                throw new ArgumentException("mode");

            this._sessionMode = sessionMode;
            _gameWindow = _game.Window;

            _deviceState = XRDeviceState.Enabled;
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

            if (_headsetState.LeftEye.View == default(Matrix))
                return default(HeadsetState);

            //TODO: get HeadPose from VrRendererOnDrawFrame(VRCardboard.HeadTransform headTransform, ...)
            headsetState.HeadPose = Pose3.Identity;

            Matrix LEyeTransform = Matrix.Invert(_headsetState.LeftEye.View);
            Matrix REyeTransform = Matrix.Invert(_headsetState.RightEye.View);
            Pose3 LEyePose = default;
            Pose3 REyePose = default;
            LEyeTransform.Decompose(out _, out LEyePose.Orientation, out LEyePose.Translation);
            REyeTransform.Decompose(out _, out REyePose.Orientation, out REyePose.Translation);
            headsetState.LEyePose = LEyePose;
            headsetState.REyePose = REyePose;

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

        public override void EndSessionAsync()
        {
        }

        public override void TrackFloorLevelAsync(bool enable)
        {
            if (enable == true)
            {
                throw new NotImplementedException();
            }
            else
            {
                _isTrackFloorLevelEnabled = enable;
            }
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