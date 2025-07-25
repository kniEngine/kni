﻿// MonoGame - Copyright (C) The MonoGame Team
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

        internal D3D11.PixelShader DXPixelShader { get { return _pixelShader; } }


        internal ConcretePixelShader(GraphicsContextStrategy contextStrategy, ShaderVersion shaderVersion, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderVersion, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            CreatePixelShader();
        }

        protected override void PlatformGraphicsContextLost()
        {
            DX.Utilities.Dispose(ref _pixelShader);

            base.PlatformGraphicsContextLost();
        }

        private void CreatePixelShader()
        {
            _pixelShader = new D3D11.PixelShader(base.GraphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, ShaderBytecode);
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
