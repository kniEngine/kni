// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteSamplerStateCollection : SamplerStateCollectionStrategy
    {

        internal SamplerState[] InternalActualSamplers { get { return base._actualSamplers; } }


        internal ConcreteSamplerStateCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
        }


        public override SamplerState this[int index]
        {
            get { return base[index]; }
            set
            {
                base[index] = value;
            }
        }

        public override void Clear()
        {
            base.Clear();
        }

        public override void Dirty()
        {
            base.Dirty();
        }

    }
}
