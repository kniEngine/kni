// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class Effect
    {
        private class EffectReader09 : BinaryReader
        {
            private readonly GraphicsDevice graphicsDevice;
            private readonly MGFXHeader header;

            public EffectReader09(MemoryStream stream, GraphicsDevice graphicsDevice, MGFXHeader header) : base(stream)
            {
                this.header = header;
                this.graphicsDevice = graphicsDevice;
            }

            private int ReadPackedInt()
            {
                unchecked
                {
                    // read zigzag encoded int
                    int zzint = Read7BitEncodedInt();
                    return ((int)((uint)zzint >> 1) ^ (-(zzint & 1)));
                }
            }

            internal Effect ReadEffect()
            {
                var effect = new Effect(graphicsDevice);

                effect.ConstantBuffers = ReadConstantBuffers();
                effect._shaders = ReadShaders();
                effect.Parameters = ReadParameters();
                effect.Techniques = ReadTechniques(effect);

                effect.CurrentTechnique = effect.Techniques[0];

                return effect;
            }

            private ConstantBuffer[] ReadConstantBuffers()
            {
                var buffersCount = (int)ReadByte();
                var constantBuffers = new ConstantBuffer[buffersCount];
                for (var c = 0; c < buffersCount; c++)
                    constantBuffers[c] = ReadConstantBuffer();
                return constantBuffers;
            }

            ConstantBuffer ReadConstantBuffer()
            {
                var name = ReadString();

                // Create the backing system memory buffer.
                var sizeInBytes = (int)ReadInt16();

                // Read the parameter index values.
                var parameters = new int[ReadByte()];
                var offsets = new int[parameters.Length];
                for (var i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = (int)ReadByte();
                    offsets[i] = (int)ReadUInt16();
                }

                var buffer = new ConstantBuffer(graphicsDevice,
                                                sizeInBytes,
                                                parameters,
                                                offsets,
                                                name);
                return buffer;
            }

            private Shader[] ReadShaders()
            {
                var shadersCount = (int)ReadByte();
                var shaders = new Shader[shadersCount];
                for (var s = 0; s < shadersCount; s++)
                    shaders[s] = ReadShader();
                return shaders;
            }

            private Shader ReadShader()
            {
                return new Shader(graphicsDevice, this);
            }

            private EffectParameterCollection ReadParameters()
            {
                //TNC: fallback to version 8
                int count = (header.Version == 8)
                    ? (int)ReadByte()
                    : Read7BitEncodedInt();

                if (count == 0)
                    return EffectParameterCollection.Empty;

                var parameters = new EffectParameter[count];
                for (var i = 0; i < count; i++)
                {
                    var class_ = (EffectParameterClass)ReadByte();
                    var type = (EffectParameterType)ReadByte();
                    var name = ReadString();
                    var semantic = ReadString();
                    var annotations = ReadAnnotations();
                    var rowCount = (int)ReadByte();
                    var columnCount = (int)ReadByte();

                    var elements = ReadParameters();
                    var structMembers = ReadParameters();

                    object data = null;
                    if (elements.Count == 0 && structMembers.Count == 0)
                    {
                        switch (type)
                        {
                            case EffectParameterType.Bool:
                            case EffectParameterType.Int32:
#if !OPENGL
                                // Under most platforms we properly store integers and 
                                // booleans in an integer type.
                                //
                                // MojoShader on the otherhand stores everything in float
                                // types which is why this code is disabled under OpenGL.
					            {
					                var buffer = new int[rowCount * columnCount];								
                                    for (var j = 0; j < buffer.Length; j++)
                                        buffer[j] = ReadInt32();
                                    data = buffer;
                                    break;
					            }
#endif

                            case EffectParameterType.Single:
                                {
                                    var buffer = new float[rowCount * columnCount];
                                    for (var j = 0; j < buffer.Length; j++)
                                        buffer[j] = ReadSingle();
                                    data = buffer;
                                    break;
                                }

                            case EffectParameterType.String:
                                // TODO: We have not investigated what a string
                                // type should do in the parameter list.  Till then
                                // throw to let the user know.
                                throw new NotSupportedException();

                            default:
                                // NOTE: We skip over all other types as they 
                                // don't get added to the constant buffer.
                                break;
                        }
                    }

                    parameters[i] = new EffectParameter(
                        class_, type, name, rowCount, columnCount, semantic,
                        annotations, elements, structMembers, data);
                }

                return new EffectParameterCollection(parameters);
            }

            private EffectTechniqueCollection ReadTechniques(Effect effect)
            {
                var techniqueCount = (int)ReadByte();

                var techniques = new EffectTechnique[techniqueCount];
                for (var t = 0; t < techniqueCount; t++)
                {
                    var name = ReadString();
                    var annotations = ReadAnnotations();
                    var passes = ReadPasses(effect);
                    techniques[t] = new EffectTechnique(effect, name, passes, annotations);
                }

                return new EffectTechniqueCollection(techniques);
            }

            private EffectPassCollection ReadPasses(Effect effect)
            {
                var passesCount = (int)ReadByte();
                var passes = new EffectPass[passesCount];
                for (var i = 0; i < passesCount; i++)
                    ReadEffectPass(effect, passes, i);
                return new EffectPassCollection(passes);
            }

            private void ReadEffectPass(Effect effect, EffectPass[] passes, int i)
            {
                var name = ReadString();
                var annotations = ReadAnnotations();

                // Get the vertex and pixel shader.
                Shader vertexShader = null;
                Shader pixelShader = null;
                {
                    var vertexShaderIndex = (int)ReadByte();
                    var pixelShaderIndex = (int)ReadByte();
                    if (vertexShaderIndex != 255)
                        vertexShader = effect._shaders[vertexShaderIndex];
                    if (pixelShaderIndex != 255)
                        pixelShader = effect._shaders[pixelShaderIndex];
                }

                BlendState blend = ReadBoolean() ? ReadBlendState() : null;
                DepthStencilState depth = ReadBoolean() ? ReadDepthStencilState() : null;
                RasterizerState rasterizer = ReadBoolean() ? ReadRasterizerState() : null;

                passes[i] = new EffectPass(effect, name, vertexShader, pixelShader, blend, depth, rasterizer, annotations);
            }

            private BlendState ReadBlendState()
            {
                return new BlendState
                {
                    AlphaBlendFunction = (BlendFunction)ReadByte(),
                    AlphaDestinationBlend = (Blend)ReadByte(),
                    AlphaSourceBlend = (Blend)ReadByte(),
                    BlendFactor = new Color(ReadByte(), ReadByte(), ReadByte(), ReadByte()),
                    ColorBlendFunction = (BlendFunction)ReadByte(),
                    ColorDestinationBlend = (Blend)ReadByte(),
                    ColorSourceBlend = (Blend)ReadByte(),
                    ColorWriteChannels = (ColorWriteChannels)ReadByte(),
                    ColorWriteChannels1 = (ColorWriteChannels)ReadByte(),
                    ColorWriteChannels2 = (ColorWriteChannels)ReadByte(),
                    ColorWriteChannels3 = (ColorWriteChannels)ReadByte(),
                    MultiSampleMask = ReadInt32(),
                };
            }

            private DepthStencilState ReadDepthStencilState()
            {
                return new DepthStencilState
                {
                    CounterClockwiseStencilDepthBufferFail = (StencilOperation)ReadByte(),
                    CounterClockwiseStencilFail = (StencilOperation)ReadByte(),
                    CounterClockwiseStencilFunction = (CompareFunction)ReadByte(),
                    CounterClockwiseStencilPass = (StencilOperation)ReadByte(),
                    DepthBufferEnable = ReadBoolean(),
                    DepthBufferFunction = (CompareFunction)ReadByte(),
                    DepthBufferWriteEnable = ReadBoolean(),
                    ReferenceStencil = ReadInt32(),
                    StencilDepthBufferFail = (StencilOperation)ReadByte(),
                    StencilEnable = ReadBoolean(),
                    StencilFail = (StencilOperation)ReadByte(),
                    StencilFunction = (CompareFunction)ReadByte(),
                    StencilMask = ReadInt32(),
                    StencilPass = (StencilOperation)ReadByte(),
                    StencilWriteMask = ReadInt32(),
                    TwoSidedStencilMode = ReadBoolean(),
                };
            }

            private RasterizerState ReadRasterizerState()
            {
                return new RasterizerState
                {
                    CullMode = (CullMode)ReadByte(),
                    DepthBias = ReadSingle(),
                    FillMode = (FillMode)ReadByte(),
                    MultiSampleAntiAlias = ReadBoolean(),
                    ScissorTestEnable = ReadBoolean(),
                    SlopeScaleDepthBias = ReadSingle(),
                };
            }

            private EffectAnnotationCollection ReadAnnotations()
            {
                var count = (int)ReadByte();
                if (count == 0)
                    return EffectAnnotationCollection.Empty;

                var annotations = new EffectAnnotation[count];

                // TODO: Annotations are not implemented!

                return new EffectAnnotationCollection(annotations);
            }

        }
    }
}
