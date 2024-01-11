// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsDebugStrategy
    {
        internal readonly GraphicsContextStrategy _contextStrategy;

        internal GraphicsDebugStrategy(GraphicsContextStrategy contextStrategy)
        {
            _contextStrategy = contextStrategy;

        }

        public abstract bool TryDequeueMessage(out GraphicsDebugMessage message);


        public T ToConcrete<T>() where T : GraphicsDebugStrategy
        {
            return (T)this;
        }
        
    }
}
