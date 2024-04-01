// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public abstract class TouchPanelStrategy
    {
        private IntPtr _windowHandle;
        private int _displayWidth;
        private int _displayHeight;
        private DisplayOrientation _displayOrientation;
        private GestureType _enabledGestures;

        protected TouchPanelCapabilities _capabilities;

        public virtual IntPtr WindowHandle
        {
            get { return _windowHandle; }
            set { _windowHandle = value; }

        }

        public virtual int DisplayWidth
        {
            get { return _displayWidth; }
            set { _displayWidth = value; }
        }

        public virtual int DisplayHeight
        {
            get { return _displayHeight; }
            set { _displayHeight = value; }
        }

        public virtual DisplayOrientation DisplayOrientation
        {
            get { return _displayOrientation; }
            set { _displayOrientation = value; }
        }
         
        public virtual GestureType EnabledGestures
        {
            get { return _enabledGestures; }
            set { _enabledGestures = value; }
        }

        public virtual bool IsGestureAvailable
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual TouchPanelCapabilities GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public virtual TouchCollection GetState()
        {
            throw new NotImplementedException();
        }

        public virtual GestureSample ReadGesture()
        {
            throw new NotImplementedException();
        }

        public virtual void AddEvent(int id, TouchLocationState state, Vector2 position)
        {
            throw new NotImplementedException();
        }

        public virtual void ReleaseAllTouches()
        {
            throw new NotImplementedException();
        }

    }
}