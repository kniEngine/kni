// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Input.Oculus;
using Microsoft.Xna.Platform.XR;
using nkast.Wasm.Canvas.WebGL;
using nkast.Wasm.Dom;
using nkast.Wasm.Input;
using nkast.Wasm.XR;
using WebXREye = nkast.Wasm.XR.XREye;
using SysNumerics = System.Numerics;

namespace Microsoft.Xna.Framework.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {
        Game _game;
        IGraphicsDeviceService _graphics;
        XRSessionMode _sessionMode;
        XRDeviceState _deviceState;
        bool _isTrackFloorLevelEnabled = false;

        private bool? _isVRSupported;
        private bool? _isARSupported;

        XRSystem _xr;
        XRSession _xrsession;
        XRReferenceSpace _localSpace;
        XRReferenceSpace _localFloorSpace;
        Task<XRReferenceSpace> _createLocalFloorSpaceTask;
        XRWebGLLayer _glLayer;

        int _xrAnimationHandle;
        bool _xrInAnimationFrame;

        HandsState _handsState;

        public override bool IsVRSupported
        {
            get { return _isVRSupported.GetValueOrDefault(); }
        }

        public override bool IsARSupported
        {
            get { return _isARSupported.GetValueOrDefault(); }
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
            get { return (_isTrackFloorLevelEnabled && _localFloorSpace != null); }
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

            this._handsState.LGripTransform = Matrix.Identity;
            this._handsState.RGripTransform = Matrix.Identity;
            this._handsState.LHandTransform = Matrix.Identity;
            this._handsState.RHandTransform = Matrix.Identity;

            this._xr = XRSystem.FromNavigator(Window.Current.Navigator);
            if (this._xr == null)
            {
                _deviceState = XRDeviceState.Disabled;
                return;
            }

            this._deviceState = XRDeviceState.InitializingDevice;
            InitXRDeviceAsync();
        }

        private async void InitXRDeviceAsync()
        {
            this._isVRSupported = await _xr.IsSessionSupportedAsync(ModeToString(XRSessionMode.VR));
            this._isARSupported = await _xr.IsSessionSupportedAsync(ModeToString(XRSessionMode.AR));
                        
            this._deviceState = XRDeviceState.Disabled;
        }

        public ConcreteXRDevice(string applicationName, IServiceProvider services)
        {
            throw new PlatformNotSupportedException("WebXR requires a Game reference.");
        }

        public override int BeginSessionAsync(XRSessionMode sessionMode)
        {
            if (this.DeviceState != XRDeviceState.Disabled
            &&  this.DeviceState != XRDeviceState.NoPermissions)
            {
                return -1;
            }

            switch (sessionMode)
            {
                case XRSessionMode.VR:
                    if (_isVRSupported == false)
                        return -1; //throw new NotSupportedException("VR");
                    _deviceState = XRDeviceState.InitializingSession;
                    InitXRSessionAsync(XRSessionMode.VR);
                    break;

                case XRSessionMode.AR:
                    if (_isARSupported == false)
                        return -1; //throw new NotSupportedException("AR");
                    _deviceState = XRDeviceState.InitializingSession;
                    InitXRSessionAsync(XRSessionMode.AR);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return 0;
        }

        public override int BeginFrame()
        {
            return 0;
        }

        public override HeadsetState GetHeadsetState()
        {
            return _headsetState;
        }

        public override IEnumerable<XREye> GetEyes()
        {
            if (_currentXRFrame != null)
            {
                using (XRViewerPose viewerPose = _currentXRFrame.GetViewerPose(_localSpace))
                {
                    if (viewerPose != null)
                    {
                        foreach (XRView xrView in viewerPose.Views)
                        {
                            XREye viewEye = (XREye)xrView.Eye;
                            yield return viewEye;
                        }
                        yield break;
                    }
                }
            }

            yield return XREye.None;
        }


        RenderTarget2D[] _rt = new RenderTarget2D[2];

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            RenderTarget2D rt;
            int w = 0, h = 0;

            XRWebGLLayer glLayer = _currentRenderState.BaseLayer;

            using (XRViewerPose viewerPose = _currentXRFrame.GetViewerPose(_localSpace))
            {
                if (viewerPose != null)
                {
                    foreach (XRView xrView in viewerPose.Views)
                    {
                        if (eye == (XREye)xrView.Eye)
                        {
                            XRViewport xrViewport = glLayer.GetViewport(xrView);
                            w = xrViewport.Width;
                            h = xrViewport.Height;
                        }
                    }
                }
            }

            if (w == 0)
                return null;

            int eyeIndex = (int)eye - 1;
            if (eyeIndex == -1)
                eyeIndex = 0;

            if (_rt[eyeIndex] != null
            && (_rt[eyeIndex].Width != w || _rt[eyeIndex].Height != h)
            )
            {
                _rt[eyeIndex].Dispose();
                _rt[eyeIndex] = null;
            }

            if (_rt[eyeIndex] == null)
            {
                _rt[eyeIndex] = new RenderTarget2D(_graphics.GraphicsDevice, w, h, false,
                                              SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 4, RenderTargetUsage.PreserveContents);
            }

            return _rt[eyeIndex];
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            switch (eye)
            {
                case XREye.None: return CreateProjection(_lproj, znear, zfar);
                case XREye.Left: return CreateProjection(_lproj, znear, zfar);
                case XREye.Right: return CreateProjection(_rproj, znear, zfar);
                default: throw new ArgumentException("Eye");
            }
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            int eyeIndex = (int)eye - 1;
            if (eyeIndex == -1)
                eyeIndex = 0;

            Debug.Assert(_rt[eyeIndex] == rt);

            return;
        }

