// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public abstract partial class TouchPanelStrategy
    {
        private readonly Stopwatch _stopwatch;

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
                return this.LegacyIsGestureAvailable;
            }
        }

        protected TouchPanelStrategy()
        {
            _stopwatch = Stopwatch.StartNew();

            // the current implementation need to update on each frame
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).OnUpdate += this.UpdateCurrentTimestamp;
        }

        public virtual TouchPanelCapabilities GetCapabilities()
        {
            throw new NotImplementedException();
        }

        public virtual TouchCollection GetState()
        {
            return this.LegacyGetState();
        }

        public virtual GestureSample ReadGesture()
        {
            return this.LegacyReadGesture();
        }

        public abstract void AddPressedEvent(int nativeTouchId, Vector2 position);
        public abstract void AddMovedEvent(int nativeTouchId, Vector2 position);
        public abstract void AddReleasedEvent(int nativeTouchId, Vector2 position);
        public abstract void AddCanceledEvent(int nativeTouchId, Vector2 position);

        /// <summary>
        /// This will invalidate the touch panel state.
        /// </summary>
        /// <remarks>
        /// Called from orientation change on mobiles, window clientBounds changes, minimize, etc
        /// </remarks>
        public void BlazorCancelAllTouches()
        {
            // local copy of touchStates
            int nativeTouchIdsCount = _touchIdsMap.Count;
            int[] nativeTouchIds = new int[nativeTouchIdsCount];
            _touchIdsMap.Keys.CopyTo(nativeTouchIds, 0);

            // submit a fake Canceled event for each touch Id
            for (int i = 0; i < nativeTouchIdsCount; i++)
                AddCanceledEvent(nativeTouchIds[i], Vector2.Zero);
        }

        public void TestReleaseAllTouches()
        {
            // local copy of touchStates
            int nativeTouchIdsCount = _touchIdsMap.Count;
            int[] nativeTouchIds = new int[nativeTouchIdsCount];
            _touchIdsMap.Keys.CopyTo(nativeTouchIds, 0);

            for (int i = 0; i < nativeTouchIdsCount; i++)
                AddReleasedEvent(nativeTouchIds[i], Vector2.Zero);
        }

        protected TouchPanelCapabilities CreateTouchPanelCapabilities(int maximumTouchCount, bool isConnected, bool hasPressure)
        {
            TouchPanelCapabilities caps = new TouchPanelCapabilities();
            caps._maximumTouchCount = maximumTouchCount;
            caps._isConnected = isConnected;
            caps._hasPressure = hasPressure;

            return caps;
        }
    }
}