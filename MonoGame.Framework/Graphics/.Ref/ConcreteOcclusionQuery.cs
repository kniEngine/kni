// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConcreteOcclusionQuery : OcclusionQueryStrategy
    {
        internal ConcreteOcclusionQuery(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
        }

        public override void PlatformBegin()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformEnd()
        {
            throw new PlatformNotSupportedException();
        }

        public override void PlatformConstructOcclusionQuery()
        {
            throw new PlatformNotSupportedException();
        }

        public override bool PlatformGetResult(out int pixelCount)
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
