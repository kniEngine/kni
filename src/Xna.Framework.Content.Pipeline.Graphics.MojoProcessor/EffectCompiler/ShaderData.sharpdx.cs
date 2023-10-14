using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3D = SharpDX.Direct3D;
using D3DC = SharpDX.D3DCompiler;

// Copyright (C)2022 Nick Kastellanos

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal partial class ShaderData
    {
        public static ShaderData CreateHLSL(D3DC.ShaderBytecode shaderBytecodeDX11, ShaderStage shaderStage, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, EffectProcessorDebugMode debugMode)
        {
            ShaderData dxshader = new ShaderData(shaderStage, sharedIndex);
            dxshader._attributes = new Attribute[0];

            // Strip the bytecode we're gonna save!
            D3DC.StripFlags stripFlags = D3DC.StripFlags.CompilerStripReflectionData |
                                         D3DC.StripFlags.CompilerStripTestBlobs;

            if (debugMode != EffectProcessorDebugMode.Debug)
                stripFlags |= D3DC.StripFlags.CompilerStripDebugInformation;

            // Strip the bytecode for saving to disk.
            D3DC.ShaderBytecode stripped = shaderBytecodeDX11.Strip(stripFlags);
            {
                // Only SM4 and above works with strip... so this can return null!
                if (stripped != null)
                {
                    dxshader.ShaderCode = stripped;
                }
                else
                {
                    // TODO: There is a way to strip SM3 and below
                    // but we have to write the method ourselves.
                    // 
                    // If we need to support it then consider porting
                    // this code over...
                    //
                    // http://entland.homelinux.com/blog/2009/01/15/stripping-comments-from-shader-bytecodes/
                    //
                    dxshader.ShaderCode = (byte[])shaderBytecodeDX11.Data.Clone();
                }
            }

            // Use reflection to get details of the shader.
            using (D3DC.ShaderReflection refelect = new D3DC.ShaderReflection(shaderBytecodeDX11.Data))
            {
                // Get the samplers.
                List<SamplerInfo> samplers = new List<SamplerInfo>();
                for (int i = 0; i < refelect.Description.BoundResources; i++)
                {
                    D3DC.InputBindingDescription rdesc = refelect.GetResourceBindingDescription(i);
                    if (rdesc.Type == D3DC.ShaderInputType.Texture)
                    {
                        string samplerName = rdesc.Name;

                        SamplerInfo sampler = new SamplerInfo();
                        sampler.textureSlot = rdesc.BindPoint;
                        sampler.samplerSlot = rdesc.BindPoint;

                        // default to String.Empty for DX.
                        sampler.GLsamplerName = String.Empty;

                        sampler.textureName = samplerName;
                            
                        SamplerStateInfo state;
                        if (samplerStates.TryGetValue(samplerName, out state))
                        {
                            sampler.state = state.State;

                            if (state.TextureName != null)
                                sampler.textureName = state.TextureName;
                        }
                        else
                        {
                            foreach (SamplerStateInfo s in samplerStates.Values)
                            {
                                if (samplerName == s.TextureName)
                                {
                                    sampler.state = s.State;
                                    samplerName = s.Name;
                                    break;
                                }
                            }
                        }

                        // Find sampler slot, which can be different from the texture slot.
                        for (int j = 0; j < refelect.Description.BoundResources; j++)
                        {
                            D3DC.InputBindingDescription samplerrdesc = refelect.GetResourceBindingDescription(j);

                            if (samplerrdesc.Type == D3DC.ShaderInputType.Sampler && 
                                samplerrdesc.Name == samplerName)
                            {
                                sampler.samplerSlot = samplerrdesc.BindPoint;
                                break;
                            }
                        }

                        switch (rdesc.Dimension)
                        {
                            case D3D.ShaderResourceViewDimension.Texture1D:
                            case D3D.ShaderResourceViewDimension.Texture1DArray:
                                sampler.type = MojoShader.SamplerType.SAMPLER_1D;
                                break;

                            case D3D.ShaderResourceViewDimension.Texture2D:
                            case D3D.ShaderResourceViewDimension.Texture2DArray:
                            case D3D.ShaderResourceViewDimension.Texture2DMultisampled:
                            case D3D.ShaderResourceViewDimension.Texture2DMultisampledArray:
                                sampler.type = MojoShader.SamplerType.SAMPLER_2D;
                                break;

                            case D3D.ShaderResourceViewDimension.Texture3D:
                                sampler.type = MojoShader.SamplerType.SAMPLER_VOLUME;
                                break;

                            case D3D.ShaderResourceViewDimension.TextureCube:
                            case D3D.ShaderResourceViewDimension.TextureCubeArray:
                                sampler.type = MojoShader.SamplerType.SAMPLER_CUBE;
                                break;
                        }

                        samplers.Add(sampler);
                    }
                }
                dxshader._samplers = samplers.ToArray();

                // Gather all the constant buffers used by this shader.
                AddConstantBuffers(cbuffers, dxshader, refelect);
            }

            return dxshader;
        }

        private static void AddConstantBuffers(List<ConstantBufferData> cbuffers, ShaderData dxshader, D3DC.ShaderReflection refelect)
        {
            dxshader._cbuffers = new int[refelect.Description.ConstantBuffers];
            for (int i = 0; i < refelect.Description.ConstantBuffers; i++)
            {
                ConstantBufferData cb = new ConstantBufferData(refelect.GetConstantBuffer(i));

                // Look for a duplicate cbuffer in the list.
                for (int c = 0; c < cbuffers.Count; c++)
                {
                    if (cb.SameAs(cbuffers[c]))
                    {
                        cb = null;
                        dxshader._cbuffers[i] = c;
                        break;
                    }
                }

                // Add a new cbuffer.
                if (cb != null)
                {
                    dxshader._cbuffers[i] = cbuffers.Count;
                    cbuffers.Add(cb);
                }
            }
        }
    }
}
