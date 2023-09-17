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
    public abstract class ConcreteShader : ShaderStrategy
    {
        private byte[] _shaderBytecode;

        internal byte[] ShaderBytecode { get { return _shaderBytecode; } }


        internal ConcreteShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            if (profile != ShaderProfileType.DirectX_11)
                throw new Exception("This effect was built for a different platform.");

            // We need the bytecode later for allocating the
            // input layout from the vertex declaration.
            _shaderBytecode = shaderBytecode;
            _hashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(_shaderBytecode);
        }


        internal override void PlatformGraphicsDeviceResetting()
        {

            base.PlatformGraphicsDeviceResetting();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}
