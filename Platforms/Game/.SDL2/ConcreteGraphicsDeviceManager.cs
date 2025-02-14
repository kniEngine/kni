﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Platform.Graphics;
using ColorFormat = Microsoft.Xna.Platform.Graphics.OpenGL.ColorFormat;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

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

        public override void ApplyChanges()
        {
            if (this.GraphicsDevice == null)
            {
                this.CreateDevice();
            }

            // populates a gdi with settings in this gdm and allows users to override them with
            // PrepareDeviceSettings event this information should be applied to the GraphicsDevice
            var gdi = this.DoPreparingDeviceSettings();

            if (gdi.GraphicsProfile != GraphicsDevice.GraphicsProfile)
            {
                // if the GraphicsProfile changed we need to create a new GraphicsDevice
                this.GraphicsDevice.Dispose();
                this.GraphicsDevice = null;

                this.GraphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);

                // update the touchpanel display size when the graphicsdevice is reset
                this.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset_UpdateTouchPanel;
                ((IPlatformGraphicsDevice)this.GraphicsDevice).PresentationChanged += this.GraphicsDevice_PresentationChanged_UpdateGamePlatform;

                this.OnDeviceCreated(EventArgs.Empty);
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

            int maxMultiSampleCount = 0;
            if (this.PreferMultiSampling)
            {
                if (GraphicsDevice != null)
                {
                    maxMultiSampleCount = ((IPlatformGraphicsDevice)GraphicsDevice).Strategy.ToConcrete<ConcreteGraphicsDevice>().GetMaxMultiSampleCount(((IPlatformGraphicsDevice)GraphicsDevice).Strategy.PresentationParameters.BackBufferFormat);
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
                case SurfaceFormat.ColorSRgba:
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
            if (this.GraphicsDevice != null)
                return;

            var gdi = this.DoPreparingDeviceSettings();

            this.PlatformInitialize(gdi.PresentationParameters);

            this.GraphicsDevice = new GraphicsDevice(gdi.Adapter, gdi.GraphicsProfile, this.PreferHalfPixelOffset, gdi.PresentationParameters);

            // update the touchpanel display size when the graphicsdevice is reset
            this.GraphicsDevice.DeviceReset += GraphicsDevice_DeviceReset_UpdateTouchPanel;
            ((IPlatformGraphicsDevice)this.GraphicsDevice).PresentationChanged += this.GraphicsDevice_PresentationChanged_UpdateGamePlatform;

            this.OnDeviceCreated(EventArgs.Empty);

            PresentationParameters gdpp = this.GraphicsDevice.PresentationParameters;
            this.GraphicsDevice.Viewport = new Viewport(0, 0, gdpp.BackBufferWidth, gdpp.BackBufferHeight);

            ((SdlGameWindow)this.Game.Window).EndScreenDeviceChange(string.Empty, gdpp.BackBufferWidth, gdpp.BackBufferHeight, gdpp.IsFullScreen);
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

            int displayIndex = SDL.WINDOW.GetDisplayIndex(Game.Window.Handle);
            string displayName = SDL.DISPLAY.GetDisplayName(displayIndex);
            ((SdlGameWindow)this.Game.Window).EndScreenDeviceChange(displayName, pp.BackBufferWidth, pp.BackBufferHeight, pp.IsFullScreen);
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
