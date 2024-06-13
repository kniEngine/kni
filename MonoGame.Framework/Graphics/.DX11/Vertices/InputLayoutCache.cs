// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Caches DirectX input layouts for the input assembler stage.
    /// </summary>
    internal class InputLayoutCache : IDisposable
    {
#if DEBUG
        // Flag to print warning only once per shader.
        private bool _printWarning = true;
#endif
        private readonly GraphicsDeviceStrategy _graphicsDeviceStrategy;
        private readonly byte[] _shaderByteCode;
        private readonly Dictionary<VertexInputLayout, D3D11.InputLayout> _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="InputLayoutCache"/> class.
        /// </summary>
        /// <param name="graphicsDeviceStrategy">The graphics device strategy.</param>
        /// <param name="shaderByteCode">The byte code of the vertex shader.</param>
        public InputLayoutCache(GraphicsDeviceStrategy graphicsDeviceStrategy, byte[] shaderByteCode)
        {
            Debug.Assert(graphicsDeviceStrategy != null);
            Debug.Assert(shaderByteCode != null);

            _graphicsDeviceStrategy = graphicsDeviceStrategy;
            _shaderByteCode = shaderByteCode;
            _cache = new Dictionary<VertexInputLayout, D3D11.InputLayout>();
        }

        /// <summary>
        /// Releases all resources used by an instance of the <see cref="InputLayoutCache"/> class.
        /// </summary>
        /// <remarks>
        /// This method calls the virtual <see cref="Dispose(bool)"/> method, passing in
        /// <see langword="true"/>, and then suppresses finalization of the instance.
        /// </remarks>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by an instance of the
        /// <see cref="InputLayoutCache"/> class and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <see langword="true"/> to release both managed and unmanaged resources;
        /// <see langword="false"/> to release only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose managed resources.
                foreach (var entry in _cache)
                    entry.Value.Dispose();

                _cache.Clear();
            }
        }

        /// <summary>
        /// Gets or create the DirectX input layout for the specified vertex buffers.
        /// </summary>
        /// <param name="vertexInputLayout">The vertex buffers.</param>
        /// <returns>The DirectX input layout.</returns>
        public D3D11.InputLayout GetOrCreate(VertexInputLayout vertexInputLayout)
        {
            D3D11.InputLayout inputLayout;
            if (_cache.TryGetValue(vertexInputLayout, out inputLayout))
                return inputLayout;

            // Create an 'ImmutableVertexInputLayout' that can be used as a key in the 'InputLayoutCache'.
            VertexDeclaration[] vertexDeclarations = new VertexDeclaration[vertexInputLayout.Count];
            int[] instanceFrequencies = new int[vertexInputLayout.Count];
            Array.Copy(vertexInputLayout.VertexDeclarations, vertexDeclarations, vertexDeclarations.Length);
            Array.Copy(vertexInputLayout.InstanceFrequencies, instanceFrequencies, instanceFrequencies.Length);
            ImmutableVertexInputLayout immutableVertexInputLayout = new ImmutableVertexInputLayout(vertexDeclarations, instanceFrequencies);

            // Get inputElements
            D3D11.InputElement[] inputElements;
            {
                List<D3D11.InputElement> list = new List<D3D11.InputElement>();
                for (int i = 0; i < vertexInputLayout.Count; i++)
                {
                    VertexElement[] vertexElements = ((IPlatformVertexDeclaration)vertexInputLayout.VertexDeclarations[i]).InternalVertexElements;

                    for (int v = 0; v < vertexElements.Length; v++)
                    {
                        D3D11.InputElement inputElement = GetInputElement(ref vertexElements[v], i, vertexInputLayout.InstanceFrequencies[i]);

                        list.Add(inputElement);
                    }
                }
                inputElements = list.ToArray();

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
            }

            try
            {
                inputLayout = new D3D11.InputLayout(_graphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _shaderByteCode, inputElements);
            }
            catch (DX.SharpDXException ex)
            {
                if (ex.Descriptor != DX.Result.InvalidArg)
                    throw;

                // If InputLayout ctor fails with InvalidArg then it's most likely because the 
                // vertex declaration doesn't match the vertex shader. 
                // Shader probably used the semantic "SV_Position" in the vertex shader input.
                // Background information:
                // "SV_Position" is a "system-value semantic" which is interpreted by the
                // rasterizer stage. This means it needs to be used in the vertex shader output
                // or the pixel shader input. (See
                // https://msdn.microsoft.com/en-us/library/windows/desktop/bb509647.aspx)
                //
                // However, some effects (notably the original XNA stock effects) use
                // "SV_Position" for the vertex shader input. This is technically allowed, but
                // rather uncommon and causes problems:
                // - XNA/MonoGame only has VertexElementUsage.Position, so there is no way to
                //   distinguish between "POSITION" and "SV_Position".
                // - "SV_Position" cannot be used with any index other than 0, i.e. the DirectX
                //   FX compiler does not accept "SV_Position1", "SV_Position2", ...
                //   This is a problem when using multiple vertex streams, e.g. for blend shape
                //   animations. It makes it impossible to correctly match the vertex
                //   declaration with the vertex shader signature.
                //
                // Conclusion:
                // - MonoGame needs to translate VertexElementUsage.Position to "POSITION".
                // - MonoGame effects should always use "POSITION" for vertex shader inputs.

                // Here is a workaround ("hack") for old vertex shaders which haven't been
                // updated: Rename "POSITION0" to "SV_Position" and try again.
                bool retry = false;
                for (int i = 0; i < inputElements.Length; i++)
                {
                    if (inputElements[i].SemanticIndex == 0 && inputElements[i].SemanticName.Equals("POSITION", StringComparison.OrdinalIgnoreCase))
                    {
                        inputElements[i].SemanticName = "SV_Position";
                        retry = true;
                        break;
                    }
                }

                if (!retry)
                    throw new InvalidOperationException(GetInvalidArgMessage(inputElements), ex);

                try
                {
                    inputLayout = new D3D11.InputLayout(_graphicsDeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, _shaderByteCode, inputElements);

                    // Workaround succeeded? This means that there is a vertex shader that needs
                    // to be updated.
#if DEBUG
                    if (_printWarning)
                    {
                        Debug.WriteLine(
                            "Warning: Vertex shader uses semantic 'SV_Position' for input register. " +
                            "Please update the shader and use the semantic 'POSITION0' instead. The " +
                            "semantic 'SV_Position' should only be used for the vertex shader output or " +
                            "pixel shader input!");
                        _printWarning = false;
                    }
#endif
                }
                catch (DX.SharpDXException)
                {
                    // Workaround failed.
                    throw new InvalidOperationException(GetInvalidArgMessage(inputElements), ex);
                }
            }

            _cache.Add(immutableVertexInputLayout, inputLayout);

            return inputLayout;
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
        internal static D3D11.InputElement GetInputElement(ref VertexElement vertexElement, int slot, int instanceFrequency)
        {
            D3D11.InputElement inputElement = new D3D11.InputElement();
            inputElement.SemanticName = ToDXSemanticName(vertexElement.VertexElementUsage);
            inputElement.SemanticIndex = vertexElement.UsageIndex;
            inputElement.Format = ToDXFormat(vertexElement.VertexElementFormat);
            inputElement.Slot = slot;
            inputElement.AlignedByteOffset = vertexElement.Offset;

            // Note that instancing is only supported in feature level 9.3 and above.
            inputElement.Classification = (instanceFrequency == 0)
                                     ? D3D11.InputClassification.PerVertexData
                                     : D3D11.InputClassification.PerInstanceData;
            inputElement.InstanceDataStepRate = instanceFrequency;

            return inputElement;
        }
   
        private static string ToDXSemanticName(VertexElementUsage vertexElementUsage)
        {
            switch (vertexElementUsage)
            {
                case VertexElementUsage.Position:
                    return "POSITION";
                case VertexElementUsage.Color:
                    return "COLOR";
                case VertexElementUsage.Normal:
                    return "NORMAL";
                case VertexElementUsage.TextureCoordinate:
                    return "TEXCOORD";
                case VertexElementUsage.BlendIndices:
                    return "BLENDINDICES";
                case VertexElementUsage.BlendWeight:
                    return "BLENDWEIGHT";
                case VertexElementUsage.Binormal:
                    return "BINORMAL";
                case VertexElementUsage.Tangent:
                    return "TANGENT";
                case VertexElementUsage.PointSize:
                    return "PSIZE";

                default:
                    throw new NotSupportedException("Unknown vertex element usage!");
            }
        }

        private static DXGI.Format ToDXFormat(VertexElementFormat vertexElementFormat)
        {
            switch (vertexElementFormat)
            {
                case VertexElementFormat.Single:
                    return DXGI.Format.R32_Float;
                case VertexElementFormat.Vector2:
                    return DXGI.Format.R32G32_Float;
                case VertexElementFormat.Vector3:
                    return DXGI.Format.R32G32B32_Float;
                case VertexElementFormat.Vector4:
                    return DXGI.Format.R32G32B32A32_Float;
                case VertexElementFormat.Color:
                    return DXGI.Format.R8G8B8A8_UNorm;
                case VertexElementFormat.Byte4:
                    return DXGI.Format.R8G8B8A8_UInt;
                case VertexElementFormat.Short2:
                    return DXGI.Format.R16G16_SInt;
                case VertexElementFormat.Short4:
                    return DXGI.Format.R16G16B16A16_SInt;
                case VertexElementFormat.NormalizedShort2:
                    return DXGI.Format.R16G16_SNorm;
                case VertexElementFormat.NormalizedShort4:
                    return DXGI.Format.R16G16B16A16_SNorm;
                case VertexElementFormat.HalfVector2:
                    return DXGI.Format.R16G16_Float;
                case VertexElementFormat.HalfVector4:
                    return DXGI.Format.R16G16B16A16_Float;

                default:
                    throw new NotSupportedException("Unknown vertex element format!");
            }
        }

        /// <summary>
        /// Gets a more helpful message for the SharpDX invalid arg error.
        /// </summary>
        /// <param name="inputElements">The input elements.</param>
        /// <returns>The exception message.</returns>
        private static string GetInvalidArgMessage(D3D11.InputElement[] inputElements)
        {
            string elements = string.Join(", ", inputElements.Select(x => x.SemanticName + x.SemanticIndex));
            return "An error occurred while preparing to draw. "
                   + "This is probably because the current vertex declaration does not include all the elements "
                   + "required by the current vertex shader. The current vertex declaration includes these elements: "
                   + elements + ".";
        }
    }
}
