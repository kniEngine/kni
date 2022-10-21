// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using MonoGame.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        private readonly Dictionary<int, VertexDeclarationAttributeInfo> _shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

        internal VertexDeclarationAttributeInfo GetAttributeInfo(Shader shader, int programHash)
        {
            VertexDeclarationAttributeInfo attrInfo;
            if (_shaderAttributeInfo.TryGetValue(programHash, out attrInfo))
                return attrInfo;

            // Get the vertex attribute info and cache it
            attrInfo = new VertexDeclarationAttributeInfo(GraphicsDevice.MaxVertexAttributes);

            foreach (var ve in InternalVertexElements)
            {
                var attributeLocation = shader.GetAttribLocation(ve.VertexElementUsage, ve.UsageIndex);
                // XNA appears to ignore usages it can't find a match for, so we will do the same
                if (attributeLocation < 0)
                    continue;

                attrInfo.Elements.Add(new VertexDeclarationAttributeInfo.Element
                {
                    AttributeLocation = attributeLocation,
                    NumberOfElements = ToGLNumberOfElements(ve.VertexElementFormat),
                    VertexAttribPointerType = ToGLVertexAttribPointerType(ve.VertexElementFormat),
                    Normalized = ToGLVertexAttribNormalized(ve),
                    Offset = ve.Offset,
                });
                attrInfo.EnabledAttributes[attributeLocation] = true;
            }

            _shaderAttributeInfo.Add(programHash, attrInfo);
            return attrInfo;
        }


		internal void Apply(Shader shader, IntPtr offset, int programHash)
		{
            var attrInfo = GetAttributeInfo(shader, programHash);

            // Apply the vertex attribute info
            for (int i=0; i< attrInfo.Elements.Count; i++)
            {
                var element = attrInfo.Elements[i];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    VertexStride,
                    (IntPtr)(offset.ToInt64() + element.Offset));
                GraphicsExtensions.CheckGLError();

#if !(GLES || MONOMAC)
                if (GraphicsDevice.GraphicsCapabilities.SupportsInstancing)
                {
                    GL.VertexAttribDivisor(element.AttributeLocation, 0);
                    GraphicsExtensions.CheckGLError();
                }
#endif
            }
            GraphicsDevice.SetVertexAttributeArray(attrInfo.EnabledAttributes);
		    GraphicsDevice._attribsDirty = true;
		}

        private static int ToGLNumberOfElements(VertexElementFormat elementFormat)
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
        
        private static VertexAttribPointerType ToGLVertexAttribPointerType(VertexElementFormat elementFormat)
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
                    return VertexAttribPointerType.Short;
                case VertexElementFormat.NormalizedShort4:
                    return VertexAttribPointerType.Short;

#if DESKTOPGL
                case VertexElementFormat.HalfVector2:
                    return VertexAttribPointerType.HalfFloat;
                case VertexElementFormat.HalfVector4:
                    return VertexAttribPointerType.HalfFloat;
