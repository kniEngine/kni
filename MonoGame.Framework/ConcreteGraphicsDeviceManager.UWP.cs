using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Graphics;

#if UAP
using Windows.UI.Xaml.Controls;
#endif

#if WINUI
using Microsoft.UI.Xaml.Controls;
#endif

namespace Microsoft.Xna.Platform
{
    internal class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {
        internal bool _initialized = false;
        public SwapChainPanel SwapChainPanel { get; set; }


        public ConcreteGraphicsDeviceManager(Game game) : base(game)
        {
            var clientBounds = base.Game.Window.ClientBounds;
            base.PreferredBackBufferWidth = clientBounds.Width;
            base.PreferredBackBufferHeight = clientBounds.Height;

            base.IsFullScreen = ((UAPGameWindow)game.Window).AppView.IsFullScreenMode;
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
            //base.ApplyChanges();

            base.IsFullScreen = !base.IsFullScreen;
            ApplyChanges();
        }

        public override void ApplyChanges()
        {
            //base.ApplyChanges();

            if (this.GraphicsDevice == null)
            {
                //this.CreateDevice();
                return;
            }

            this.Game.Window.SetSupportedOrientations(this.SupportedOrientations);

            // UAP PlatformApplyChanges
            ((UAPGameWindow)base.Game.Window).SetClientSize(base.PreferredBackBufferWidth, base.PreferredBackBufferHeight);

            // populates a gdi with settings in this gdm and allows users to override them with
            // PrepareDeviceSettings event this information should be applied to the GraphicsDevice
            var gdi = this.DoPreparingDeviceSettings();

            if (gdi.GraphicsProfile != GraphicsDevice.GraphicsProfile)
            {
                // if the GraphicsProfile changed we need to create a new GraphicsDevice
                this.GraphicsDevice.Dispose();
                this.GraphicsDevice = null;

                this.ToConcrete<ConcreteGraphicsDeviceManager>().CreateDevice(gdi);
            }
            else
            {
                GraphicsDevice.Reset(gdi.PresentationParameters);
            }
        }

        /// <summary>
        /// This populates a GraphicsDeviceInformation instance and invokes PreparingDeviceSettings to
        /// allow users to change the settings. Then returns that GraphicsDeviceInformation.
        /// Throws NullReferenceException if users set GraphicsDeviceInformation.PresentationParameters to null.
        /// </summary>
        internal GraphicsDeviceInformation DoPreparingDeviceSettings()
        {
            var gdi = new GraphicsDeviceInformation();
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.GraphicsProfile = GraphicsProfile;

            PresentationParameters pp = new PresentationParameters();
            pp.BackBufferFormat = this.PreferredBackBufferFormat;
            pp.DepthStencilFormat = this.PreferredDepthStencilFormat;
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
                if (GraphicsDevice != null)
                {
                    maxMultiSampleCount = ((IPlatformGraphicsDevice)GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(((IPlatformGraphicsDevice)GraphicsDevice).Strategy.PresentationParameters.BackBufferFormat);
                }
                else
                {
                    maxMultiSampleCount = 32;
                }
            }
            pp.MultiSampleCount = maxMultiSampleCount;

            gdi.PresentationParameters = pp;
            var args = new PreparingDeviceSettingsEventArgs(gdi);
            this.OnPreparingDeviceSettings(args);

            if (gdi.PresentationParameters == null || gdi.Adapter == null)
                throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");

            return gdi;
        }

        public override void CreateDevice()
        {
            //base.CreateDevice();

            if (this.GraphicsDevice != null)
                return;

            if (!this._initialized)
            {
                this.Game.Window.SetSupportedOrientations(this.SupportedOrientations);

                this._initialized = true;
            }

            var gdi = this.DoPreparingDeviceSettings();
            this.CreateDevice(gdi);
        }

        internal void CreateDevice(GraphicsDeviceInformation gdi)
        {
            this.GraphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);

            // update the touchpanel display size when the graphicsdevice is reset
            this.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset_UpdateTouchPanel;
            this.GraphicsDevice.PresentationChanged += this.GraphicsDevice_PresentationChanged_UpdateGamePlatform;

            this.OnDeviceCreated(EventArgs.Empty);
        }

        private void GraphicsDevice_DeviceReset_UpdateTouchPanel(object sender, EventArgs eventArgs)
        {
            TouchPanel.DisplayWidth = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;
        }

        private void GraphicsDevice_PresentationChanged_UpdateGamePlatform(object sender, PresentationEventArgs args)
        {
            base.Game.Strategy.OnPresentationChanged(args.PresentationParameters);
        }


        #region IGraphicsDeviceManager strategy

        public override bool BeginDraw()
        {
            //return base.BeginDraw();

            GraphicsDevice device = this.GraphicsDevice;
            if (device != null)
            {
                // For a UAP app we need to re-apply the
                // render target before every draw.
                // I guess the OS changes it and doesn't restore it?
                ((IPlatformGraphicsContext)((IPlatformGraphicsDevice)device).Strategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContext>().UAP_ResetRenderTargets();
            }

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
