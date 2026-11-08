using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {

        public ConcreteGraphicsDeviceManager(Game game) : base(game)
        {
            var clientBounds = base.Game.Window.ClientBounds;
            base.PreferredBackBufferWidth = clientBounds.Width;
            base.PreferredBackBufferHeight = clientBounds.Height;

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
            get { return base.IsFullScreen; }
            set { base.IsFullScreen = value; }
        }

        public override DisplayOrientation SupportedOrientations
        {
            get { return base.SupportedOrientations; }
            set { base.SupportedOrientations = value; }
        }

        public override void ToggleFullScreen()
        {
            base.IsFullScreen = !base.IsFullScreen;
            ApplyChanges();
        }

        public override void CreateDevice()
        {
            if (this.GraphicsDevice != null)
                return;

            GraphicsDeviceInformation gdi = this.PrepareDeviceInformation();
            var deviceSettingsArgs = new PreparingDeviceSettingsEventArgs(gdi);
            this.OnPreparingDeviceSettings(deviceSettingsArgs);
            if (gdi.PresentationParameters == null || gdi.Adapter == null)
                throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");

            this.GraphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);

            // update the touchpanel display size when the graphicsdevice is reset
            this.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset_UpdateTouchPanel;
            ((IPlatformGraphicsDevice)this.GraphicsDevice).PresentationChanged += this.GraphicsDevice_PresentationChanged_UpdateGamePlatform;

            this.OnDeviceCreated(EventArgs.Empty);

            PresentationParameters pp = this.GraphicsDevice.PresentationParameters;

            WinFormsGameWindow gameWindow = (WinFormsGameWindow)Game.Window;

            gameWindow.ChangeClientSize(pp.BackBufferWidth, pp.BackBufferHeight);

            if (pp.IsFullScreen)
            {
                gameWindow.EnterFullScreen(pp);
            }
        }

        public override void ApplyChanges()
        {
            if (this.GraphicsDevice == null)
            {
                this.CreateDevice();
            }

            GraphicsDeviceInformation gdi = this.PrepareDeviceInformation();
            var deviceSettingsArgs = new PreparingDeviceSettingsEventArgs(gdi);
            this.OnPreparingDeviceSettings(deviceSettingsArgs);
            if (gdi.PresentationParameters == null || gdi.Adapter == null)
                throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");

            // if the GraphicsProfile changed we need to create a new GraphicsDevice
            if (gdi.GraphicsProfile != GraphicsDevice.GraphicsProfile)
            {
                this.GraphicsDevice.Dispose();
                this.GraphicsDevice = null;

                this.GraphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);

                // update the touchpanel display size when the graphicsdevice is reset
                this.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset_UpdateTouchPanel;
                ((IPlatformGraphicsDevice)this.GraphicsDevice).PresentationChanged += this.GraphicsDevice_PresentationChanged_UpdateGamePlatform;

                this.OnDeviceCreated(EventArgs.Empty);
                return;
            }
            
            GraphicsDevice.Reset(gdi.PresentationParameters);
        }

        private GraphicsDeviceInformation PrepareDeviceInformation()
        {
            GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.GraphicsProfile = GraphicsProfile;

            PresentationParameters pp = new PresentationParameters();
            pp.DepthStencilFormat = this.PreferredDepthStencilFormat;
            pp.BackBufferFormat = this.PreferredBackBufferFormat;
            pp.BackBufferWidth = this.PreferredBackBufferWidth;
            pp.BackBufferHeight = this.PreferredBackBufferHeight;
            pp.IsFullScreen = this.IsFullScreen;
            pp.HardwareModeSwitch = this.HardwareModeSwitch;
            pp.PresentationInterval = this.SynchronizeWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;
            pp.DisplayOrientation = this.Game.Window.CurrentOrientation;
            pp.DeviceWindowHandle = this.Game.Window.Handle;

            // always initialize MultiSampleCount to the maximum, if users want to overwrite
            // this they have to respond to the PreparingDeviceSettingsEvent and modify
            // args.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount
            int maxMultiSampleCount = 0;
            if (this.PreferMultiSampling)
            {
                maxMultiSampleCount = 32;

                if (GraphicsDevice != null)
                {
                    maxMultiSampleCount = ((IPlatformGraphicsDevice)GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(((IPlatformGraphicsDevice)GraphicsDevice).Strategy.PresentationParameters.BackBufferFormat);
                }
            }
            pp.MultiSampleCount = maxMultiSampleCount;

            gdi.PresentationParameters = pp;

            return gdi;
        }

        private void GraphicsDevice_DeviceReset_UpdateTouchPanel(object sender, EventArgs eventArgs)
        {
            TouchPanel.DisplayWidth = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;
        }

        private void GraphicsDevice_PresentationChanged_UpdateGamePlatform(object sender, PresentationEventArgs args)
        {
            PresentationParameters pp = args.PresentationParameters;

            ((WinFormsGameWindow)this.Game.Window).OnPresentationChanged(pp);
        }

        internal void UpdateBackBufferSize(Rectangle clientBounds)
        {
            if (this.GraphicsDevice != null)
            {
                if (clientBounds.Width  != this.PreferredBackBufferWidth
                ||  clientBounds.Height != this.PreferredBackBufferHeight)
                {
                    // Set the default new back buffer size
                    this.PreferredBackBufferWidth  = clientBounds.Width;
                    this.PreferredBackBufferHeight = clientBounds.Height;
                    this.ApplyChanges();
                }
            }
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
                device.Present();
            }
        }

        #endregion IGraphicsDeviceManager strategy

    }
}
