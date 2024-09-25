using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Graphics;
using Android.Views;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {
        public ConcreteGraphicsDeviceManager(Game game) : base(game)
        {
            var clientBounds = base.Game.Window.ClientBounds;
            // Preferred buffer width/height is used to determine default supported orientations,
            // so set the default values to match Xna behaviour of landscape only by default.
            // Note also that it's using the device window dimensions.
            base.PreferredBackBufferWidth = Math.Max(clientBounds.Width, clientBounds.Height);
            base.PreferredBackBufferHeight = Math.Min(clientBounds.Height, clientBounds.Width);

        }

        public override bool PreferHalfPixelOffset
        {
            get { return base.PreferHalfPixelOffset; }
            set
            {
                //TODO: move the check in ApplyChanges
                if (base.GraphicsDevice != null)
                    throw new InvalidOperationException("Setting PreferHalfPixelOffset is not allowed after the creation of GraphicsDevice.");

                base.PreferHalfPixelOffset = value;
            }
        }

        public override bool IsFullScreen
        {
            get
            {
                if (base.GraphicsDevice != null)
                    return base.GraphicsDevice.PresentationParameters.IsFullScreen;

                return base.IsFullScreen;
            }
            set
            {
                base.IsFullScreen = value;

                if (base.GraphicsDevice != null)
                    base.GraphicsDevice.PresentationParameters.IsFullScreen = value;

                if (base.GraphicsDevice != null)
                    this.ForceSetFullScreen(base.IsFullScreen);
            }
        }

        public override DisplayOrientation SupportedOrientations
        {
            get { return base.SupportedOrientations; }
            set
            {
                base.SupportedOrientations = value;

                if (base.Game.Window != null)
                    ((AndroidGameWindow)base.Game.Window)._supportedOrientations = base.SupportedOrientations;
            }
        }

        public override void ToggleFullScreen()
        {
            this.IsFullScreen = !this.IsFullScreen;
            //ApplyChanges();
        }

        public override void CreateDevice()
        {
            PresentationParameters pp = new PresentationParameters();
            pp.DepthStencilFormat = DepthFormat.Depth24;
            pp.BackBufferWidth = 800;
            pp.BackBufferHeight = 480;

            // Set "full screen"  as default
            pp.IsFullScreen = true;
            
            pp.DeviceWindowHandle = this.Game.Window.Handle;

            GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
            gdi.GraphicsProfile = this.GraphicsProfile; // Microsoft defaults this to Reach.
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.PresentationParameters = pp;
            var pe = new PreparingDeviceSettingsEventArgs(gdi);

            this.OnPreparingDeviceSettings(pe);

            pp = gdi.PresentationParameters;
            this.GraphicsProfile = gdi.GraphicsProfile;

            // Needs to be before ApplyChanges()
            this.GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, this.PreferHalfPixelOffset, pp);

            this.ApplyChanges();

            // Set the new display size on the touch panel.
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;

            this.OnDeviceCreated(EventArgs.Empty);

            AndroidGameWindow gameWindow = (AndroidGameWindow)Game.Window;

            Android.App.Activity activity = AndroidGameWindow.Activity;
            DisplayOrientation currentOrientation = AndroidCompatibility.Current.GetAbsoluteOrientation(activity);
            switch (activity.Resources.Configuration.Orientation)
            {
                case Android.Content.Res.Orientation.Portrait:
                    gameWindow.SetOrientation((currentOrientation == DisplayOrientation.PortraitDown)
                                                    ? DisplayOrientation.PortraitDown
                                                    : DisplayOrientation.Portrait,
                                                    false);
                    break;
                default:
                    gameWindow.SetOrientation((currentOrientation == DisplayOrientation.LandscapeRight)
                                                    ? DisplayOrientation.LandscapeRight
                                                    : DisplayOrientation.LandscapeLeft,
                                                    false);
                    break;
            }

            // ResetClientBounds
            {
                // TODO: check if the PreferredBackBufferWidth/Hight is supported and throw an error similar to fullscreen Windows Desktop.
                View view = ((AndroidGameWindow)base.Game.Window).GameView;
                int viewWidth = view.Width;
                int viewHeight = view.Height;

                base.GraphicsDevice.PresentationParameters.BackBufferWidth = viewWidth;
                base.GraphicsDevice.PresentationParameters.BackBufferHeight = viewHeight;

                // Set the viewport from PresentationParameters
                if (!((IPlatformGraphicsContext)((IPlatformGraphicsDevice)base.GraphicsDevice).Strategy.MainContext).Strategy.IsRenderTargetBound)
                {
                    PresentationParameters pp2 = this.GraphicsDevice.PresentationParameters;
                    base.GraphicsDevice.Viewport = new Viewport(0, 0, pp2.BackBufferWidth, pp2.BackBufferHeight);
                    base.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, pp2.BackBufferWidth, pp2.BackBufferHeight);
                }

                ((AndroidGameWindow)base.Game.Window).ChangeClientBounds(new Rectangle(0, 0, viewWidth, viewHeight));
                // Set the new display size on the touch panel.
                TouchPanel.DisplayWidth  = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
                TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
            }
        }

        public override void ApplyChanges()
        {
            if (base.GraphicsDevice == null)
            {
                // TODO: Calling ApplyChanges() before Game Initialize should create the device.
                //this.CreateDevice();
                return;
            }

            // Trigger a change in orientation in case the supported orientations have changed
            ((AndroidGameWindow)base.Game.Window).SetOrientation(base.Game.Window.CurrentOrientation, false);

            // Ensure the presentation parameter orientation and buffer size matches the window
            base.GraphicsDevice.PresentationParameters.DisplayOrientation = base.Game.Window.CurrentOrientation;

            // Set the presentation parameters' actual buffer size to match the orientation
            bool isLandscape = (0 != (base.Game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)));
            int w = PreferredBackBufferWidth;
            int h = PreferredBackBufferHeight;

            base.GraphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
            base.GraphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);

            // ResetClientBounds
            {
                // TODO: check if the PreferredBackBufferWidth/Hight is supported and throw an error similar to fullscreen Windows Desktop.
                View view = ((AndroidGameWindow)base.Game.Window).GameView;
                int viewWidth = view.Width;
                int viewHeight = view.Height;

                base.GraphicsDevice.PresentationParameters.BackBufferWidth = viewWidth;
                base.GraphicsDevice.PresentationParameters.BackBufferHeight = viewHeight;

                // Set the viewport from PresentationParameters
                if (!((IPlatformGraphicsContext)((IPlatformGraphicsDevice)base.GraphicsDevice).Strategy.MainContext).Strategy.IsRenderTargetBound)
                {
                    PresentationParameters pp2 = this.GraphicsDevice.PresentationParameters;
                    base.GraphicsDevice.Viewport = new Viewport(0, 0, pp2.BackBufferWidth, pp2.BackBufferHeight);
                    base.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, pp2.BackBufferWidth, pp2.BackBufferHeight);
                }

                ((AndroidGameWindow)base.Game.Window).ChangeClientBounds(new Rectangle(0, 0, viewWidth, viewHeight));
                // Set the new display size on the touch panel.
                TouchPanel.DisplayWidth  = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
                TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
            }

        }

        private void ForceSetFullScreen(bool _isFullScreen)
        {
            if (_isFullScreen)
            {
                AndroidGameWindow.Activity.Window.ClearFlags(Android.Views.WindowManagerFlags.ForceNotFullscreen);
                AndroidGameWindow.Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            }
            else
            {
                AndroidGameWindow.Activity.Window.SetFlags(WindowManagerFlags.ForceNotFullscreen, WindowManagerFlags.ForceNotFullscreen);
            }
        }

        internal void InternalForceSetFullScreen()
        {
            this.ForceSetFullScreen(IsFullScreen);
        }


        #region IGraphicsDeviceManager strategy

        public override bool BeginDraw()
        {
            //return base.BeginDraw();

            return true;
        }

        public override void EndDraw()
        {
            //base.EndDraw();

            GraphicsDevice device = this.GraphicsDevice;
            if (device != null)
            {
                // // Surface is presented by OnFinishFrame.
                //device.Present();
            }
        }

        #endregion IGraphicsDeviceManager strategy

    }
}
