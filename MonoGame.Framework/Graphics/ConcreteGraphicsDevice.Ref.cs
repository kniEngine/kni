// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {

        internal ConcreteGraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Reset()
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


        internal override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            return new ConcreteGraphicsContext(context);
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
