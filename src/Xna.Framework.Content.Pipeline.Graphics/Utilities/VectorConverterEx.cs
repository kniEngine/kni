// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

namespace Microsoft.Xna.Framework.Content.Pipeline.Utilities
{
    interface VectorConverterEx<T> where T : struct
    {
        Vector4 ToVector4(T value);
        T FromVector4(Vector4 value);
    }

    /// <summary>
    /// Helper class used by PixelBitmapContent.TryCopyFrom and TryCopyTo
    /// to convert between non-PackedValue types and Vector4.
    /// </summary>
    class VectorConverterEx :
        VectorConverterEx<byte>,
        VectorConverterEx<short>,
        VectorConverterEx<int>,
        VectorConverterEx<float>,
        VectorConverterEx<Color>,
        VectorConverterEx<Vector4>
    {
        Vector4 VectorConverterEx<byte>.ToVector4(byte value)
        {
            var f = (float)value / (float)byte.MaxValue;
            return new Vector4(f, 0f, 0f, 1f);
        }

        Vector4 VectorConverterEx<short>.ToVector4(short value)
        {
            var f = (float)value / (float)short.MaxValue;
            return new Vector4(f, 0f, 0f, 1f);
        }

        Vector4 VectorConverterEx<int>.ToVector4(int value)
        {
            var f = (float)value / (float)int.MaxValue;
            return new Vector4(f, 0f, 0f, 1f);
        }

        Vector4 VectorConverterEx<float>.ToVector4(float value)
        {
            return new Vector4(value, 0f, 0f, 1f);
        }

        Vector4 VectorConverterEx<Color>.ToVector4(Color value)
        {
            return value.ToVector4();
        }

        Vector4 VectorConverterEx<Vector4>.ToVector4(Vector4 value)
        {
            return value;
        }

        byte VectorConverterEx<byte>.FromVector4(Vector4 value)
        {
            return (byte)(value.X * (float)byte.MaxValue);
        }

        short VectorConverterEx<short>.FromVector4(Vector4 value)
        {
            return (short)(value.X * (float)short.MaxValue);
        }

        int VectorConverterEx<int>.FromVector4(Vector4 value)
        {
            return (int)(value.X * (float)int.MaxValue);
        }

        float VectorConverterEx<float>.FromVector4(Vector4 value)
        {
            return value.X;
        }

        Color VectorConverterEx<Color>.FromVector4(Vector4 value)
        {
            return new Color(value);
        }

        Vector4 VectorConverterEx<Vector4>.FromVector4(Vector4 value)
        {
            return value;
        }
    }
}
