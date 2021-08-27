// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public static class VectorConverter
    {
        public static bool TryGetSurfaceFormat(Type vectorType, out SurfaceFormat surfaceFormat)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetVectorType(SurfaceFormat surfaceFormat, out Type vectorType)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetVectorType(VertexElementFormat vertexElementFormat, out Type vectorType)
        {
            throw new NotImplementedException();
        }

        public static bool TryGetVertexElementFormat(Type vectorType, out VertexElementFormat vertexElementFormat)
        {
            throw new NotImplementedException();
        }

        // see: https://docs.microsoft.com/en-us/dotnet/api/system.converter-2
        public static Converter<TInput, TOutput> GetConverter<TInput, TOutput>()
        {
            if (typeof(TInput) == typeof(Vector2) && typeof(TOutput) == typeof(Vector4))
            {
                var converter = new Converter<Vector2, Vector4>(Vector2ToVector4);
                return (Converter<TInput, TOutput>)(object)converter;
            }
            if (typeof(TInput) == typeof(Vector3) && typeof(TOutput) == typeof(Vector4))
            {
                var converter = new Converter<Vector3, Vector4>(Vector3ToVector4);
                return (Converter<TInput, TOutput>)(object)converter;
            }
            if (typeof(TInput) == typeof(Vector4) && typeof(TOutput) == typeof(Vector4))
            {
                var converter = new Converter<Vector4, Vector4>(Vector4ToVector4);
                return (Converter<TInput, TOutput>)(object)converter;
            }

            if (typeof(TInput) == typeof(Vector4) && typeof(TOutput) == typeof(float))
            {
                var converter = new Converter<Vector4, float>(Vector4ToFloat);
                return (Converter<TInput, TOutput>)(object)converter;
            }
            if (typeof(TInput) == typeof(Vector4) && typeof(TOutput) == typeof(Vector2))
            {
                var converter = new Converter<Vector4, Vector2>(Vector4ToVector2);
                return (Converter<TInput, TOutput>)(object)converter;
            }
            if (typeof(TInput) == typeof(Vector4) && typeof(TOutput) == typeof(Vector3))
            {
                var converter = new Converter<Vector4, Vector3>(Vector4ToVector3);
                return (Converter<TInput, TOutput>)(object)converter;
            }

            if (typeof(TInput) == typeof(Vector4) && typeof(TOutput).GetInterface("IPackedVector") != null)
            {
                var converter = new Converter<Vector4, TOutput>(Vector4ToPackedVector<TOutput>);
                return (Converter<TInput, TOutput>)(object)converter;
            }

            throw new NotImplementedException(
                string.Format("TypeConverter for {0} -> {1} is not implemented.",
                typeof(TInput).Name, typeof(TOutput).Name));
        }

        private static Vector4 Vector2ToVector4(Vector2 value)
        {
            return new Vector4(value.X, value.Y, 0.0f, 0.0f);
        }

        private static Vector4 Vector3ToVector4(Vector3 value)
        {
            return new Vector4(value.X, value.Y, value.Z, 0.0f);
        }

        private static Vector4 Vector4ToVector4(Vector4 value)
        {
            return value;
        }

        private static float Vector4ToFloat(Vector4 value)
        {
            return value.X;
        }

        private static Vector2 Vector4ToVector2(Vector4 value)
        {
            return new Vector2(value.X, value.Y);
        }

        private static Vector3 Vector4ToVector3(Vector4 value)
        {
            return new Vector3(value.X, value.Y, value.Z);
        }

        private static TPacked Vector4ToPackedVector<TPacked>(Vector4 value)
        {
            var packedVec = (IPackedVector)Activator.CreateInstance(typeof(TPacked));
            packedVec.PackFromVector4(value);
            return (TPacked)packedVec;
        }
    }
}
