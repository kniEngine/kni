// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Input.XR;
using Microsoft.Xna.Framework.XR;
using nkast.LibOXR;
using Silk.NET.Core;
using Silk.NET.OpenXR;
using SilkXR = Silk.NET.OpenXR.XR;
using XrAction = Silk.NET.OpenXR.Action;
//using OpenGL = Microsoft.Xna.Platform.Graphics.OpenGL;

namespace Microsoft.Xna.Platform.XR
{
    public interface IOculusDeviceStrategy
    {
        SessionState SessionState { get; }
    }

    internal class ConcreteXRDevice : XRDeviceStrategy
        , IOculusDeviceStrategy
    {
        const string _engineName = "KNI Engine";

        private string _applicationName;
        private IServiceProvider _services;
        private IGraphicsDeviceService _graphics;
        XRSessionMode _sessionMode;
        XRDeviceState _deviceState;
        bool _isTrackFloorLevelEnabled = false;

        OxrAPI _xrAPI;
        IList<string> _extensions;
        OxrInstance _oxrInstance;
        OxrSession _oxrSession;

        int _CpuLevel;
        int _GpuLevel;

        float[] _SupportedDisplayRefreshRates;
        uint _RequestedDisplayRefreshRateIndex;
        uint _NumSupportedDisplayRefreshRates;
        xrGetDisplayRefreshRateFB _pfnxrGetDisplayRefreshRateFB;
        xrRequestDisplayRefreshRateFB _pfnxrRequestDisplayRefreshRateFB;

        OxrSpace _HeadSpace;

        OxrSpace _LocalSpace;
        OxrSpace _LocalFloorSpace;
        OxrSpace _FakeStageSpace;
        OxrSpace _StageSpace;

        bool _isLocalFloorSupported;
        bool _isStageSupported;
        OxrSpace _CurrentStageSpace;

        OxrPassthroughFB _passthroughFB;
        OxrPassthroughLayerFB _passthroughLayerFB;

        bool _useSimpleProfile;

        OxrActionSet _runningActionSet;
        public XrAction _actionButtonA;
        public XrAction _actionButtonB;
        public XrAction _actionButtonRightThumbstick;
        public XrAction _actionButtonX;
        public XrAction _actionButtonY;
        public XrAction _actionButtonLeftThumbstick;

        public XrAction _actionButtonAtouch;
        public XrAction _actionButtonBtouch;
        public XrAction _actionButtonRightThumbsticktouch;
        public XrAction _actionButtonXtouch;
        public XrAction _actionButtonYtouch;
        public XrAction _actionButtonLeftThumbsticktouch;

        public XrAction _moveOnXActionL;
        public XrAction _moveOnXActionR;
        public XrAction _moveOnYActionL;
        public XrAction _moveOnYActionR;
        public XrAction _moveOnJoystickActionL;
        public XrAction _moveOnJoystickActionR;

        public XrAction _actionTriggerLeftTouch;
        public XrAction _actionTriggerRightTouch;
        // BUG: Grip touch doesn't work on native OpenXR.
        // https://communityforums.atmeta.com/t5/OpenXR-Development/unable-to-get-grip-button-input-on-quest-1-getting-path/td-p/833021
        //public XrAction _actionGripLeftTouch;
        //public XrAction _actionGripRightTouch;

        public XrAction _vibrateLeftFeedback;
        public XrAction _vibrateRightFeedback;

        XrAction _aimPoseAction;
        XrAction _gripPoseAction;
        ActionPath _leftHandPath;
        ActionPath _rightHandPath;

        OxrSpace _leftControllerAimSpace;
        OxrSpace _rightControllerAimSpace;
        OxrSpace _leftControllerGripSpace;
        OxrSpace _rightControllerGripSpace;


        const int FRAMEBUFFER_SRGB = 0x8DB9;
        const long SRGB8_ALPHA8 = (long)Graphics.OpenGL.PixelInternalFormat.Srgba;

        long _frameIndex = 0;

        private HeadsetState _headsetState;
        private HandsState _handsState;

        OxrSwapChainDataBase[] _swapChainData = new OxrSwapChainDataBase[2];

        Posef _xfLocalFromHead;
        Posef _xfLocalFloorFromHead;
        Posef _xfStageFromHead;
        Posef _xfStageFromLocal;
        View[] _projections = new View[ovrMaxNumEyes];

        Pose3[] _viewTransform = new Pose3[ovrMaxNumEyes];

        FrameState _frameState;
        ovrRenderer _Renderer = new ovrRenderer();

        ovrCompositorLayer_Union[] _Layers = new ovrCompositorLayer_Union[ovrMaxLayerCount];

        internal OxrSession Session { get { return _oxrSession; } }

        public OxrInstance Instance { get { return _oxrInstance; } }

        //OvrPosef[] _hmdToEyePose = new OvrPosef[2];
        //OvrLayerEyeFov _layer;

        //OvrStatusBits _statusFlags;

        public override bool IsVRSupported
        {
            get { return true; }
        }

        public override bool IsARSupported
        {
            get
            {
                if (_extensions.Contains(nameof(OxrExtensions.XR_FB_passthrough)))
                    return true;

                return false;
            }
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
            : this(applicationName, game.Services)
        {
        }

        public unsafe ConcreteXRDevice(string applicationName, IServiceProvider services)
        {
            this._applicationName = applicationName;
            this._services = services;

            _graphics = _services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;

            if (_graphics == null)
                throw new ArgumentNullException("IGraphicsDeviceService not found in services.");

            _graphics.DeviceResetting += GraphicsDeviceResetting;
            _graphics.DeviceReset += GraphicsDeviceReset;
            _graphics.DeviceDisposing += GraphicsDeviceDisposing;


            Android.App.Activity activity = _services.GetService(typeof(Android.App.Activity)) as Android.App.Activity;
            if (activity == null)
                throw new InvalidOperationException("Activity not found in services.");

            LoaderInitInfoAndroidKHR loaderInitInfo = new LoaderInitInfoAndroidKHR(
                                applicationVM: (void*)Java.Interop.JniRuntime.CurrentRuntime.InvocationPointer,
                                applicationContext: (void*)Android.Runtime.JNIEnv.ToJniHandle(activity)
                                );
            Result xrResult;

            xrResult = OxrAPI.InitializeLoader((LoaderInitInfoBaseHeaderKHR*)(&loaderInitInfo), out _xrAPI);
            if (xrResult != Result.Success)
            {
                Console.WriteLine($"Failed to initialize OpenXR Loader: {xrResult}");
                this._deviceState = XRDeviceState.NoPermissions;
            }
            else
                Console.WriteLine("OpenXR Loader initialized successfully.");

            // Log available layers.
            {
                IList<string> layers = _xrAPI.GetLayers();
                foreach (string layerName in layers)
                {
                    Console.WriteLine("Found layer {0}", layerName);
                }
            }

            // Log available extensions.
            _extensions = _xrAPI.GetExtensions();
            for (int i = 0; i < _extensions.Count; i++)
            {
                string extensionName = _extensions[i];
                Console.WriteLine("Extension {0} = {1}", i, extensionName);
            }

            this._deviceState = XRDeviceState.Disabled;
        }

        public override int BeginSessionAsync(XRSessionMode sessionMode)
        {
            if (sessionMode == XRSessionMode.AR)
            {
                if (!this.IsARSupported)
                    throw new ArgumentException("mode");
            }

            _sessionMode = sessionMode;

            GraphicsDevice graphicsDevice = _graphics.GraphicsDevice;
            if (graphicsDevice == null)
            {
                throw new ArgumentNullException("graphics.GraphicsDevice");
                //_graphics.DeviceCreated += GraphicsDeviceCreated;
            }

            int result = Initialize(graphicsDevice);
            if (result == 0)
            {
                ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += ovrApp_HandleXrEvents;
            }

            return result;
        }

        bool stageBoundsDirty = true;
        public unsafe override int BeginFrame()
        {
            ovrApp_HandleXrEvents();

            bool notVisible;

            if (_SessionActive == false)
            {
                //return -1;
            }

            Result xrResult;

            if (_leftControllerAimSpace == null)
            {
                _leftControllerAimSpace = _oxrSession.CreateActionSpace(_aimPoseAction, _leftHandPath);
            }
            if (_rightControllerAimSpace == null)
            {
                _rightControllerAimSpace = _oxrSession.CreateActionSpace(_aimPoseAction, _rightHandPath);
            }
            if (_leftControllerGripSpace == null)
            {
                _leftControllerGripSpace = _oxrSession.CreateActionSpace(_gripPoseAction, _leftHandPath);
            }
            if (_rightControllerGripSpace == null)
            {
                _rightControllerGripSpace = _oxrSession.CreateActionSpace(_gripPoseAction, _rightHandPath);
            }

            // Create the scene if not yet created.
            // The scene is created here to be able to show a loading icon.
            if (!ovrScene_IsCreated(ref _Scene))
            {
                ovrScene_Create(ref _Scene);
            }

            if (stageBoundsDirty)
            {
                UpdateStageBounds();
                stageBoundsDirty = false;
            }

            // NOTE: OpenXR does not use the concept of frame indices. Instead,
            // XrWaitFrame returns the predicted display time.

            //return 0;

            xrResult = _oxrSession.WaitFrame(out FrameWaitInfo waitFrameInfo, out _frameState);
            Debug.Assert(xrResult == Result.Success, "WaitFrame");

            // Get the HMD pose, predicted for the middle of the time period during which
            // the new eye images will be displayed. The number of frames predicted ahead
            // depends on the pipeline depth of the engine and the synthesis rate.
            // The better the prediction, the less black will be pulled in at the edges.
            xrResult = _oxrSession.BeginFrame(out FrameBeginInfo beginFrameDesc);
            Debug.Assert(xrResult == Result.Success, "WaitFrame");

            long time = _frameState.PredictedDisplayTime;
            //double time = _ovrSession.GetPredictedDisplayTime(_frameIndex);

            xrResult = _oxrInstance.LocateSpace(
                _HeadSpace, _LocalSpace, _frameState.PredictedDisplayTime, out _xfLocalFromHead, out SpaceLocationFlags localLocationFlags);
            Debug.Assert(xrResult == Result.Success, "LocateSpace");

            if (_LocalFloorSpace != null)
            {
                xrResult = _oxrInstance.LocateSpace(
                    _HeadSpace, _LocalFloorSpace, _frameState.PredictedDisplayTime, out _xfLocalFloorFromHead, out SpaceLocationFlags localFloorLocationFlags);
                Debug.Assert(xrResult == Result.Success, "LocateSpace");
            }

            xrResult = _oxrInstance.LocateSpace(
                _HeadSpace, _CurrentStageSpace, _frameState.PredictedDisplayTime, out _xfStageFromHead, out SpaceLocationFlags stageLocationFlags);
            Debug.Assert(xrResult == Result.Success, "LocateSpace");

            xrResult = _oxrInstance.LocateSpace(
                _LocalSpace, _CurrentStageSpace, _frameState.PredictedDisplayTime, out _xfStageFromLocal, out SpaceLocationFlags stage2LocationFlags);
            Debug.Assert(xrResult == Result.Success, "LocateSpace");

            uint projectionCapacityInput = ovrMaxNumEyes;
            uint projectionCountOutput = projectionCapacityInput;

            xrResult = _oxrSession.LocateView(
                _viewportConfig.ViewConfigurationType,
                _frameState.PredictedDisplayTime,
                _HeadSpace,
                out ViewState viewState,
                projectionCapacityInput,
                out projectionCountOutput,
                _projections);
            Debug.Assert(xrResult == Result.Success, "xrLocateView");


            Pose3 HeadPose = (_isTrackFloorLevelEnabled) ? _xfLocalFloorFromHead.ToPose3() : _xfLocalFromHead.ToPose3();
            _headsetState.HeadPose = HeadPose;

            for (int eye = 0; eye < ovrMaxNumEyes; eye++)
            {
                Pose3 xfHeadFromEye   = _projections[eye].Pose.ToPose3();
                Pose3 xfStageFromEye  = xfHeadFromEye * HeadPose;

                _viewTransform[eye]   = xfStageFromEye;

                switch (eye)
                {
                    case 0:
                        _headsetState.LEyePose = _viewTransform[eye];
                        break;
                    case 1:
                        _headsetState.REyePose = _viewTransform[eye];
                        break;
                }
            }

            // update input information
            XrAction[] controller = new XrAction[] {_aimPoseAction, _gripPoseAction, _aimPoseAction, _gripPoseAction};
            ActionPath[] subactionPath = new ActionPath[] { _leftHandPath, _leftHandPath, _rightHandPath, _rightHandPath };
            OxrSpace[] controllerSpace = new OxrSpace[]
            {
                _leftControllerAimSpace,
                _leftControllerGripSpace,
                _rightControllerAimSpace,
                _rightControllerGripSpace,
            };

            for (int i = 0; i < 4; i++)
            {
                // ActionPoseIsActive
                xrResult = _oxrSession.GetActionStatePose(controller[i], subactionPath[i], out ActionStatePose state);
                Debug.Assert(xrResult == Result.Success, "GetActionStatePose");
                if (state.IsActive != 0)
                {
                    //Console.WriteLine("Controller {0} is active." + i);

                    LocVel lv = GetSpaceLocVel(controllerSpace[i], _frameState.PredictedDisplayTime);
                    _Scene.TrackedController[i].Active =
                        (lv.LocationFlags & SpaceLocationFlags.PositionValidBit) != 0;
                    _Scene.TrackedController[i].Pose = lv.Pose;
                    _Scene.TrackedController[i].LinearVelocity = lv.LinearVelocity;

                    if (i == 0)
                    {
                        if ((lv.LocationFlags & SpaceLocationFlags.PositionValidBit) != 0)
                        {
                            _handsState.LHandPose = _Scene.TrackedController[i].Pose;
                            _handsState.LHandLinearVelocity = _Scene.TrackedController[i].LinearVelocity;
                        }
                        else
                        {
                            _handsState.LHandPose = Pose3.Identity;
                            _handsState.LHandLinearVelocity = Vector3.Zero;
                        }
                    }
                    if (i == 1)
                    {
                        if ((lv.LocationFlags & SpaceLocationFlags.PositionValidBit) != 0)
                        {
                            _handsState.LGripPose = _Scene.TrackedController[i].Pose;
                        }
                        else
                        {
                            _handsState.LGripPose = Pose3.Identity;
                        }
                    }
                    if (i == 2)
                    {
                        if ((lv.LocationFlags & SpaceLocationFlags.PositionValidBit) != 0)
                        {
                            _handsState.RHandPose = _Scene.TrackedController[i].Pose;
                            _handsState.RHandLinearVelocity = _Scene.TrackedController[i].LinearVelocity;
                        }
                        else
                        {
                            _handsState.RHandPose = Pose3.Identity;
                            _handsState.RHandLinearVelocity = Vector3.Zero;
                        }
                    }
                    if (i == 3)
                    {
                        if ((lv.LocationFlags & SpaceLocationFlags.PositionValidBit) != 0)
                        {
                            _handsState.RGripPose = _Scene.TrackedController[i].Pose;
                        }
                        else
                        {
                            _handsState.RGripPose = Pose3.Identity;
                        }
                    }


                    float dt = 0.01f; // use 0.2f for testing velocity vectors
                    {
                        _Scene.TrackedController[i].Pose.Translation += lv.LinearVelocity * dt;
                    }
                }
                else
                {
                    //Console.WriteLine("Controller {0} is inactive."+i);

                    _Scene.TrackedController[i].Clear();
                }
            }

            // OpenXR input
            {
                // sync action data
                xrResult = _oxrSession.SyncActions(_runningActionSet, 0);
                if (xrResult != Result.SessionNotFocused)
                    Debug.Assert(xrResult == Result.Success, "SyncActions");

                // query input action states
                ActionStateGetInfo getInfo = new ActionStateGetInfo(StructureType.ActionStateGetInfo);
                getInfo.SubactionPath = 0;

                ActionStateBoolean thumbstickLClickState = _oxrSession.GetActionStateBoolean(_actionButtonLeftThumbstick);
                ActionStateBoolean thumbstickRClickState = _oxrSession.GetActionStateBoolean(_actionButtonRightThumbstick);

                // Update app logic based on input
                if ((thumbstickLClickState.ChangedSinceLastSync != 0 
                  && thumbstickLClickState.CurrentState != 0) 
                ||  (thumbstickRClickState.ChangedSinceLastSync != 0
                  && thumbstickRClickState.CurrentState != 0)
                )
                {
                    float currentRefreshRate = 0.0f;
                    xrResult = _pfnxrGetDisplayRefreshRateFB(_oxrSession.Session, &currentRefreshRate);
                    Debug.Assert(xrResult == Result.Success, "_pfnxrGetDisplayRefreshRateFB");
                    Console.WriteLine("Current Display Refresh Rate: {0}", currentRefreshRate);

                    int requestedRateIndex = (int) (_RequestedDisplayRefreshRateIndex++ %
                        _NumSupportedDisplayRefreshRates);

                    float requestRefreshRate =
                        _SupportedDisplayRefreshRates[requestedRateIndex];
                    Console.WriteLine("Requesting Display Refresh Rate: {0}", requestRefreshRate);
                    xrResult = _pfnxrRequestDisplayRefreshRateFB(_oxrSession.Session, requestRefreshRate);
                    Debug.Assert(xrResult == Result.Success, "_pfnxrRequestDisplayRefreshRateFB");
                }

                // The KHR simple profile doesn't have these actions, so the getters will fail
                // and flood the log with errors.
                if (_useSimpleProfile == false)
                {
                    ActionStateFloat moveXStateL = _oxrSession.GetActionStateFloat(_moveOnXActionL);
                    ActionStateFloat moveXStateR = _oxrSession.GetActionStateFloat(_moveOnXActionR);

                    ActionStateFloat moveYStateL = _oxrSession.GetActionStateFloat(_moveOnYActionL);
                    ActionStateFloat moveYStateR = _oxrSession.GetActionStateFloat(_moveOnYActionR);

                    if (moveXStateL.ChangedSinceLastSync != 0)
                    {
                        //appQuadPositionX = moveYStateL.CurrentState; 
                        Console.WriteLine("L Grip " + moveXStateL.CurrentState);
                    }
                    if (moveYStateR.ChangedSinceLastSync != 0)
                    {
                        //appQuadPositionX = moveXStateR.CurrentState;
                        Console.WriteLine("R Grip " + moveXStateR.CurrentState);
                    }

                    if (moveYStateL.ChangedSinceLastSync != 0)
                    {
                        //appQuadPositionY = moveYStateL.CurrentState; 
                        Console.WriteLine("L Trigger " + moveYStateL.CurrentState);
                    }
                    if (moveYStateR.ChangedSinceLastSync != 0)
                    {
                        //appQuadPositionY = moveYStateR.CurrentState;
                        Console.WriteLine("R Trigger " + moveYStateR.CurrentState);
                    }

                    ActionStateVector2f moveJoystickStateL =
                    _oxrSession.GetActionStateVector2(_moveOnJoystickActionL);
                    if (moveJoystickStateL.ChangedSinceLastSync!=0)
                    {
                        //appCylPositionX = moveJoystickStateL.currentState.x;
                        //appCylPositionY = moveJoystickStateL.currentState.y;
                        //Console.WriteLine("L ThumpStick " + moveJoystickStateL.CurrentState.X + " , " + moveJoystickStateL.CurrentState.Y);
                    }

                    ActionStateVector2f moveJoystickStateR =
                    _oxrSession.GetActionStateVector2(_moveOnJoystickActionR);
                    if (moveJoystickStateR.ChangedSinceLastSync != 0)
                    {
                        //appCylPositionX = moveJoystickStateR.currentState.x;
                        //appCylPositionY = moveJoystickStateR.currentState.y;
                        //Console.WriteLine("R ThumpStick "+ moveJoystickStateR.CurrentState.X +" , "+ moveJoystickStateR.CurrentState.Y);
                    }
                }
            }

            return 0;
        }

        public override HeadsetState GetHeadsetState()
        {
            return _headsetState;
        }

        public override IEnumerable<XREye> GetEyes()
        {
            yield return XREye.Left;
            yield return XREye.Right;
        }

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            int eyeIndex = (int)eye - 1;
            RenderTarget2D rt = _Renderer.FrameBuffer[eyeIndex].RenderTarget;
            return rt;
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            int eyeIndex = (int)eye - 1;

            GraphicsBackend graphicsBackend = _graphics.GraphicsDevice.Adapter.Backend;

            Fovf fov = _projections[eyeIndex].Fov;
            float tanLeft = (float)Math.Tan(fov.AngleLeft);
            float tanRight = (float)Math.Tan(fov.AngleRight);
            float tanDown = (float)Math.Tan(fov.AngleDown);
            float tanUp = (float)Math.Tan(fov.AngleUp);

            Matrix proj = XrMatrixf_CreateProjection(graphicsBackend, tanLeft, tanRight, tanUp, tanDown, znear, zfar);
            return proj;
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            int eyeIndex = (int)eye - 1;

            GraphicsDevice device = this._graphics.GraphicsDevice;
            device.SetRenderTarget(null);

            ovrRenderer_RenderFrame_Acquire(ref _Renderer.FrameBuffer[eyeIndex]);

            uint textureIndex = _Renderer.FrameBuffer[eyeIndex].TextureSwapChainIndex;

            uint txSC = _Renderer.FrameBuffer[eyeIndex].ColorSwapChainImage[textureIndex].Image;

            //ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)_graphics.GraphicsDevice.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            //var GL = adapter.Ogl;

            // bind DrawFramebuffer
            Android.Opengl.GLES30.GlBindFramebuffer(Android.Opengl.GLES30.GlDrawFramebuffer, _Renderer.FrameBuffer[eyeIndex].FrameBuffers[textureIndex]);
            //GL.BindFramebuffer(OpenGL.FramebufferTarget.DrawFramebuffer, _Renderer.FrameBuffer[eyeIndex].FrameBuffers[textureIndex]);
            //GL.CheckGLError();

            // fix oversaturation
            Android.Opengl.GLES30.GlDisable(FRAMEBUFFER_SRGB);

            // copy rendertarget to texture
            {
                int[] sourceFramebuffer = new int[1];
                Android.Opengl.GLES30.GlGenFramebuffers(sourceFramebuffer.Length, sourceFramebuffer, 0);
                //int sourceFramebuffer = GL.GenFramebuffer();
                //GL.CheckGLError();

                Android.Opengl.GLES30.GlBindFramebuffer(Android.Opengl.GLES30.GlReadFramebuffer, sourceFramebuffer[0]);
                //GL.BindFramebuffer(OpenGL.FramebufferTarget.ReadFramebuffer, sourceFramebuffer);
                //GL.CheckGLError();

                Android.Opengl.GLES30.GlFramebufferTexture2D(
                    Android.Opengl.GLES30.GlReadFramebuffer,
                    Android.Opengl.GLES30.GlColorAttachment0,
                    Android.Opengl.GLES30.GlTexture2d, (int)rt.GetSharedHandle(), 0);
                //GL.FramebufferTexture2D(
                //    OpenGL.FramebufferTarget.ReadFramebuffer,
                //    OpenGL.FramebufferAttachment.ColorAttachment0,
                //    OpenGL.TextureTarget.Texture2D, (int)rt.GetSharedHandle(), 0
                //);
                //GL.CheckGLError();

                // copy and y-flip
                Android.Opengl.GLES30.GlBlitFramebuffer(0, 0, rt.Width, rt.Height,
                                   0, _Renderer.FrameBuffer[eyeIndex].Height - 1, _Renderer.FrameBuffer[eyeIndex].Width, 0,
                                   Android.Opengl.GLES30.GlColorBufferBit, Android.Opengl.GLES30.GlNearest);
                //GL.BlitFramebuffer(0, 0, rt.Width, rt.Height,
                //                   0, _Renderer.FrameBuffer[eyeIndex].Height - 1, _Renderer.FrameBuffer[eyeIndex].Width, 0,
                //                   OpenGL.ClearBufferMask.ColorBufferBit, OpenGL.BlitFramebufferFilter.Nearest);
                //GL.CheckGLError();

                Android.Opengl.GLES30.GlBindFramebuffer(Android.Opengl.GLES30.GlReadFramebuffer, sourceFramebuffer[0]);
                //GL.BindFramebuffer(OpenGL.FramebufferTarget.ReadFramebuffer, 0);
                //GL.CheckGLError();

                Android.Opengl.GLES30.GlDeleteFramebuffers(sourceFramebuffer.Length, sourceFramebuffer, 0);
                //GL.DeleteFramebuffer(sourceFramebuffer);
                //GL.CheckGLError();
            }

            ovrRenderer_RenderFrame_Release(ref _Renderer, eyeIndex);

            // unbind DrawFramebuffer
            Android.Opengl.GLES30.GlBindFramebuffer(Android.Opengl.GLES30.GlDrawFramebuffer, 0);
            //GL.BindFramebuffer(OpenGL.FramebufferTarget.DrawFramebuffer, 0);
            //GL.CheckGLError();
        }

        public unsafe override int EndFrame()
        {
            Result xrResult;
            
            OxrSpace currentSpace = (_isTrackFloorLevelEnabled) ? _LocalFloorSpace : _LocalSpace;

            // Set-up the compositor layers for this frame.
            // NOTE: Multiple independent layers are allowed, but they need to be added
            // in a depth consistent order.


            uint LayerCount = 0;
            _Layers = new ovrCompositorLayer_Union[ovrMaxLayerCount];

            if (this.SessionMode == XRSessionMode.AR)
            {
                CompositionLayerPassthroughFB passthroughLayer = default;
                passthroughLayer.Type = StructureType.CompositionLayerPassthroughFB;
                passthroughLayer.LayerHandle = _passthroughLayerFB.PassthroughLayerFB;
                passthroughLayer.Space = default;
                passthroughLayer.Flags = CompositionLayerFlags.BlendTextureSourceAlphaBit;

                _Layers[LayerCount++].PassthroughFB = passthroughLayer;
            }

            CompositionLayerProjectionView[] projection_layer_elements = new CompositionLayerProjectionView[]
            {
                new CompositionLayerProjectionView(StructureType.CompositionLayerProjectionView),
                new CompositionLayerProjectionView(StructureType.CompositionLayerProjectionView),
            };

            {
                fixed (CompositionLayerProjectionView* pprojection_layer_elements = projection_layer_elements)
                {
                    CompositionLayerProjection projection_layer = new CompositionLayerProjection(StructureType.CompositionLayerProjection);
                    projection_layer.LayerFlags = CompositionLayerFlags.BlendTextureSourceAlphaBit;
                    projection_layer.LayerFlags |= CompositionLayerFlags.CorrectChromaticAberrationBit;
                    projection_layer.Space = currentSpace.Space;
                    projection_layer.ViewCount = ovrMaxNumEyes;
                    projection_layer.Views = pprojection_layer_elements;

                    for (int eye = 0; eye < ovrMaxNumEyes; eye++)
                    {
                        projection_layer_elements[eye] = new CompositionLayerProjectionView(StructureType.CompositionLayerProjectionView);
                        
                        projection_layer_elements[eye].Pose = _viewTransform[eye].ToPosef();
                        projection_layer_elements[eye].Fov = _projections[eye].Fov;

                        projection_layer_elements[eye].SubImage = new SwapchainSubImage();
                        projection_layer_elements[eye].SubImage.Swapchain =
                                _Renderer.FrameBuffer[eye].ColorSwapChain.Swapchain;
                        projection_layer_elements[eye].SubImage.ImageRect.Offset.X = 0;
                        projection_layer_elements[eye].SubImage.ImageRect.Offset.Y = 0;
                        projection_layer_elements[eye].SubImage.ImageRect.Extent.Width =
                                (int)_Renderer.FrameBuffer[eye].ColorSwapChain.Width;
                        projection_layer_elements[eye].SubImage.ImageRect.Extent.Height =
                                (int)_Renderer.FrameBuffer[eye].ColorSwapChain.Height;
                        projection_layer_elements[eye].SubImage.ImageArrayIndex = 0;
                    }

                    _Layers[LayerCount++].Projection = projection_layer;
                }
            }

            // Build the cylinder layer
            if (false)
            {
                float appCylPositionX = 0.0f;
                float appCylPositionY = 0.0f;

                CompositionLayerCylinderKHR cylinder_layer = new CompositionLayerCylinderKHR(StructureType.CompositionLayerCylinderKhr);
                cylinder_layer.LayerFlags = CompositionLayerFlags.BlendTextureSourceAlphaBit;
                cylinder_layer.Space = _LocalSpace.Space;
                cylinder_layer.EyeVisibility = EyeVisibility.Both;
                cylinder_layer.SubImage = new SwapchainSubImage();
                cylinder_layer.SubImage.Swapchain = _Scene.CylinderSwapChain.Swapchain;
                cylinder_layer.SubImage.ImageRect.Offset.X = 0;
                cylinder_layer.SubImage.ImageRect.Offset.Y = 0;
                cylinder_layer.SubImage.ImageRect.Extent.Width  = (int)_Scene.CylinderSwapChain.Width;
                cylinder_layer.SubImage.ImageRect.Extent.Height = (int) _Scene.CylinderSwapChain.Height;
                cylinder_layer.SubImage.ImageArrayIndex = 0;
                Vector3f axis = new Vector3f(0.0f, 1.0f, 0.0f);
                Vector3f pos = new Vector3f(appCylPositionX, appCylPositionY, 0.0f);
                cylinder_layer.Pose.Orientation = XrQuaternionf_CreateFromAxisAngle(axis, -45.0f * (float)(Math.PI / 180.0f));
                cylinder_layer.Pose.Position = pos;
                cylinder_layer.Radius = 2.0f;

                cylinder_layer.CentralAngle = (float)(Math.PI / 4.0);
                cylinder_layer.AspectRatio = 2.0f;

                _Layers[LayerCount++].Cylinder = cylinder_layer;
            }

            // Compose the layers for this frame.
            CompositionLayerBaseHeader*[] layers = new CompositionLayerBaseHeader*[ovrMaxLayerCount];
            fixed (ovrCompositorLayer_Union* p_Layers = _Layers)
            for (int i = 0; i < LayerCount; i++)
            {
                layers[i] = &p_Layers[i].BaseHeader;
            }

            // end frame
            fixed (CompositionLayerBaseHeader** players = layers)
            {
                FrameEndInfo endFrameInfo = new FrameEndInfo(StructureType.FrameEndInfo);
                endFrameInfo.DisplayTime = _frameState.PredictedDisplayTime;
                endFrameInfo.EnvironmentBlendMode = EnvironmentBlendMode.Opaque;
                endFrameInfo.LayerCount = LayerCount;
                endFrameInfo.Layers = (CompositionLayerBaseHeader**)players;

                xrResult = _oxrSession.EndFrame(ref endFrameInfo);
                Debug.Assert(xrResult == Result.Success, "EndFrame");
            }

            return 0;
        }

        public override HandsState GetHandsState()
        {
            return _handsState;
        }

        public override void EndSessionAsync()
        {
            throw new PlatformNotSupportedException();
        }

        public override void TrackFloorLevelAsync(bool enable)
        {
            if (enable == true)
            {
                if (!_isLocalFloorSupported)
                    throw new NotImplementedException();
            }

            _isTrackFloorLevelEnabled = enable;
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

        private unsafe int Initialize(GraphicsDevice graphicsDevice)
        {
            // check supported feature level
            switch (graphicsDevice.Adapter.Backend)
            {
                case GraphicsBackend.DirectX11:
                case GraphicsBackend.DirectX12:
                    //if (graphicsDevice.GraphicsProfile < GraphicsProfile.FL10_0)
                    //    throw new InvalidOperationException("GraphicsProfile must be FL10_0 or higher.");
                    break;
                case GraphicsBackend.OpenGL:
                case GraphicsBackend.GLES:
                    if (graphicsDevice.GraphicsProfile < GraphicsProfile.HiDef)
                        throw new InvalidOperationException("GraphicsProfile must be HiDef or higher.");
                    break;
            }
            
            Result xrResult;

            // Check that the extensions required are present.
            List<string> requiredExtensionNames = new List<string>();
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_EXT_local_floor));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_KHR_opengl_es_enable));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_EXT_performance_settings));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_KHR_android_thread_settings));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_KHR_composition_layer_cube));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_KHR_composition_layer_cylinder));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_KHR_composition_layer_equirect2));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_display_refresh_rate));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_color_space));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_swapchain_update_state));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_swapchain_update_state_opengl_es));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_foveation));
            requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_foveation_configuration));

            if (this.SessionMode == XRSessionMode.AR)
                requiredExtensionNames.Add(nameof(OxrExtensions.XR_FB_passthrough));

            // Check the list of required extensions against what is supported by the runtime.
            uint numRequiredExtensions = (uint)requiredExtensionNames.Count;
            for (int i = 0; i < numRequiredExtensions; i++)
            {
                if (_extensions.Contains(requiredExtensionNames[i]))
                {
                    Console.WriteLine("Found required extension {0}", requiredExtensionNames[i]);
                }
                else
                {
                    Console.WriteLine("Failed to find required extension {0}", requiredExtensionNames[i]);
                    //throw new Exception();
                }
            }


            // Create the OpenXR instance.
            xrResult = _xrAPI.CreateInstance(_applicationName, _engineName,
                                             requiredExtensionNames, out _oxrInstance);
            if (xrResult != Result.Success)
            {
                Console.WriteLine("Failed to create XR instance: {0}.", xrResult);
                return (int)xrResult;
            }
            Console.WriteLine("OpenXR Instance initialized successfully. {0}.", _oxrInstance.Instance.Handle);

            Console.WriteLine("Runtime: {0}.", _oxrInstance.RuntimeName);
            Console.WriteLine("Version: {0}.", _oxrInstance.RuntimeVersion);
            
            if (xrResult == Result.ErrorFormFactorUnavailable)
            {
                Console.WriteLine("Failed to get system; the specified form factor is not available. Is your headset connected?");
                return (int)xrResult;
            }
            if (xrResult != Result.Success)
            {
                Console.WriteLine("xrGetSystem failed, error {0}", xrResult);
                return (int)xrResult;
            }
            Console.WriteLine("systemId. {0}.", _oxrInstance.SystemId);

            SystemColorSpacePropertiesFB colorSpacePropertiesFB = new SystemColorSpacePropertiesFB(StructureType.SystemColorSpacePropertiesFB);

            xrResult = _oxrInstance.GetSystemProperties(&colorSpacePropertiesFB, out SystemProperties systemProperties);
            Debug.Assert(xrResult == Result.Success, "GetSystemProperties");

            string systemName = Encoding.ASCII.GetString(systemProperties.SystemName, (int)SilkXR.MaxSystemNameSize);
            systemName = systemName.TrimEnd('\0');
            Console.WriteLine("System Properties: Name={0} VendorId={1}",
               systemName,
               systemProperties.VendorId);
            Console.WriteLine("System Graphics Properties: MaxWidth={0} MaxHeight={1} MaxLayers={2}",
                systemProperties.GraphicsProperties.MaxSwapchainImageWidth,
                systemProperties.GraphicsProperties.MaxSwapchainImageHeight,
                systemProperties.GraphicsProperties.MaxLayerCount);
            Console.WriteLine("System Tracking Properties: OrientationTracking={0} PositionTracking={1}",
                systemProperties.TrackingProperties.OrientationTracking != 0 ? "True" : "False",
                systemProperties.TrackingProperties.PositionTracking != 0 ? "True" : "False");

            Console.WriteLine("System Color Space Properties: colorspace={0}", colorSpacePropertiesFB.ColorSpace);

            Debug.Assert(ovrMaxLayerCount <= systemProperties.GraphicsProperties.MaxLayerCount);

            // Get the graphics requirements.

            xrResult = _oxrInstance.GetInstanceProcAddr<xrGetOpenGLESGraphicsRequirementsKHRDelegate>("xrGetOpenGLESGraphicsRequirementsKHR",
                out xrGetOpenGLESGraphicsRequirementsKHRDelegate Fun_xrGetOpenGLESGraphicsRequirementsKHR);
            Debug.Assert(xrResult == Result.Success, "GetInstanceProcAddr");

            GraphicsRequirementsOpenGLESKHR graphicsRequirements = new GraphicsRequirementsOpenGLESKHR(StructureType.GraphicsRequirementsOpenglESKhr);

            xrResult = Fun_xrGetOpenGLESGraphicsRequirementsKHR(_oxrInstance.Instance, _oxrInstance.SystemId, &graphicsRequirements);
            Debug.Assert(xrResult == Result.Success, "Fun_xrGetOpenGLESGraphicsRequirementsKHR");
            OxrVersion minApiVersionSupported = new OxrVersion(graphicsRequirements.MinApiVersionSupported);
            OxrVersion maxApiVersionSupported = new OxrVersion(graphicsRequirements.MaxApiVersionSupported);
            Console.WriteLine("GLES MinApiVersionSupported {0}, MaxApiVersionSupported {1} ", minApiVersionSupported, maxApiVersionSupported);
   
            /*
            var adapter = ((IPlatformGraphicsAdapter)_graphics.GraphicsDevice.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            var GL = adapter.Ogl;

            // Check the graphics requirements.
            int eglMajor = 0;
            int eglMinor = 0;
            GL.GetInteger(OpenGL.GetPName.MajorVersion, out eglMajor);
            GL.CheckGLError();
            GL.GetInteger(OpenGL.GetPName.MinorVersion, out eglMinor);
            GL.CheckGLError();
            OxrVersion eglVersion = new OxrVersion((short)eglMajor, (short)eglMinor, 0);
            if (eglVersion.Packed < minApiVersionSupported.Packed
            || eglVersion.Packed > maxApiVersionSupported.Packed)
            {
                Console.WriteLine("GLES version {0} not supported", eglVersion);
                return (int)xrResult;
            }
            */

            _CpuLevel = CPU_LEVEL;
            _GpuLevel = GPU_LEVEL;

            Android.Opengl.EGLDisplay display = Android.Opengl.EGL14.EglGetCurrentDisplay();
            Android.Opengl.EGLContext context = Android.Opengl.EGL14.EglGetCurrentContext();
            int[] currentConfig = new int[1];
            Android.Opengl.EGL14.EglQueryContext(display, context, Android.Opengl.EGL14.EglConfigId, currentConfig, 0);
            int eglConfig = currentConfig[0];

            // Create the OpenXR Session.
            GraphicsBindingOpenGLESAndroidKHR graphicsBindingAndroidGLES = new GraphicsBindingOpenGLESAndroidKHR(StructureType.GraphicsBindingOpenglESAndroidKhr);
            graphicsBindingAndroidGLES.Display = display.Handle;
            graphicsBindingAndroidGLES.Context = context.Handle;
            graphicsBindingAndroidGLES.Config = eglConfig;

            //AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(_graphics.GraphicsDevice.PresentationParameters.DeviceWindowHandle);
            //graphicsBindingAndroidGLES.Display = _graphics.GraphicsDevice.Adapter.MonitorHandle;
            //graphicsBindingAndroidGLES.Context = gameWindow.EglContext.Handle;
            //graphicsBindingAndroidGLES.Config = gameWindow.EglConfig.Handle;


            xrResult = _oxrInstance.CreateSession(&graphicsBindingAndroidGLES, out _oxrSession);
            if (xrResult != Result.Success)
            {
                Console.WriteLine("Failed to create XR session: {0}.", xrResult);
                return (int)xrResult;
            }
            Console.WriteLine("XR session created successfully.");


            // App only supports the primary stereo view config.
            const ViewConfigurationType supportedViewConfigType = ViewConfigurationType.PrimaryStereo;

            // Enumerate the viewport configurations.
            xrResult = _oxrInstance.EnumerateViewConfiguration(out ViewConfigurationType[] viewportConfigurationTypes);
            Console.WriteLine("Available Viewport Configuration Types: {0}", viewportConfigurationTypes.Length);

            ViewConfigurationView[] ViewConfigurationView = new ViewConfigurationView[ovrMaxNumEyes];

            for (uint i = 0; i < viewportConfigurationTypes.Length; i++)
            {
                ViewConfigurationType viewportConfigType = viewportConfigurationTypes[i];

                Console.WriteLine(
                    "Viewport configuration type {0} : {1}",
                    viewportConfigType,
                    viewportConfigType == supportedViewConfigType ? "Selected" : "");

                xrResult = _oxrInstance.GetViewConfigurationProperties(viewportConfigType, out _viewportConfig);
                Debug.Assert(xrResult == Result.Success, "GetViewConfigurationProperties");

                Console.WriteLine(
                    "FovMutable={0} ConfigurationType {0}",
                    _viewportConfig.FovMutable != 0 ? "true" : "false",
                    _viewportConfig.ViewConfigurationType);

                xrResult = _oxrInstance.EnumerateViewConfigurationView(viewportConfigType, out ViewConfigurationView[] elements);

                if (elements.Length > 0)
                {
                    // Log the view config info for each view type for debugging purposes.
                    for (uint e = 0; e < elements.Length; e++)
                    {
                        ViewConfigurationView element = elements[e];

                        Console.WriteLine(
                            "Viewport [{0}]: Recommended Width={1} Height={2} SampleCount={3}",
                            e,
                            element.RecommendedImageRectWidth,
                            element.RecommendedImageRectHeight,
                            element.RecommendedSwapchainSampleCount);

                        Console.WriteLine(
                            "Viewport [{0}]: Max Width={1} Height={2} SampleCount={3}",
                            e,
                            element.MaxImageRectWidth,
                            element.MaxImageRectHeight,
                            element.MaxSwapchainSampleCount);
                    }

                    // Cache the view config properties for the selected config type.
                    if (viewportConfigType == supportedViewConfigType)
                    {
                        Debug.Assert(elements.Length == ovrMaxNumEyes);
                        for (uint e = 0; e < elements.Length; e++)
                        {
                            ViewConfigurationView[e] = elements[e];
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Empty viewport configuration type: {0}", elements.Length);
                }
            }

            // Get the viewport configuration info for the chosen viewport configuration type.
            xrResult = _oxrInstance.GetViewConfigurationProperties(supportedViewConfigType, out ViewConfigurationProperties ViewportConfig);
            Debug.Assert(xrResult == Result.Success, "GetViewConfigurationProperties");

            // Enumerate the supported color space options for the system.
            {
                xrResult = _oxrInstance.GetInstanceProcAddr<xrEnumerateColorSpacesFB>("xrEnumerateColorSpacesFB",
                    out xrEnumerateColorSpacesFB Fun_xrEnumerateColorSpacesFB);
                Debug.Assert(xrResult == Result.Success, "xrGetInstanceProcAddr");

                uint colorSpaceCountOutput = 0;
                xrResult = Fun_xrEnumerateColorSpacesFB(_oxrSession.Session, 0, &colorSpaceCountOutput, null);
                Debug.Assert(xrResult == Result.Success, "Fun_xrEnumerateColorSpacesFB");

                ColorSpaceFB[] colorSpaces = new ColorSpaceFB[colorSpaceCountOutput];

                fixed (ColorSpaceFB* pcolorSpaces = colorSpaces)
                    xrResult = Fun_xrEnumerateColorSpacesFB(_oxrSession.Session, colorSpaceCountOutput, &colorSpaceCountOutput, pcolorSpaces);
                Debug.Assert(xrResult == Result.Success, "Fun_xrEnumerateColorSpacesFB");
         
                Console.WriteLine("Supported ColorSpaces:");
                for (uint i = 0; i < colorSpaceCountOutput; i++)
                {
                    Console.WriteLine("{0}:{1}", i, colorSpaces[i]);
                }

                ColorSpaceFB requestColorSpace = ColorSpaceFB.AdobeRgbFB;

                xrResult = _oxrInstance.GetInstanceProcAddr<xrSetColorSpaceFB>("xrSetColorSpaceFB",
                    out xrSetColorSpaceFB Fun_xrSetColorSpaceFB);
                Debug.Assert(xrResult == Result.Success, "xrGetInstanceProcAddr");

                xrResult = Fun_xrSetColorSpaceFB(_oxrSession.Session, requestColorSpace);
                Debug.Assert(xrResult == Result.Success, "Fun_xrEnumerateColorSpacesFB");
            }

            // Get the supported display refresh rates for the system.
            {
                _oxrInstance.GetInstanceProcAddr<xrEnumerateDisplayRefreshRatesFB>("xrEnumerateDisplayRefreshRatesFB",
                    out xrEnumerateDisplayRefreshRatesFB pfnxrEnumerateDisplayRefreshRatesFB);

                _NumSupportedDisplayRefreshRates = 0;
                xrResult = pfnxrEnumerateDisplayRefreshRatesFB(_oxrSession.Session, 0, ref _NumSupportedDisplayRefreshRates, null);
                Debug.Assert(xrResult == Result.Success, "pfnxrEnumerateDisplayRefreshRatesFB");

                _SupportedDisplayRefreshRates = new float[_NumSupportedDisplayRefreshRates];
                
                fixed (float* pSupportedDisplayRefreshRates = _SupportedDisplayRefreshRates)
                    xrResult = pfnxrEnumerateDisplayRefreshRatesFB(_oxrSession.Session, _NumSupportedDisplayRefreshRates, ref _NumSupportedDisplayRefreshRates, pSupportedDisplayRefreshRates);
                Debug.Assert(xrResult == Result.Success, "pfnxrEnumerateDisplayRefreshRatesFB");
              
                Console.WriteLine("Supported Refresh Rates:");
                for (uint i = 0; i < _NumSupportedDisplayRefreshRates; i++)
                {
                    Console.WriteLine("{0}:{1}", i, _SupportedDisplayRefreshRates[i]);
                }

                _oxrInstance.GetInstanceProcAddr<xrGetDisplayRefreshRateFB>("xrGetDisplayRefreshRateFB",
                    out _pfnxrGetDisplayRefreshRateFB);

                float currentDisplayRefreshRate = 0.0f;
                xrResult = _pfnxrGetDisplayRefreshRateFB(_oxrSession.Session, &currentDisplayRefreshRate);
                Debug.Assert(xrResult == Result.Success, "pfnxrGetDisplayRefreshRateFB");
                Console.WriteLine("Current System Display Refresh Rate: {0}", currentDisplayRefreshRate);

                _oxrInstance.GetInstanceProcAddr<xrRequestDisplayRefreshRateFB>("xrRequestDisplayRefreshRateFB",
                    out _pfnxrRequestDisplayRefreshRateFB);

                // Test requesting the system default.
                xrResult = _pfnxrRequestDisplayRefreshRateFB(_oxrSession.Session, 0.0f);
                Debug.Assert(xrResult == Result.Success, "pfnRequestDisplayRefreshRate");
                Console.WriteLine("Requesting system default display refresh rate");
            }

            xrResult = _oxrSession.EnumerateReferenceSpaces(out ReferenceSpaceType[] referenceSpaces);
            Debug.Assert(xrResult == Result.Success, "EnumerateReferenceSpaces");

            for (uint i = 0; i < referenceSpaces.Length; i++)
            {
                if (referenceSpaces[i] == ReferenceSpaceType.LocalFloor)
                    _isLocalFloorSupported = true;
                if (referenceSpaces[i] == ReferenceSpaceType.Stage)
                    _isStageSupported = true;
            }

            // Create a space to the first path
            xrResult = _oxrSession.CreateReferenceSpace(ReferenceSpaceType.View, XrPosef_CreateIdentity(), out _HeadSpace);
            Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");

            xrResult = _oxrSession.CreateReferenceSpace(ReferenceSpaceType.Local, XrPosef_CreateIdentity(), out _LocalSpace);
            Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");

            if (_isLocalFloorSupported)
            {
                xrResult = _oxrSession.CreateReferenceSpace(ReferenceSpaceType.LocalFloor, XrPosef_CreateIdentity(), out _LocalFloorSpace);
                Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");
                Console.WriteLine("Created local floor space");
            }

            // Create a default stage space to use if SPACE_TYPE_STAGE is not
            // supported, or calls to xrGetReferenceSpaceBoundsRect fail.
            {
                Posef poseInReferenceSpace = XrPosef_CreateIdentity();
                poseInReferenceSpace.Position.Y = -1.6750f;
                xrResult = _oxrSession.CreateReferenceSpace(ReferenceSpaceType.Local, poseInReferenceSpace, out _FakeStageSpace);
                Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");
                Console.WriteLine("Created fake stage space from local space with offset");
                _CurrentStageSpace = _FakeStageSpace;
            }

            if (_isStageSupported)
            {
                xrResult = _oxrSession.CreateReferenceSpace(ReferenceSpaceType.Stage, XrPosef_CreateIdentity(), out _StageSpace);
                Debug.Assert(xrResult == Result.Success, "CreateReferenceSpace");
                Console.WriteLine("Created stage space");
                _CurrentStageSpace = _StageSpace;
            }

            for (int eye = 0; eye < ovrMaxNumEyes; eye++)
            {
                _projections[eye].Type = StructureType.View;
            }

            // setup passthrough
            if (this.SessionMode == XRSessionMode.AR)
            {
                _passthroughFB = _oxrSession.CreatePassthroughFB(default(PassthroughFlagsFB), _oxrInstance);
                _passthroughLayerFB = _oxrSession.CreatePassthroughLayerFB(_passthroughFB, PassthroughLayerPurposeFB.ReconstructionFB, default(PassthroughFlagsFB), _oxrInstance);

                xrResult = _passthroughFB.Start();
                xrResult = _passthroughLayerFB.Resume();
            }

            // Actions
            _runningActionSet =
                _oxrInstance.CreateActionSet(1, "running_action_set", "Action Set used on main loop");

            _actionButtonX = _runningActionSet.CreateAction(ActionType.BooleanInput, "x_button", "Button X", 0, null);
            _actionButtonY = _runningActionSet.CreateAction(ActionType.BooleanInput, "y_button", "Button Y", 0, null);
            _actionButtonLeftThumbstick = _runningActionSet.CreateAction(ActionType.BooleanInput, "left_thumbstick_click", "Button LeftThumbstick", 0, null);

            _actionButtonA = _runningActionSet.CreateAction(ActionType.BooleanInput, "a_button", "Button A", 0, null);
            _actionButtonB = _runningActionSet.CreateAction(ActionType.BooleanInput, "b_button", "Button B", 0, null);
            _actionButtonRightThumbstick = _runningActionSet.CreateAction(ActionType.BooleanInput, "right_thumbstick_click", "Button RightThumbstick", 0, null);

            _actionButtonXtouch = _runningActionSet.CreateAction(ActionType.BooleanInput, "x_touch", "Touch  X", 0, null);
            _actionButtonYtouch = _runningActionSet.CreateAction(ActionType.BooleanInput, "y_touch", "Touch  Y", 0, null);
            _actionButtonLeftThumbsticktouch = _runningActionSet.CreateAction(ActionType.BooleanInput, "left_thumbstick_touch", "Touch LeftThumbstick", 0, null);

            _actionButtonAtouch = _runningActionSet.CreateAction(ActionType.BooleanInput, "a_touch", "Touch  A", 0, null);
            _actionButtonBtouch = _runningActionSet.CreateAction(ActionType.BooleanInput, "b_touch", "Touch  B", 0, null);
            _actionButtonRightThumbsticktouch = _runningActionSet.CreateAction(ActionType.BooleanInput, "right_thumbstick_touch", "Touch RightThumbstick", 0, null);

            /*
             Right Hand: right_menu_click
                left_menu_click

            Select Button (Often associated with the A or X buttons, depending on the controller):
                right_select_click or right_a_click
                left_select_click or left_x_click

            Trigger Button:
                right_trigger_click
                left_trigger_click
            Grip Button (Squeeze):
                right_grip_click or right_squeeze_click
                left_grip_click or left_squeeze_click
            
             */

            _moveOnXActionL = _runningActionSet.CreateAction(
                 ActionType.FloatInput, "left_move_on_x", "Move on Left X", 0, null);
            _moveOnXActionR = _runningActionSet.CreateAction(
                 ActionType.FloatInput, "right_move_on_x", "Move on Right X", 0, null);

            _moveOnYActionL = _runningActionSet.CreateAction(
                ActionType.FloatInput, "left_move_on_y", "Move on Left Y", 0, null);
            _moveOnYActionR = _runningActionSet.CreateAction(
                ActionType.FloatInput, "right_move_on_y", "Move on Right Y", 0, null);

            _actionTriggerLeftTouch = _runningActionSet.CreateAction(
                ActionType.FloatInput, "left_trigger_touch", "Left Trigger Touch", 0, null);
            _actionTriggerRightTouch = _runningActionSet.CreateAction(
                ActionType.FloatInput, "right_trigger_touch", "Right Trigger Touch", 0, null);

            //_actionGripLeftTouch = _runningActionSet.CreateAction(
            //    ActionType.FloatInput, "left_squeeze_touch", "Left Squeeze Touch", 0, null);
            //_actionGripRightTouch = _runningActionSet.CreateAction(
            //    ActionType.FloatInput, "right_squeeze_touch", "Right Squeeze Touch", 0, null);

            _moveOnJoystickActionL = _runningActionSet.CreateAction(
                ActionType.Vector2fInput, "left_move_on_joy", "Move on Left Joy", 0, null);
            _moveOnJoystickActionR = _runningActionSet.CreateAction(
                ActionType.Vector2fInput, "right_move_on_joy", "Move on Right Joy", 0, null);

            _vibrateLeftFeedback = _runningActionSet.CreateAction(
                ActionType.VibrationOutput,
                "vibrate_left_feedback",
                "Vibrate Left Controller Feedback",
                0,
                null);
            _vibrateRightFeedback = _runningActionSet.CreateAction(
                ActionType.VibrationOutput,
                "vibrate_right_feedback",
                "Vibrate Right Controller Feedback",
                0,
                null);

            xrResult = _oxrInstance.StringToPath("/user/hand/left", out _leftHandPath);
            Debug.Assert(xrResult == Result.Success, "StringToPath");
            xrResult = _oxrInstance.StringToPath("/user/hand/right", out _rightHandPath);
            Debug.Assert(xrResult == Result.Success, "StringToPath");
            ActionPath[] handSubactionPaths = new ActionPath[] { _leftHandPath, _rightHandPath };

            _aimPoseAction = _runningActionSet.CreateAction(
                ActionType.PoseInput, "aim_pose", null, 2, handSubactionPaths);
            _gripPoseAction = _runningActionSet.CreateAction(
                ActionType.PoseInput, "grip_pose", null, 2, handSubactionPaths);

            ActionPath interactionProfilePath = default;
            ActionPath interactionProfilePathTouch = default;
            ActionPath interactionProfilePathKHRSimple = default;

            xrResult = _oxrInstance.StringToPath("/interaction_profiles/oculus/touch_controller", out interactionProfilePathTouch);
            Debug.Assert(xrResult == Result.Success, "StringToPath");
            xrResult = _oxrInstance.StringToPath("/interaction_profiles/khr/simple_controller", out interactionProfilePathKHRSimple);
            Debug.Assert(xrResult == Result.Success, "StringToPath");

            // Toggle this to force simple as a first choice, otherwise use it as a last resort
            _useSimpleProfile = false; /// true;
            if (_useSimpleProfile)
            {
                Console.WriteLine("xrSuggestInteractionProfileBindings found bindings for Khronos SIMPLE controller");
                interactionProfilePath = interactionProfilePathKHRSimple;
            }
            else
            {
                // Query Set
                OxrActionSet queryActionSet =
                    _oxrInstance.CreateActionSet(1, "query_action_set", "Action Set used to query device caps");
                XrAction dummyAction = queryActionSet.CreateAction(
                    ActionType.BooleanInput, "dummy_action", "Dummy Action", 0, null);

                // Map bindings
                ActionSuggestedBinding[] bindings = new ActionSuggestedBinding[1];
                bindings[0] = _oxrInstance.ActionSuggestedBinding(dummyAction, "/user/hand/right/input/system/click");

                // Try all
                Result suggestTouchResult =
                        _oxrInstance.SuggestInteractionProfileBinding(interactionProfilePathTouch, bindings);
                    Debug.Assert(suggestTouchResult == Result.Success, "StringToPath");

                if (Result.Success == suggestTouchResult)
                {
                    Console.WriteLine("xrSuggestInteractionProfileBindings found bindings for QUEST controller");
                    interactionProfilePath = interactionProfilePathTouch;
                }
                
                if (interactionProfilePath.Handle == 0)
                {   
                    // Simple as a fallback
                    bindings[0] = _oxrInstance.ActionSuggestedBinding(dummyAction, "/user/hand/right/input/select/click");

                    Result suggestKHRSimpleResult =
                        _oxrInstance.SuggestInteractionProfileBinding(interactionProfilePathKHRSimple, bindings);
                    Debug.Assert(suggestKHRSimpleResult == Result.Success, "xrSuggestInteractionProfileBindings");

                    if (Result.Success == suggestKHRSimpleResult)
                    {
                        Console.WriteLine(
                            "xrSuggestInteractionProfileBindings found bindings for Khronos SIMPLE controller");
                        interactionProfilePath = interactionProfilePathKHRSimple;
                    }
                    else
                    {
                        Console.WriteLine("xrSuggestInteractionProfileBindings did NOT find any bindings.");
                        Debug.Assert(false);
                    }
                }
            }

            // Action creation
            {
                // Map bindings
                ActionSuggestedBinding[] bindings = null;
                if (interactionProfilePath == interactionProfilePathTouch)
                {
                    bindings = new ActionSuggestedBinding[]
                    {
                        _oxrInstance.ActionSuggestedBinding(_actionButtonX, "/user/hand/left/input/x/click"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonY, "/user/hand/left/input/y/click"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonLeftThumbstick, "/user/hand/left/input/thumbstick/click"),

                        _oxrInstance.ActionSuggestedBinding(_actionButtonA, "/user/hand/right/input/a/click"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonB, "/user/hand/right/input/b/click"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonRightThumbstick, "/user/hand/right/input/thumbstick/click"),

                        _oxrInstance.ActionSuggestedBinding(_actionButtonXtouch, "/user/hand/left/input/x/touch"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonYtouch, "/user/hand/left/input/y/touch"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonLeftThumbsticktouch, "/user/hand/left/input/thumbstick/touch"),

                        _oxrInstance.ActionSuggestedBinding(_actionButtonAtouch, "/user/hand/right/input/a/touch"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonBtouch, "/user/hand/right/input/b/touch"),
                        _oxrInstance.ActionSuggestedBinding(_actionButtonRightThumbsticktouch, "/user/hand/right/input/thumbstick/touch"),

                        _oxrInstance.ActionSuggestedBinding(_moveOnYActionL, "/user/hand/left/input/trigger/value"),
                        _oxrInstance.ActionSuggestedBinding(_moveOnYActionR, "/user/hand/right/input/trigger/value"),
                        _oxrInstance.ActionSuggestedBinding(_moveOnXActionL, "/user/hand/left/input/squeeze/value"),
                        _oxrInstance.ActionSuggestedBinding(_moveOnXActionR, "/user/hand/right/input/squeeze/value"),

                        _oxrInstance.ActionSuggestedBinding(_actionTriggerLeftTouch, "/user/hand/left/input/trigger/touch"),
                        _oxrInstance.ActionSuggestedBinding(_actionTriggerRightTouch, "/user/hand/right/input/trigger/touch"),

                        // BUG: Grip touch doesn't work on native OpenXR.
                        // https://communityforums.atmeta.com/t5/OpenXR-Development/unable-to-get-grip-button-input-on-quest-1-getting-path/td-p/833021
                        //_oxrInstance.ActionSuggestedBinding(_actionGripLeftTouch, "/user/hand/left/input/squeeze/touch"),
                        //_oxrInstance.ActionSuggestedBinding(_actionGripRightTouch, "/user/hand/right/input/squeeze/touch"),

                        _oxrInstance.ActionSuggestedBinding(_moveOnJoystickActionL, "/user/hand/left/input/thumbstick"),
                        _oxrInstance.ActionSuggestedBinding(_moveOnJoystickActionR, "/user/hand/right/input/thumbstick"),

                        _oxrInstance.ActionSuggestedBinding(_vibrateLeftFeedback, "/user/hand/left/output/haptic"),
                        _oxrInstance.ActionSuggestedBinding(_vibrateRightFeedback, "/user/hand/right/output/haptic"),

                        _oxrInstance.ActionSuggestedBinding(_aimPoseAction, "/user/hand/left/input/aim/pose"),
                        _oxrInstance.ActionSuggestedBinding(_aimPoseAction, "/user/hand/right/input/aim/pose"),
                        _oxrInstance.ActionSuggestedBinding(_gripPoseAction, "/user/hand/left/input/grip/pose"),
                        _oxrInstance.ActionSuggestedBinding(_gripPoseAction, "/user/hand/right/input/grip/pose"),
                    };
                }

                if (interactionProfilePath == interactionProfilePathKHRSimple)
                {
                    bindings = new ActionSuggestedBinding[]
                    {
                        _oxrInstance.ActionSuggestedBinding(_vibrateLeftFeedback, "/user/hand/left/output/haptic"),
                        _oxrInstance.ActionSuggestedBinding(_vibrateRightFeedback, "/user/hand/right/output/haptic"),

                        _oxrInstance.ActionSuggestedBinding(_aimPoseAction, "/user/hand/left/input/aim/pose"),
                        _oxrInstance.ActionSuggestedBinding(_aimPoseAction, "/user/hand/right/input/aim/pose"),
                        _oxrInstance.ActionSuggestedBinding(_gripPoseAction, "/user/hand/left/input/grip/pose"),
                        _oxrInstance.ActionSuggestedBinding(_gripPoseAction, "/user/hand/right/input/grip/pose"),
                    };
                }

                xrResult = _oxrInstance.SuggestInteractionProfileBinding(interactionProfilePath, bindings);
                Debug.Assert(xrResult == Result.Success, "SuggestInteractionProfileBinding");

                // Attach to session
                xrResult = _oxrSession.AttachSessionActionSets(_runningActionSet);
                Debug.Assert(xrResult == Result.Success, "AttachSessionActionSets");


                // Enumerate actions                    
                XrAction[] actionsToEnumerate =
                {
                    _actionButtonX,
                    _actionButtonY,
                    _actionButtonRightThumbstick,
                    _actionButtonA,
                    _actionButtonB,
                    _actionButtonLeftThumbstick,

                    _actionButtonXtouch,
                    _actionButtonYtouch,
                    _actionButtonRightThumbsticktouch,
                    _actionButtonAtouch,
                    _actionButtonBtouch,
                    _actionButtonLeftThumbsticktouch,

                    _moveOnXActionL,
                    _moveOnXActionR,
                    _moveOnYActionL,
                    _moveOnYActionR,

                    _actionTriggerLeftTouch,
                    _actionTriggerRightTouch,
                    //_actionGripLeftTouch,
                    //_actionGripRightTouch,

                    _moveOnJoystickActionL,
                    _moveOnJoystickActionR,

                    _vibrateLeftFeedback,
                    _vibrateRightFeedback,
                    _aimPoseAction,
                    _gripPoseAction,
                };

                for (int i = 0; i < actionsToEnumerate.Length; i++)
                {
                    ulong[] actionPathsBuffer = _oxrSession.EnumerateBoundSourcesForAction(actionsToEnumerate[i]);

                    Console.WriteLine(
                        "xrEnumerateBoundSourcesForAction action={0} count={1}",
                        actionsToEnumerate[i].Handle,
                        actionPathsBuffer.Length);

                    for (uint a = 0; a < actionPathsBuffer.Length; a++)
                    {
                        InputSourceLocalizedNameGetInfo nameGetInfo = new InputSourceLocalizedNameGetInfo(StructureType.InputSourceLocalizedNameGetInfo);
                        nameGetInfo.SourcePath = actionPathsBuffer[a];
                        nameGetInfo.WhichComponents = InputSourceLocalizedNameFlags.UserPathBit
                                                    | InputSourceLocalizedNameFlags.InteractionProfileBit
                                                    | InputSourceLocalizedNameFlags.ComponentBit;

                        uint stringCount = 0;
                        xrResult = _oxrInstance.OXRAPI.Api.GetInputSourceLocalizedName(_oxrSession.Session, &nameGetInfo, 0, &stringCount, (byte*)null);
                        Debug.Assert(xrResult == Result.Success, "GetInputSourceLocalizedName");
                            
                        char[] stringBuffer = new char[stringCount];
                        string localizedName;

                        fixed (void* pstringBuffer = stringBuffer)
                        {
                            xrResult = _oxrInstance.OXRAPI.Api.GetInputSourceLocalizedName(_oxrSession.Session, &nameGetInfo, stringCount, &stringCount, (byte*)pstringBuffer);
                            Debug.Assert(xrResult == Result.Success, "GetInputSourceLocalizedName");
                            localizedName = Encoding.ASCII.GetString((byte*)pstringBuffer, (int)stringCount);
                            Console.Write("Bound source: " + localizedName);
                        }


                        string strpathStr;
                        byte[] pathStr = new byte[stringCount];
                        fixed (byte* ppathStr = pathStr)
                        {
                            uint strLen = 0;
                            xrResult = _oxrInstance.OXRAPI.Api.PathToString(
                                _oxrInstance.Instance,
                                actionPathsBuffer[a],
                                stringCount,
                                &strLen,
                                ppathStr);

                            strpathStr = Encoding.ASCII.GetString((byte*)ppathStr, (int)strLen);
                        }

                        Console.Write(
                            "  -> path = {0} `{1}` -> `{2}`",
                            actionPathsBuffer[a],
                            strpathStr,
                            localizedName //stringBuffer
                            );
                    }
                }


                // Create the frame buffers.
                for (int eye = 0; eye < ovrMaxNumEyes; eye++)
                {
                    ovrFramebuffer_Create(
                        ref _Renderer.FrameBuffer[eye],
                        SurfaceFormat.ColorSRgba, //SRGB8_ALPHA8
                        (int)ViewConfigurationView[0].RecommendedImageRectWidth,
                        (int)ViewConfigurationView[0].RecommendedImageRectHeight,
                        NUM_MULTI_SAMPLES
                        );
                }

                ovrRenderer_SetFoveation(
                    ref _Renderer,
                    FoveationLevelFB.HighFB,
                    0,
                    FoveationDynamicFB.DisabledFB
                    );

                //CreateDefaultLayer(graphicsDevice,
                //    SurfaceFormat.Color,
                //    DepthFormat.Depth24Stencil8,
                //    4, 1,
                //    out _layer);

                TouchController.DeviceHandle = new ConcreteTouchControllerStrategy(this);
                this._deviceState = XRDeviceState.Enabled;

                return 0;
            }
        }

        private unsafe bool ovrFramebuffer_Create(
            ref ovrFramebuffer frameBuffer,
            SurfaceFormat colorFormat,
            int width,
            int height,
            int multisamples)
        {
            Result xrResult;

            frameBuffer.Width = width;
            frameBuffer.Height = height;
            frameBuffer.Multisamples = multisamples;

            // Get the number of supported formats.
            xrResult = _oxrSession.EnumerateSwapchainFormats(out long[] supportedFormats);
            Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainFormats");
            Array.Sort<long>(supportedFormats);

            long requestedGLFormat = SRGB8_ALPHA8;

            // Verify the requested format is supported.
            long selectedFormat = 0;
            for (uint i = 0; i < supportedFormats.Length; i++)
            {
                if (supportedFormats[i] == requestedGLFormat)
                {
                    selectedFormat = supportedFormats[i];
                    break;
                }
            }
            Debug.Assert(selectedFormat != 0, "requestedGLFormat not supported.");
            if (selectedFormat == 0)
            {
                Console.WriteLine("Format not supported");
            }

            SwapchainCreateInfo swapChainCreateInfo = new SwapchainCreateInfo(StructureType.SwapchainCreateInfo);
            swapChainCreateInfo.UsageFlags = SwapchainUsageFlags.SampledBit
                                            | SwapchainUsageFlags.ColorAttachmentBit;
            swapChainCreateInfo.Format = selectedFormat;
            swapChainCreateInfo.SampleCount = 1;
            swapChainCreateInfo.Width = (uint)width;
            swapChainCreateInfo.Height = (uint)height;
            swapChainCreateInfo.FaceCount = 1;
            swapChainCreateInfo.ArraySize = 1;
            swapChainCreateInfo.MipCount = 1;

            // Enable Foveation on this swapchain
            SwapchainCreateInfoFoveationFB swapChainFoveationCreateInfo = new SwapchainCreateInfoFoveationFB(StructureType.SwapchainCreateInfoFoveationFB);
            swapChainCreateInfo.Next = &swapChainFoveationCreateInfo;

            // Create the swapchain.
            xrResult = _oxrSession.CreateSwapchain(ref swapChainCreateInfo, out frameBuffer.ColorSwapChain);
            Debug.Assert(xrResult == Result.Success, "CreateSwapchain");
            // Get the number of swapchain images.
            frameBuffer.TextureSwapChainLength = frameBuffer.ColorSwapChain.GetImagesCount();

            // Allocate the swapchain images array.
            SwapchainImageOpenGLESKHR[] colorSwapChainImage = new SwapchainImageOpenGLESKHR[frameBuffer.TextureSwapChainLength];
            frameBuffer.ColorSwapChainImage = colorSwapChainImage;

            // Populate the swapchain image array.
            for (uint i = 0; i < frameBuffer.TextureSwapChainLength; i++)
            {
                colorSwapChainImage[i].Type = StructureType.SwapchainImageOpenglESKhr;
                colorSwapChainImage[i].Next = null;
            }

            fixed (SwapchainImageOpenGLESKHR* pcolorSwapChainImage = colorSwapChainImage)
            {
                xrResult = _oxrInstance.OXRAPI.Api.EnumerateSwapchainImages(
                    frameBuffer.ColorSwapChain.Swapchain,
                    frameBuffer.TextureSwapChainLength,
                    ref frameBuffer.TextureSwapChainLength,
                    (SwapchainImageBaseHeader*)pcolorSwapChainImage);
                Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainImages");
            }

            frameBuffer.RenderTarget = new RenderTarget2D(_graphics.GraphicsDevice,
                        width, height,
                        false, SurfaceFormat.Color,
                        DepthFormat.Depth24Stencil8,
                        4,
                        RenderTargetUsage.DiscardContents,
                        true
                        );

            frameBuffer.FrameBuffers = new int[frameBuffer.TextureSwapChainLength];

            Android.Opengl.GLES30.GlGenFramebuffers((int)frameBuffer.TextureSwapChainLength, frameBuffer.FrameBuffers, 0);

            //ConcreteGraphicsAdapter adapter = ((IPlatformGraphicsAdapter)_graphics.GraphicsDevice.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
            //var GL = adapter.Ogl;

            for (uint i = 0; i < frameBuffer.TextureSwapChainLength; i++)
            {
                // Create the color buffer texture.
                uint colorTexture = frameBuffer.ColorSwapChainImage[i].Image;

                // create DrawFramebuffer
                //frameBuffer.FrameBuffers[i] = GL.GenFramebuffer();
                //GL.CheckGLError();

                Android.Opengl.GLES30.GlBindFramebuffer(Android.Opengl.GLES30.GlDrawFramebuffer, frameBuffer.FrameBuffers[i]);
                //GL.BindFramebuffer(OpenGL.FramebufferTarget.DrawFramebuffer, frameBuffer.FrameBuffers[i]);
                //GL.CheckGLError();

                Android.Opengl.GLES30.GlFramebufferTexture2D(
                    Android.Opengl.GLES30.GlDrawFramebuffer,
                    Android.Opengl.GLES30.GlColorAttachment0,
                    Android.Opengl.GLES30.GlTexture2d, (int)colorTexture, 0);
                //GL.FramebufferTexture2D(
                //  OpenGL.FramebufferTarget.DrawFramebuffer,
                //  OpenGL.FramebufferAttachment.ColorAttachment0,
                //  OpenGL.TextureTarget.Texture2D, (int)colorTexture, 0);
                //GL.CheckGLError();
            }

            return true;
        }


        public unsafe delegate Result xrGetOpenGLESGraphicsRequirementsKHRDelegate(Instance xrInstance, ulong systemId, GraphicsRequirementsOpenGLESKHR* graphicsRequirements);
        public unsafe delegate Result xrEnumerateColorSpacesFB(Session xrSession, uint count, uint* outcount, ColorSpaceFB* colorSpaces);
        public unsafe delegate Result xrSetColorSpaceFB(Session xrSession, ColorSpaceFB colorSpace);

        public unsafe delegate Result xrEnumerateDisplayRefreshRatesFB(Session xrSession, uint count, ref uint outcount, float* displayRefreshRates);
        public unsafe delegate Result xrGetDisplayRefreshRateFB(Session xrSession, float* displayRefreshRate);
        public unsafe delegate Result xrRequestDisplayRefreshRateFB(Session xrSession, float displayRefreshRate);

        public unsafe delegate Result xrPerfSettingsSetPerformanceLevelEXT(Session xrSession, PerfSettingsDomainEXT perfDomain, PerfSettingsLevelEXT perfLevel);

        const int ovrMaxLayerCount = 16;
        const int ovrMaxNumEyes = 2;

        const int CPU_LEVEL = 2;
        const int GPU_LEVEL = 3;
        const int NUM_MULTI_SAMPLES = 4;

        public struct ovrRenderer
        {
            public ovrFramebuffer[] FrameBuffer = new ovrFramebuffer[ovrMaxNumEyes];

            public ovrRenderer()
            {
            }
        }

        private long ToXrTime(double sec)
        {
            return (long)(sec * 1000 * 1000 * 1000); //nSec
        }

        private unsafe LocVel GetSpaceLocVel(OxrSpace space, long time)
        {
            Result xrResult;

            OxrSpace currentSpace = (_isTrackFloorLevelEnabled) ? _LocalFloorSpace : _LocalSpace;

            SpaceVelocity velocity = new SpaceVelocity(StructureType.SpaceVelocity);

            xrResult = _oxrInstance.LocateSpace(space, currentSpace, time, &velocity,
                out Posef pose, out SpaceLocationFlags locationFlags);
            Debug.Assert(xrResult == Result.Success, "LocateSpace");

            LocVel lv = new LocVel();
            lv.LocationFlags = locationFlags;
            lv.Pose = pose.ToPose3();

            lv.VelocityFlags = velocity.VelocityFlags;
            lv.LinearVelocity = velocity.LinearVelocity.ToVector3();
            lv.AngularVelocity = velocity.AngularVelocity.ToVector3();

            return lv;
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

        public struct LocVel
        {
            public SpaceLocationFlags LocationFlags;
            public Pose3 Pose;

            public SpaceVelocityFlags VelocityFlags;
            public Vector3 LinearVelocity;
            public Vector3 AngularVelocity;

        }

        // https://github.com/KhronosGroup/OpenXR-SDK/blob/main/src/common/xr_linear.h

        static unsafe Vector3f XrVector3f_Set(float value)
        {
            Vector3f result;
            result.X = value;
            result.Y = value;
            result.Z = value;
            return result;
        }

        static unsafe Vector3f XrVector3f_Add(Vector3f a, Vector3f b)
        {
            Vector3f result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            return result;
        }

        static unsafe Vector3f XrVector3f_Scale(Vector3f a, float scaleFactor)
        {
            Vector3f result;
            result.X = a.X * scaleFactor;
            result.Y = a.Y * scaleFactor;
            result.Z = a.Z * scaleFactor;
            return result;
        }

        static unsafe float XrRcpSqrt(float x) 
        {
            float SMALLEST_NON_DENORMAL = 1.1754943508222875e-038f;  // ( 1U << 23 )
            float rcp = (x >= SMALLEST_NON_DENORMAL) ? 1.0f / (float)Math.Sqrt(x) : 1.0f;
            return rcp;
        }

        static unsafe Quaternionf XrQuaternionf_CreateIdentity()
        {
            Quaternionf result;
            result.X = 0.0f;
            result.Y = 0.0f;
            result.Z = 0.0f;
            result.W = 1.0f;
            return result;
        }

        static unsafe Quaternionf XrQuaternionf_CreateFromAxisAngle(Vector3f axis, float angleInRadians) 
        {
            Quaternionf result;
            float s = (float)Math.Sin(angleInRadians / 2.0f);
            float lengthRcp = XrRcpSqrt(axis.X * axis.X + axis.Y * axis.Y + axis.Z * axis.Z);
            result.X = s * axis.X * lengthRcp;
            result.Y = s * axis.Y * lengthRcp;
            result.Z = s * axis.Z * lengthRcp;
            result.W = (float)Math.Cos(angleInRadians / 2.0f);
            return result;
        }

        static unsafe Quaternionf XrQuaternionf_Multiply(Quaternionf a, Quaternionf b)
        {
            Quaternionf result;
            result.X = (b.W * a.X) + (b.X * a.W) + (b.Y * a.Z) - (b.Z * a.Y);
            result.Y = (b.W * a.Y) - (b.X * a.Z) + (b.Y * a.W) + (b.Z * a.X);
            result.Z = (b.W * a.Z) + (b.X * a.Y) - (b.Y * a.X) + (b.Z * a.W);
            result.W = (b.W * a.W) - (b.X * a.X) - (b.Y * a.Y) - (b.Z * a.Z);
            return result;
        }

        static unsafe Quaternionf XrQuaternionf_Invert(Quaternionf q)
        {
            Quaternionf result;
            result.X = -q.X;
            result.Y = -q.Y;
            result.Z = -q.Z;
            result.W =  q.W;
            return result;
        }

        static unsafe Vector3f XrQuaternionf_RotateVector3f(Quaternionf a, Vector3f v) 
        {
            Quaternionf q = new Quaternionf(v.X, v.Y, v.Z, 0.0f);
            Quaternionf aq = XrQuaternionf_Multiply(q, a);
            Quaternionf aInv = XrQuaternionf_Invert(a);
            Quaternionf aqaInv = XrQuaternionf_Multiply(aInv, aq);

            Vector3f result;
            result.X = aqaInv.X;
            result.Y = aqaInv.Y;
            result.Z = aqaInv.Z;
            return result;
        }

        static unsafe Vector3f XrQuaternionf_RotateVector3f_Opt(Quaternionf q, Vector3f vec)
        {
            Vector4f qvec;
            qvec.X = +(q.W * vec.X) -(q.Z * vec.Y) +(q.Y * vec.Z);
            qvec.Y = +(q.Z * vec.X) +(q.W * vec.Y) -(q.X * vec.Z);
            qvec.Z = -(q.Y * vec.X) +(q.X * vec.Y) +(q.W * vec.Z);
            qvec.W = -(q.X * vec.X) -(q.Y * vec.Y) -(q.Z * vec.Z);

            Vector3f result;
            result.X = +(qvec.X * q.W) -(qvec.Y * q.Z) +(qvec.Z * q.Y) -(qvec.W * q.X);
            result.Y = +(qvec.X * q.Z) +(qvec.Y * q.W) -(qvec.Z * q.X) -(qvec.W * q.Y);
            result.Z = -(qvec.X * q.Y) +(qvec.Y * q.X) +(qvec.Z * q.W) -(qvec.W * q.Z);

            return result;
        }

        static unsafe Posef XrPosef_CreateIdentity()
        {
            Posef result;
            result.Orientation = XrQuaternionf_CreateIdentity();
            result.Position = XrVector3f_Set(0);
            return result;
        }

        static unsafe Vector3f XrPosef_TransformVector3f(Posef a, Vector3f v)
        {
            Vector3f result;
            Vector3f r0 = XrQuaternionf_RotateVector3f_Opt(a.Orientation, v);
            result = XrVector3f_Add(r0, a.Position);
            return result;
        }

        static unsafe Posef XrPosef_Multiply(Posef a, Posef b)
        {
            Posef result;
            result.Orientation = XrQuaternionf_Multiply(b.Orientation, a.Orientation);
            result.Position = XrPosef_TransformVector3f(a, b.Position);
            return result;
        }

        static unsafe Posef XrPosef_Invert(Posef a)
        {
            Posef result;
            result.Orientation = XrQuaternionf_Invert(a.Orientation);
            Vector3f aPosNeg = XrVector3f_Scale(a.Position, -1.0f);
            result.Position = XrQuaternionf_RotateVector3f_Opt(result.Orientation, aPosNeg);
            return result;
        }

        // Creates a projection matrix based on the specified FOV.
        static unsafe Matrix XrMatrixf_CreateProjectionFov(GraphicsBackend graphicsApi, Fovf fov,
                                                            float nearZ, float farZ)
        {
            float tanLeft = (float)Math.Tan(fov.AngleLeft);
            float tanRight = (float)Math.Tan(fov.AngleRight);
            float tanDown = (float)Math.Tan(fov.AngleDown);
            float tanUp = (float)Math.Tan(fov.AngleUp);


            Matrix result;
            result = XrMatrixf_CreateProjection(graphicsApi, tanLeft, tanRight, tanUp, tanDown, nearZ, farZ);
            return result;
        }

        // Creates a projection matrix based on the specified dimensions.
        // The projection matrix transforms -Z=forward, +Y=up, +X=right to the appropriate clip space for the graphics API.
        // The far plane is placed at infinity if farZ <= nearZ.
        // An infinite projection matrix is preferred for rasterization because, except for
        // things *right* up against the near plane, it always provides better precision:
        //              "Tightening the Precision of Perspective Rendering"
        //              Paul Upchurch, Mathieu Desbrun
        //              Journal of Graphics Tools, Volume 16, Issue 1, 2012
        static unsafe Matrix XrMatrixf_CreateProjection(GraphicsBackend graphicsApi, float tanAngleLeft,
                                                 float tanAngleRight, float tanAngleUp, float tanAngleDown,
                                                 float nearZ, float farZ)
        {
            float tanAngleWidth = tanAngleRight - tanAngleLeft;

            // Set to tanAngleDown - tanAngleUp for a clip space with positive Y down (Vulkan).
            // Set to tanAngleUp - tanAngleDown for a clip space with positive Y up (OpenGL / D3D / Metal).
            float tanAngleHeight = graphicsApi == GraphicsBackend.Vulkan ? (tanAngleDown - tanAngleUp) : (tanAngleUp - tanAngleDown);

            // Set to nearZ for a [-1,1] Z clip space (OpenGL / OpenGL ES).
            // Set to zero for a [0,1] Z clip space (Vulkan / D3D / Metal).
            float offsetZ = (graphicsApi == GraphicsBackend.OpenGL || graphicsApi == GraphicsBackend.GLES) ? nearZ : 0;

            if (farZ <= nearZ)
            {
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

                result.M31 = (tanAngleRight + tanAngleLeft) / tanAngleWidth;
                result.M32 = (tanAngleUp + tanAngleDown) / tanAngleHeight;
                result.M33 = -1.0f;
                result.M34 = -1.0f;

                result.M41 = 0.0f;
                result.M42 = 0.0f;
                result.M43 = -(nearZ + offsetZ);
                result.M44 = 0.0f;

                return result;
            }
            else
            {
                Matrix result;

                // normal projection
                result.M11 = (2f) / tanAngleWidth;
                result.M12 = result.M13 = result.M14 = 0;

                result.M22 = (2f) / tanAngleHeight;
                result.M21 = result.M23 = result.M24 = 0;

                result.M31 = (tanAngleRight + tanAngleLeft) / tanAngleWidth;
                result.M32 = (tanAngleUp + tanAngleDown) / tanAngleHeight;
                result.M33 = (farZ + offsetZ) / (nearZ-farZ);
                result.M34 = -1;

                result.M43 = (farZ * (nearZ + offsetZ)) / (nearZ-farZ);
                result.M41 = result.M42 = result.M44 = 0;

                return result;
            }
        }


        private unsafe void ovrRenderer_RenderFrame_Acquire(ref ovrFramebuffer frameBuffer)
        {
            Result xrResult;

            // Acquire the swapchain image
            xrResult = frameBuffer.ColorSwapChain.AcquireSwapchainImage(ref frameBuffer.TextureSwapChainIndex);
            Debug.Assert(xrResult == Result.Success, "AcquireSwapchainImage");

            SwapchainImageWaitInfo waitInfo = new SwapchainImageWaitInfo(StructureType.SwapchainImageWaitInfo);
            waitInfo.Timeout = 1000000000; /* timeout in nanoseconds */
            xrResult = frameBuffer.ColorSwapChain.WaitSwapchainImage(ref waitInfo);

            int i = 0;
            while (xrResult == Result.TimeoutExpired)
            {
                xrResult = frameBuffer.ColorSwapChain.WaitSwapchainImage(ref waitInfo);
                i++;
                Console.WriteLine(
                    " Retry xrWaitSwapchainImage {0} times due to XR_TIMEOUT_EXPIRED (duration {0} seconds)",
                    i,
                    waitInfo.Timeout * (1E-9));
            }
        }

        private unsafe void ovrRenderer_RenderFrame_Release(ref ovrRenderer renderer, int eye)
        {
            fixed (ovrFramebuffer* frameBuffer = &renderer.FrameBuffer[eye])
            {
                Result xrResult;

                xrResult = frameBuffer->ColorSwapChain.ReleaseSwapchainImage();
            }
        }

        private void UpdateStageBounds()
        {
            Extent2Df stageBounds = default;

            Result xrResult;
            xrResult = _oxrSession.GetReferenceSpaceBoundsRect(ReferenceSpaceType.Stage, out stageBounds);
            if (xrResult != Result.Success)
            {
                Console.WriteLine("Stage bounds query failed: using small defaults");
                stageBounds.Width = 1.0f;
                stageBounds.Height = 1.0f;

                _CurrentStageSpace = _FakeStageSpace;
            }

            Console.WriteLine("Stage bounds: width = {0}, depth {1}", stageBounds.Width, stageBounds.Height);

            float halfWidth = stageBounds.Width * 0.5f;
            float halfDepth = stageBounds.Height * 0.5f;

            //ovrGeometry_Destroy(&pappState->Scene.GroundPlane);
            //ovrGeometry_DestroyVAO(&pappState->Scene.GroundPlane);
            //ovrGeometry_CreateStagePlane(
            //    &pappState->Scene.GroundPlane, -halfWidth, -halfDepth, halfWidth, halfDepth);
            //ovrGeometry_CreateVAO(&pappState->Scene.GroundPlane);
        }


        public struct ovrTrackedController
        {
            public bool Active;
            public Pose3 Pose;
            internal Vector3 LinearVelocity;

            internal void Clear()
            {
                this.Active = false;
                this.Pose = Pose3.Identity;
                this.LinearVelocity = Vector3.Zero;
            }
        }

        public struct ovrScene
        {
            public bool CreatedScene;
            public ovrTrackedController[] TrackedController = new ovrTrackedController[4]; // left aim, left grip, right aim, right grip

            public OxrSwapChain CylinderSwapChain;
            public SwapchainImageOpenGLESKHR[] CylinderSwapChainImage;
            //internal RenderTargetSwapChain CylinderSwapRenderTarget;

            public ovrScene()
            {
            }
        }

        internal abstract class OxrSwapChainDataBase
        {
            //internal abstract OvrTextureSwapChain SwapChain { get; }

            internal abstract RenderTarget2D GetRenderTarget(int eye);
            internal abstract int SubmitRenderTarget(GraphicsDevice graphicsDevice, RenderTarget2D rt);
        }


        ovrScene _Scene = new ovrScene();

        private unsafe bool ovrScene_IsCreated(ref ovrScene scene)
        {
            return scene.CreatedScene;
        }

        private unsafe void ovrScene_Create(ref ovrScene scene)
        {
            // Simple ground plane and box geometry.
            {
            }


            // Simple cubemap loaded from ktx file on the sdcard. NOTE: Currently only
            // handles texture2d or cubemap types.
            // CubeMapSwapChain
            { 
            }

            // Simple checkerboard pattern.
            //  EquirectSwapChain
            { 
            }


            // Simple checkerboard pattern.
            // CylinderSwapChain
            { 
            }

            // Simple checkerboard pattern.
            // QuadSwapChain
            if (false)
            {
                Result xrResult;

                int CYLINDER_WIDTH = 512;
                int CYLINDER_HEIGHT = 128;

                SwapchainCreateInfo swapChainCreateInfo = new SwapchainCreateInfo(StructureType.SwapchainCreateInfo);
                swapChainCreateInfo.CreateFlags = SwapchainCreateFlags.StaticImageBit;
                swapChainCreateInfo.UsageFlags =
                     SwapchainUsageFlags.SampledBit | SwapchainUsageFlags.ColorAttachmentBit;
                swapChainCreateInfo.Format = SRGB8_ALPHA8;
                swapChainCreateInfo.SampleCount = 1;
                swapChainCreateInfo.Width = (uint)CYLINDER_WIDTH;
                swapChainCreateInfo.Height = (uint)CYLINDER_HEIGHT;
                swapChainCreateInfo.FaceCount = 1;
                swapChainCreateInfo.ArraySize = 1;
                swapChainCreateInfo.MipCount = 1;

                // Create the swapchain.
                xrResult = _oxrSession.CreateSwapchain(ref swapChainCreateInfo, out scene.CylinderSwapChain);
                Debug.Assert(xrResult == Result.Success, "CreateSwapchain");
                // Get the number of swapchain images.
                uint length;
                xrResult = _oxrInstance.OXRAPI.Api.EnumerateSwapchainImages(scene.CylinderSwapChain.Swapchain, 0, &length, null);
                Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainImages");

                SwapchainImageOpenGLESKHR[] cylinderSwapChainImage = new SwapchainImageOpenGLESKHR[length];
                scene.CylinderSwapChainImage = cylinderSwapChainImage;
                for (uint i = 0; i < length; i++)
                {
                    scene.CylinderSwapChainImage[i].Type = StructureType.SwapchainImageOpenglESKhr;
                    scene.CylinderSwapChainImage[i].Next = null;
                }
                fixed (SwapchainImageOpenGLESKHR* pcylinderSwapChainImage = cylinderSwapChainImage)
                {
                    xrResult = _oxrInstance.OXRAPI.Api.EnumerateSwapchainImages(
                    scene.CylinderSwapChain.Swapchain,
                    length,
                    &length,
                    (SwapchainImageBaseHeader*)pcylinderSwapChainImage);
                    Debug.Assert(xrResult == Result.Success, "EnumerateSwapchainImages");
                }

                uint[] data = new uint[CYLINDER_WIDTH * CYLINDER_HEIGHT];
                fixed (uint* texData = data)
                {
                    for (int y = 0; y < CYLINDER_HEIGHT; y++)
                    {
                        for (int x = 0; x < CYLINDER_WIDTH; x++)
                        {
                            texData[y * CYLINDER_WIDTH + x] = ((x ^ y) & 64)!=0 ? 0xFF6464F0 : 0xFFF06464;
                        }
                    }
                    for (int y = 0; y < CYLINDER_HEIGHT; y++)
                    {
                        int g = (int)(255.0f * (y / (CYLINDER_HEIGHT - 1.0f)));
                        texData[y * CYLINDER_WIDTH] = (uint)(0xff000000 | (g << 8));
                    }
                    for (int x = 0; x < CYLINDER_WIDTH; x++)
                    {
                        int r = (int)(255.0f * (x / (CYLINDER_WIDTH - 1.0f)));
                        texData[x] = (uint)(0xff000000 | r);
                    }

                    int texId = (int)scene.CylinderSwapChainImage[0].Image;

                    //scene.CylinderSwapRenderTarget = new RenderTargetSwapChain(_graphics.GraphicsDevice,
                    //    new IntPtr(texId),
                    //    CYLINDER_WIDTH,
                    //    CYLINDER_HEIGHT,
                    //    false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents,
                    //    PresentInterval.Default
                    //    );
                    //scene.CylinderSwapRenderTarget.SetData<uint>(0, 
                    //    new Rectangle(0,0, CYLINDER_WIDTH, CYLINDER_HEIGHT), 
                    //    data, 0, data.Length);


                    //var adapter = ((IPlatformGraphicsAdapter)_graphics.GraphicsDevice.Adapter).Strategy.ToConcrete<ConcreteGraphicsAdapter>();
                    //var GL = adapter.Ogl;

                    //GL.BindTexture(OpenGL.TextureTarget.Texture2D, texId);
                    //GL.CheckGLError();
                    //GL.TexSubImage2D(
                    //    OpenGL.TextureTarget.Texture2D,
                    //    0,
                    //    0,
                    //    0,
                    //    CYLINDER_WIDTH,
                    //    CYLINDER_HEIGHT,
                    //    OpenGL.PixelFormat.Rgba,
                    //    OpenGL.PixelType.UnsignedByte,
                    //    new IntPtr(texData));
                    //GL.CheckGLError();

                    //GL.TexParameteri(OpenGL.TextureTarget.Texture2D, OpenGL.TextureParameterName.TextureWrapS, (int)OpenGL.TextureWrapMode.ClampToBorder);
                    //GL.CheckGLError();
                    //GL.TexParameteri(OpenGL.TextureTarget.Texture2D, OpenGL.TextureParameterName.TextureWrapT, (int)OpenGL.TextureWrapMode.ClampToBorder);
                    //GL.CheckGLError();
                    //float[] borderColor = { 0.0f, 0.0f, 0.0f, 0.0f };
                    //fixed(float* pborderColor = borderColor)
                    //GL.TexParameterfv(OpenGL.TextureTarget.Texture2D, OpenGL.TextureParameterName.TextureBorderColor, pborderColor);
                    //GL.CheckGLError();

                    //GL.BindTexture(OpenGL.TextureTarget.Texture2D, 0);
                    //GL.CheckGLError();

                } // free(texData);

                uint index = 0;
                xrResult = scene.CylinderSwapChain.AcquireSwapchainImage(ref index);
                Debug.Assert(xrResult == Result.Success, "AcquireSwapchainImage");

                SwapchainImageWaitInfo waitInfo = new SwapchainImageWaitInfo(StructureType.SwapchainImageWaitInfo);
                xrResult = scene.CylinderSwapChain.WaitSwapchainImage(ref waitInfo);
                Debug.Assert(xrResult == Result.Success, "WaitSwapchainImage");

                xrResult = scene.CylinderSwapChain.ReleaseSwapchainImage();
                Debug.Assert(xrResult == Result.Success, "ReleaseSwapchainImage");
            }

            scene.CreatedScene = true;
            return;
        }

        private unsafe void ovrApp_HandleXrEvents()
        {
            Result xrResult;

            EventDataBuffer eventDataBuffer = new EventDataBuffer(StructureType.EventDataBuffer);

            // Poll for events
            for (; ; )
            {
                EventDataBaseHeader* baseEventHeader = (EventDataBaseHeader*)(&eventDataBuffer);
                baseEventHeader->Type = StructureType.EventDataBuffer;
                baseEventHeader->Next = null;

                Result r;
                r = _oxrInstance.PollEvent(ref eventDataBuffer);
                if (r != Result.Success)
                    break;

                switch (baseEventHeader->Type)
                {
                    case StructureType.EventDataEventsLost:
                        Console.WriteLine("xrPollEvent: received XR_TYPE_EVENT_DATA_EVENTS_LOST event");
                        break;
                    case StructureType.EventDataInstanceLossPending:
                        {
                            EventDataInstanceLossPending* instance_loss_pending_event =
                                (EventDataInstanceLossPending*)(baseEventHeader);
                            Console.WriteLine(
                                "xrPollEvent: received XR_TYPE_EVENT_DATA_INSTANCE_LOSS_PENDING event: time {0}",
                                instance_loss_pending_event->LossTime // FromXrTime(instance_loss_pending_event->LossTime)
                                );
                        }
                        break;
                    case StructureType.EventDataInteractionProfileChanged:
                        Console.WriteLine("xrPollEvent: received XR_TYPE_EVENT_DATA_INTERACTION_PROFILE_CHANGED event");
                        break;
                    case StructureType.EventDataPerfSettingsExt:
                        {
                            EventDataPerfSettingsEXT* perf_settings_event =
                                (EventDataPerfSettingsEXT*)(baseEventHeader);
                            Console.WriteLine(
                                "xrPollEvent: received XR_TYPE_EVENT_DATA_PERF_SETTINGS_EXT event: type {0} subdomain {1} : level {2} -> level {3}",
                                perf_settings_event->Type,
                                perf_settings_event->SubDomain,
                                perf_settings_event->FromLevel,
                                perf_settings_event->ToLevel);
                        }
                        break;
                    case StructureType.EventDataDisplayRefreshRateChangedFB:
                        {
                            EventDataDisplayRefreshRateChangedFB* refresh_rate_changed_event =
                                (EventDataDisplayRefreshRateChangedFB*)(baseEventHeader);
                            Console.WriteLine(
                                "xrPollEvent: received XR_TYPE_EVENT_DATA_DISPLAY_REFRESH_RATE_CHANGED_FB event: fromRate {0} -> toRate {1}",
                                refresh_rate_changed_event->FromDisplayRefreshRate,
                                refresh_rate_changed_event->ToDisplayRefreshRate);
                        }
                        break;
                    case StructureType.EventDataReferenceSpaceChangePending:
                        {
                            EventDataReferenceSpaceChangePending* ref_space_change_event =
                                (EventDataReferenceSpaceChangePending*)(baseEventHeader);
                            Console.WriteLine(
                                "xrPollEvent: received XR_TYPE_EVENT_DATA_REFERENCE_SPACE_CHANGE_PENDING event: changed space: {0} for session {1} at time {2}",
                                ref_space_change_event->ReferenceSpaceType,
                                ref_space_change_event->Session.Handle,
                                ref_space_change_event->ChangeTime //FromXrTime(ref_space_change_event->ChangeTime)
                                );
                        }
                        break;

                    case StructureType.EventDataSessionStateChanged:
                        {
                            EventDataSessionStateChanged* session_state_changed_event =
                                (EventDataSessionStateChanged*)(baseEventHeader);
                            Console.WriteLine(
                                 "xrPollEvent: received XR_TYPE_EVENT_DATA_SESSION_STATE_CHANGED: {0} for session {1} at time {2}",
                                    session_state_changed_event->State,
                                    session_state_changed_event->Session.Handle,
                                    session_state_changed_event->Time //FromXrTime(session_state_changed_event->Time)
                                    );

                            this._sessionState = session_state_changed_event->State;

                            switch (session_state_changed_event->State)
                            {
                                case SessionState.Idle:
                                    break;
                                case SessionState.Focused:
                                    _Focused = true;
                                    break;
                                case SessionState.Visible:
                                    _Focused = false;
                                    break;
                                case SessionState.Ready:
                                    ovrApp_HandleSessionStateChangesReady(session_state_changed_event->State);
                                    break;
                                case SessionState.Stopping:
                                    ovrApp_HandleSessionStateChangesStopping(session_state_changed_event->State);
                                    break;

                                case SessionState.Synchronized:
                                    break;
                                case SessionState.LossPending:
                                    break;
                                case SessionState.Exiting:
                                    break;

                                default:
                                    Console.WriteLine("Unknown State: {0}.", session_state_changed_event->State);
                                    break;
                            }
                        }
                        break;

                    default:
                        Console.WriteLine("xrPollEvent: Unknown event {0}." , baseEventHeader->Type);
                        break;
                }
            }

        }

        ViewConfigurationProperties _viewportConfig;
        private bool _Resumed;
        private bool _Focused;
        private bool _SessionActive;
        SessionState _sessionState;

        SessionState IOculusDeviceStrategy.SessionState { get { return _sessionState; } }

        private unsafe void ovrApp_HandleSessionStateChangesReady(SessionState state)
        {
            //Debug.Assert(_Resumed);
            Debug.Assert(_SessionActive == false);

            Result xrResult;
            xrResult = _oxrSession.BeginSession(_viewportConfig.ViewConfigurationType);
            Debug.Assert(xrResult == Result.Success, "BeginSession");

            _SessionActive = (xrResult == Result.Success);

            // Set session state once we have entered VR mode and have a valid session object.
            if (_SessionActive)
            {
                PerfSettingsLevelEXT cpuPerfLevel = PerfSettingsLevelEXT.SustainedHighExt;
                switch (_CpuLevel)
                {
                    case 0:
                        cpuPerfLevel = PerfSettingsLevelEXT.PowerSavingsExt;
                        break;
                    case 1:
                        cpuPerfLevel = PerfSettingsLevelEXT.SustainedLowExt;
                        break;
                    case 2:
                        cpuPerfLevel = PerfSettingsLevelEXT.SustainedHighExt;
                        break;
                    case 3:
                        cpuPerfLevel = PerfSettingsLevelEXT.BoostExt;
                        break;
                    default:
                        Console.WriteLine("Invalid CPU level {0}", _CpuLevel);
                        break;
                }

                PerfSettingsLevelEXT gpuPerfLevel = PerfSettingsLevelEXT.SustainedHighExt;
                switch (_GpuLevel)
                {
                    case 0:
                        gpuPerfLevel = PerfSettingsLevelEXT.PowerSavingsExt;
                        break;
                    case 1:
                        gpuPerfLevel = PerfSettingsLevelEXT.SustainedLowExt;
                        break;
                    case 2:
                        gpuPerfLevel = PerfSettingsLevelEXT.SustainedHighExt;
                        break;
                    case 3:
                        gpuPerfLevel = PerfSettingsLevelEXT.BoostExt;
                        break;
                    default:
                        Console.WriteLine("Invalid GPU level {0}", _GpuLevel);
                        break;
                }

                xrResult = _oxrInstance.GetInstanceProcAddr<xrPerfSettingsSetPerformanceLevelEXT>(
                    "xrPerfSettingsSetPerformanceLevelEXT",
                    out xrPerfSettingsSetPerformanceLevelEXT pfnPerfSettingsSetPerformanceLevelEXT);
                Debug.Assert(xrResult == Result.Success, "xrPerfSettingsSetPerformanceLevelEXT");

                xrResult = pfnPerfSettingsSetPerformanceLevelEXT(
                    _oxrSession.Session, PerfSettingsDomainEXT.CpuExt, cpuPerfLevel);
                Debug.Assert(xrResult == Result.Success, "pfnPerfSettingsSetPerformanceLevelEXT");
                xrResult = pfnPerfSettingsSetPerformanceLevelEXT(
                    _oxrSession.Session, PerfSettingsDomainEXT.GpuExt, gpuPerfLevel);
                Debug.Assert(xrResult == Result.Success, "pfnPerfSettingsSetPerformanceLevelEXT");
            }
        }

        private void ovrApp_HandleSessionStateChangesStopping(SessionState state)
        {
            Debug.Assert(_Resumed == false);
            Debug.Assert(_SessionActive);

            Result xrResult;

            xrResult = _oxrSession.EndSession();
            Debug.Assert(xrResult == Result.Success, "EndSession");
            _SessionActive = false;
        }

        private unsafe void OnDisplayLost()
        {
            //OvrGraphicsLuid prevGraphicsLuid = _ovrSession.GraphicsLuid;

            // destroy session
            //_ovrSession.Dispose();
            //_ovrSession = null;
            //_ovrClient.Dispose();
            //_ovrClient = null;

            _deviceState = XRDeviceState.Disabled;
        }

        unsafe public struct ovrFramebuffer
        {
            public int Width;
            public int Height;
            public int Multisamples;

            public uint TextureSwapChainLength;
            public uint TextureSwapChainIndex;
            public OxrSwapChain ColorSwapChain;

            public SwapchainImageOpenGLESKHR[] ColorSwapChainImage;
            public RenderTarget2D RenderTarget;
            internal int[] FrameBuffers;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ovrCompositorLayer_Union
        {
            [FieldOffset(0)]
            public CompositionLayerBaseHeader BaseHeader;

            [FieldOffset(0)]
            public CompositionLayerProjection Projection;
            [FieldOffset(0)]
            public CompositionLayerQuad Quad;
            [FieldOffset(0)]
            public CompositionLayerCylinderKHR Cylinder;
            [FieldOffset(0)]
            public CompositionLayerCubeKHR Cube;
            [FieldOffset(0)]
            public CompositionLayerEquirect2KHR Equirect2;

            [FieldOffset(0)]
            public CompositionLayerPassthroughFB PassthroughFB;
        }

        public unsafe delegate Result xrCreateFoveationProfileFBDelegate(Session session, FoveationProfileCreateInfoFB* profileCreateInfo, FoveationProfileFB* foveationProfile);
        public unsafe delegate Result xrDestroyFoveationProfileFBDelegate(FoveationProfileFB* foveationProfile);
        public unsafe delegate Result xrUpdateSwapchainFBDelegate(Swapchain ColorSwapChainHandle, SwapchainStateBaseHeaderFB* foveationUpdateState);

        private unsafe bool ovrRenderer_SetFoveation(
            ref ovrRenderer renderer,
            FoveationLevelFB level,
            float verticalOffset,
            FoveationDynamicFB dynamic
            )
        {
            Result xrResult;

            xrResult = _oxrInstance.GetInstanceProcAddr<xrCreateFoveationProfileFBDelegate>("xrCreateFoveationProfileFB",
                out xrCreateFoveationProfileFBDelegate Fun_pfnCreateFoveationProfileFB);
            Debug.Assert(xrResult == Result.Success, "GetInstanceProcAddr");

            xrResult = _oxrInstance.GetInstanceProcAddr<xrDestroyFoveationProfileFBDelegate>("xrDestroyFoveationProfileFB",
                out xrDestroyFoveationProfileFBDelegate Fun_pfnDestroyFoveationProfileFB);
            Debug.Assert(xrResult == Result.Success, "GetInstanceProcAddr");

            PfnVoidFunction pfnUpdateSwapchainFB;
            xrResult = _oxrInstance.GetInstanceProcAddr<xrUpdateSwapchainFBDelegate>("xrUpdateSwapchainFB", 
                out xrUpdateSwapchainFBDelegate Fun_pfnUpdateSwapchainFB);
            Debug.Assert(xrResult == Result.Success, "GetInstanceProcAddr");

            for (int eye = 0; eye < ovrMaxNumEyes; eye++)
            {
                FoveationLevelProfileCreateInfoFB levelProfileCreateInfo = new FoveationLevelProfileCreateInfoFB(StructureType.FoveationLevelProfileCreateInfoFB);
                levelProfileCreateInfo.Level = level;
                levelProfileCreateInfo.VerticalOffset = verticalOffset;
                levelProfileCreateInfo.Dynamic = dynamic;

                FoveationProfileCreateInfoFB profileCreateInfo = new FoveationProfileCreateInfoFB(StructureType.FoveationProfileCreateInfoFB);
                profileCreateInfo.Next = &levelProfileCreateInfo;

                FoveationProfileFB foveationProfile = default;
                xrResult = Fun_pfnCreateFoveationProfileFB(_oxrSession.Session, &profileCreateInfo, &foveationProfile);
                Debug.Assert(xrResult == Result.Success, "Fun_pfnCreateFoveationProfileFB");


                SwapchainStateFoveationFB foveationUpdateState = new SwapchainStateFoveationFB(StructureType.SwapchainStateFoveationFB);
                foveationUpdateState.Profile = foveationProfile;

                xrResult = Fun_pfnUpdateSwapchainFB(
                    renderer.FrameBuffer[eye].ColorSwapChain.Swapchain,
                    (SwapchainStateBaseHeaderFB*)&foveationUpdateState);
                Debug.Assert(xrResult == Result.Success, "Fun_pfnUpdateSwapchainFB");


                //xrResult = Fun_pfnDestroyFoveationProfileFB(&foveationProfile);
                //Debug.Assert(xrResult == Result.Success, "Fun_pfnDestroyFoveationProfileFB");
            }

            return false;
        }

        private int CreateDefaultLayer(GraphicsDevice graphicsDevice,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
            int pixelsPerDisplayPixel
            //,
            //out OvrLayerEyeFov layer
            )
        {
            int ovrResult = 0;

            //OvrHmdDesc HmdDesc = _ovrSession.GetHmdDesc();

            // create layer
            //layer = default(OvrLayerEyeFov);
            //layer.Header.Type = OvrLayerType.EyeFov;
            //layer.Header.Flags = 0;

            for (int eye = 0; eye < 2; eye++)
            {
                //OvrFovPort fov = HmdDesc.DefaultEyeFov[eye];
                //OvrSizei texRes = _ovrSession.GetFovTextureSize((OvrEyeType)eye, fov, pixelsPerDisplayPixel);
                //layer.Viewport[eye] = new OvrRecti(0, 0, texRes.W, texRes.H);

                //ovrResult = ConcreteOvrSwapChainData.CreateSwapChain(
                //    graphicsDevice, _ovrSession,
                //    texRes.W, texRes.H,
                //    preferredFormat, preferredDepthFormat, preferredMultiSampleCount,
                //    out _swapChainData[eye]);
                //layer.ColorTexture[eye] = _swapChainData[eye].SwapChain.NativePtr;

                //OvrEyeRenderDesc renderDesc = _ovrSession.GetRenderDesc((OvrEyeType)eye, fov);
                //layer.Fov[eye] = renderDesc.Fov;
                //_hmdToEyePose[eye] = renderDesc.HmdToEyePose;
            }

            return 0;
        }






        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_passthroughFB != null)
                    _passthroughFB.Dispose();
                if (_passthroughLayerFB != null)
                    _passthroughLayerFB.Dispose();

                if (_HeadSpace != null)
                    _HeadSpace.Dispose();
                if (_LocalSpace != null)
                    _LocalSpace.Dispose();
                if (_LocalFloorSpace != null)
                    _LocalFloorSpace.Dispose();
                if (_FakeStageSpace != null)
                    _FakeStageSpace.Dispose();
                if (_StageSpace != null)
                    _StageSpace.Dispose();

                if (_leftControllerAimSpace != null)
                    _leftControllerAimSpace.Dispose();
                if (_rightControllerAimSpace != null)
                    _rightControllerAimSpace.Dispose();
                if (_leftControllerGripSpace != null)
                    _leftControllerGripSpace.Dispose();
                if (_rightControllerGripSpace != null)
                    _rightControllerGripSpace.Dispose();
            }

            _passthroughFB = null;
            _passthroughLayerFB = null;

            _HeadSpace = null;
            _LocalSpace = null;
            _LocalFloorSpace = null;
            _FakeStageSpace = null;
            _StageSpace = null;
            _CurrentStageSpace = null;

            _leftControllerAimSpace = null;
            _rightControllerAimSpace = null;
            _leftControllerGripSpace = null;
            _rightControllerGripSpace = null;

        }

    }


    internal static class OpenXRExtensions
    {
        public static Vector3 ToVector3(this Vector3f value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        public static Quaternion ToQuaternion(this Quaternionf value)
        {
            return new Quaternion(value.X, value.Y, value.Z, value.W);
        }

        public static Pose3 ToPose3(this Posef value)
        {
            return new Pose3(
                        value.Orientation.ToQuaternion(),
                        value.Position.ToVector3()
                );
        }


        public static Vector3f ToVector3f(this Vector3 value)
        {
            return new Vector3f(value.X, value.Y, value.Z);
        }

        public static Quaternionf ToQuaternionf(this Quaternion value)
        {
            return new Quaternionf(value.X, value.Y, value.Z, value.W);
        }

        public static Posef ToPosef(this Pose3 value)
        {
            return new Posef(
                        value.Orientation.ToQuaternionf(),
                        value.Translation.ToVector3f()
                );
        }

    }
}