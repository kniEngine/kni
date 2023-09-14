// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteOcclusionQuery : OcclusionQueryStrategy
    {

        public override int PixelCount { get { throw new PlatformNotSupportedException(); } }

        public override bool IsComplete { get { throw new PlatformNotSupportedException(); } }

        internal ConcreteOcclusionQuery(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformBegin()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformEnd()
        {
            throw new PlatformNotSupportedException();
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
