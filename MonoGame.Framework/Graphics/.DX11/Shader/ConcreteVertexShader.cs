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
    public sealed class ConcreteVertexShader : ConcreteShader
    {
        private D3D11.VertexShader _vertexShader;
        // Caches the DirectX input layouts for this vertex shader.
        private InputLayoutCache _inputLayouts;

        internal InputLayoutCache InputLayouts { get { return _inputLayouts; } }
        internal D3D11.VertexShader DXVertexShader { get { return _vertexShader; } }

        public override ShaderStage Stage { get { return ShaderStage.Vertex; } }


        internal ConcreteVertexShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
                CreateVertexShader();
        }

        private void CreateVertexShader()
        {
            _vertexShader = new D3D11.VertexShader(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, ShaderBytecode, null);
            _inputLayouts = new InputLayoutCache(GraphicsDevice, ShaderBytecode);
        }

        internal override void PlatformGraphicsDeviceResetting()
        {
            DX.Utilities.Dispose(ref _inputLayouts);
            DX.Utilities.Dispose(ref _vertexShader);

            base.PlatformGraphicsDeviceResetting();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref _inputLayouts);
                DX.Utilities.Dispose(ref _vertexShader);
            }

            base.Dispose(disposing);
        }
    }
}
