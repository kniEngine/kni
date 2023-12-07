// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class VertexDeclaration
    {
        private readonly Dictionary<int, VertexDeclarationAttributeInfo> _shaderAttributeInfo = new Dictionary<int, VertexDeclarationAttributeInfo>();

        internal VertexDeclarationAttributeInfo GetAttributeInfo(Shader vertexShader, int programHash, int maxVertexBufferSlots)
        {
            VertexDeclarationAttributeInfo attrInfo;
            if (_shaderAttributeInfo.TryGetValue(programHash, out attrInfo))
                return attrInfo;

            // Get the vertex attribute info and cache it
            attrInfo = new VertexDeclarationAttributeInfo(maxVertexBufferSlots);

            foreach (var ve in InternalVertexElements)
            {
                var attributeLocation = ((ConcreteVertexShader)vertexShader.Strategy).GetAttributeLocation(ve.VertexElementUsage, ve.UsageIndex);
                // XNA appears to ignore usages it can't find a match for, so we will do the same
                if (attributeLocation < 0)
                    continue;

                attrInfo.Elements.Add(new VertexDeclarationAttributeInfoElement
                {
                    AttributeLocation = attributeLocation,
                    NumberOfElements = ve.VertexElementFormat.ToGLNumberOfElements(),
                    VertexAttribPointerType = ve.VertexElementFormat.ToGLVertexAttribPointerType(),
                    Normalized = ve.ToGLVertexAttribNormalized(),
                    Offset = ve.Offset,
                });
                attrInfo.EnabledAttributes[attributeLocation] = true;
            }

            _shaderAttributeInfo.Add(programHash, attrInfo);
            return attrInfo;
        }

    }
}
