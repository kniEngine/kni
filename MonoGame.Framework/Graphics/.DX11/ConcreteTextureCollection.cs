// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteTextureCollection : TextureCollectionStrategy
    {

        internal ConcreteTextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
        }


        public override void Clear()
        {
            base.Clear();
        }

        internal void ClearTargets(RenderTargetBinding[] targets, D3D11.CommonShaderStage shaderStage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Make one pass across all the texture slots.
            for (int i = 0; i < _textures.Length; i++)
            {
                if (_textures[i] == null)
                    continue;

                for (int k = 0; k < targets.Length; k++)
                {
                    if (_textures[i] == targets[k].RenderTarget)
                    {
                        uint mask = ((uint)1) << i;
                        // clear texture bit
                        _dirty &= ~mask;
                        _textures[i] = null;
                        shaderStage.SetShaderResource(i, null);
                        break;
                    }
                }
            }
        }

        internal void PlatformApply(D3D11.CommonShaderStage shaderStage)
        {
            for (int i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                // NOTE: We make the assumption here that the caller has
                // locked the d3dContext for us to use.

                Texture texture = _textures[i];

                if (texture != null && !texture.IsDisposed)
                {
                    shaderStage.SetShaderResource(i, ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>().GetShaderResourceView());

                    unchecked { _contextStrategy.Context._graphicsMetrics._textureCount++; }
                }
                else
                    shaderStage.SetShaderResource(i, null);

                // clear texture bit
                _dirty &= ~mask;
            }
        }

    }
}
