// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteSamplerStateCollection : SamplerStateCollectionStrategy
    {
        private uint _d3dDirty;

        internal SamplerState[] InternalActualSamplers { get { return base._actualSamplers; } }

        internal uint InternalD3dDirty
        {
            get { return _d3dDirty; }
            set { _d3dDirty = value; }
        }


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

                uint mask = ((uint)1) << index;
                _d3dDirty |= mask;
            }
        }

        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < _actualSamplers.Length; i++)
                _d3dDirty |= (((uint)1) << i);
        }

        public override void Dirty()
        {
            base.Dirty();
            for (int i = 0; i < _actualSamplers.Length; i++)
                _d3dDirty |= (((uint)1) << i);
        }

    }
}
