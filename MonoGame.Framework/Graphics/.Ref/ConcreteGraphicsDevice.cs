// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {

        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Present()
        {
            throw new PlatformNotSupportedException();
        }

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            throw new PlatformNotSupportedException();
        }


        protected override void PlatformSetup(PresentationParameters presentationParameters)
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }


        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            return new ConcreteGraphicsContext(context);
        }

        public override System.Reflection.Assembly ConcreteAssembly
        {
            get { return ReflectionHelpers.GetAssembly(typeof(ConcreteGraphicsDevice)); }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
