// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Oculus;
using Microsoft.Xna.Platform.XR;

namespace Microsoft.Xna.Framework.XR
{
    internal class ConcreteXRDevice : XRDeviceStrategy
    {

        public override XRMode Mode
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override XRDeviceState State
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override bool TrackFloorLevelOrigin
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }


        public ConcreteXRDevice(string applicationName, Game game, XRMode mode)
            : this(applicationName, game.Services, mode)
        {
        }

        public ConcreteXRDevice(string applicationName, IServiceProvider services, XRMode mode)
        {
            throw new PlatformNotSupportedException();
        }

        public override int CreateDevice()
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

        internal void GetCapabilities(TouchControllerType controllerType, ref GamePadType gamePadType, ref string displayName, ref string identifier, ref bool isConnected, ref Buttons buttons, ref bool hasLeftVibrationMotor, ref bool hasRightVibrationMotor, ref bool hasVoiceSupport)
        {
            throw new PlatformNotSupportedException();
        }

        internal TouchControllerState GetTouchControllerState(TouchControllerType controllerType)
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