using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Graphics;
using ColorFormat = Microsoft.Xna.Platform.Graphics.OpenGL.ColorFormat;

namespace Microsoft.Xna.Platform
{
    internal class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

        internal bool _initialized = false;


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
            //base.ApplyChanges();

            base.IsFullScreen = !base.IsFullScreen;
            ApplyChanges();
        }

        public override void ApplyChanges()
        {
            //base.ApplyChanges();

            if (this.GraphicsDevice == null)
            {
                this.CreateDevice();
            }

            this.Game.Window.SetSupportedOrientations(this.SupportedOrientations);

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

            PresentationParameters presentationParameters = new PresentationParameters();
            presentationParameters.BackBufferFormat = this.PreferredBackBufferFormat;
            presentationParameters.BackBufferWidth = this.PreferredBackBufferWidth;
            presentationParameters.BackBufferHeight = this.PreferredBackBufferHeight;
            presentationParameters.DepthStencilFormat = this.PreferredDepthStencilFormat;
            presentationParameters.IsFullScreen = this.IsFullScreen;
            presentationParameters.HardwareModeSwitch = this.HardwareModeSwitch;
            presentationParameters.PresentationInterval = this.SynchronizeWithVerticalRetrace ? PresentInterval.One : PresentInterval.Immediate;
            presentationParameters.DisplayOrientation = this.Game.Window.CurrentOrientation;
            presentationParameters.DeviceWindowHandle = this.Game.Window.Handle;

            int maxMultiSampleCount = 0;
            if (this.PreferMultiSampling)
            {
                if (GraphicsDevice != null)
                {
                    maxMultiSampleCount = GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(GraphicsDevice.Strategy.PresentationParameters.BackBufferFormat);
                }
            }
            presentationParameters.MultiSampleCount = maxMultiSampleCount;

            gdi.PresentationParameters = presentationParameters;
            var args = new PreparingDeviceSettingsEventArgs(gdi);
            this.OnPreparingDeviceSettings(args);

            if (gdi.PresentationParameters == null || gdi.Adapter == null)
                throw new NullReferenceException("Members should not be set to null in PreparingDeviceSettingsEventArgs");

            return gdi;
        }

        private void PlatformInitialize(PresentationParameters presentationParameters)
        {
            var surfaceFormat = ToGLColorFormat(this.PreferredBackBufferFormat);
            var depthStencilFormat = this.PreferredDepthStencilFormat;

            // TODO Need to get this data from the Presentation Parameters
            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.RedSize, surfaceFormat.R);
            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.GreenSize, surfaceFormat.G);
            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.BlueSize, surfaceFormat.B);
            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.AlphaSize, surfaceFormat.A);

            switch (depthStencilFormat)
            {
                case DepthFormat.None:
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.DepthSize, 0);
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth16:
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.DepthSize, 16);
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24:
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.StencilSize, 0);
                    break;
                case DepthFormat.Depth24Stencil8:
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.DepthSize, 24);
                    SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.StencilSize, 8);
                    break;
            }

            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.DoubleBuffer, 1);
            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextMajorVersion, 2);
            SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.ContextMinorVersion, 1);

            if (presentationParameters.MultiSampleCount > 0)
            {
                SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.MultiSampleBuffers, 1);
                SDL.OpenGL.SetAttribute(Sdl.GL.Attribute.MultiSampleSamples, presentationParameters.MultiSampleCount);
            }

            ((SdlGameWindow)Game.Window).CreateWindow();
            presentationParameters.DeviceWindowHandle = Game.Window.Handle;
        }

        /// <summary>
        /// Convert a <see cref="SurfaceFormat"/> to an GL ColorFormat.
        /// This is used for setting up the backbuffer format of the OpenGL context.
        /// </summary>
        /// <returns>A GL ColorFormat instance.</returns>
        /// <param name="format">The <see cref="SurfaceFormat"/> to convert.</param>
        private static ColorFormat ToGLColorFormat(SurfaceFormat format)
        {
            switch (format)
            {
                case SurfaceFormat.Alpha8:
                    return new ColorFormat(0, 0, 0, 8);
                case SurfaceFormat.Bgr565:
                    return new ColorFormat(5, 6, 5, 0);
                case SurfaceFormat.Bgra4444:
                    return new ColorFormat(4, 4, 4, 4);
                case SurfaceFormat.Bgra5551:
                    return new ColorFormat(5, 5, 5, 1);
                case SurfaceFormat.Bgr32:
                    return new ColorFormat(8, 8, 8, 0);
                case SurfaceFormat.Bgra32:
                case SurfaceFormat.Color:
                case SurfaceFormat.ColorSRgb:
                    return new ColorFormat(8, 8, 8, 8);
                case SurfaceFormat.Rgba1010102:
                    return new ColorFormat(10, 10, 10, 2);

                default:
                    // Floating point backbuffers formats could be implemented
                    // but they are not typically used on the backbuffer. In
                    // those cases it is better to create a render target instead.
                    throw new NotSupportedException();
            }
        }

        public override void CreateDevice()
        {
            //base.CreateDevice();

            if (this.GraphicsDevice != null)
                return;

            var gdi = this.DoPreparingDeviceSettings();

            if (!this._initialized)
            {
                this.Game.Window.SetSupportedOrientations(this.SupportedOrientations);

                this.PlatformInitialize(gdi.PresentationParameters);

                this._initialized = true;
            }

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
