// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform
{
    internal class ConcreteGraphicsDeviceManager : GraphicsDeviceManagerStrategy
    {

        public ConcreteGraphicsDeviceManager(Game game) : base(game)
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PreferHalfPixelOffset
        {
            get { return base.PreferHalfPixelOffset; }
            set { base.PreferHalfPixelOffset = value; }
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
            base.ToggleFullScreen();
        }

        public override void ApplyChanges()
        {
            base.ApplyChanges();
        }


        #region IGraphicsDeviceManager strategy

        public override void CreateDevice()
        {
            base.CreateDevice();
        }

        public override bool BeginDraw()
        {
            //return base.BeginDraw();

            return true;
        }

        public override void EndDraw()
        {
            base.EndDraw();
        }

        #endregion IGraphicsDeviceManager strategy

    }
}
