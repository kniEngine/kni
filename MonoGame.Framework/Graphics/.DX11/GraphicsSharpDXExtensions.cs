// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;
using D3D11 = SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    static public class GraphicsSharpDXExtensions
    {
        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Device.
        /// </summary>
        public static object GetD3D11Device(this GraphicsDevice device)
        {
            D3D11.Device d3dDevice = ((IPlatformGraphicsDevice)device).Strategy.ToConcrete<ConcreteGraphicsDevice>()._d3dDevice;
            return d3dDevice;
        }

        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Resource.
        /// </summary>
        public static object GetD3D11Resource(this Texture texture)
        {
            D3D11.Resource d3dResource = texture.GetTextureStrategy<ConcreteTexture>().GetTexture();
            return d3dResource;
        }
    }
}
