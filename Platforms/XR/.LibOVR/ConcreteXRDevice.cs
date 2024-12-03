// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Framework.XR;
using Microsoft.Xna.Platform.Input.Oculus;
using nkast.LibOVR;
using static Microsoft.Xna.Framework.XR.XRDevice;

namespace Microsoft.Xna.Platform.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {
        //Game _game;
        IGraphicsDeviceService _graphics;

        OvrClient _ovrClient;
        OvrSession _ovrSession;

        long _frameIndex = 0;

        private HeadsetState _headsetState;
        private HandsState _handsState;

        internal OvrSession Session { get { return _ovrSession; } }

        OvrPosef[] _hmdToEyePose = new OvrPosef[2];
        OvrLayerEyeFov _layer;

        OvrStatusBits _statusFlags;


        OvrSwapChainDataBase[] _swapChainData = new OvrSwapChainDataBase[2];

        bool _isConnected;


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
            get { return _isConnected; }
        }

        public override bool TrackFloorLevelOrigin
        {
            get { return _ovrSession.GetTrackingOriginType() == OvrTrackingOrigin.FloorLevel; }
            set { _ovrSession.SetTrackingOriginType(value ? OvrTrackingOrigin.FloorLevel : OvrTrackingOrigin.EyeLevel); }
        }

        public ConcreteXRDevice(Game game, IGraphicsDeviceService graphics, XRMode mode)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (mode != XRMode.VR)
                throw new ArgumentException("mode");

            //this._game = game;
            this._graphics = graphics;
            this.Mode = mode;


            _graphics = graphics;
            _graphics.DeviceResetting += GraphicsDeviceResetting;
            _graphics.DeviceReset += GraphicsDeviceReset;
            _graphics.DeviceDisposing += GraphicsDeviceDisposing;
        }

        public override int CreateDevice()
        {
            GraphicsDevice graphicsDevice = _graphics.GraphicsDevice;
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphics.GraphicsDevice");
                //_graphics.DeviceCreated += GraphicsDeviceCreated;
            }
            else
            {
                return Initialize(graphicsDevice);
            }
        }

        public unsafe override int BeginFrame()
        {
            //_frameIndex++; // next frame

            bool notVisible;
            int ovrResult = 0;

            try
            {
                ovrResult = _ovrSession.WaitToBeginFrame(_frameIndex);
                notVisible = (ovrResult == 1000);

                ovrResult = _ovrSession.BeginFrame(_frameIndex);
            }
            catch (OvrException ovre)
            {
                switch (ovre.OvrResult)
                {
                    case -6000:
                        OnDisplayLost();
                        return ovre.OvrResult;
                    case -1015: // InvalidOperation
                        throw;
                    default:
                        throw;
                }
            }

            double time = _ovrSession.GetPredictedDisplayTime(_frameIndex);

            OvrTrackingState trackingState = _ovrSession.GetTrackingState(time, OvrBool.True);

            OvrPosef* eyePoses = stackalloc OvrPosef[2];
            double sensorSampleTime;

            fixed (OvrPosef* phmdToEyePose = _hmdToEyePose)
            {
                _ovrSession.ovr_GetEyePoses(_frameIndex, OvrBool.True, phmdToEyePose, eyePoses, out sensorSampleTime);
            }

            //layer.SensorSampleTime = sensorSampleTime;
            _layer.RenderPose[0] = eyePoses[0];
            _layer.RenderPose[1] = eyePoses[1];

            _statusFlags = trackingState.StatusFlags;
            _headsetState.HeadTransform = new OvrMatrix4f(trackingState.HeadPose.ThePose).ToTransposedMatrix();
            _headsetState.LEyeTransform = new OvrMatrix4f(eyePoses[0]).ToTransposedMatrix();
            _headsetState.REyeTransform = new OvrMatrix4f(eyePoses[1]).ToTransposedMatrix();

            _handsState.LHandTransform = new OvrMatrix4f(trackingState.HandPoses[0].ThePose).ToTransposedMatrix();
            _handsState.RHandTransform = new OvrMatrix4f(trackingState.HandPoses[1].ThePose).ToTransposedMatrix();

            return ovrResult;
        }

        public override HeadsetState GetHeadsetState()
        {
            return _headsetState;
        }

        public override IEnumerable<XREye> GetEyes()
        {
            throw new NotImplementedException();
        }

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            int eyeIndex = (int)eye - 1;

            int index;
            int ovrResult = _swapChainData[eyeIndex].SwapChain.GetCurrentIndex(out index);
            return _swapChainData[eyeIndex].GetRenderTarget(eyeIndex);
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            int eyeIndex = (int)eye - 1;

            OvrFovPort fov = _layer.Fov[eyeIndex];
            Matrix proj;

            //OvrMatrix4f orvProj = OvrMatrix4f.CreateProjection(fov, znear, zfar, OvrProjectionModifier.None);
            //Matrix proj1 = orvProj.ToTransposedMatrix();

            //Matrix proj2 = Matrix.CreatePerspectiveOffCenter(-fov.LeftTan * znear, fov.RightTan * znear, -fov.DownTan * znear, fov.UpTan * znear, znear, zfar);

            CreatePerspectiveFieldOfView(fov.LeftTan, fov.RightTan, fov.DownTan, fov.UpTan, znear, zfar, out proj);

            return proj;
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            int ovrResult = 0;

            int eyeIndex = (int)eye - 1;

            // SubmitRenderTarget
            ovrResult = _swapChainData[eyeIndex].SubmitRenderTarget(_graphics.GraphicsDevice, rt);

            // commit
            ovrResult = _swapChainData[eyeIndex].SwapChain.Commit();

            return;
        }

        public unsafe override int EndFrame()
        {
            int ovrResult = 0;

            OvrLayerHeader** layerPtrList = stackalloc OvrLayerHeader*[1];
            fixed (OvrLayerHeader* pLayerHeader = &_layer.Header)
            {
                layerPtrList[0] = pLayerHeader;
                try
                {
                    ovrResult = _ovrSession.EndFrame(_frameIndex, IntPtr.Zero, layerPtrList, 1);
                }
                catch (OvrException ovre)
                {
                    switch (ovre.OvrResult)
                    {
                        case -6000:
                            OnDisplayLost();
                            return ovre.OvrResult;
                        default:
                            throw;
                    }
                }

                return ovrResult;
            }
        }

        public override HandsState GetHandsState()
        {
            return _handsState;
        }


        private void GraphicsDeviceCreated(object sender, EventArgs e)
        {
        }
        private void GraphicsDeviceResetting(object sender, EventArgs e)
        {
        }
        private void GraphicsDeviceReset(object sender, EventArgs e)
        {
        }
        private void GraphicsDeviceDisposing(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private int Initialize(GraphicsDevice graphicsDevice)
        {
            // check supported feature level
            switch (graphicsDevice.Adapter.Backend)
            {
                case GraphicsBackend.DirectX11:
                    if (graphicsDevice.GraphicsProfile < GraphicsProfile.FL11_0)
                        throw new InvalidOperationException("GraphicsProfile must be FL11_0 or higher.");
                    break;
            }

            int ovrResult = 0;

            OvrDetectResult detectResults = OvrClient.Detect(0);
            if (detectResults.IsServiceRunning != OvrBool.True || detectResults.IsHMDConnected != OvrBool.True)
                return -1;

            if (_ovrClient == null)
            {
                ovrResult = OvrClient.TryInitialize(out _ovrClient);
                if ((ovrResult) < 0)
                    return ovrResult;
            }

            ovrResult = _ovrClient.TryCreateSession(out _ovrSession);
            if ((ovrResult) < 0)
                return ovrResult;

            CreateDefaultLayer(graphicsDevice,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8,
                4, 1,
                out _layer);

            TouchController.DeviceHandle = new ConcreteTouchControllerStrategy(this);
            _isConnected = true;

            return 0;
        }

        private int CreateDefaultLayer(GraphicsDevice graphicsDevice,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
            int pixelsPerDisplayPixel,
            out OvrLayerEyeFov layer)
        {
            int ovrResult = 0;

            OvrHmdDesc HmdDesc = _ovrSession.GetHmdDesc();


            // create layer
            layer = default(OvrLayerEyeFov);
            layer.Header.Type = OvrLayerType.EyeFov;
            layer.Header.Flags = 0;

            for (int eye = 0; eye < 2; eye++)
            {
                OvrFovPort fov = HmdDesc.DefaultEyeFov[eye];
                OvrSizei texRes = _ovrSession.GetFovTextureSize((OvrEyeType)eye, fov, pixelsPerDisplayPixel);
                layer.Viewport[eye] = new OvrRecti(0, 0, texRes.W, texRes.H);

                ovrResult = ConcreteOvrSwapChainData.CreateSwapChain(
                    graphicsDevice, _ovrSession,
                    texRes.W, texRes.H,
                    preferredFormat, preferredDepthFormat, preferredMultiSampleCount,
                    out _swapChainData[eye]);
                layer.ColorTexture[eye] = _swapChainData[eye].SwapChain.NativePtr;

                OvrEyeRenderDesc renderDesc = _ovrSession.GetRenderDesc((OvrEyeType)eye, fov);
                layer.Fov[eye] = renderDesc.Fov;
                _hmdToEyePose[eye] = renderDesc.HmdToEyePose;
            }

            return 0;
        }

        private unsafe void OnDisplayLost()
        {
            OvrGraphicsLuid prevGraphicsLuid = _ovrSession.GraphicsLuid;

            // destroy session
            _ovrSession.Dispose();
            _ovrSession = null;
            _ovrClient.Dispose();
            _ovrClient = null;
            _isConnected = false;
        }

        private static void CreatePerspectiveFieldOfView(float leftTan, float rightTan, float bottomTan, float topTan, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
        {
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentException("nearPlaneDistance <= 0");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentException("farPlaneDistance <= 0");
            }
            if (nearPlaneDistance >= farPlaneDistance)
            {
                throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
            }

            result.M11 = (2f) / (leftTan + rightTan);
            result.M12 = result.M13 = result.M14 = 0;

            result.M22 = (2f) / (topTan + bottomTan);
            result.M21 = result.M23 = result.M24 = 0;

            result.M31 = (rightTan - leftTan) / (leftTan + rightTan);
            result.M32 = (topTan - bottomTan) / (bottomTan + topTan);
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1;

            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
            result.M41 = result.M42 = result.M44 = 0;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_ovrSession != null)
                    _ovrSession.Dispose();
                if (_ovrClient != null)
                    _ovrClient.Dispose();
            }

            _ovrSession = null;
            _ovrClient = null;
        }


        internal abstract class OvrSwapChainDataBase
        {
            internal abstract OvrTextureSwapChain SwapChain { get; }
            internal abstract RenderTarget2D GetRenderTarget(int eye);
            internal abstract int SubmitRenderTarget(GraphicsDevice graphicsDevice, RenderTarget2D rt);
        }
    }


    internal static class OvrExtensions
    {
        public static Matrix ToMatrix(this OvrMatrix4f ovrMatrix4f)
        {
            return new Matrix(
                    ovrMatrix4f.M11, ovrMatrix4f.M12, ovrMatrix4f.M13, ovrMatrix4f.M14,
                    ovrMatrix4f.M21, ovrMatrix4f.M22, ovrMatrix4f.M23, ovrMatrix4f.M24,
                    ovrMatrix4f.M31, ovrMatrix4f.M32, ovrMatrix4f.M33, ovrMatrix4f.M34,
                    ovrMatrix4f.M41, ovrMatrix4f.M42, ovrMatrix4f.M43, ovrMatrix4f.M44
                );
        }

        public static Matrix ToTransposedMatrix(this OvrMatrix4f ovrMatrix4f)
        {
            return new Matrix(
                    ovrMatrix4f.M11, ovrMatrix4f.M21, ovrMatrix4f.M31, ovrMatrix4f.M41,
                    ovrMatrix4f.M12, ovrMatrix4f.M22, ovrMatrix4f.M32, ovrMatrix4f.M42,
                    ovrMatrix4f.M13, ovrMatrix4f.M23, ovrMatrix4f.M33, ovrMatrix4f.M43,
                    ovrMatrix4f.M14, ovrMatrix4f.M24, ovrMatrix4f.M34, ovrMatrix4f.M44
                );
        }
    }
}