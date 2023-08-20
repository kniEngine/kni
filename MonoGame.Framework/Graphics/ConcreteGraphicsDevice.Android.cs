// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : ConcreteGraphicsDeviceGL
    {

        internal ConcreteGraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Present()
        {
            base.Present();

            try
            {
                IntPtr windowHandle = this.PresentationParameters.DeviceWindowHandle;
                AndroidGameWindow gameWindow = AndroidGameWindow.FromHandle(windowHandle);

                gameWindow.GameView.SwapBuffers();
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("Error in swap buffers", ex.ToString());
            }
        }


        internal override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsDevice device)
        {
            return new ConcreteGraphicsContext(device);
        }

        internal override TextureCollectionStrategy CreateTextureCollectionStrategy(GraphicsDevice device, GraphicsContext context, int capacity)
        {
            return new ConcreteTextureCollection(device, context, capacity);
        }

        internal override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(GraphicsDevice device, GraphicsContext context, int capacity)
        {
            return new ConcreteSamplerStateCollection(device, context, capacity);
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
