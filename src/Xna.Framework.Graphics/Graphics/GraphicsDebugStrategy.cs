﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformGraphicsDebug
    {
        GraphicsDebugStrategy Strategy { get; }
    }

    public abstract class GraphicsDebugStrategy
    {
        protected readonly GraphicsContextStrategy _contextStrategy;

        protected GraphicsDebugStrategy(GraphicsContextStrategy contextStrategy)
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