        public override int EndFrame()
        {
            var graphicsDevice = _graphics.GraphicsDevice;
            var GL = (IWebGL2RenderingContext)((IPlatformGraphicsContext)((IPlatformGraphicsDevice)graphicsDevice).Strategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            XRWebGLLayer glLayer = _currentRenderState.BaseLayer;


            using (XRViewerPose viewerPose = _currentXRFrame.GetViewerPose(_localSpace))
            {
                if (viewerPose != null)
                {
                    WebGLFramebuffer glFramebuffer = glLayer.Framebuffer;
                    WebGLFramebuffer glDefaultFramebuffer = ((IPlatformGraphicsDevice)graphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>()._glDefaultFramebuffer;

                    foreach (XRView xrView in viewerPose.Views)
                    {
                        WebXREye viewEye = xrView.Eye;
                        int eye = (int)viewEye - 1;
                        if (eye == -1)
                            eye = 0;

                        XRViewport xrViewport = glLayer.GetViewport(xrView);
                        if (xrViewport.Width == 0)
                            return 0;
                        if (_rt[eye] == null)
                            return 0;

                        ConcreteTexture concreteTexture = ((IPlatformTexture)_rt[eye]).GetTextureStrategy<ConcreteTexture>();

                        // copy rendertarget to glFramebuffer
                        using (WebGLFramebuffer sourceFramebuffer = GL.CreateFramebuffer())
                        {
                            //unbind glDefaultFramebuffer 
                            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, null);
                            //GL.CheckGLError();

                            if (glFramebuffer != null)
                            {
                                // bind DrawFramebuffer
                                GL.BindFramebuffer(WebGL2FramebufferType.DRAW_FRAMEBUFFER, glFramebuffer);
                                //GL.CheckGLError();
                            }
                            else
                            {
                                // bind DrawFramebuffer
                                GL.BindFramebuffer(WebGL2FramebufferType.DRAW_FRAMEBUFFER, glDefaultFramebuffer);
                                //GL.CheckGLError();
                            }

                            GL.BindFramebuffer(WebGL2FramebufferType.READ_FRAMEBUFFER, sourceFramebuffer);
                            //GL.CheckGLError();

                            GL.FramebufferTexture2D(
                                WebGL2FramebufferType.READ_FRAMEBUFFER,
                                WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0,
                                WebGLTextureTarget.TEXTURE_2D,
                                concreteTexture._glTexture);
                            //GL.CheckGLError();

                            //CheckFramebufferStatus(WebGL2FramebufferType.DRAW_FRAMEBUFFER);
                            //CheckFramebufferStatus(WebGL2FramebufferType.READ_FRAMEBUFFER);

                            // copy and y-flip
                            GL.BlitFramebuffer(0, 0, _rt[eye].Width, _rt[eye].Height,
                                                xrViewport.X, xrViewport.Y + xrViewport.Height,
                                                xrViewport.X + xrViewport.Width, xrViewport.Y,
                                                WebGLBufferBits.COLOR, WebGLTexParam.NEAREST);
                            //GL.CheckGLError();

                            //GL.BindFramebuffer(WebGL2FramebufferType.READ_FRAMEBUFFER, sourceFramebuffer);
                            GL.BindFramebuffer(WebGL2FramebufferType.READ_FRAMEBUFFER, null);
                            //GL.CheckGLError();

                            // unbind DrawFramebuffer
                            GL.BindFramebuffer(WebGL2FramebufferType.DRAW_FRAMEBUFFER, null);
                            //GL.CheckGLError();

                            //rebind glDefaultFramebuffer 
                            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glDefaultFramebuffer);
                            //GL.CheckGLError();

                            //CheckFramebufferStatus(WebGL2FramebufferType.DRAW_FRAMEBUFFER);
                        }
                    }
                }
            }

            return 0;
        }

