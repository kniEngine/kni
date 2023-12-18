using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Android.Views;

namespace Microsoft.Xna.Platform
{
    internal class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
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
                    base.Game.Window.SetSupportedOrientations(base.SupportedOrientations);
            }
        }

        public override void ToggleFullScreen()
        {
            //base.ApplyChanges();

            this.IsFullScreen = !this.IsFullScreen;
            //ApplyChanges();
        }

        public override void CreateDevice()
        {
            //base.CreateDevice();
            
            PresentationParameters pp = new PresentationParameters();
            pp.DepthStencilFormat = DepthFormat.Depth24;

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
            TouchPanel.DisplayWidth = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;

            this.OnDeviceCreated(EventArgs.Empty);
        }

        public override void ApplyChanges()
        {
            //base.ApplyChanges();

            if (base.GraphicsDevice == null)
            {
                // TODO: Calling ApplyChanges() before Game Initialize should create the device.
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

            ResetClientBounds();

            // Set the new display size on the touch panel.
            //
            // TODO: In XNA this seems to be done as part of the 
            // GraphicsDevice.DeviceReset event... we need to get 
            // those working.
            //
            TouchPanel.DisplayWidth = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;

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

        private void ResetClientBounds()
        {
            var newClientBounds = new Rectangle();
            // Set the ClientBounds to match the DisplayMode
            // TODO: check if the PreferredBackBufferWidth/Hight is supported and throw an error similar to fullscreen Windows Desktop.
            newClientBounds.X = 0;
            newClientBounds.Y = 0;
            newClientBounds.Width = base.GraphicsDevice.DisplayMode.Width;
            newClientBounds.Height = base.GraphicsDevice.DisplayMode.Height;

            // Ensure buffer size is reported correctly
            base.GraphicsDevice.PresentationParameters.BackBufferWidth = newClientBounds.Width;
            base.GraphicsDevice.PresentationParameters.BackBufferHeight = newClientBounds.Height;

            // Set the viewport from client bounds
            if (!base.GraphicsDevice.Strategy._mainContext.IsRenderTargetBound)
            {
                base.GraphicsDevice.Viewport = new Viewport(0, 0, newClientBounds.Width, newClientBounds.Height);
                base.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, newClientBounds.Width, newClientBounds.Height);
            }

            ((AndroidGameWindow)base.Game.Window).ChangeClientBounds(newClientBounds);
            Android.Util.Log.Debug("Kni", "GraphicsDeviceManager.ResetClientBounds: newClientBounds=" + newClientBounds.ToString());

            // Touch panel needs latest buffer size for scaling
            TouchPanel.DisplayWidth = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
        }

        internal void InternalForceSetFullScreen()
        {
            this.ForceSetFullScreen(IsFullScreen);
        }
        internal void InternalResetClientBounds()
        {
            this.ResetClientBounds();
        }


        #region IGraphicsDeviceManager strategy

        public override bool BeginDraw()
        {
            //return base.BeginDraw();

            PrimaryThreadLoader.DoLoads();

            return true;
        }

        public override void EndDraw()
        {
            //base.EndDraw();

            GraphicsDevice device = this.GraphicsDevice;
            if (device != null)
            {
                device.Present();
            }
        }

        #endregion IGraphicsDeviceManager strategy

    }
}
