// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.XR;

namespace Microsoft.Xna.Platform.XR
{
    public interface IPlatformXRDevice
    {
        T GetStrategy<T>() where T : XRDeviceStrategy;
    }

    public abstract class XRDeviceStrategy : IDisposable
    {
        public abstract XRMode Mode { get; }
        public abstract XRDeviceState State { get; }
        public abstract bool IsConnected { get; }
        public abstract bool TrackFloorLevelOrigin { get; set; }

        public abstract int CreateDevice();
        public abstract int BeginFrame();
        public abstract HeadsetState GetHeadsetState();
        public abstract IEnumerable<XREye> GetEyes();
        public abstract RenderTarget2D GetEyeRenderTarget(XREye eye);
        internal abstract Viewport GetEyeViewport(XREye eye);
        public abstract Matrix CreateProjection(XREye eye, float znear, float zfar);
        public abstract void CommitRenderTarget(XREye eye, RenderTarget2D rt);
        public abstract int EndFrame();
        public abstract HandsState GetHandsState();


        #region IDisposable
        ~XRDeviceStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        #endregion
    }
}