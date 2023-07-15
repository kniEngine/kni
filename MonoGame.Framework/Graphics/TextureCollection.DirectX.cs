// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {

        void PlatformInit(int capacity)
        {
        }

        void PlatformClear()
        {
        }

        internal void ClearTargets(RenderTargetBinding[] targets, D3D11.CommonShaderStage shaderStage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Make one pass across all the texture slots.
            for (var i = 0; i < _strategy._textures.Length; i++)
            {
                if (_strategy._textures[i] == null)
                    continue;

                for (int k = 0; k < targets.Length; k++)
                {
                    if (_strategy._textures[i] == targets[k].RenderTarget)
                    {
                        uint mask = ((uint)1) << i;
                        // clear texture bit
                        _strategy._dirty &= ~mask;
                        _strategy._textures[i] = null;
                        shaderStage.SetShaderResource(i, null);
                        break;
                    }
                }
            }
        }

        internal void PlatformApply(D3D11.CommonShaderStage shaderStage)
        {
            for (var i = 0; _strategy._dirty != 0 && i < _strategy._textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_strategy._dirty & mask) == 0)
                    continue;

                // NOTE: We make the assumption here that the caller has
                // locked the d3dContext for us to use.

                Texture tex = _strategy._textures[i];

                if (tex != null && !tex.IsDisposed)
                {
                    shaderStage.SetShaderResource(i, tex.GetShaderResourceView());

                    unchecked { _strategy._context._graphicsMetrics._textureCount++; }
                }
                else
                    shaderStage.SetShaderResource(i, null);

                // clear texture bit
                _strategy._dirty &= ~mask;
            }
        }

    }
}