#endif

                default:
                    throw new ArgumentException();
            }
        }

        private static bool ToGLVertexAttribNormalized(VertexElement element)
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
        
        private static VertexPointerType ToGLVertexPointerType(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return VertexPointerType.Float;
                case VertexElementFormat.Vector2:
                    return VertexPointerType.Float;
                case VertexElementFormat.Vector3:
                    return VertexPointerType.Float;
                case VertexElementFormat.Vector4:
                    return VertexPointerType.Float;
                case VertexElementFormat.Color:
                    return VertexPointerType.Short;
                case VertexElementFormat.Byte4:
                    return VertexPointerType.Short;
                case VertexElementFormat.Short2:
                    return VertexPointerType.Short;
                case VertexElementFormat.Short4:
                    return VertexPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return VertexPointerType.Short;
                case VertexElementFormat.NormalizedShort4:
                    return VertexPointerType.Short;

                case VertexElementFormat.HalfVector2:
                    return VertexPointerType.Float;
                case VertexElementFormat.HalfVector4:
                    return VertexPointerType.Float;

                default:
                    throw new ArgumentException();
            }
        }

        private static ColorPointerType ToGLColorPointerType(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return ColorPointerType.Float;
                case VertexElementFormat.Vector2:
                    return ColorPointerType.Float;
                case VertexElementFormat.Vector3:
                    return ColorPointerType.Float;
                case VertexElementFormat.Vector4:
                    return ColorPointerType.Float;
                case VertexElementFormat.Color:
                    return ColorPointerType.UnsignedByte;
                case VertexElementFormat.Byte4:
                    return ColorPointerType.UnsignedByte;
                case VertexElementFormat.Short2:
                    return ColorPointerType.Short;
                case VertexElementFormat.Short4:
                    return ColorPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return ColorPointerType.UnsignedShort;
                case VertexElementFormat.NormalizedShort4:
                    return ColorPointerType.UnsignedShort;

#if MONOMAC
                case VertexElementFormat.HalfVector2:
                    return ColorPointerType.HalfFloat;
                case VertexElementFormat.HalfVector4:
                    return ColorPointerType.HalfFloat;
#endif

                default:
                    throw new ArgumentException();
            }
        }

        private static NormalPointerType ToGLNormalPointerType(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return NormalPointerType.Float;
                case VertexElementFormat.Vector2:
                    return NormalPointerType.Float;
                case VertexElementFormat.Vector3:
                    return NormalPointerType.Float;
                case VertexElementFormat.Vector4:
                    return NormalPointerType.Float;
                case VertexElementFormat.Color:
                    return NormalPointerType.Byte;
                case VertexElementFormat.Byte4:
                    return NormalPointerType.Byte;
                case VertexElementFormat.Short2:
                    return NormalPointerType.Short;
                case VertexElementFormat.Short4:
                    return NormalPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return NormalPointerType.Short;
                case VertexElementFormat.NormalizedShort4:
                    return NormalPointerType.Short;

#if MONOMAC
                case VertexElementFormat.HalfVector2:
                    return NormalPointerType.HalfFloat;
                case VertexElementFormat.HalfVector4:
                    return NormalPointerType.HalfFloat;
#endif

                default:
                    throw new ArgumentException();
            }
        }

        private static TexCoordPointerType ToGLTexCoordPointerType(VertexElementFormat elementFormat)
        {
            switch (elementFormat)
            {
                case VertexElementFormat.Single:
                    return TexCoordPointerType.Float;
                case VertexElementFormat.Vector2:
                    return TexCoordPointerType.Float;
                case VertexElementFormat.Vector3:
                    return TexCoordPointerType.Float;
                case VertexElementFormat.Vector4:
                    return TexCoordPointerType.Float;
                case VertexElementFormat.Color:
                    return TexCoordPointerType.Float;
                case VertexElementFormat.Byte4:
                    return TexCoordPointerType.Float;
                case VertexElementFormat.Short2:
                    return TexCoordPointerType.Short;
                case VertexElementFormat.Short4:
                    return TexCoordPointerType.Short;

                case VertexElementFormat.NormalizedShort2:
                    return TexCoordPointerType.Short;
                case VertexElementFormat.NormalizedShort4:
                    return TexCoordPointerType.Short;

#if MONOMAC
                case VertexElementFormat.HalfVector2:
                    return TexCoordPointerType.HalfFloat;
                case VertexElementFormat.HalfVector4:
                    return TexCoordPointerType.HalfFloat;
#endif

                default:
                    throw new ArgumentException();
            }

        }

        /// <summary>
        /// Vertex attribute information for a particular shader/vertex declaration combination.
        /// </summary>
        internal class VertexDeclarationAttributeInfo
        {
            internal bool[] EnabledAttributes;

            internal class Element
            {
                public int AttributeLocation;
                public int NumberOfElements;
                public VertexAttribPointerType VertexAttribPointerType;
                public bool Normalized;
                public int Offset;
            }

            internal List<Element> Elements;

            internal VertexDeclarationAttributeInfo(int maxVertexAttributes)
            {
                EnabledAttributes = new bool[maxVertexAttributes];
                Elements = new List<Element>();
            }
        }
    }
}
