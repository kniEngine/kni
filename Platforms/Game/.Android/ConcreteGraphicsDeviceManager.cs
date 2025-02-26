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

            this.Game.Window.OrientationChanged += GameWindow_OrientationChanged;

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
                {
                    // TODO: Set fullscreen in ApplyChanges()
                    IntPtr windowHandle = base.GraphicsDevice.PresentationParameters.DeviceWindowHandle;
                    AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);
                    gameWindow.ForceSetFullScreen(base.IsFullScreen);
                }
            }
        }

        public override DisplayOrientation SupportedOrientations
        {
            get { return base.SupportedOrientations; }
            set { base.SupportedOrientations = value; }
        }

        public override void ToggleFullScreen()
        {
            this.IsFullScreen = !this.IsFullScreen;
            //ApplyChanges();
        }

        public override void CreateDevice()
        {
            PresentationParameters pp = new PresentationParameters();
            pp.MultiSampleCount = (this.PreferMultiSampling) ? 4 : 1;
            pp.BackBufferFormat = this.PreferredBackBufferFormat;
            pp.DepthStencilFormat = this.PreferredDepthStencilFormat;
            pp.BackBufferWidth = 800;
            pp.BackBufferHeight = 480;

            // force "full screen" as default on Android
            pp.IsFullScreen = true;
            
            pp.DeviceWindowHandle = this.Game.Window.Handle;

            GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
            gdi.GraphicsProfile = this.GraphicsProfile;
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.PresentationParameters = pp;
            var pe = new PreparingDeviceSettingsEventArgs(gdi);

            this.OnPreparingDeviceSettings(pe);

            pp = gdi.PresentationParameters;
            this.GraphicsProfile = gdi.GraphicsProfile;

            AndroidGameWindow androidGameWindow = (AndroidGameWindow)base.Game.Window;
            View surfaceView = androidGameWindow.GameView;

            pp.DisplayOrientation = base.Game.Window.CurrentOrientation;

            pp.BackBufferWidth = surfaceView.Width;
            pp.BackBufferHeight = surfaceView.Height;

            this.GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, this.PreferHalfPixelOffset, pp);

            // ApplyChanges
            {
                TouchPanel.DisplayWidth = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
                TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
            }

            // TODO: In XNA this seems to be done as part of the GraphicsDevice.DeviceReset event...
            //       we need to get those working.
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;

            this.OnDeviceCreated(EventArgs.Empty);


            Android.App.Activity activity = AndroidGameWindow.Activity;
            DisplayOrientation currentOrientation = AndroidCompatibility.Current.GetAbsoluteOrientation(activity);
            DisplayOrientation supported2 = this.SupportedOrientations;
            if (supported2 == DisplayOrientation.Default)
            {
                if (this.PreferredBackBufferWidth <= this.PreferredBackBufferHeight)
                    supported2 = (DisplayOrientation.Portrait | DisplayOrientation.PortraitDown);
                else
                    supported2 = (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
            }
            androidGameWindow.SetOrientation(currentOrientation, supported2);

        }

        private void GameWindow_OrientationChanged(object sender, EventArgs e)
        {
            if (GraphicsDevice != null)
            {
                AndroidGameWindow androidGameWindow = (AndroidGameWindow)base.Game.Window;
                View surfaceView = androidGameWindow.GameView;

                base.GraphicsDevice.PresentationParameters.DisplayOrientation = base.Game.Window.CurrentOrientation;

                base.GraphicsDevice.PresentationParameters.BackBufferWidth = surfaceView.Width;
                base.GraphicsDevice.PresentationParameters.BackBufferHeight = surfaceView.Height;

                if (!((IPlatformGraphicsContext)((IPlatformGraphicsDevice)base.GraphicsDevice).Strategy.MainContext).Strategy.IsRenderTargetBound)
                {
                    PresentationParameters pp = this.GraphicsDevice.PresentationParameters;
                    base.GraphicsDevice.Viewport = new Viewport(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
                    base.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, pp.BackBufferWidth, pp.BackBufferHeight);
                }

                TouchPanel.DisplayWidth  = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
                TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
                TouchPanel.DisplayOrientation = base.GraphicsDevice.PresentationParameters.DisplayOrientation;
            }

        }

        public override void ApplyChanges()
        {
            if (base.GraphicsDevice == null)
            {
                // TODO: Calling ApplyChanges() before Game.Initialize() should create the device.
                System.Diagnostics.Debug.Assert(false);
                //this.CreateDevice();
                return;
            }

            AndroidGameWindow androidGameWindow = (AndroidGameWindow)base.Game.Window;
            View surfaceView = androidGameWindow.GameView;

            // Trigger a change in orientation in case the supported orientations have changed
            DisplayOrientation supported = this.SupportedOrientations;
            if (supported == DisplayOrientation.Default)
            {
                if (this.PreferredBackBufferWidth <= this.PreferredBackBufferHeight)
                    supported = (DisplayOrientation.Portrait | DisplayOrientation.PortraitDown);
                else
                    supported = (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight);
            }
            androidGameWindow.SetOrientation(base.Game.Window.CurrentOrientation, supported);

        }


        #region IGraphicsDeviceManager strategy

        public override bool BeginDraw()
        {
            //return base.BeginDraw();

            return true;
        }

        public override void EndDraw()
        {
#if OCULUS
            return; // On Oculus, do not Present() the backbuffer.
#endif

            //base.EndDraw();

            GraphicsDevice device = this.GraphicsDevice;
            if (device != null)
            {
                device.Present();
            }
        }

        #endregion IGraphicsDeviceManager strategy


        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (base.Game != null && base.Game.Window != null)
                {
                    base.Game.Window.OrientationChanged -= GameWindow_OrientationChanged;
                }

            }

        }

    }
}
