// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public sealed class ConcreteVertexShader : ConcreteShader
    {
        private readonly Dictionary<VertexElement[], VertexDeclarationAttributeInfo> _vertexAttribInfoCache = new Dictionary<VertexElement[], VertexDeclarationAttributeInfo>();

        internal ConcreteVertexShader(GraphicsContextStrategy contextStrategy, byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
            : base(contextStrategy, shaderBytecode, samplers, cBuffers, attributes, profile)
        {
            base.CreateShader(contextStrategy, ShaderType.VertexShader, shaderBytecode);
        }

        private int GetAttributeLocation(VertexElementUsage usage, int index)
        {
            for (int i = 0; i < Attributes.Length; i++)
            {
                if ((Attributes[i].usage == usage) && (Attributes[i].index == index))
                    return Attributes[i].location;
            }
            return -1;
        }

        internal VertexDeclarationAttributeInfo GetVertexAttribInfo(VertexDeclaration vertexDeclaration, int maxVertexBufferSlots)
        {
            VertexElement[] vertexElements = ((IPlatformVertexDeclaration)vertexDeclaration).InternalVertexElements;

            VertexDeclarationAttributeInfo vertexAttribInfo;
            if (_vertexAttribInfoCache.TryGetValue(vertexElements, out vertexAttribInfo))
                return vertexAttribInfo;

            vertexAttribInfo = ConcreteVertexShader.CreateVertexAttribInfo(this, vertexElements, maxVertexBufferSlots);
            _vertexAttribInfoCache.Add(vertexElements, vertexAttribInfo);
            return vertexAttribInfo;
        }

        private static VertexDeclarationAttributeInfo CreateVertexAttribInfo(ConcreteVertexShader vertexShaderStrategy, VertexElement[] vertexElements, int maxVertexBufferSlots)
        {
            // Get the vertex attribute info and cache it
            VertexDeclarationAttributeInfo attrInfo = new VertexDeclarationAttributeInfo(maxVertexBufferSlots);

            for (int v = 0; v < vertexElements.Length; v++)
            {
                int attributeLocation = vertexShaderStrategy.GetAttributeLocation(vertexElements[v].VertexElementUsage, vertexElements[v].UsageIndex);
                // XNA appears to ignore usages it can't find a match for, so we will do the same
                if (attributeLocation < 0)
                    continue;

                VertexDeclarationAttributeInfoElement vertexAttribInfoElement = new VertexDeclarationAttributeInfoElement();
                vertexAttribInfoElement.NumberOfElements = vertexElements[v].VertexElementFormat.ToGLNumberOfElements();
                vertexAttribInfoElement.VertexAttribPointerType = vertexElements[v].VertexElementFormat.ToGLVertexAttribPointerType();
                vertexAttribInfoElement.Normalized = vertexElements[v].ToGLVertexAttribNormalized();
                vertexAttribInfoElement.Offset = vertexElements[v].Offset;
                vertexAttribInfoElement.AttributeLocation = attributeLocation;

                attrInfo.Elements.Add(vertexAttribInfoElement);
                attrInfo.EnabledAttributes[vertexAttribInfoElement.AttributeLocation] = true;
            }

            return attrInfo;
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
