// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform;

namespace Microsoft.Xna.Framework
{
    partial class UAPGameWindow : GameWindow
    {
        private static Dictionary<IntPtr, UAPGameWindow> _instances = new Dictionary<IntPtr, UAPGameWindow>();

        internal static UAPGameWindow FromHandle(IntPtr handle)
        {
            return _instances[handle];
        }

        private DisplayOrientation _supportedOrientations;
        private DisplayOrientation _orientation;
        private CoreWindow _coreWindow;
        private DisplayInformation _dinfo;
        private ApplicationView _appView;
        private Rectangle _viewBounds;

        private InputEvents _inputEvents;
        private bool _backPressed = false;

        #region Internal Properties
        
        internal CoreWindow CoreWindow { get { return _coreWindow; } }

        internal Game Game { get; set; }

        public ApplicationView AppView { get { return _appView; } }

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

        private GameStrategy GameStrategy { get { return ConcreteGame.ConcreteGameInstance; } }

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
                var gdm = Game.Strategy.GraphicsDeviceManager;
                if (gdm.PreferredBackBufferWidth > gdm.PreferredBackBufferHeight)
                    supported = FromOrientation(DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
                else
                    supported = FromOrientation(DisplayOrientation.Portrait | DisplayOrientation.PortraitDown);
            }
            else
                supported = FromOrientation(orientations);

            DisplayInformation.AutoRotationPreferences = supported;
        }

        #endregion

        static public UAPGameWindow Instance { get; private set; }

        static UAPGameWindow()
        {
            Instance = new UAPGameWindow();
        }

        internal UAPGameWindow()
        {

        }

        internal SwapChainPanel SwapChainPanel { get; private set; }

        public void Initialize(CoreWindow coreWindow, SwapChainPanel swapChainPanel = null)
        {
            _coreWindow = coreWindow;
            _instances.Add(this.Handle, this);

            this.SwapChainPanel = swapChainPanel;
            _inputEvents = new InputEvents(_coreWindow, this.SwapChainPanel);

            _dinfo = DisplayInformation.GetForCurrentView();
            _appView = ApplicationView.GetForCurrentView();

            _orientation = ToOrientation(_dinfo.CurrentOrientation);
            _dinfo.OrientationChanged += DisplayProperties_OrientationChanged;

            _coreWindow.SizeChanged += Window_SizeChanged;

            _coreWindow.Closed += Window_Closed;
            _coreWindow.Activated += Window_FocusChanged;
            _coreWindow.VisibilityChanged += Window_VisibilityChanged;
            _coreWindow.Dispatcher.AcceleratorKeyActivated += Dispatcher_AcceleratorKeyActivated;

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += this.HardwareButtons_BackPressed;
            else
                SystemNavigationManager.GetForCurrentView().BackRequested += this.BackRequested;

            SetViewBounds(_appView.VisibleBounds.Width, _appView.VisibleBounds.Height);

            SetCursor(false);
        }