        public override HandsState GetHandsState()
        {
            return _handsState;
        }

        public override void EndSessionAsync()
        {
            _xrsession.End();
        }

        public override void TrackFloorLevelAsync(bool enable)
        {
            if (enable == true)
            {
                _isTrackFloorLevelEnabled = enable;

                if (_localFloorSpace == null)
                {
                    // create _localFloorSpace
                    if (_createLocalFloorSpaceTask == null)
                    {
                        try
                        {
                            _createLocalFloorSpaceTask = _currentXRSession.RequestReferenceSpace("local-floor");
                            _createLocalFloorSpaceTask.ContinueWith((t) =>
                            {
                                if (_createLocalFloorSpaceTask.IsCompletedSuccessfully)
                                {
                                    _localFloorSpace = _createLocalFloorSpaceTask.Result;
                                }
                                _createLocalFloorSpaceTask = null;
                            });
                        }
                        catch
                        {
                            /* local-floor not supported */
                            _createLocalFloorSpaceTask = null;
                        }
                    }
                }
            }
            else
            {
                _isTrackFloorLevelEnabled = enable;
            }
        }

        private string ModeToString(XRSessionMode mode)
        {
            switch (mode)
            {
                case XRSessionMode.VR:
                    return "immersive-vr";
                case XRSessionMode.AR:
                    return "immersive-ar";

                default:
                    throw new ArgumentException("mode");
            }
            throw new NotImplementedException();
        }

