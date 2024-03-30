// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    public interface ITouchPanel
    {
        IntPtr WindowHandle { get; set; }
        int DisplayWidth { get; set; }
        int DisplayHeight { get; set; }
        DisplayOrientation DisplayOrientation { get; set; }
        GestureType EnabledGestures { get; set; }
        bool IsGestureAvailable { get; }

        TouchPanelCapabilities GetCapabilities();
        TouchCollection GetState();
        GestureSample ReadGesture();
    }

    public interface IPlatformTouchPanel
    {
        T GetStrategy<T>() where T : TouchPanelStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Allows retrieval of information from Touch Panel device.
    /// </summary>
    public sealed class TouchPanel : ITouchPanel
        , IPlatformTouchPanel
    {
        private static TouchPanel _current;

        /// <summary>
        /// Returns the current Keyboard instance.
        /// </summary> 
        public static TouchPanel Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(Keyboard))
                {
                    if (_current == null)
                        _current = new TouchPanel();

                    return _current;
                }
            }
        }


        /// <summary>
        /// The window handle of the touch panel. Purely for Xna compatibility.
        /// </summary>
        public static IntPtr WindowHandle
        {
            get { return ((ITouchPanel)TouchPanel.Current).WindowHandle; }
            set { ((ITouchPanel)TouchPanel.Current).WindowHandle = value; }
        }

        /// <summary>
        /// Gets or sets the display width of the touch panel.
        /// </summary>
        public static int DisplayWidth
        {
            get { return ((ITouchPanel)TouchPanel.Current).DisplayWidth; }
            set { ((ITouchPanel)TouchPanel.Current).DisplayWidth = value; }
        }

        /// <summary>
        /// Gets or sets the display height of the touch panel.
        /// </summary>
        public static int DisplayHeight
        {
            get { return ((ITouchPanel)TouchPanel.Current).DisplayHeight; }
            set { ((ITouchPanel)TouchPanel.Current).DisplayHeight = value; }
        }

        /// <summary>
        /// Gets or sets the display orientation of the touch panel.
        /// </summary>
        public static DisplayOrientation DisplayOrientation
        {
            get { return ((ITouchPanel)TouchPanel.Current).DisplayOrientation; }
            set { ((ITouchPanel)TouchPanel.Current).DisplayOrientation = value; }
        }

        /// <summary>
        /// Gets or sets enabled gestures.
        /// </summary>
        public static GestureType EnabledGestures
        {
            get { return ((ITouchPanel)TouchPanel.Current).EnabledGestures; }
            set { ((ITouchPanel)TouchPanel.Current).EnabledGestures = value; }
        }

        /// <summary>
        /// Returns true if a touch gesture is available.
        /// </summary>
        public static bool IsGestureAvailable
        {
            get { return ((ITouchPanel)TouchPanel.Current).IsGestureAvailable; }
        }

        /// <summary>
        /// Gets the current state of the touch panel.
        /// </summary>
        /// <returns><see cref="TouchCollection"/></returns>
        public static TouchCollection GetState()
        {
            return ((ITouchPanel)TouchPanel.Current).GetState();
        }

        public static TouchPanelCapabilities GetCapabilities()
        {
            return ((ITouchPanel)TouchPanel.Current).GetCapabilities();
        }

        /// <summary>
        /// Returns the next available gesture on touch panel device.
        /// </summary>
        /// <returns><see cref="GestureSample"/></returns>
        public static GestureSample ReadGesture()
        {
            return ((ITouchPanel)TouchPanel.Current).ReadGesture();
        }

        private TouchPanelStrategy _strategy;

        T IPlatformTouchPanel.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private TouchPanel()
        {
            _strategy = new ConcreteTouchPanel();
        }

        #region ITouchPanel

        IntPtr ITouchPanel.WindowHandle
        {
            get { return _strategy.WindowHandle; }
            set { _strategy.WindowHandle = value; }
        }

        int ITouchPanel.DisplayWidth
        {
            get { return _strategy.DisplayWidth; }
            set { _strategy.DisplayWidth = value; }
        }

        int ITouchPanel.DisplayHeight
        {
            get { return _strategy.DisplayHeight; }
            set { _strategy.DisplayHeight = value; }
        }

        DisplayOrientation ITouchPanel.DisplayOrientation
        {
            get { return _strategy.DisplayOrientation; }
            set { _strategy.DisplayOrientation = value; }
        }

        GestureType ITouchPanel.EnabledGestures
        {
            get { return _strategy.EnabledGestures; }
            set { _strategy.EnabledGestures = value; }
        }

        bool ITouchPanel.IsGestureAvailable
        {
            get { return _strategy.IsGestureAvailable; }
        }

        TouchPanelCapabilities ITouchPanel.GetCapabilities()
        {
            return _strategy.GetCapabilities();
        }

        TouchCollection ITouchPanel.GetState()
        {
            return _strategy.GetState();
        }


        GestureSample ITouchPanel.ReadGesture()
        {
            return _strategy.ReadGesture();
        }

        #endregion ITouchPanel

    }
}
