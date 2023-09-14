// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class OcclusionQueryStrategy : GraphicsResourceStrategy
    {
        internal readonly GraphicsContextStrategy _contextStrategy;

        internal bool _inBeginEndPair;  // true if Begin was called and End was not yet called.
        internal bool _queryPerformed;  // true if Begin+End were called at least once.
        internal bool _isComplete;      // true if the result is available in _pixelCount.
        internal int _pixelCount;       // The query result.

        internal OcclusionQueryStrategy(GraphicsContextStrategy contextStrategy)
            : base(contextStrategy)
        {
        }

        public abstract void PlatformBegin();

        public abstract void PlatformEnd();

        public abstract void PlatformConstructOcclusionQuery();

        public abstract bool PlatformGetResult(out int pixelCount);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }

    }
}
