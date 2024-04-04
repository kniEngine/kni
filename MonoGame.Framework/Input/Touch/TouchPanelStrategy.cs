// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public abstract partial class TouchPanelStrategy
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
                return this.LegacyIsGestureAvailable;
            }
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

        public abstract void AddEvent(int id, TouchLocationState state, Vector2 position);

        /// <summary>
        /// This will invalidate the touch panel state.
        /// </summary>
        /// <remarks>
        /// Called from orientation change on mobiles, window clientBounds changes, minimize, etc
        /// </remarks>
        public virtual void InvalidateTouches()
        {
            // store enabled gesture types
            GestureType enabledGestures = this.EnabledGestures;

            try
            {
                // Invalidate Gestures
                // this should remove all pending gestures
                // we don't want the released touches to triger any gestures (tap, hold, etc ...)
                this.EnabledGestures = GestureType.None;

                if (_touchIdsMap.Count > 0)
                {
                    // local copy of touchStates
                    int nativeTouchIdsCount = _touchIdsMap.Count;
                    int[] nativeTouchIds = new int[nativeTouchIdsCount];
                    _touchIdsMap.Keys.CopyTo(nativeTouchIds, 0);

                    for (int i = 0; i < nativeTouchIdsCount; i++)
                    {
                        // submit a fake Released event for each touch Id
                        AddEvent(nativeTouchIds[i], TouchLocationState.Released, Vector2.Zero);
                    }
                }
            }
            finally
            {
                // restore enabled gesture types
                this.EnabledGestures = GestureType.None;
            }
        }

    }
}