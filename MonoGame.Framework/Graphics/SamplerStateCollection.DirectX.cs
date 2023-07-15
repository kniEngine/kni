// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
    {
        private uint _d3dDirty;

        private void PlatformSetSamplerState(int index)
        {
            uint mask = ((uint)1) << index;
            _d3dDirty |= mask;
        }

        private void PlatformClear()
        {
            PlatformDirty();
        }

        private void PlatformDirty()
        {
            for (var i = 0; i < _actualSamplers.Length; i++)
                _d3dDirty |= (((uint)1) << i);
        }

        internal void PlatformApply(D3D11.CommonShaderStage shaderStage)
        {
            for (var i = 0; _d3dDirty != 0 && i < _actualSamplers.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_d3dDirty & mask) == 0)
                    continue;

                // NOTE: We make the assumption here that the caller has
                // locked the d3dContext for us to use.

                SamplerState sampler = _actualSamplers[i];
                D3D11.SamplerState state = null;
                if (sampler != null)
                    state = sampler.GetState(_device);

                shaderStage.SetSampler(i, state);

                // clear sampler bit
                _d3dDirty &= ~mask;
            }
        }

    }
}
