// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System.IO;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    internal partial class Shader
    {
        private D3D11.VertexShader _vertexShader;
        private D3D11.PixelShader _pixelShader;
        private byte[] _shaderBytecode;

        // Caches the DirectX input layouts for this vertex shader.
        private InputLayoutCache _inputLayouts;

        internal byte[] Bytecode
        {
            get { return _shaderBytecode; }
        }

        internal InputLayoutCache InputLayouts
        {
            get { return _inputLayouts; }
        }

        internal D3D11.VertexShader VertexShader
        {
            get
            {
                if (_vertexShader == null)
                    CreateVertexShader();
                return _vertexShader;
            }
        }

        internal D3D11.PixelShader PixelShader
        {
            get
            {
                if (_pixelShader == null)
                    CreatePixelShader();
                return _pixelShader;
            }
        }

        private static ShaderProfileType PlatformProfile()
        {
            return ShaderProfileType.DirectX_11;
        }

        private void PlatformConstructShader(ShaderStage stage, byte[] shaderBytecode)
        {
            // We need the bytecode later for allocating the
            // input layout from the vertex declaration.
            _shaderBytecode = shaderBytecode;

            HashKey = MonoGame.Framework.Utilities.Hash.ComputeHash(Bytecode);
            
            if (stage == ShaderStage.Vertex)
                CreateVertexShader();
            else
                CreatePixelShader();
        }

        private void PlatformGraphicsDeviceResetting()
        {
            DX.Utilities.Dispose(ref _vertexShader);
            DX.Utilities.Dispose(ref _pixelShader);
            DX.Utilities.Dispose(ref _inputLayouts);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref _vertexShader);
                DX.Utilities.Dispose(ref _pixelShader);
                DX.Utilities.Dispose(ref _inputLayouts);
            }

            base.Dispose(disposing);
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
            _inputLayouts = new InputLayoutCache(GraphicsDevice, Bytecode);
        }
    }
}
