// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    partial class VertexInputLayout
    {
        internal D3D11.InputElement[] GetInputElements()
        {
            var list = new List<D3D11.InputElement>();
            for (int i = 0; i < Count; i++)
            {
                foreach (VertexElement vertexElement in VertexDeclarations[i].InternalVertexElements)
                {
                    D3D11.InputElement inputElement = GetInputElement(vertexElement, i, InstanceFrequencies[i]);
                    list.Add(inputElement);
                }
            }

            D3D11.InputElement[] inputElements = list.ToArray();

            // Fix semantics indices. (If there are more vertex declarations with, for example, 
            // POSITION0, the indices are changed to POSITION1/2/3/...)
            for (int i = 1; i < inputElements.Length; i++)
            {
                string semanticName = inputElements[i].SemanticName;
                int semanticIndex = inputElements[i].SemanticIndex;
                for (int j = 0; j < i; j++)
                {
                    if (inputElements[j].SemanticName == semanticName && inputElements[j].SemanticIndex == semanticIndex)
                    {
                        // Semantic index already used.
                        semanticIndex++;
                    }
                }

                inputElements[i].SemanticIndex = semanticIndex;
            }

            return inputElements;
        }

        /// <summary>
        /// Gets the DirectX <see cref="SharpDX.Direct3D11.InputElement"/>.
        /// </summary>
        /// <param name="vertexElement">The vertexElement.</param>
        /// <param name="slot">The input resource slot.</param>
        /// <param name="instanceFrequency">
        /// The number of instances to draw using the same per-instance data before advancing in the
        /// buffer by one element. This value must be 0 for an element that contains per-vertex
        /// data.
        /// </param>
        /// <returns><see cref="SharpDX.Direct3D11.InputElement"/>.</returns>
        /// <exception cref="NotSupportedException">
        /// Unknown vertex element format or usage!
        /// </exception>
        internal static D3D11.InputElement GetInputElement(VertexElement vertexElement, int slot, int instanceFrequency)
        {
            D3D11.InputElement element = new D3D11.InputElement();

            switch (vertexElement.VertexElementUsage)
            {
                case VertexElementUsage.Position:
                    element.SemanticName = "POSITION";
                    break;
                case VertexElementUsage.Color:
                    element.SemanticName = "COLOR";
                    break;
                case VertexElementUsage.Normal:
                    element.SemanticName = "NORMAL";
                    break;
                case VertexElementUsage.TextureCoordinate:
                    element.SemanticName = "TEXCOORD";
                    break;
                case VertexElementUsage.BlendIndices:
                    element.SemanticName = "BLENDINDICES";
                    break;
                case VertexElementUsage.BlendWeight:
                    element.SemanticName = "BLENDWEIGHT";
                    break;
                case VertexElementUsage.Binormal:
                    element.SemanticName = "BINORMAL";
                    break;
                case VertexElementUsage.Tangent:
                    element.SemanticName = "TANGENT";
                    break;
                case VertexElementUsage.PointSize:
                    element.SemanticName = "PSIZE";
                    break;
                default:
                    throw new NotSupportedException("Unknown vertex element usage!");
            }

            element.SemanticIndex = vertexElement.UsageIndex;

            switch (vertexElement.VertexElementFormat)
            {
                case VertexElementFormat.Single:
                    element.Format = SharpDX.DXGI.Format.R32_Float;
                    break;
                case VertexElementFormat.Vector2:
                    element.Format = SharpDX.DXGI.Format.R32G32_Float;
                    break;
                case VertexElementFormat.Vector3:
                    element.Format = SharpDX.DXGI.Format.R32G32B32_Float;
                    break;
                case VertexElementFormat.Vector4:
                    element.Format = SharpDX.DXGI.Format.R32G32B32A32_Float;
                    break;
                case VertexElementFormat.Color:
                    element.Format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
                    break;
                case VertexElementFormat.Byte4:
                    element.Format = SharpDX.DXGI.Format.R8G8B8A8_UInt;
                    break;
                case VertexElementFormat.Short2:
                    element.Format = SharpDX.DXGI.Format.R16G16_SInt;
                    break;
                case VertexElementFormat.Short4:
                    element.Format = SharpDX.DXGI.Format.R16G16B16A16_SInt;
                    break;
                case VertexElementFormat.NormalizedShort2:
                    element.Format = SharpDX.DXGI.Format.R16G16_SNorm;
                    break;
                case VertexElementFormat.NormalizedShort4:
                    element.Format = SharpDX.DXGI.Format.R16G16B16A16_SNorm;
                    break;
                case VertexElementFormat.HalfVector2:
                    element.Format = SharpDX.DXGI.Format.R16G16_Float;
                    break;
                case VertexElementFormat.HalfVector4:
                    element.Format = SharpDX.DXGI.Format.R16G16B16A16_Float;
                    break;
                default:
                    throw new NotSupportedException("Unknown vertex element format!");
            }

            element.Slot = slot;
            element.AlignedByteOffset = vertexElement.Offset;

            // Note that instancing is only supported in feature level 9.3 and above.
            element.Classification = (instanceFrequency == 0)
                                     ? SharpDX.Direct3D11.InputClassification.PerVertexData
                                     : SharpDX.Direct3D11.InputClassification.PerInstanceData;
            element.InstanceDataStepRate = instanceFrequency;

            return element;
        }
    }
}