        void Window_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            GameStrategy.IsVisible = args.Visible;
        }

        private void Window_FocusChanged(CoreWindow sender, WindowActivatedEventArgs args)
        {
            switch (args.WindowActivationState)
            {
                case CoreWindowActivationState.Deactivated:
                    GameStrategy.IsActive = false;
                    break;

                case CoreWindowActivationState.PointerActivated:
                case CoreWindowActivationState.CodeActivated:
                    GameStrategy.IsActive = true;
                    break;

                default:
#if DEBUG
                    throw new InvalidOperationException();
#endif
                    break;
            }
        }

        private void Window_Closed(CoreWindow sender, CoreWindowEventArgs args)
        {
            Game.SuppressDraw();
            Game.Strategy.TickExiting();
        }

        private void SetViewBounds(double width, double height)
        {
            int pixelWidth = Math.Max(1, (int)Math.Round(width * _dinfo.RawPixelsPerViewPixel));
            int pixelHeight = Math.Max(1, (int)Math.Round(height * _dinfo.RawPixelsPerViewPixel));
            _viewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);
        }

        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            int pixelWidth  = Math.Max(1, (int)Math.Round(args.Size.Width * _dinfo.RawPixelsPerViewPixel));
            int pixelHeight = Math.Max(1, (int)Math.Round(args.Size.Height * _dinfo.RawPixelsPerViewPixel));

            // Set the new client bounds.
            _viewBounds = new Rectangle(0, 0, pixelWidth, pixelHeight);

            // Set the default new back buffer size and viewport, but this
            // can be overloaded by the two events below.

            var gdm = Game.Strategy.GraphicsDeviceManager;
            gdm.IsFullScreen = _appView.IsFullScreenMode;
            gdm.PreferredBackBufferWidth = _viewBounds.Width;
            gdm.PreferredBackBufferHeight = _viewBounds.Height;
            gdm.ApplyChanges();

            // Set the new view state which will trigger the 
            // Game.ApplicationViewChanged event and signal
            // the client size changed event.
            OnClientSizeChanged();
        }

        private void Dispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs args)
        {
            // NOTE: Dispatcher event is used because KeyDown event doesn't handle Alt key
            var key = InputEvents.KeyTranslate(args.VirtualKey, args.KeyStatus);
            switch (args.EventType)
            {
                case CoreAcceleratorKeyEventType.KeyDown:
                case CoreAcceleratorKeyEventType.SystemKeyDown:
                    Platform_OnKeyDown(key);
                    break;
                case CoreAcceleratorKeyEventType.KeyUp:
                case CoreAcceleratorKeyEventType.SystemKeyUp:
                    Platform_OnKeyUp(key);
                    break;
                default:
                    break;
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

        internal void SetClientSize(int width, int height)
        {
            if (_appView.IsFullScreenMode)
                return;

            if (_viewBounds.Width == width &&
                _viewBounds.Height == height)
                return;

            double rawPixelsPerViewPixel = 1.0d;
            if (CoreWindow.GetForCurrentThread() != null)
                rawPixelsPerViewPixel = _dinfo.RawPixelsPerViewPixel;
            else
                Task.Run(async () =>
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                        CoreDispatcherPriority.Normal, () => { rawPixelsPerViewPixel = _dinfo.RawPixelsPerViewPixel; });
                }).Wait();
            var viewSize = new Windows.Foundation.Size(width / rawPixelsPerViewPixel, height / rawPixelsPerViewPixel);

            //_appView.SetPreferredMinSize(viewSize);
            if (!_appView.TryResizeView(viewSize))
            {
                // TODO: What now?
            }
        }

        private void DisplayProperties_OrientationChanged(DisplayInformation dinfo, object sender)
        {
            // Set the new orientation.
            _orientation = ToOrientation(dinfo.CurrentOrientation);

            // Call the user callback.
            OnOrientationChanged();

            // If we have a valid client bounds then update the graphics device.
            if (_viewBounds.Width > 0 && _viewBounds.Height > 0)
            {
                var gdm = Game.Strategy.GraphicsDeviceManager;
                gdm.ApplyChanges();
            }
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            // We need to manually hide the keyboard input UI when the back button is pressed
            if (KeyboardInput.IsVisible)
                KeyboardInput.Cancel(null);
            else
                _backPressed = true;

            e.Handled = true;
        }

        private void BackRequested(object sender, BackRequestedEventArgs e)
        {
            // Prevent Xbox from suspending the app when the user press 'B' button.
            e.Handled = true;
        }

        private void UpdateBackButton()
        {
            GamePad.Back = _backPressed;
            _backPressed = false;
        }

        protected override void SetTitle(string title)
        {
            Debug.WriteLine("WARNING: GameWindow.Title has no effect under UWP.");
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

        internal void Tick()
        {
            // Update input
            _inputEvents.UpdateState();

            // Update TextInput
            if (!_inputEvents.TextQueue.IsEmpty)
            {
                InputEvents.KeyChar ch;
                while (_inputEvents.TextQueue.TryDequeue(out ch))
                {
                    Platform_OnTextInput(ch.Character, ch.Key);
                }
            }

            // Update back button
            UpdateBackButton();

            // Update and render the game.
            if (Game != null)
                Game.Tick();
        }

        #region Public Methods

        public void Dispose()
        {
            //window.Dispose();

            _instances.Remove(this.Handle);
        }

        #endregion
    }
}

