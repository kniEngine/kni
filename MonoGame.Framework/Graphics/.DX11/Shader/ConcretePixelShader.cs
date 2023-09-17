// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcretePixelShader : ConcreteShader
    {
        private D3D11.PixelShader _pixelShader;

        internal D3D11.PixelShader PixelShader { get { return _pixelShader; } }

        public override ShaderStage Stage { get { return ShaderStage.Pixel; } }


        internal ConcretePixelShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            CreatePixelShader();
        }

        internal override void PlatformGraphicsDeviceResetting()
        {
            DX.Utilities.Dispose(ref _pixelShader);

            base.PlatformGraphicsDeviceResetting();
        }

        private void CreatePixelShader()
        {
            _pixelShader = new D3D11.PixelShader(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, ShaderBytecode);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref _pixelShader);
            }

            base.Dispose(disposing);
        }
    }
}
