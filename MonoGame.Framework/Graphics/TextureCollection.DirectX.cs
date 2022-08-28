// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        void PlatformInit()
        {
        }

        internal void ClearTargets(GraphicsDevice device, RenderTargetBinding[] targets)
        {
            if (!_applyToVertexStage)
            {
                ClearTargets(targets, device._d3dContext.PixelShader);
            }
            else
            {
                if (device.GraphicsCapabilities.SupportsVertexTextures)
                    ClearTargets(targets, device._d3dContext.VertexShader);
            }
        }

        private void ClearTargets(RenderTargetBinding[] targets, SharpDX.Direct3D11.CommonShaderStage shaderStage)
        {
            // NOTE: We make the assumption here that the caller has
            // locked the d3dContext for us to use.

            // Make one pass across all the texture slots.
            for (var i = 0; i < _textures.Length; i++)
            {
                if (_textures[i] == null)
                    continue;

                for (int k = 0; k < targets.Length; k++)
                {
                    if (_textures[i] == targets[k].RenderTarget)
                    {
                        var mask = 1 << i;
                        // clear texture bit
                        _dirty &= ~mask;
                        _textures[i] = null;
                        shaderStage.SetShaderResource(i, null);
                        break;
                    }
                }
            }
        }

        void PlatformClear()
        {
        }

        void PlatformSetTextures(GraphicsDevice device)
        {
            // Skip out if nothing has changed.
            if (_dirty != 0)
            {
                // NOTE: We make the assumption here that the caller has
                // locked the d3dContext for us to use.
                SharpDX.Direct3D11.CommonShaderStage shaderStage;
                if (!_applyToVertexStage)
                    shaderStage = device._d3dContext.PixelShader;
                else
                    shaderStage = device._d3dContext.VertexShader;

                for (var i = 0; _dirty != 0 && i < _textures.Length; i++)
                {
                    var mask = 1 << i;
                    if ((_dirty & mask) == 0)
                        continue;

                    var tex = _textures[i];

                    if (tex != null && !tex.IsDisposed)
                    {
                        shaderStage.SetShaderResource(i, tex.GetShaderResourceView());

                        unchecked { _graphicsDevice._graphicsMetrics._textureCount++; }
                    }
                    else
                        shaderStage.SetShaderResource(i, null);

                    _dirty &= ~mask;
                }

                _dirty = 0;
            }
        }
    }
}
