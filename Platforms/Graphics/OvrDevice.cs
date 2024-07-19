// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Platform.Input.Oculus;
using nkast.LibOVR;


namespace Microsoft.Xna.Framework.Graphics
{
    public class OvrDevice : IDisposable
    {
        private IGraphicsDeviceService _graphics;

        public bool IsConnected { get; private set; }

        OvrClient _ovrClient;
        OvrSession _ovrSession;

        long _frameIndex = 0;

        private HeadsetState _headsetState;
        private HandsState _handsState;

        OvrSwapChainDataBase[] _swapChainData = new OvrSwapChainDataBase[2];

        internal OvrSession Session { get { return _ovrSession; } }

        public HeadsetState GetHeadsetState()
        {
            return _headsetState;
        }

        public HandsState GetHandsState()
        {
            return _handsState;
        }

        public RenderTarget2D GetEyeRenderTarget(int eye)
        {
            int index;
            int ovrResult = _swapChainData[eye].SwapChain.GetCurrentIndex(out index);
            return _swapChainData[eye].GetRenderTarget(eye);
        }

        OvrPosef[] _hmdToEyePose = new OvrPosef[2];
        OvrLayerEyeFov _layer;

        OvrStatusBits _statusFlags;


        public OvrDevice(IGraphicsDeviceService graphics) : base()
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            _graphics = graphics;
            _graphics.DeviceResetting += GraphicsDeviceResetting;
            _graphics.DeviceReset += GraphicsDeviceReset;
            _graphics.DeviceDisposing += GraphicsDeviceDisposing;
        }

        // the following functions should be called in order
        public int CreateDevice()
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
            if (graphicsDevice.GraphicsProfile < GraphicsProfile.FL10_0)
                throw new InvalidOperationException("GraphicsProfile must be FL10_0 or higher.");

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
            IsConnected = true;

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

        public Matrix CreateProjection(int eye, float znear, float zfar)
        {
            OvrFovPort fov = _layer.Fov[eye];
            Matrix proj;

            //OvrMatrix4f orvProj = OvrMatrix4f.CreateProjection(fov, znear, zfar, OvrProjectionModifier.None);
            //Matrix proj1 = orvProj.ToTransposedMatrix();

            //Matrix proj2 = Matrix.CreatePerspectiveOffCenter(-fov.LeftTan * znear, fov.RightTan * znear, -fov.DownTan * znear, fov.UpTan * znear, znear, zfar);

            CreatePerspectiveFieldOfView(fov.LeftTan, fov.RightTan, fov.DownTan, fov.UpTan, znear, zfar, out proj);

            return proj;
        }

        public static void CreatePerspectiveFieldOfView(float leftTan, float rightTan, float bottomTan, float topTan, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
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


        public unsafe int BeginFrame()
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

        private unsafe void OnDisplayLost()
        {
            OvrGraphicsLuid prevGraphicsLuid = _ovrSession.GraphicsLuid;

            // destroy session
            _ovrSession.Dispose();
            _ovrSession = null;
            _ovrClient.Dispose();
            _ovrClient = null;
            IsConnected = false;
        }

        public int CommitRenderTarget(int eye, RenderTarget2D rt)
        {
            int ovrResult = 0;

            // SubmitRenderTarget
            ovrResult = _swapChainData[eye].SubmitRenderTarget(_graphics.GraphicsDevice, rt);

            // commit
            ovrResult = _swapChainData[eye].SwapChain.Commit();

            return ovrResult;
        }

        public unsafe int EndFrame()
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

        #region IDisposable
        ~OvrDevice()
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
                if (_ovrSession != null)
                    _ovrSession.Dispose();
                if (_ovrClient != null)
                    _ovrClient.Dispose();
            }

            _ovrSession = null;
            _ovrClient = null;
        }

        #endregion

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

