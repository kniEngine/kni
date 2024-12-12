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
        public abstract bool IsVRSupported { get; }
        public abstract bool IsARSupported { get; }
        public abstract XRSessionMode SessionMode { get; }
        public abstract XRDeviceState DeviceState { get; }
        public abstract bool TrackFloorLevelOrigin { get; set; }

        public abstract int BeginSessionAsync(XRSessionMode sessionMode);
        public abstract int BeginFrame();
        public abstract HeadsetState GetHeadsetState();
        public abstract IEnumerable<XREye> GetEyes();
        public abstract RenderTarget2D GetEyeRenderTarget(XREye eye);
        public abstract Matrix CreateProjection(XREye eye, float znear, float zfar);
        public abstract void CommitRenderTarget(XREye eye, RenderTarget2D rt);
        public abstract int EndFrame();
        public abstract HandsState GetHandsState();
        public abstract void EndSessionAsync();


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