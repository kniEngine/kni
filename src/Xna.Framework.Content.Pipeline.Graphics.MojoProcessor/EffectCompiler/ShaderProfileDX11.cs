// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3D = SharpDX.Direct3D;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class ShaderProfileDX11 : ShaderProfile
    {
        public override ShaderProfileType ProfileType { get { return ShaderProfileType.DirectX_11; } }
        public override string Name { get { return "DirectX_11"; } }

        public ShaderProfileDX11()
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
            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                ShaderVersion vsShaderVersion = ShaderVersion.ParseVertexShaderModel(pass.vsModel);
                
                if (vsShaderVersion.Major == -1)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}'.", pass.vsModel, pass.vsFunction));

                if (vsShaderVersion.Major < 2)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be at least SM 2.0.", pass.vsModel, pass.vsFunction));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                ShaderVersion psShaderVersion = ShaderVersion.ParsePixelShaderModel(pass.psModel);

                if (psShaderVersion.Major == -1)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}'.", pass.psModel, pass.psFunction));

                if (psShaderVersion.Major < 2)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be at least SM 2.0.", pass.psModel, pass.psFunction));
            }
        }

        internal override ShaderData CreateShader(EffectContent input, ContentProcessorContext context, EffectObject effect, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderStage shaderStage, ref string errorsAndWarnings)
        {
            string dx11ShaderProfileName = shaderProfileName;
            dx11ShaderProfileName = dx11ShaderProfileName.Replace("s_2_0", "s_4_0_level_9_1");
            dx11ShaderProfileName = dx11ShaderProfileName.Replace("s_3_0", "s_4_0_level_9_3");
            using (D3DC.ShaderBytecode shaderBytecodeDX11 = ShaderProfile.CompileHLSL(input, context, fullFilePath, fileContent, debugMode, shaderFunction, dx11ShaderProfileName, true, ref errorsAndWarnings))
            {
                ShaderData shaderDataDX11 = ShaderProfileDX11.CreateHLSL(input, context, shaderInfo, shaderBytecodeDX11, shaderStage, effect.ConstantBuffers, effect.Shaders.Count, debugMode);
                return shaderDataDX11;
            }
        }

        private static ShaderData CreateHLSL(EffectContent input, ContentProcessorContext context, ShaderInfo shaderInfo, D3DC.ShaderBytecode shaderBytecodeDX11, ShaderStage shaderStage, List<ConstantBufferData> cbuffers, int sharedIndex, EffectProcessorDebugMode debugMode)
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
            using (D3DC.ShaderReflection shaderReflectionDX11 = new D3DC.ShaderReflection(shaderBytecodeDX11.Data))
            {
                LogDX11ShaderReflection(shaderReflectionDX11);

                List<D3DC.InputBindingDescription> samplersMap = new List<D3DC.InputBindingDescription>();
                List<D3DC.InputBindingDescription> texturesMap = new List<D3DC.InputBindingDescription>();

                for (int i = 0; i < shaderReflectionDX11.Description.BoundResources; i++)
                {
                    D3DC.InputBindingDescription ibDesc = shaderReflectionDX11.GetResourceBindingDescription(i);
                    switch (ibDesc.Type)
                    {
                        case D3DC.ShaderInputType.Sampler:
                            samplersMap.Add(ibDesc);
                            break;
                        case D3DC.ShaderInputType.Texture:
                            texturesMap.Add(ibDesc);
                            break;
                    }
                }

                // Get the textures.
                List<SamplerInfo> textures = new List<SamplerInfo>();
                foreach (D3DC.InputBindingDescription txDesc in texturesMap)
                {
                    // Init samplerInfo
                    SamplerInfo textureInfo = new SamplerInfo();
                    textureInfo.type = MojoShader.SamplerType.SAMPLER_UNKNOWN;
                    textureInfo.GLsamplerName = String.Empty;
                    textureInfo.samplerSlot = -1;
                    textureInfo.state = null;
                    textureInfo.textureSlot = -1;
                    textureInfo.textureName = null;
                    textureInfo.textureParameter = -1;

                    textureInfo.textureSlot = txDesc.BindPoint;
                    textureInfo.textureName = txDesc.Name;
                    textureInfo.type = DXToSamplerType(txDesc.Dimension);

                    textures.Add(textureInfo);
                }

                // Get the samplers.
                List<SamplerInfo> samplers = new List<SamplerInfo>();
                foreach (D3DC.InputBindingDescription samplerDesc in samplersMap)
                {
                    // Init samplerInfo
                    SamplerInfo samplerInfo = new SamplerInfo();
                    samplerInfo.type = MojoShader.SamplerType.SAMPLER_UNKNOWN;
                    samplerInfo.GLsamplerName = String.Empty;
                    samplerInfo.samplerSlot = -1;
                    samplerInfo.state = null;
                    samplerInfo.textureSlot = -1;
                    samplerInfo.textureName = null;
                    samplerInfo.textureParameter = -1;

                    samplerInfo.samplerSlot = samplerDesc.BindPoint;
                    samplerInfo.GLsamplerName = samplerDesc.Name;

                    SamplerStateInfo samplerStateInfo = shaderInfo.SamplerStates[samplerDesc.Name];
                    samplerInfo.textureName = samplerStateInfo.TextureName;
                    samplerInfo.state = samplerStateInfo.State;

                    samplers.Add(samplerInfo);
                }

                // merge paired samples & textures
                // & resolve textureName from sample.
                for(int t = 0; t< textures.Count; t++)
                {
                    SamplerInfo textureInfo = textures[t];

                    for (int s = 0; s < samplers.Count; s++)
                    {
                        if (textureInfo.textureName == samplers[s].GLsamplerName)
                        {
                            if (samplers[s].textureName != null)
                                textureInfo.textureName = samplers[s].textureName;

                            textureInfo.samplerSlot = samplers[s].samplerSlot;
                            if (samplers[s].state != null)
                            {
                                textureInfo.state = samplers[s].state;
                            }

                            samplers.RemoveAt(s--);
                            break;
                        }
                    }

                    textures[t] = textureInfo; // update struct
                }

                // merge remaining samples into textures 
                for (int t = 0; t < textures.Count; t++)
                {
                    SamplerInfo textureInfo = textures[t];

                    for (int s = 0; s < samplers.Count; s++)
                    {
                        if (textureInfo.samplerSlot == -1)
                        {
                            textureInfo.samplerSlot = samplers[s].samplerSlot;
                            if (samplers[s].state != null)
                            {
                                textureInfo.state = samplers[s].state;
                            }

                            samplers.RemoveAt(s--);
                            break;
                        }
                    }

                    textures[t] = textureInfo; // update struct
                }

                if (samplers.Count > 0)
                    throw new InvalidOperationException("samplers not merged.");

                dxshader._samplers = textures.ToArray();

                // Gather all the constant buffers used by this shader.
                ConstantBufferData[] cbuffersData = GetDX11ConstantBuffers(shaderReflectionDX11);

                // map ConstantBuffers
                int[] cbindexmap = new int[cbuffersData.Length];
                for (int i = 0; i < cbuffersData.Length; i++)
                {
                    // Look for a duplicate cbuffer in the list.
                    int cbindex = 0;
                    for (; cbindex < cbuffers.Count; cbindex++)
                        if (cbuffersData[i].SameAs(cbuffers[cbindex]))
                            break;
                    // Add a new cbuffer.
                    if (cbindex == cbuffers.Count)
                        cbuffers.Add(cbuffersData[i]);

                    cbindexmap[i] = cbindex;
                }
                dxshader._cbuffers = cbindexmap;
            }

            return dxshader;
        }

        private static MojoShader.SamplerType DXToSamplerType(D3D.ShaderResourceViewDimension dimension)
        {
            switch (dimension)
            {
                case D3D.ShaderResourceViewDimension.Texture1D:
                case D3D.ShaderResourceViewDimension.Texture1DArray:
                    return MojoShader.SamplerType.SAMPLER_1D;

                case D3D.ShaderResourceViewDimension.Texture2D:
                case D3D.ShaderResourceViewDimension.Texture2DArray:
                case D3D.ShaderResourceViewDimension.Texture2DMultisampled:
                case D3D.ShaderResourceViewDimension.Texture2DMultisampledArray:
                    return MojoShader.SamplerType.SAMPLER_2D;

                case D3D.ShaderResourceViewDimension.Texture3D:
                    return MojoShader.SamplerType.SAMPLER_VOLUME;

                case D3D.ShaderResourceViewDimension.TextureCube:
                case D3D.ShaderResourceViewDimension.TextureCubeArray:
                    return MojoShader.SamplerType.SAMPLER_CUBE;

                default:
                    throw new InvalidOperationException();
            }
        }

    }
}
