// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// A snapshot of rendering statistics from <see cref="GraphicsDevice.Metrics"/> to be used for runtime debugging and profiling.
    /// </summary>
    public struct GraphicsMetrics
    {
        internal long _clearCount;
        internal long _drawCount;
        internal long _pixelShaderCount;
        internal long _primitiveCount;
        internal long _spriteCount;
        internal long _targetCount;
        internal long _textureCount;
        internal long _vertexShaderCount;

        /// <summary>
        /// Number of times Clear was called.
        /// </summary>
        public long ClearCount { get { return _clearCount; } }

        /// <summary>
        /// Number of times Draw was called.
        /// </summary>
        public long DrawCount { get { return _drawCount; } }

        /// <summary>
        /// Number of times the pixel shader was changed on the GPU.
        /// </summary>
        public long PixelShaderCount { get { return _pixelShaderCount; } }

        /// <summary>
        /// Number of rendered primitives.
        /// </summary>
        public long PrimitiveCount { get { return _primitiveCount; } }

        /// <summary>
        /// Number of sprites and text characters rendered via <see cref="SpriteBatch"/>.
        /// </summary>
        public long SpriteCount { get { return _spriteCount; } }

        /// <summary>
        /// Number of times a target was changed on the GPU.
        /// </summary>
        public long TargetCount {get { return _targetCount; } }

        /// <summary>
        /// Number of times a texture was changed on the GPU.
        /// </summary>
        public long TextureCount { get { return _textureCount; } }

        /// <summary>
        /// Number of times the vertex shader was changed on the GPU.
        /// </summary>
        public long VertexShaderCount { get { return _vertexShaderCount; } }

        /// <summary>
        /// Returns the difference between two sets of metrics.
        /// </summary>
        /// <param name="left">Source <see cref="GraphicsMetrics"/> on the left of the sub sign.</param>
        /// <param name="right">Source <see cref="GraphicsMetrics"/> on the right of the sub sign.</param>
        /// <returns>Difference between two sets of metrics.</returns>
        public static GraphicsMetrics operator -(GraphicsMetrics left, GraphicsMetrics right)
        {
            return new GraphicsMetrics()
            {
                _clearCount = left._clearCount - right._clearCount,
                _drawCount = left._drawCount - right._drawCount,
                _pixelShaderCount = left._pixelShaderCount - right._pixelShaderCount,
                _primitiveCount = left._primitiveCount - right._primitiveCount,
                _spriteCount = left._spriteCount - right._spriteCount,
                _targetCount = left._targetCount - right._targetCount,
                _textureCount = left._textureCount - right._textureCount,
                _vertexShaderCount = left._vertexShaderCount - right._vertexShaderCount
            };
        }

        /// <summary>
        /// Returns the combination of two sets of metrics.
        /// </summary>
        /// <param name="left">Source <see cref="GraphicsMetrics"/> on the left of the add sign.</param>
        /// <param name="right">Source <see cref="GraphicsMetrics"/> on the right of the add sign.</param>
        /// <returns>Combination of two sets of metrics.</returns>
        public static GraphicsMetrics operator +(GraphicsMetrics left, GraphicsMetrics right)
        {
            return new GraphicsMetrics()
            {
                _clearCount =  left._clearCount + right._clearCount,
                _drawCount = left._drawCount + right._drawCount,
                _pixelShaderCount = left._pixelShaderCount + right._pixelShaderCount,
                _primitiveCount = left._primitiveCount + right._primitiveCount,
                _spriteCount = left._spriteCount + right._spriteCount,
                _targetCount = left._targetCount + right._targetCount,
                _textureCount = left._textureCount + right._textureCount,
                _vertexShaderCount = left._vertexShaderCount + right._vertexShaderCount
            };
        }
    }
}
