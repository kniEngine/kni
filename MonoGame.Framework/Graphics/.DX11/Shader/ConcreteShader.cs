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

        private D3D11.PixelShader _pixelShader;
        private D3D11.VertexShader _vertexShader;
        // Caches the DirectX input layouts for this vertex shader.
        private InputLayoutCache _inputLayouts;

        internal byte[] ShaderBytecode { get { return _shaderBytecode; } }
        internal InputLayoutCache InputLayouts { get { return _inputLayouts; } }
        internal D3D11.VertexShader VertexShader { get { return _vertexShader; } }
        internal D3D11.PixelShader PixelShader { get { return _pixelShader; } }

        internal ConcreteShader(GraphicsContextStrategy contextStrategy, ShaderStage stage, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, stage, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            if (profile != ShaderProfileType.DirectX_11)
                throw new Exception("This effect was built for a different platform.");

            // We need the bytecode later for allocating the
            // input layout from the vertex declaration.
            _shaderBytecode = shaderBytecode;
            _hashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(_shaderBytecode);

            switch (stage)
            {
                case ShaderStage.Vertex:
                    CreateVertexShader();
                    break;
                case ShaderStage.Pixel:
                    CreatePixelShader();
                    break;

                default:
                    throw new InvalidOperationException("stage");
            }

        }

        private void CreatePixelShader()
        {
            System.Diagnostics.Debug.Assert(Stage == ShaderStage.Pixel);
            _pixelShader = new D3D11.PixelShader(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _shaderBytecode);
        }

        private void CreateVertexShader()
        {
            System.Diagnostics.Debug.Assert(Stage == ShaderStage.Vertex);
            _vertexShader = new D3D11.VertexShader(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _shaderBytecode, null);
            _inputLayouts = new InputLayoutCache(GraphicsDevice, ShaderBytecode);
        }

        internal override void PlatformGraphicsDeviceResetting()
        {
            switch (Stage)
            {
                case ShaderStage.Vertex:
                    DX.Utilities.Dispose(ref _inputLayouts);
                    DX.Utilities.Dispose(ref _vertexShader);
                    break;
                case ShaderStage.Pixel:
                    DX.Utilities.Dispose(ref _pixelShader);
                    break;
            }

            base.PlatformGraphicsDeviceResetting();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                switch (Stage)
                {
                    case ShaderStage.Vertex:
                        DX.Utilities.Dispose(ref _inputLayouts);
                        DX.Utilities.Dispose(ref _vertexShader);
                        break;
                    case ShaderStage.Pixel:
                        DX.Utilities.Dispose(ref _pixelShader);
                        break;
                }
            }

            base.Dispose(disposing);
        }
    }
}