        private async void InitXRSessionAsync(XRSessionMode mode)
        {
            try
            {
                var graphicsDevice = _graphics.GraphicsDevice;
                var GL = (IWebGL2RenderingContext) ((IPlatformGraphicsContext)((IPlatformGraphicsDevice)graphicsDevice).Strategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

                XRSessionOptions sessionOptions = default;
                sessionOptions.RequiredFeatures |= XRSessionFeatures.Local;
                sessionOptions.OptionalFeatures |= XRSessionFeatures.LocalFloor;
                _xrsession = await _xr.RequestSessionAsync(ModeToString(mode), sessionOptions);
                _xrsession.Ended += _xrsession_Ended;
                _xrsession.InputSourcesChanged += _xrsession_InputSourcesChanged;
                await GL.MakeXRCompatibleAsync();
                _localSpace = await _xrsession.RequestReferenceSpace("local");

                XRWebGLLayerOptions glLayerOptions = new XRWebGLLayerOptions();
                glLayerOptions.Antialias = false;
                //glLayerOptions.Depth = true;
                //glLayerOptions.IgnoreDepthValues = false;
                _glLayer = new XRWebGLLayer(_xrsession, GL, glLayerOptions);

                RenderStateAttributes attribs = new RenderStateAttributes();
                attribs.BaseLayer = _glLayer;
                _xrsession.UpdateRenderState(attribs);

                TouchController.DeviceHandle = new ConcreteTouchController(this);

                _sessionMode = mode;
                _deviceState = XRDeviceState.Enabled;
                ((IPlatformGame)_game).GetStrategy<ConcreteGame>()._suppressTick = true;
                _xrAnimationHandle = _xrsession.RequestAnimationFrame(this.AnimationFrameCallback);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("InitXRSessionAsync failed with UnauthorizedAccessException. " + ex.Message);
                _deviceState = XRDeviceState.NoPermissions;
            }
            catch (Exception ex)
            {
                Console.WriteLine("InitXRSessionAsync failed. " + ex.Message);
                _deviceState = XRDeviceState.Disabled;
                //throw;
            }
        }

        private void _xrsession_Ended(object sender, EventArgs e)
        {
            XRSession xrSession = (XRSession)sender;
            Debug.Assert(xrSession == _xrsession);

            Console.WriteLine("_xrsession_Ended");
            _xrsession.CancelAnimationFrame(_xrAnimationHandle);
            ((IPlatformGame)_game).GetStrategy<ConcreteGame>()._suppressTick = false;
            _deviceState = XRDeviceState.Disabled;

            TouchController.DeviceHandle = null;
            _xrsession = null;
            _localSpace = null;
            _glLayer = null;
        }

        private void _xrsession_InputSourcesChanged(object sender, InputSourcesChangedEventArgs e)
        {
            Console.WriteLine("_xrsession_InputSourcesChanged");
        }

        XRFrame _currentXRFrame;
        XRSession _currentXRSession;
        XRRenderState _currentRenderState;
        
        private HeadsetState _headsetState;
        private Matrix _lproj;
        private Matrix _rproj;

        private void AnimationFrameCallback(TimeSpan time, XRFrame frame)
        {
            //Console.WriteLine("AnimationFrameCallback " + time.TotalSeconds.ToString());

            try
            {
                _currentXRFrame = frame;
                _currentXRSession = _currentXRFrame.Session;
                _currentRenderState = _currentXRSession.RenderState;

                XRReferenceSpace referenceSpace = _localSpace;
                if (_isTrackFloorLevelEnabled == true && _localFloorSpace != null)
                    referenceSpace = _localFloorSpace;

                // save Gamepad state
                _lbuttons = null;
                _laxes = null;
                _rbuttons = null;
                _raxes = null;
                foreach (XRInputSource inputSource in _xrsession.InputSources)
                {
                    XRHandedness hand = inputSource.Handedness;
                    Gamepad gamepad = inputSource.Gamepad;

                    if (gamepad != null)
                    {
                        switch (hand)
                        {
                            case XRHandedness.Left:
                                {
                                    _lbuttons = gamepad.Buttons;
                                    _laxes = gamepad.Axes;
                                }
                                break;
                            case XRHandedness.Right:
                                {
                                    _rbuttons = gamepad.Buttons;
                                    _raxes = gamepad.Axes;
                                }
                                break;
                        }
                    }

                    XRRigidTransform gripTransform = default;
                    XRRigidTransform pointerTransform = default;

                    XRSpace gripSpace = inputSource.GripSpace;
                    if (gripSpace != null)
                    {
                        using (XRPose grip = _currentXRFrame.GetPose(gripSpace, referenceSpace))
                        {
                            if (grip != null)
                                gripTransform = grip.Transform;
                        }
                    }

                    XRSpace pointerSpace = inputSource.TargetRaySpace;
                    if (pointerSpace != null)
                    {
                        using (XRPose pointer = _currentXRFrame.GetPose(pointerSpace, referenceSpace))
                        {
                            if (pointer != null)
                                pointerTransform = pointer.Transform;
                        }
                    }

                    switch (hand)
                    {
                        case XRHandedness.Left:
                            {
                                _handsState.LGripTransform = (Matrix)gripTransform.Matrix;
                                _handsState.LHandTransform = (Matrix)pointerTransform.Matrix;
                            }
                            break;
                        case XRHandedness.Right:
                            {
                                _handsState.RGripTransform = (Matrix)gripTransform.Matrix;
                                _handsState.RHandTransform = (Matrix)pointerTransform.Matrix;
                            }
                            break;
                    }
                }

                using (XRViewerPose viewerPose = _currentXRFrame.GetViewerPose(referenceSpace))
                {
                    if (viewerPose != null)
                    {
                        bool emulatedPosition = viewerPose.EmulatedPosition;
                        SysNumerics.Vector4? angularVelocity = viewerPose.AngularVelocity;
                        SysNumerics.Vector4? linearVelocity = viewerPose.LinearVelocity;
                        XRRigidTransform transform = viewerPose.Transform;

                        XRWebGLLayer glLayer = _currentRenderState.BaseLayer;
                        float? depthNear = _currentRenderState.DepthNear;
                        float? depthFar = _currentRenderState.DepthFar;
                        int w = glLayer.FramebufferWidth;
                        int h = glLayer.FramebufferHeight;
                        bool ign = glLayer.IgnoreDepthValues;
                        bool antialias = glLayer.Antialias;

                        foreach (XRView xrView in viewerPose.Views)
                        {
                            WebXREye eye = xrView.Eye;

                            XRViewport xrViewport = glLayer.GetViewport(xrView);

                            float aspect = (float)xrViewport.Width / (float)xrViewport.Height;

                            XRRigidTransform viewTransform = xrView.Transform;

                            _headsetState.HeadTransform = (Matrix)transform.Matrix;

                            switch (eye)
                            {
                                case WebXREye.None:
                                    {
                                        _lproj = (Matrix)xrView.ProjectionMatrix;
                                        _headsetState.HeadTransform = (Matrix)viewTransform.Matrix;
                                    }
                                    break;
                                case WebXREye.Left:
                                    {
                                        _lproj = (Matrix)xrView.ProjectionMatrix;
                                        _headsetState.LEyeTransform = (Matrix)viewTransform.Matrix;
                                    }
                                    break;
                                case WebXREye.Right:
                                    {
                                        _rproj = (Matrix)xrView.ProjectionMatrix;
                                        _headsetState.REyeTransform = (Matrix)viewTransform.Matrix;
                                    }
                                    break;
                            }

                            xrView.Dispose();
                        }

                        _xrInAnimationFrame = true;
                        try
                        {
                            ((IPlatformGame)_game).GetStrategy<ConcreteGame>()._suppressTick = false;

                            _game.Tick();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("XR AnimationFrameCallback: exception: "+ex.Message);
                            Console.WriteLine(ex.StackTrace);
                            throw;
                        }
                        finally
                        {
                            ((IPlatformGame)_game).GetStrategy<ConcreteGame>()._suppressTick = true;
                            _xrInAnimationFrame = false;
                        }

                    }
                }

            }
            finally
            {
                _currentXRFrame = null;
                _currentXRSession = null;
                _currentRenderState = null;

                // Request next frame
                _xrAnimationHandle = _xrsession.RequestAnimationFrame(this.AnimationFrameCallback);
            }
        }
        
        [Conditional("DEBUG")]
        public void CheckFramebufferStatus(WebGL2FramebufferType framebuffer)
        {
            var graphicsDevice = _graphics.GraphicsDevice;
            var GL = (IWebGL2RenderingContext)((IPlatformGraphicsContext)((IPlatformGraphicsDevice)graphicsDevice).Strategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;

            WebGL2FramebufferStatus status = GL.CheckFramebufferStatus(framebuffer);
            switch (status)
            {
                case WebGL2FramebufferStatus.FRAMEBUFFER_COMPLETE:
                    return;
                case WebGL2FramebufferStatus.FRAMEBUFFER_INCOMPLETE_ATTACHMENT:
                    throw new InvalidOperationException("Not all framebuffer attachment points are framebuffer attachment complete.");
                case WebGL2FramebufferStatus.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT:
                    throw new InvalidOperationException("No images are attached to the framebuffer.");
                case WebGL2FramebufferStatus.FRAMEBUFFER_UNSUPPORTED:
                    throw new InvalidOperationException("The combination of internal formats of the attached images violates an implementation-dependent set of restrictions.");
                case WebGL2FramebufferStatus.FRAMEBUFFER_INCOMPLETE_DIMENSIONS:
                    throw new InvalidOperationException("Not all attached images have the same dimensions.");
                case WebGL2FramebufferStatus.FRAMEBUFFER_INCOMPLETE_MULTISAMPLE:
                    throw new InvalidOperationException("The values of RENDERBUFFER_SAMPLES are different among attached renderbuffers, or are non-zero if the attached images are a mix of renderbuffers and textures.");

                default:
                    throw new InvalidOperationException("Framebuffer Incomplete.");
            }
        }

        private Matrix CreateProjection(Matrix xrproj, float nearZ, float farZ)
        {
            // extract FOV from the xrView.ProjectionMatrix
            float tanAngleLeft   = (1 + xrproj.M31) / xrproj.M11;
            float tanAngleRight  = (1 - xrproj.M31) / xrproj.M11;
            float tanAngleBottom = (1 + xrproj.M32) / xrproj.M22;
            float tanAngleTop    = (1 - xrproj.M32) / xrproj.M22;

            // create projection with nearZ/farZ
            GraphicsBackend graphicsBackend = _graphics.GraphicsDevice.Adapter.Backend;
            if (farZ == float.PositiveInfinity)
                return CreateInfiniteProjection(graphicsBackend, tanAngleLeft, tanAngleRight, tanAngleBottom, tanAngleTop, nearZ);
            else
                return CreateProjection(graphicsBackend, tanAngleLeft, tanAngleRight, tanAngleBottom, tanAngleTop, nearZ, farZ);
        }

        static unsafe Matrix CreateProjection(GraphicsBackend graphicsBackend,
                                                 float tanAngleLeft, float tanAngleRight, 
                                                 float tanAngleBottom, float tanAngleTop,
                                                 float nearZ, float farZ)
        {
            float tanAngleWidth  = tanAngleLeft + tanAngleRight;
            float tanAngleHeight = tanAngleBottom + tanAngleTop;

            // Set offsetZ to zero  for a [ 0,+1] Z clip space (D3D / Metal / Vulkan).
            // Set offsetZ to nearZ for a [-1,+1] Z clip space (OpenGL / GLES / WebGL).
            //float offsetZ = (graphicsBackend == GraphicsBackend.OpenGL || graphicsBackend == GraphicsBackend.GLES || graphicsBackend == GraphicsBackend.WebGL) 
            //              ? nearZ 
            //              : 0;
            const float offsetZ = 0;

            Matrix result;

            // normal projection
            result.M11 = (2f) / tanAngleWidth;
            result.M12 = result.M13 = result.M14 = 0;

            result.M22 = (2f) / tanAngleHeight;
            result.M21 = result.M23 = result.M24 = 0;

            result.M31 = -(tanAngleRight - tanAngleLeft) / tanAngleWidth;
            result.M32 = -(tanAngleTop - tanAngleBottom) / tanAngleHeight;
            result.M33 = (float)((farZ + (double)offsetZ) / ((double)nearZ - farZ));
            result.M34 = -1;

            result.M43 = (float)((farZ * ((double)nearZ + offsetZ)) / ((double)nearZ - farZ));
            result.M41 = result.M42 = result.M44 = 0;

            return result;
        }

        static unsafe Matrix CreateInfiniteProjection(GraphicsBackend graphicsBackend,
                                                 float tanAngleLeft, float tanAngleRight,
                                                 float tanAngleBottom, float tanAngleTop,
                                                 float nearZ)
        {
            float tanAngleWidth = tanAngleLeft + tanAngleRight;
            float tanAngleHeight = tanAngleBottom + tanAngleTop;

            // Set offsetZ to zero  for a [ 0,+1] Z clip space (D3D / Metal / Vulkan).
            // Set offsetZ to nearZ for a [-1,+1] Z clip space (OpenGL / GLES / WebGL).
            //float offsetZ = (graphicsBackend == GraphicsBackend.OpenGL || graphicsBackend == GraphicsBackend.GLES || graphicsBackend == GraphicsBackend.WebGL) 
            //              ? nearZ 
            //              : 0;
            const float offsetZ = 0;

            Matrix result;

            // place the far plane at infinity
            result.M11 = 2.0f / tanAngleWidth;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;

            result.M21 = 0.0f;
            result.M22 = 2.0f / tanAngleHeight;
            result.M23 = 0.0f;
            result.M24 = 0.0f;

            result.M31 = -(tanAngleRight - tanAngleLeft) / tanAngleWidth;
            result.M32 = -(tanAngleTop - tanAngleBottom) / tanAngleHeight;
            result.M33 = -1.0f;
            result.M34 = -1.0f;

            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = -(nearZ + offsetZ);
            result.M44 = 0.0f;

            return result;
        }




        private GamePadState _gamePadState;
        GamepadButton[] _lbuttons;
        float[] _laxes;
        GamepadButton[] _rbuttons;
        float[] _raxes;

        internal void GetCapabilities(TouchControllerType controllerType, ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected, ref Buttons buttons, ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor, ref bool hasVoiceSupport)
        {
            gamePadType = GamePadType.GamePad;

            if (controllerType == TouchControllerType.LTouch)
            {
                if (_lbuttons != null)
                {
                    isConnected = true;

                    //// left buttons
                    if (_lbuttons != null && _lbuttons.Length >= 4)
                        buttons |= Buttons.X;
                    if (_lbuttons != null && _lbuttons.Length >= 5)
                        buttons |= Buttons.Y;
                    if (_lbuttons != null && _lbuttons.Length >= 3)
                        buttons |= Buttons.LeftStick;
                }
            }

            if (controllerType == TouchControllerType.RTouch)
            {
                if (_rbuttons != null)
                {
                    isConnected = true;

                    //// right buttons  
                    if (_rbuttons != null && _rbuttons.Length >= 4 && _rbuttons[4].Pressed)
                        buttons |= Buttons.A;
                    if (_rbuttons != null && _rbuttons.Length >= 5 && _rbuttons[5].Pressed)
                        buttons |= Buttons.B;
                    if (_rbuttons != null && _rbuttons.Length >= 3 && _rbuttons[3].Pressed)
                        buttons |= Buttons.RightStick;
                }
            }

            if (controllerType == TouchControllerType.Touch)
            {
                if (_lbuttons != null && _rbuttons != null)
                {
                    isConnected = true;

                    //// left buttons
                    if (_lbuttons != null && _lbuttons.Length >= 4)
                        buttons |= Buttons.X;
                    if (_lbuttons != null && _lbuttons.Length >= 5)
                        buttons |= Buttons.Y;
                    if (_lbuttons != null && _lbuttons.Length >= 3)
                        buttons |= Buttons.LeftStick;

                    //// right buttons  
                    if (_rbuttons != null && _rbuttons.Length >= 4 && _rbuttons[4].Pressed)
                        buttons |= Buttons.A;
                    if (_rbuttons != null && _rbuttons.Length >= 5 && _rbuttons[5].Pressed)
                        buttons |= Buttons.B;
                    if (_rbuttons != null && _rbuttons.Length >= 3 && _rbuttons[3].Pressed)
                        buttons |= Buttons.RightStick;
                }
            }


            foreach (XRInputSource inputSource in _xrsession.InputSources)
            {
                XRHandedness hand = inputSource.Handedness;
                Gamepad gamepad = inputSource.Gamepad;

                if (gamepad != null)
                {
                    switch (hand)
                    {
                        case XRHandedness.Left:
                            {
                                if (controllerType == TouchControllerType.LTouch
                                || controllerType == TouchControllerType.Touch)
                                {
                                    if (gamepad.VibrationActuator != null)
                                        hasLeftVibrationMotor = true;
                                }
                            }
                            break;
                        case XRHandedness.Right:
                            {
                                if (controllerType == TouchControllerType.RTouch
                                || controllerType == TouchControllerType.Touch)
                                {
                                    if (gamepad.VibrationActuator != null)
                                        hasRightVibrationMotor = true;
                                }
                            }
                            break;
                    }
                }
            }
        }

        internal GamePadState GetGamePadState(TouchControllerType controllerType)
        {
            Vector2 leftStick = default;
            Vector2 rightStick = default;

            if (_laxes != null && _laxes.Length >= 4)
                leftStick = new Vector2(_laxes[2], -_laxes[3]);
            if (_raxes != null && _raxes.Length >= 4)
                rightStick = new Vector2(_raxes[2], -_raxes[3]);

            GamePadThumbSticks thumbSticks = new GamePadThumbSticks(
                leftStick,
                rightStick);

            float leftTrigger = 0;
            float rightTrigger = 0;

            if (_lbuttons != null)
                leftTrigger = _lbuttons[0].Value;
            if (_rbuttons != null)
                rightTrigger = _rbuttons[0].Value;

            GamePadTriggers triggers = new GamePadTriggers(
                leftTrigger, // LeftTrigger
                rightTrigger  // RightTrigger
            );

            GamePadTriggers grips = default(GamePadTriggers);
            Buttons buttons = default(Buttons);
            Buttons touches = default(Buttons);

            float leftGrip = 0;
            float rightGrip = 0;

            if (_lbuttons != null && _lbuttons.Length >= 1)
                leftGrip = _lbuttons[1].Value;
            if (_rbuttons != null && _rbuttons.Length >= 1)
                rightGrip = _rbuttons[1].Value;

            grips = new GamePadTriggers(
                    leftTrigger: leftGrip,
                    rightTrigger: rightGrip);

            //// left buttons
            if (_lbuttons != null && _lbuttons.Length >= 4 && _lbuttons[4].Pressed)
                buttons |= Buttons.X;
            if (_lbuttons != null && _lbuttons.Length >= 5 && _lbuttons[5].Pressed)
                buttons |= Buttons.Y;
            if (_lbuttons != null && _lbuttons.Length >= 3 && _lbuttons[3].Pressed)
                buttons |= Buttons.LeftStick;
            if (_lbuttons != null && _lbuttons[0].Pressed)
                buttons |= Buttons.LeftTrigger;
            if (_lbuttons != null && _lbuttons.Length >= 1 && _lbuttons[1].Pressed)
                buttons |= Buttons.LeftGrip;

            //// right buttons  
            if (_rbuttons != null && _rbuttons.Length >= 4 && _rbuttons[4].Pressed)
                buttons |= Buttons.A;
            if (_rbuttons != null && _rbuttons.Length >= 5 && _rbuttons[5].Pressed)
                buttons |= Buttons.B;
            if (_rbuttons != null && _rbuttons.Length >= 3 && _rbuttons[3].Pressed)
                buttons |= Buttons.RightStick;
            if (_lbuttons != null && _rbuttons[0].Pressed)
                buttons |= Buttons.RightTrigger;
            if (_rbuttons != null && _rbuttons.Length >= 1 && _rbuttons[1].Pressed)
                buttons |= Buttons.RightGrip;

            float TriggerThresholdOn = 0.6f;
            float TriggerThresholdOff = 0.7f;
            float ThumbstickThresholdOn = 0.5f;
            float ThumbstickThresholdOff = 0.4f;
            //// virtual trigger buttons

            //buttons |= _virtualButtons;

            // left touches

            if (_lbuttons != null && _lbuttons.Length >= 4 && _lbuttons[4].Touched)
                touches |= Buttons.X;
            if (_lbuttons != null && _lbuttons.Length >= 5 && _lbuttons[5].Touched)
                touches |= Buttons.Y;
            if (_lbuttons != null && _lbuttons.Length >= 3 && _lbuttons[3].Touched)
                touches |= Buttons.LeftStick;
            if (_lbuttons != null && _lbuttons[0].Touched)
                touches |= Buttons.LeftTrigger;
            if (_lbuttons != null && _lbuttons.Length >= 1 && _lbuttons[1].Touched)
                touches |= Buttons.LeftGrip;

            // right touches
            if (_rbuttons != null && _rbuttons.Length >= 4 && _rbuttons[4].Touched)
                touches |= Buttons.A;
            if (_rbuttons != null && _rbuttons.Length >= 5 && _rbuttons[5].Touched)
                touches |= Buttons.B;
            if (_rbuttons != null && _rbuttons.Length >= 3 && _rbuttons[3].Touched)
                touches |= Buttons.RightStick;
            if (_rbuttons != null && _rbuttons[0].Touched)
                touches |= Buttons.RightTrigger;
            if (_rbuttons != null && _rbuttons.Length >= 1 && _rbuttons[1].Touched)
                touches |= Buttons.RightGrip;

            _gamePadState = new GamePadState(
                thumbSticks: thumbSticks,
                triggers: triggers,
                grips: grips,
                touchButtons: new GamePadTouchButtons(buttons, touches),
                dPad: default(GamePadDPad)
                );

            return _gamePadState;
        }

        internal bool SetVibration(TouchControllerType controllerType, float amplitude)
        {

            bool result = false;

            foreach (XRInputSource inputSource in _xrsession.InputSources)
            {
                XRHandedness hand = inputSource.Handedness;
                Gamepad gamepad = inputSource.Gamepad;

                if (gamepad != null)
                {
                    switch (hand)
                    {
                        case XRHandedness.Left:
                            {
                                if (controllerType == TouchControllerType.LTouch
                                || controllerType == TouchControllerType.Touch)
                                {
                                    if (gamepad.VibrationActuator != null)
                                        result |= gamepad.VibrationActuator.Pulse(amplitude, 400);
                                }
                            }
                            break;
                        case XRHandedness.Right:
                            {
                                if (controllerType == TouchControllerType.RTouch
                                || controllerType == TouchControllerType.Touch)
                                {
                                    if (gamepad.VibrationActuator != null)
                                        result |= gamepad.VibrationActuator.Pulse(amplitude, 400);
                                }
                            }
                            break;
                    }
                }
            }

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