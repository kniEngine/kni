// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDebug : GraphicsDebugStrategy
    {

        internal ConcreteGraphicsDebug(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
        }


        public override bool TryDequeueMessage(out GraphicsDebugMessage message)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
