// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteRasterizerState : ResourceRasterizerStateStrategy
    {

        internal ConcreteRasterizerState(GraphicsContextStrategy contextStrategy, IRasterizerStateStrategy source)
            : base(contextStrategy, source)
        {
        }


        internal override void PlatformGraphicsContextLost()
        {
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
