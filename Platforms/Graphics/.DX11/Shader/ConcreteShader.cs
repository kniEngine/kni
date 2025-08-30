// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteShader : ShaderStrategy
    {
        private byte[] _shaderBytecode;

        internal byte[] ShaderBytecode { get { return _shaderBytecode; } }


        internal ConcreteShader(GraphicsContextStrategy contextStrategy, ShaderVersion shaderVersion, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes)
            : base(contextStrategy, shaderVersion, shaderBytecode, samplers, cBuffers, attributes)
        {
            GraphicsProfile graphicsProfile = this.GraphicsDeviceStrategy.GraphicsProfile;
            ShaderVersion maxVersion = MaxShaderVersions[graphicsProfile];
            if (shaderVersion != default
            && shaderVersion > maxVersion)
            {
                throw new NotSupportedException(
                    $"Shader model {shaderVersion} is not supported by the current graphics profile '{graphicsProfile}'.");
            }
            // We need the bytecode later for allocating the
            // input layout from the vertex declaration.
            _shaderBytecode = shaderBytecode;
        }


        protected override void PlatformGraphicsContextLost()
        {

            base.PlatformGraphicsContextLost();
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
