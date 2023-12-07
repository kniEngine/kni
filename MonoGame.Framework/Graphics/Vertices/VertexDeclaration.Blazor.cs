// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        private readonly Dictionary<int, VertexDeclarationAttributeInfo> _shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

        internal VertexDeclarationAttributeInfo GetAttributeInfo(Shader vertexShader, int programHash, int maxVertexBufferSlots)
        {
            VertexDeclarationAttributeInfo attrInfo;
            if (!_shaderAttributeInfo.TryGetValue(programHash, out attrInfo))
            {
                attrInfo = CreateAttributeInfo(vertexShader, InternalVertexElements, maxVertexBufferSlots);
                _shaderAttributeInfo.Add(programHash, attrInfo);
            }
            return attrInfo;
        }

        private static VertexDeclarationAttributeInfo CreateAttributeInfo(Shader vertexShader, VertexElement[] internalVertexElements, int maxVertexBufferSlots)
        {
            // Get the vertex attribute info and cache it
            VertexDeclarationAttributeInfo attrInfo = new VertexDeclarationAttributeInfo(maxVertexBufferSlots);
            foreach (VertexElement ve in internalVertexElements)
            {
                int attributeLocation = ((ConcreteVertexShader)vertexShader.Strategy).GetAttributeLocation(ve.VertexElementUsage, ve.UsageIndex);
                // XNA appears to ignore usages it can't find a match for, so we will do the same
                if (attributeLocation < 0)
                    continue;

                VertexDeclarationAttributeInfoElement vertexDeclarationAttributeInfoElement = new VertexDeclarationAttributeInfoElement()
                {
                    AttributeLocation = attributeLocation,
                    NumberOfElements = ve.VertexElementFormat.ToGLNumberOfElements(),
                    VertexAttribPointerType = ve.VertexElementFormat.ToGLVertexAttribPointerType(),
                    Normalized = ve.ToGLVertexAttribNormalized(),
                    Offset = ve.Offset,
                };
                attrInfo.Elements.Add(vertexDeclarationAttributeInfoElement);
                attrInfo.EnabledAttributes[attributeLocation] = true;
            }

            return attrInfo;
        }
    }
}
