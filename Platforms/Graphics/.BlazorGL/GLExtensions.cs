// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    static class GLExtensions
    {
        public static WebGLDepthComparisonFunc ToGLComparisonFunction(this CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return WebGLDepthComparisonFunc.ALWAYS;
                case CompareFunction.Equal:
                    return WebGLDepthComparisonFunc.EQUAL;
                case CompareFunction.Greater:
                    return WebGLDepthComparisonFunc.GREATER;
                case CompareFunction.GreaterEqual:
                    return WebGLDepthComparisonFunc.GEQUAL;
                case CompareFunction.Less:
                    return WebGLDepthComparisonFunc.LESS;
                case CompareFunction.LessEqual:
                    return WebGLDepthComparisonFunc.LEQUAL;
                case CompareFunction.Never:
                    return WebGLDepthComparisonFunc.NEVER;
                case CompareFunction.NotEqual:
                    return WebGLDepthComparisonFunc.NOTEQUAL;

                default:
                    throw new ArgumentOutOfRangeException("compare");
            }
        }

        internal static int ToGLNumberOfElements(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return 1;
                case VertexElementFormat.Vector2:
                    return 2;
                case VertexElementFormat.Vector3:
                    return 3;
                case VertexElementFormat.Vector4:
                    return 4;
                case VertexElementFormat.Color:
                    return 4;
                case VertexElementFormat.Byte4:
                    return 4;
                case VertexElementFormat.Short2:
                    return 2;
                case VertexElementFormat.Short4:
                    return 4;

                case VertexElementFormat.NormalizedShort2:
                    return 2;
                case VertexElementFormat.NormalizedShort4:
                    return 4;

                case VertexElementFormat.HalfVector2:
                    return 2;
                case VertexElementFormat.HalfVector4:
                    return 4;

                default:
                    throw new ArgumentException();
            }
        }

        internal static WebGLDataType ToGLVertexAttribPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return WebGLDataType.FLOAT;
                case VertexElementFormat.Vector2:
                    return WebGLDataType.FLOAT;
                case VertexElementFormat.Vector3:
                    return WebGLDataType.FLOAT;
                case VertexElementFormat.Vector4:
                    return WebGLDataType.FLOAT;
                case VertexElementFormat.Color:
                    return WebGLDataType.UBYTE;
                case VertexElementFormat.Byte4:
                    return WebGLDataType.UBYTE;
                case VertexElementFormat.Short2:
                    return WebGLDataType.SHORT;
                case VertexElementFormat.Short4:
                    return WebGLDataType.SHORT;
                case VertexElementFormat.NormalizedShort2:
                    return WebGLDataType.SHORT;
                case VertexElementFormat.NormalizedShort4:
                    return WebGLDataType.SHORT;

                default:
                    throw new ArgumentException();
            }
        }

        internal static bool ToGLVertexAttribNormalized(this VertexElement element)
        {
            // TODO: This may or may not be the right behavor.  
            //
            // For instance the VertexElementFormat.Byte4 format is not supposed
            // to be normalized, but this line makes it so.
            //
            // The question is in MS XNA are types normalized based on usage or
            // normalized based to their format?
            //
            if (element.VertexElementUsage == VertexElementUsage.Color)
                return true;

            switch (element.VertexElementFormat)
            {
                case VertexElementFormat.NormalizedShort2:
                case VertexElementFormat.NormalizedShort4:
                    return true;

                default:
                    return false;
            }
        }


        [Conditional("DEBUG")]
        internal static void CheckGLError(this IWebGLRenderingContext GL)
        {
            WebGLErrorCode error = GL.GetError();
            if (error == WebGLErrorCode.NO_ERROR)
                return;

            string errorMsg = String.Format("GL_ERROR: {0} ({1:4X})", error, (int)error);
            Console.WriteLine(errorMsg);
            throw new InvalidOperationException("GL.GetError() returned " + errorMsg);
        }

    }
}
