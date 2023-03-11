// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {

        void PlatformInit()
        {
        }

        void PlatformClear()
        {
        }

        internal void ClearTargets(GraphicsDevice device, RenderTargetBinding[] targets)
        {
            switch (_stage)
            {
                case ShaderStage.Pixel:
                    ClearTargets(targets, device.CurrentD3DContext.PixelShader);
                    break;
                case ShaderStage.Vertex:
                    if (!device.GraphicsCapabilities.SupportsVertexTextures)
                        return;

                    ClearTargets(targets, device.CurrentD3DContext.VertexShader);
                    break;

                default: throw new InvalidOperationException(_stage.ToString());
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

        void PlatformApply()
        {
            for (var i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                // NOTE: We make the assumption here that the caller has
                // locked the d3dContext for us to use.
                SharpDX.Direct3D11.CommonShaderStage shaderStage;
                switch (_stage)
                {
                    case ShaderStage.Pixel: shaderStage = _device.CurrentD3DContext.PixelShader; break;
                    case ShaderStage.Vertex: shaderStage = _device.CurrentD3DContext.VertexShader; break;
                    default: throw new InvalidOperationException();
                }

                var tex = _textures[i];

                if (tex != null && !tex.IsDisposed)
                {
                    shaderStage.SetShaderResource(i, tex.GetShaderResourceView());

                    unchecked { _device.CurrentContext._graphicsMetrics._textureCount++; }
                }
                else
                    shaderStage.SetShaderResource(i, null);

                // clear texture bit
                _dirty &= ~mask;
            }
        }

    }
}
