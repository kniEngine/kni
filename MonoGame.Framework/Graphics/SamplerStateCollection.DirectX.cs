// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
    {
        private int _d3dDirty;

        private void PlatformSetSamplerState(int index)
        {
            var mask = 1 << index;
            _d3dDirty |= mask;
        }

        private void PlatformClear()
        {
            PlatformDirty();
        }

        private void PlatformDirty()
        {
            for (var i = 0; i < _actualSamplers.Length; i++)
                _d3dDirty |= (1 << i);
        }

        internal void PlatformSetSamplers(GraphicsDevice device)
        {
            if (!_applyToVertexStage || device.GraphicsCapabilities.SupportsVertexTextures)
            {
                for (var i = 0; _d3dDirty != 0 && i < _actualSamplers.Length; i++)
                {
                    var mask = 1 << i;
                    if ((_d3dDirty & mask) == 0)
                        continue;

                    // NOTE: We make the assumption here that the caller has
                    // locked the d3dContext for us to use.
                    SharpDX.Direct3D11.CommonShaderStage shaderStage;
                    if (!_applyToVertexStage)
                        shaderStage = device._d3dContext.PixelShader;
                    else
                        shaderStage = device._d3dContext.VertexShader;

                    var sampler = _actualSamplers[i];
                    SharpDX.Direct3D11.SamplerState state = null;
                    if (sampler != null)
                        state = sampler.GetState(device);

                    shaderStage.SetSampler(i, state);

                    // clear sampler bit
                    _d3dDirty &= ~mask;
                }
            }
        }
    }
}
