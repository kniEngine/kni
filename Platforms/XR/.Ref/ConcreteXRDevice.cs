// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.XR;
using Microsoft.Xna.Platform.XR;

namespace Microsoft.Xna.Framework.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {


        public override bool IsVRSupported
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool IsARSupported
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override XRSessionMode SessionMode
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override XRDeviceState DeviceState
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool IsTrackFloorLevelEnabled
        {
            get { throw new PlatformNotSupportedException(); }
        }


        public ConcreteXRDevice(string applicationName, Game game)
            : this(applicationName, game.Services)
        {
        }

        public ConcreteXRDevice(string applicationName, IServiceProvider services)
        {
            throw new PlatformNotSupportedException();
        }

        public override int BeginSessionAsync(XRSessionMode sessionMode)
        {
            throw new PlatformNotSupportedException();
        }

        public override int BeginFrame()
        {
            throw new PlatformNotSupportedException();
        }

        public override HeadsetState GetHeadsetState()
        {
            throw new PlatformNotSupportedException();
        }

        public override IEnumerable<XREye> GetEyes()
        {
            throw new PlatformNotSupportedException();
        }

        public override RenderTarget2D GetEyeRenderTarget(XREye eye)
        {
            throw new PlatformNotSupportedException();
        }

        public override Matrix CreateProjection(XREye eye, float znear, float zfar)
        {
            throw new PlatformNotSupportedException();
        }

        public override void CommitRenderTarget(XREye eye, RenderTarget2D rt)
        {
            throw new PlatformNotSupportedException();
        }

        public override int EndFrame()
        {
            throw new PlatformNotSupportedException();
        }

        public override HandsState GetHandsState()
        {
            throw new PlatformNotSupportedException();
        }

        public override void EndSessionAsync()
        {
            throw new PlatformNotSupportedException();
        }

        public override void TrackFloorLevelAsync(bool enable)
        {
            throw new PlatformNotSupportedException();
        }


        internal void GetCapabilities(TouchControllerType controllerType, ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected, ref Buttons buttons, ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor, ref bool hasVoiceSupport)
        {
            throw new PlatformNotSupportedException();
        }

        internal GamePadState GetGamePadState(TouchControllerType controllerType)
        {
            throw new PlatformNotSupportedException();
        }

        internal bool SetVibration(TouchControllerType controllerType, float amplitude)
        {
            throw new PlatformNotSupportedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

        }

    }
}