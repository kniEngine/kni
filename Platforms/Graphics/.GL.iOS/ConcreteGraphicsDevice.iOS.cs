// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : ConcreteGraphicsDeviceGL
    {

        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Present()
        {
            base.Present();
        }

        
        public override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            return new ConcreteGraphicsContext(context);
        }


        protected override void PlatformSetup(PresentationParameters presentationParameters)
        {
            _mainContext = base.CreateGraphicsContext();

            int glMajorVersion = ((ConcreteGraphicsContextGL)((IPlatformGraphicsContext)_mainContext).Strategy)._glMajorVersion;
            int glMinorVersion = ((ConcreteGraphicsContextGL)((IPlatformGraphicsContext)_mainContext).Strategy)._glMinorVersion;

            _capabilities = new ConcreteGraphicsCapabilities();
            ((ConcreteGraphicsCapabilities)_capabilities).PlatformInitialize(this, glMajorVersion, glMinorVersion);

            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>()._newEnabledVertexAttributes = new bool[this.Capabilities.MaxVertexBufferSlots];
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
