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

        internal void PlatformApply(D3D11.CommonShaderStage shaderStage)
        {
            for (int i = 0; _d3dDirty != 0 && i < _actualSamplers.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_d3dDirty & mask) == 0)
                    continue;

                // NOTE: We make the assumption here that the caller has
                // locked the d3dContext for us to use.

                SamplerState sampler = _actualSamplers[i];
                D3D11.SamplerState state = null;
                if (sampler != null)
                {
                    state = ((IPlatformSamplerState)sampler).GetStrategy<ConcreteSamplerState>().GetDxState();

                    Debug.Assert(sampler.GraphicsDevice == ((IPlatformGraphicsContext)_contextStrategy.Context).DeviceStrategy.Device, "The state was created for a different device!");
                }

                shaderStage.SetSampler(i, state);

                // clear sampler bit
                _d3dDirty &= ~mask;
            }
        }

    }
}
