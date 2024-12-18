// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Input.Oculus;
using Microsoft.Xna.Platform.XR;

namespace Microsoft.Xna.Framework.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {
        IGraphicsDeviceService _graphics;
        XRSessionMode _sessionMode;
        bool _isTrackFloorLevelEnabled = false;

        HandsState _handsState;
        HeadsetState _headsetState;


        public override bool IsVRSupported
        {
            get { return false; }
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
            get { return XRDeviceState.Disabled; }
        }

        public override bool IsTrackFloorLevelEnabled
        {
            get { return _isTrackFloorLevelEnabled; }
        }


        public ConcreteXRDevice(string applicationName, Game game)
            : this(applicationName, game.Services)
        {
        }

        public ConcreteXRDevice(string applicationName, IServiceProvider services)
        {
            IGraphicsDeviceService graphics = services.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService;

            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this._graphics = graphics;

        }

        public override int BeginSessionAsync(XRSessionMode sessionMode)
        {
            return -1;
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
            yield return XREye.None;
        }

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            return null;
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            switch (eye)
            {
                case XREye.None:
                case XREye.Left:
                case XREye.Right:
                    throw new NotImplementedException();

                default: 
                    throw new ArgumentException("Eye");
            }
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            Debug.Assert(null == rt);

            return;
        }

        public override int EndFrame()
        {
            return 0;
        }

        public override HandsState GetHandsState()
        {
            return _handsState;
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


        internal void GetCapabilities(TouchControllerType controllerType, ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected, ref Buttons buttons, ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor, ref bool hasVoiceSupport)
        {
            controllerType = (TouchControllerType)0;
            gamePadType = GamePadType.Unknown;
            displayName = String.Empty;
            identifier = String.Empty;
            isConnected = false;

            buttons = default(Buttons);
            hasLeftVibrationMotor = false;
            hasRightVibrationMotor = false;
            hasVoiceSupport = false;
        }

        internal GamePadState GetGamePadState(TouchControllerType controllerType)
        {
            return default(GamePadState);
        }

        internal bool SetVibration(TouchControllerType controllerType, float amplitude)
        {
            bool result = false;

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