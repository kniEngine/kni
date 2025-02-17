// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public sealed class ConcreteTouchPanel : TouchPanelStrategy
    {

        public override IntPtr WindowHandle
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        public override int DisplayWidth
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        public override int DisplayHeight
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        public override DisplayOrientation DisplayOrientation
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }

        public override GestureType EnabledGestures
        {
            get { throw new PlatformNotSupportedException(); }
            set { throw new PlatformNotSupportedException(); }
        }


        public override bool IsGestureAvailable
        {
            get { throw new PlatformNotSupportedException(); }
        }

        public override TouchPanelCapabilities GetCapabilities()
        {
            throw new PlatformNotSupportedException();
        }

        public override TouchCollection GetState()
        {
            throw new PlatformNotSupportedException();
        }

        public override GestureSample ReadGesture()
        {
            throw new PlatformNotSupportedException();
        }

        public override void AddPressedEvent(int nativeTouchId, Vector2 position)
        {
            throw new PlatformNotSupportedException();
        }

        public override void AddMovedEvent(int nativeTouchId, Vector2 position)
        {
            throw new PlatformNotSupportedException();
        }

        public override void AddReleasedEvent(int nativeTouchId, Vector2 position)
        {
            throw new PlatformNotSupportedException();
        }

        public override void AddCanceledEvent(int nativeTouchId, Vector2 position)
        {
            throw new PlatformNotSupportedException();
        }

        public override void InvalidateTouches()
        {
            throw new PlatformNotSupportedException();
        }

    }
}