// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3D = SharpDX.Direct3D;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class DirectX11ShaderProfile : ShaderProfile
    {
        private static readonly Regex HlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_3))?$", RegexOptions.Compiled);
        private static readonly Regex HlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_3))?$", RegexOptions.Compiled);

        public DirectX11ShaderProfile()
            : base("DirectX_11", ShaderProfileType.DirectX_11)
        {
        }

        internal override IEnumerable<KeyValuePair<string, string>> GetMacros()
        {
            yield return new KeyValuePair<string, string>("__DIRECTX__", "1");

            // deprecated macros. Left for backward compatibility with MonoGame.
            yield return new KeyValuePair<string, string>("HLSL", "1");
            yield return new KeyValuePair<string, string>("SM4", "1");
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                ParseShaderModel(pass.vsModel, HlslVertexShaderRegex, out major, out minor);
                if (major <= 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 4.0 level 9.1 or higher!", pass.vsModel, pass.vsFunction));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                ParseShaderModel(pass.psModel, HlslPixelShaderRegex, out major, out minor);
                if (major <= 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 4.0 level 9.1 or higher!", pass.vsModel, pass.psFunction));
            }
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, ShaderStage shaderStage, EffectObject effect, ref string errorsAndWarnings)
        {
            ShaderInfo shaderInfo = shaderResult.ShaderInfo;

            System.Diagnostics.Debug.Assert(shaderResult.Profile.ProfileType == ShaderProfileType.DirectX_11);

            using (D3DC.ShaderBytecode shaderBytecodeDX11 = EffectObject.CompileHLSL(shaderResult, shaderFunction, shaderProfile, true, ref errorsAndWarnings))
            {
                ShaderData shaderDataDX11 = DirectX11ShaderProfile.CreateHLSL(shaderBytecodeDX11, shaderStage, effect.ConstantBuffers, effect.Shaders.Count, shaderInfo.SamplerStates, shaderResult.Debug);
                return shaderDataDX11;
            }
        }

        private static ShaderData CreateHLSL(D3DC.ShaderBytecode shaderBytecodeDX11, ShaderStage shaderStage, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, EffectProcessorDebugMode debugMode)
        {
            ShaderData dxshader = new ShaderData(shaderStage, sharedIndex);
            dxshader._attributes = new ShaderData.Attribute[0];

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

                        SamplerInfo samplerInfo = new SamplerInfo();
                        samplerInfo.textureSlot = rdesc.BindPoint;
                        samplerInfo.samplerSlot = rdesc.BindPoint;

                        // default to String.Empty for DX.
                        samplerInfo.GLsamplerName = String.Empty;

                        samplerInfo.textureName = samplerName;

                        SamplerStateInfo state;
                        if (samplerStates.TryGetValue(samplerName, out state))
                        {
                            samplerInfo.state = state.State;

                            if (state.TextureName != null)
                                samplerInfo.textureName = state.TextureName;
                        }
                        else
                        {
                            foreach (SamplerStateInfo s in samplerStates.Values)
                            {
                                if (samplerName == s.TextureName)
                                {
                                    samplerInfo.state = s.State;
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
                                samplerInfo.samplerSlot = samplerrdesc.BindPoint;
                                break;
                            }
                        }

                        switch (rdesc.Dimension)
                        {
                            case D3D.ShaderResourceViewDimension.Texture1D:
                            case D3D.ShaderResourceViewDimension.Texture1DArray:
                                samplerInfo.type = MojoShader.SamplerType.SAMPLER_1D;
                                break;

                            case D3D.ShaderResourceViewDimension.Texture2D:
                            case D3D.ShaderResourceViewDimension.Texture2DArray:
                            case D3D.ShaderResourceViewDimension.Texture2DMultisampled:
                            case D3D.ShaderResourceViewDimension.Texture2DMultisampledArray:
                                samplerInfo.type = MojoShader.SamplerType.SAMPLER_2D;
                                break;

                            case D3D.ShaderResourceViewDimension.Texture3D:
                                samplerInfo.type = MojoShader.SamplerType.SAMPLER_VOLUME;
                                break;

                            case D3D.ShaderResourceViewDimension.TextureCube:
                            case D3D.ShaderResourceViewDimension.TextureCubeArray:
                                samplerInfo.type = MojoShader.SamplerType.SAMPLER_CUBE;
                                break;
                        }

                        samplers.Add(samplerInfo);
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
