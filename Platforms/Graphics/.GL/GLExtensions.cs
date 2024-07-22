// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    static class GLExtensions
    {
        public static ComparisonFunc ToGLComparisonFunction(this CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return ComparisonFunc.Always;
                case CompareFunction.Equal:
                    return ComparisonFunc.Equal;
                case CompareFunction.Greater:
                    return ComparisonFunc.Greater;
                case CompareFunction.GreaterEqual:
                    return ComparisonFunc.Gequal;
                case CompareFunction.Less:
                    return ComparisonFunc.Less;
                case CompareFunction.LessEqual:
                    return ComparisonFunc.Lequal;
                case CompareFunction.Never:
                    return ComparisonFunc.Never;
                case CompareFunction.NotEqual:
                    return ComparisonFunc.Notequal;

                default:
                    return ComparisonFunc.Always;
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

        internal static VertexAttribPointerType ToGLVertexAttribPointerType(this VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Vector2:
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Vector3:
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Vector4:
                    return VertexAttribPointerType.Float;
                case VertexElementFormat.Color:
                    return VertexAttribPointerType.UnsignedByte;
                case VertexElementFormat.Byte4:
                    return VertexAttribPointerType.UnsignedByte;
                case VertexElementFormat.Short2:
                    return VertexAttribPointerType.Short;
                case VertexElementFormat.Short4:
                    return VertexAttribPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return VertexAttribPointerType.Short; // UnsignedShort?
                case VertexElementFormat.NormalizedShort4:
                    return VertexAttribPointerType.Short; // UnsignedShort?

#if DESKTOPGL // HiDef?
                case VertexElementFormat.HalfVector2:
                    return VertexAttribPointerType.HalfFloat;
                case VertexElementFormat.HalfVector4:
                    return VertexAttribPointerType.HalfFloat;
#endif

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
    }
        
}
