// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.Graphics.Display;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Framework
{
    partial class MetroGameWindow : GameWindow
    {
        private DisplayOrientation _supportedOrientations;
        private DisplayOrientation _orientation;
        private CoreWindow _coreWindow;
        private Rectangle _viewBounds;
#if !WP81
        private ApplicationViewState _currentViewState;
#endif

        private object _eventLocker = new object();
        
        private InputEvents _inputEvents;
        private bool _isSizeChanged = false;
        private Rectangle _newViewBounds;
        private bool _isOrientationChanged = false;
        private DisplayOrientation _newOrientation;
        private bool _isFocusChanged = false;
        private CoreWindowActivationState _newActivationState;


        #region Internal Properties

        internal CoreWindow CoreWindow { get { return _coreWindow; } }

        internal Game Game { get; set; }

        internal bool IsExiting { get; set; }

        #endregion

        #region Public Properties

        public override IntPtr Handle { get { return Marshal.GetIUnknownForObject(_coreWindow); } }

        public override string ScreenDeviceName { get { return String.Empty; } } // window.Title

        public override Rectangle ClientBounds { get { return _viewBounds; } }

        public override bool AllowUserResizing
        {
            get { return false; }
            set 
            {
                // You cannot resize a Metro window!
            }
        }

        public override DisplayOrientation CurrentOrientation
        {
            get { return _orientation; }
        }

        private MetroGamePlatform Platform { get { return Game.Instance.Platform as MetroGamePlatform; } }

        protected internal override void SetSupportedOrientations(DisplayOrientation orientations)
        {
            // We don't want to trigger orientation changes 
            // when no preference is being changed.
            if (_supportedOrientations == orientations)
                return;
            
            _supportedOrientations = orientations;
            
            DisplayOrientations supported;
            if (orientations == DisplayOrientation.Default)
            {
                // Make the decision based on the preferred backbuffer dimensions.
                var manager = Game.graphicsDeviceManager;
                if (manager.PreferredBackBufferWidth > manager.PreferredBackBufferHeight)
                    supported = FromOrientation(DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
                else
                    supported = FromOrientation(DisplayOrientation.Portrait | DisplayOrientation.PortraitDown);
            }
            else
                supported = FromOrientation(orientations);

            DisplayProperties.AutoRotationPreferences = supported;
        }

        #endregion

        static public MetroGameWindow Instance { get; private set; }

        static MetroGameWindow()
        {
            Instance = new MetroGameWindow();
        }

        public void Initialize(CoreWindow coreWindow, UIElement inputElement, TouchQueue touchQueue)
        {
            _coreWindow = coreWindow;
            _inputEvents = new InputEvents(_coreWindow, inputElement, touchQueue);

            _orientation = ToOrientation(DisplayProperties.CurrentOrientation);
            DisplayProperties.OrientationChanged += DisplayProperties_OrientationChanged;

            _coreWindow.SizeChanged += Window_SizeChanged;
            _coreWindow.Closed += Window_Closed;

            _coreWindow.Activated += Window_FocusChanged;
            _coreWindow.VisibilityChanged += _coreWindow_VisibilityChanged;
#if !WP81
            _currentViewState = ApplicationView.Value;
#endif
            var bounds = _coreWindow.Bounds;
            SetViewBounds(bounds.Width, bounds.Height);

            SetCursor(false);
        }

        void _coreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            Platform.IsVisible = args.Visible;
        }

        private void Window_FocusChanged(CoreWindow sender, WindowActivatedEventArgs args)
        {
            lock (_eventLocker)
            {
                _isFocusChanged = true;
                _newActivationState = args.WindowActivationState;
            }
        }

        private void UpdateFocus()
        {
            lock (_eventLocker)
            {
                _isFocusChanged = false;

                if (_newActivationState == CoreWindowActivationState.Deactivated)
                    Platform.IsActive = false;
                else
                    Platform.IsActive = true;
            }
        }

        private void Window_Closed(CoreWindow sender, CoreWindowEventArgs args)
        {
            Game.SuppressDraw();
            Game.Platform.Exit();
        }

        private void SetViewBounds(double width, double height)
        {
            var dpi = DisplayProperties.LogicalDpi;
            var pixelWidth = (int)Math.Round(width * dpi / 96.0);
            var pixelHeight = (int)Math.Round(height * dpi / 96.0);

            _viewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);
        }

        private void Window_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            lock (_eventLocker)
            {
                _isSizeChanged = true;
                var dpi = DisplayProperties.LogicalDpi;
                var pixelWidth = (int)Math.Round(args.Size.Width * dpi / 96.0);
                var pixelHeight = (int)Math.Round(args.Size.Height * dpi / 96.0);
                _newViewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);
            }
        }

        private void UpdateSize()
        {
            lock (_eventLocker)
            {
                _isSizeChanged = false;

                var manager = Game.graphicsDeviceManager;
                
                // Set the new client bounds.
                _viewBounds = _newViewBounds;

                // Set the default new back buffer size and viewport, but this
                // can be overloaded by the two events below.
            
                
                manager.PreferredBackBufferWidth = _viewBounds.Width;
                manager.PreferredBackBufferHeight = _viewBounds.Height;
                if(manager.GraphicsDevice!=null)
                    manager.GraphicsDevice.Viewport = new Viewport(0, 0, _viewBounds.Width, _viewBounds.Height);

                // If we have a valid client bounds then 
                // update the graphics device.
                if (_viewBounds.Width > 0 && _viewBounds.Height > 0)
                    manager.ApplyChanges();

                // Set the new view state which will trigger the 
                // Game.ApplicationViewChanged event and signal
                // the client size changed event.
#if !WP81
                Platform.ViewState = ApplicationView.Value;
#endif
                OnClientSizeChanged();
            }
        }

        private static DisplayOrientation ToOrientation(DisplayOrientations orientations)
        {
            var result = DisplayOrientation.Default;
            if ((orientations & DisplayOrientations.Landscape) != 0)
                result |= DisplayOrientation.LandscapeLeft;
            if ((orientations & DisplayOrientations.LandscapeFlipped) != 0)
                result |= DisplayOrientation.LandscapeRight;
            if ((orientations & DisplayOrientations.Portrait) != 0)
                result |= DisplayOrientation.Portrait;
            if ((orientations & DisplayOrientations.PortraitFlipped) != 0)
                result |= DisplayOrientation.PortraitDown;

            return result;
        }

        private static DisplayOrientations FromOrientation(DisplayOrientation orientation)
        {
            var result = DisplayOrientations.None;
            if ((orientation & DisplayOrientation.LandscapeLeft) != 0)
                result |= DisplayOrientations.Landscape;
            if ((orientation & DisplayOrientation.LandscapeRight) != 0)
                result |= DisplayOrientations.LandscapeFlipped;
            if ((orientation & DisplayOrientation.Portrait) != 0)
                result |= DisplayOrientations.Portrait;
            if ((orientation & DisplayOrientation.PortraitDown) != 0)
                result |= DisplayOrientations.PortraitFlipped;

            return result;
        }

        private void DisplayProperties_OrientationChanged(object sender)
        {
            lock(_eventLocker)
            {
                _isOrientationChanged = true;
                _newOrientation = ToOrientation(DisplayProperties.CurrentOrientation);
            }
        }

        private void UpdateOrientation()
        {
            lock (_eventLocker)
            {
                _isOrientationChanged = false;

                // Set the new orientation.
                _orientation = _newOrientation;

                // Call the user callback.
                OnOrientationChanged();

                // If we have a valid client bounds then update the graphics device.
                if (_viewBounds.Width > 0 && _viewBounds.Height > 0)
                    Game.graphicsDeviceManager.ApplyChanges();
            }
        }

        protected override void SetTitle(string title)
        {
            // NOTE: There is no concept of a window 
            // title in a Metro application.
        }

        internal void SetCursor(bool visible)
        {
            if ( _coreWindow == null )
                return;

            var asyncResult = _coreWindow.Dispatcher.RunIdleAsync( (e) =>
            {            
                if (visible)
                    _coreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 0);
                else
                    _coreWindow.PointerCursor = null;
            });
        }

        internal void RunLoop()
        {
            SetCursor(Game.IsMouseVisible);
            _coreWindow.Activate();

            while (true)
            {
                // Process events incoming to the window.
                _coreWindow.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessAllIfPresent);

                Tick();

                if (IsExiting)
                    break;
            }
        }

        void ProcessWindowEvents()
        {
            // Update input
            _inputEvents.UpdateState();

            // Update size
            if (_isSizeChanged)
                UpdateSize();

            // Update orientation
            if (_isOrientationChanged)
                UpdateOrientation();

            // Update focus
            if (_isFocusChanged)
                UpdateFocus();

        }

        internal void Tick()
        {
            // Update state based on window events.
            ProcessWindowEvents();

            // Update and render the game.
            if (Game != null)
                Game.Tick();
        }

        #region Public Methods

        public void Dispose()
        {
            //window.Dispose();
        }

        public override void BeginScreenDeviceChange(bool willBeFullScreen)
        {
        }

        public override void EndScreenDeviceChange(string screenDeviceName, int clientWidth, int clientHeight)
        {

        }

        #endregion
    }
#if !WP81
    [CLSCompliant(false)]
    public class ViewStateChangedEventArgs : EventArgs
    {
        public readonly ApplicationViewState ViewState;

        public ViewStateChangedEventArgs(ApplicationViewState newViewstate)
        {
            ViewState = newViewstate;
        }
    }
#endif
}

