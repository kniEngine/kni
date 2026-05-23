// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public sealed class ConcreteTouchPanel : TouchPanelStrategy
    {
        private readonly HashSet<int> _canceledTouchIds = new HashSet<int>();
        private readonly HashSet<int> _releasedTouchIds = new HashSet<int>();

        public override IntPtr WindowHandle
        {
            get { return base.WindowHandle; }
            set { base.WindowHandle = value; }
        }

        public override int DisplayWidth
        {
            get { return base.DisplayWidth; }
            set { base.DisplayWidth = value; }
        }

        public override int DisplayHeight
        {
            get { return base.DisplayHeight; }
            set { base.DisplayHeight = value; }
        }

        public override DisplayOrientation DisplayOrientation
        {
            get { return base.DisplayOrientation; }
            set { base.DisplayOrientation = value; }
        }

        public override GestureType EnabledGestures
        {
            get { return base.EnabledGestures; }
            set { base.EnabledGestures = value; }
        }


        public override bool IsGestureAvailable
        {
            get { return base.IsGestureAvailable; }
        }

        public ConcreteTouchPanel()
            : base()
        {
            // Initialize Capabilities
            int maximumTouchCount = 0;
            bool isConnected = false;
            bool hasPressure = false;

            int maximumTouches = GetSystemMetrics(SystemMetrics.SM_MAXIMUMTOUCHES);
            InputDigitizer digitizer = (InputDigitizer)GetSystemMetrics(SystemMetrics.SM_DIGITIZER);

            if ((digitizer & InputDigitizer.NID_READY) != 0
            &&  (digitizer & InputDigitizer.NID_INTEGRATED_TOUCH) != 0
            &&  maximumTouches > 0)
            {
                maximumTouchCount = maximumTouches;
                isConnected = true;
            }

            _capabilities = base.CreateTouchPanelCapabilities(maximumTouchCount, isConnected, hasPressure);
        }

        public override TouchPanelCapabilities GetCapabilities()
        {
            return _capabilities;
        }

        public override TouchCollection GetState()
        {
            return base.GetState();
        }

        public override GestureSample ReadGesture()
        {
            return base.ReadGesture();
        }

        public override void AddPressedEvent(int nativeTouchId, Vector2 position)
        {
            System.Diagnostics.Debug.Assert(!_canceledTouchIds.Contains(nativeTouchId), "nativeTouchId already registered");

            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = WinFormsGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                base.AddPressedEvent(nativeTouchId, position, winSize);
            }
        }

        public override void AddMovedEvent(int nativeTouchId, Vector2 position)
        {
            if (_canceledTouchIds.Contains(nativeTouchId))
                return;

            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = WinFormsGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                Vector2 clampedPosition = new Vector2(
                        Math.Max(0, Math.Min(position.X, winSize.X - 1)),
                        Math.Max(0, Math.Min(position.Y, winSize.Y - 1))
                );
                if (_releasedTouchIds.Contains(nativeTouchId))
                {
                    if (clampedPosition == position)
                    {
                        _releasedTouchIds.Remove(nativeTouchId);
                        base.AddPressedEvent(nativeTouchId, position, winSize);
                    }
                    return;
                }
                if (clampedPosition != position)
                {
                    if (_releasedTouchIds.Add(nativeTouchId))
                        base.AddReleasedEvent(nativeTouchId, clampedPosition, winSize);
                    return;
                }

                base.AddMovedEvent(nativeTouchId, position, winSize);
            }
        }
   
        public override void AddReleasedEvent(int nativeTouchId, Vector2 position)
        {
            if (_canceledTouchIds.Remove(nativeTouchId))
                return;
            if (_releasedTouchIds.Remove(nativeTouchId))
                return;

            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = WinFormsGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                Vector2 clampedPosition = new Vector2(
                        Math.Max(0, Math.Min(position.X, winSize.X - 1)),
                        Math.Max(0, Math.Min(position.Y, winSize.Y - 1))
                );
                base.AddReleasedEvent(nativeTouchId, clampedPosition, winSize);
            }
        }

        public override void AddCanceledEvent(int nativeTouchId, Vector2 position)
        {
            IntPtr wndHandle = this.WindowHandle;
            if (wndHandle != IntPtr.Zero)
            {
                GameWindow gameWindow = WinFormsGameWindow.FromHandle(wndHandle);
                Rectangle windowsBounds = gameWindow.ClientBounds;
                Point winSize = new Point(windowsBounds.Width, windowsBounds.Height);

                base.AddReleasedEvent(nativeTouchId, position, winSize);
            }
        }

        /// <summary>
        /// This will invalidate the touch panel state.
        /// </summary>
        /// <remarks>
        /// Called from orientation change on window clientBounds changes, minimize, etc
        /// </remarks>
        internal void WinFormsCancelAllTouches()
        {
            // local copy of touchStates
            int[] nativeTouchIds = GetTouchIds();

            for (int i = 0; i < nativeTouchIds.Length; i++)
                _canceledTouchIds.Add(nativeTouchIds[i]);
            
            // submit a Canceled event for each touch Id
            for (int i = 0; i < nativeTouchIds.Length; i++)
                AddCanceledEvent(nativeTouchIds[i], Vector2.Zero);
        }

        private enum SystemMetrics
        {
            SM_DIGITIZER = 94,
            SM_MAXIMUMTOUCHES = 95,
        }

        [Flags]
        private enum InputDigitizer
        {
            NID_INTEGRATED_TOUCH = 0x01,
            NID_EXTERNAL_TOUCH   = 0x02,
            NID_INTEGRATED_PEN   = 0x04,
            NID_EXTERNAL_PEN     = 0x08,
            NID_MULTI_INPUT      = 0x40,
            NID_READY            = 0x80
        }

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, ExactSpelling = true)]
        static extern int GetSystemMetrics(SystemMetrics nIndex);

    }
}