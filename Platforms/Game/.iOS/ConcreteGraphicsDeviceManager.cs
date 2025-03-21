﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

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
                {
                    base.GraphicsDevice.PresentationParameters.IsFullScreen = value;
                    UIKit.UIApplication.SharedApplication.StatusBarHidden = base.GraphicsDevice.PresentationParameters.IsFullScreen;
                }
            }
        }

        public override DisplayOrientation SupportedOrientations
        {
            get { return base.SupportedOrientations; }
            set
            {
                base.SupportedOrientations = value;

                if (base.Game.Window != null)
                    ((iOSGameWindow)base.Game.Window).ViewController.SupportedOrientations = value;
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

            {
                // Mainscreen.Bounds does not account for the device's orientation. it ALWAYS assumes portrait
                int width = (int)(UIKit.UIScreen.MainScreen.Bounds.Width * UIKit.UIScreen.MainScreen.Scale);
                int height = (int)(UIKit.UIScreen.MainScreen.Bounds.Height * UIKit.UIScreen.MainScreen.Scale);

                // Flip the dimensions if we need to.
                if (TouchPanel.DisplayOrientation == DisplayOrientation.LandscapeLeft
                ||  TouchPanel.DisplayOrientation == DisplayOrientation.LandscapeRight)
                {
                    width = height;
                    height = (int)(UIKit.UIScreen.MainScreen.Bounds.Width * UIKit.UIScreen.MainScreen.Scale);
                }

                pp.BackBufferWidth = width;
                pp.BackBufferHeight = height;
            }

            // force "full screen" as default on iOS
            pp.IsFullScreen = true;
            UIKit.UIApplication.SharedApplication.StatusBarHidden = pp.IsFullScreen;

            GraphicsDeviceInformation gdi = new GraphicsDeviceInformation();
            gdi.GraphicsProfile = this.GraphicsProfile; // Microsoft defaults this to Reach.
            gdi.Adapter = GraphicsAdapter.DefaultAdapter;
            gdi.PresentationParameters = pp;
            var pe = new PreparingDeviceSettingsEventArgs(gdi);

            this.OnPreparingDeviceSettings(pe);

            pp = gdi.PresentationParameters;
            this.GraphicsProfile = gdi.GraphicsProfile;

            this.GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile, this.PreferHalfPixelOffset, pp);

            // ApplyChanges
            {
                base.GraphicsDevice.PresentationParameters.DisplayOrientation = base.Game.Window.CurrentOrientation;

                bool isLandscape = ((base.Game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)) != 0);
                int w = PreferredBackBufferWidth;
                int h = PreferredBackBufferHeight;

                base.GraphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
                base.GraphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);
            }

            // TODO: In XNA this seems to be done as part of the GraphicsDevice.DeviceReset event...
            //       we need to get those working.
            TouchPanel.DisplayWidth = this.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = this.GraphicsDevice.PresentationParameters.BackBufferHeight;
            TouchPanel.DisplayOrientation = this.GraphicsDevice.PresentationParameters.DisplayOrientation;

            this.OnDeviceCreated(EventArgs.Empty);

            ((iOSGameWindow)Game.Window).ViewController.View.LayoutSubviews();

            PresentationParameters pp3 = this.GraphicsDevice.PresentationParameters;
            this.GraphicsDevice.Viewport = new Viewport(0, 0, pp3.BackBufferWidth, pp3.BackBufferHeight);
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

            base.GraphicsDevice.PresentationParameters.DisplayOrientation = base.Game.Window.CurrentOrientation;

            bool isLandscape = ((base.Game.Window.CurrentOrientation & (DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight)) != 0);
            int w = PreferredBackBufferWidth;
            int h = PreferredBackBufferHeight;

            base.GraphicsDevice.PresentationParameters.BackBufferWidth = isLandscape ? Math.Max(w, h) : Math.Min(w, h);
            base.GraphicsDevice.PresentationParameters.BackBufferHeight = isLandscape ? Math.Min(w, h) : Math.Max(w, h);

            // TODO: In XNA this seems to be done as part of the GraphicsDevice.DeviceReset event...
            //       we need to get those working.
            TouchPanel.DisplayWidth = base.GraphicsDevice.PresentationParameters.BackBufferWidth;
            TouchPanel.DisplayHeight = base.GraphicsDevice.PresentationParameters.BackBufferHeight;
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

        }

        #endregion IGraphicsDeviceManager strategy

    }
}
