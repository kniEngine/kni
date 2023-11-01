// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3D = SharpDX.Direct3D;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class ShaderProfileDX11 : ShaderProfile
    {
        public override ShaderProfileType ProfileType { get { return ShaderProfileType.DirectX_11; } }
        public override string Name { get { return "DirectX_11"; } }


        private static readonly Regex HlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_3))?$", RegexOptions.Compiled);
        private static readonly Regex HlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_3))?$", RegexOptions.Compiled);

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

        internal override ShaderData CreateShader(EffectObject effect, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderStage shaderStage, ref string errorsAndWarnings)
        {
            using (D3DC.ShaderBytecode shaderBytecodeDX11 = ShaderProfile.CompileHLSL(fullFilePath, fileContent, debugMode, shaderFunction, shaderProfileName, true, ref errorsAndWarnings))
            {
                ShaderData shaderDataDX11 = ShaderProfileDX11.CreateHLSL(shaderInfo, shaderBytecodeDX11, shaderStage, effect.ConstantBuffers, effect.Shaders.Count, debugMode);
                return shaderDataDX11;
            }
        }

        private static ShaderData CreateHLSL(ShaderInfo shaderInfo, D3DC.ShaderBytecode shaderBytecodeDX11, ShaderStage shaderStage, List<ConstantBufferData> cbuffers, int sharedIndex, EffectProcessorDebugMode debugMode)
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
            using (D3DC.ShaderReflection shaderReflection = new D3DC.ShaderReflection(shaderBytecodeDX11.Data))
            {
                LogShaderReflection(shaderReflection);

                List<D3DC.InputBindingDescription> samplersMap = new List<D3DC.InputBindingDescription>();
                List<D3DC.InputBindingDescription> texturesMap = new List<D3DC.InputBindingDescription>();

                for (int i = 0; i < shaderReflection.Description.BoundResources; i++)
                {
                    D3DC.InputBindingDescription ibDesc = shaderReflection.GetResourceBindingDescription(i);
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

                // Get the samplers.
                List<SamplerInfo> samplers = new List<SamplerInfo>();
                foreach(D3DC.InputBindingDescription txDesc in texturesMap)
                {
                    string samplerName = txDesc.Name;

                    SamplerInfo samplerInfo = new SamplerInfo();
                    samplerInfo.textureSlot = txDesc.BindPoint;
                    samplerInfo.samplerSlot = txDesc.BindPoint;

                    // default to String.Empty for DX.
                    samplerInfo.GLsamplerName = String.Empty;

                    samplerInfo.textureName = samplerName;

                    SamplerStateInfo state;
                    if (shaderInfo.SamplerStates.TryGetValue(samplerName, out state))
                    {
                        samplerInfo.state = state.State;

                        if (state.TextureName != null)
                            samplerInfo.textureName = state.TextureName;
                    }
                    else
                    {
                        foreach (SamplerStateInfo s in shaderInfo.SamplerStates.Values)
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
                    foreach(D3DC.InputBindingDescription samplerDesc in samplersMap)
                    {
                        if (samplerDesc.Name == samplerName)
                        {
                            samplerInfo.samplerSlot = samplerDesc.BindPoint;
                            break;
                        }
                    }

                    switch (txDesc.Dimension)
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
                dxshader._samplers = samplers.ToArray();

                // Gather all the constant buffers used by this shader.
                AddConstantBuffers(cbuffers, dxshader, shaderReflection);
            }

            return dxshader;
        }

        [Conditional("DEBUG")]
        private static void LogShaderReflection(D3DC.ShaderReflection shaderReflection)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LogShaderReflection ");

            for (int r = 0; r < shaderReflection.Description.BoundResources; r++)
            {
                D3DC.InputBindingDescription ibDesc = shaderReflection.GetResourceBindingDescription(r);

                sb.AppendLine("");
                sb.AppendLine("ResourceBindingDescription: #" + r);
                sb.AppendLine("Name: '" + ibDesc.Name+"'");
                sb.AppendLine("Type: " + ibDesc.Type);
                sb.AppendLine("BindPoint: " + ibDesc.BindPoint);
                sb.AppendLine("BindCount: " + ibDesc.BindCount);
                sb.AppendLine("Flags: " + ibDesc.Flags);
                sb.AppendLine("ReturnType: " + ibDesc.ReturnType);
                sb.AppendLine("Dimension: " + ibDesc.Dimension);
            }

            for (int i = 0; i < shaderReflection.Description.InputParameters; i++)
            {
                D3DC.ShaderParameterDescription paramDesc = shaderReflection.GetInputParameterDescription(i);

                sb.AppendLine("");
                sb.AppendLine("InputParameterDescription: #" + i);
                sb.AppendLine("SemanticName: '" + paramDesc.SemanticName + "'");
                sb.AppendLine("SemanticIndex: " + paramDesc.SemanticIndex);
                sb.AppendLine("Register: " + paramDesc.Register);
                sb.AppendLine("SystemValueType: " + paramDesc.SystemValueType);
                sb.AppendLine("UsageMask: " + paramDesc.UsageMask);
            }

            for (int o = 0; o < shaderReflection.Description.OutputParameters; o++)
            {
                D3DC.ShaderParameterDescription paramDesc = shaderReflection.GetOutputParameterDescription(o);

                sb.AppendLine("");
                sb.AppendLine("OutputParameterDescription: #" + o);
                sb.AppendLine("SemanticName: '" + paramDesc.SemanticName + "'");
                sb.AppendLine("SemanticIndex: " + paramDesc.SemanticIndex);
                sb.AppendLine("Register: " + paramDesc.Register);
                sb.AppendLine("SystemValueType: " + paramDesc.SystemValueType);
                sb.AppendLine("UsageMask: " + paramDesc.UsageMask);
            }
            
            for (int c = 0; c < shaderReflection.Description.ConstantBuffers; c++)
            {
                D3DC.ConstantBuffer cbuffer = shaderReflection.GetConstantBuffer(c);

                sb.AppendLine("");
                sb.AppendLine("ConstantBuffer: #" + c);
                sb.AppendLine("Tag: " + cbuffer.Tag);
                sb.AppendLine("Name: '" + cbuffer.Description.Name + "'");
                sb.AppendLine("Size: " + cbuffer.Description.Size);
                sb.AppendLine("Flags: " + cbuffer.Description.Flags);
                for (int v = 0; v < cbuffer.Description.VariableCount; v++)
                {
                    D3DC.ShaderReflectionVariable variable = cbuffer.GetVariable(v);
                    D3DC.ShaderReflectionType type = variable.GetVariableType();

                    sb.AppendLine("");
                    sb.AppendLine("  Variable: #" + v);
                    sb.AppendLine("  Name: '" + variable.Description.Name + "'");
                    sb.AppendLine("  Flags: " + variable.Description.Flags);
                    sb.AppendLine("  TypeClass: " + type.Description.Class);
                    sb.AppendLine("  StartOffset: " + variable.Description.StartOffset);
                    sb.AppendLine("  Size: " + variable.Description.Size);
                    sb.AppendLine("  StartSampler: " + variable.Description.StartSampler);
                    sb.AppendLine("  SamplerSize: " + variable.Description.SamplerSize);
                    sb.AppendLine("  StartTexture: " + variable.Description.StartTexture);
                    sb.AppendLine("  TextureSize: " + variable.Description.TextureSize);
                }
            }

            for (int p = 0; p < shaderReflection.Description.PatchConstantParameters; p++)
            {
                D3DC.ShaderParameterDescription paramDesc = shaderReflection.GetPatchConstantParameterDescription(p);

                sb.AppendLine("");
                sb.AppendLine("PatchConstantParameterDescription: #" + p);
                sb.AppendLine("SemanticName: '" + paramDesc.SemanticName + "'");
                sb.AppendLine("SemanticIndex: " + paramDesc.SemanticIndex);
                sb.AppendLine("Register: " + paramDesc.Register);
                sb.AppendLine("SystemValueType: " + paramDesc.SystemValueType);
                sb.AppendLine("UsageMask: " + paramDesc.UsageMask);
            }

            sb.AppendLine("");

            string msg = sb.ToString();
            Debug.WriteLine(msg);

            return;
        }

        private static void AddConstantBuffers(List<ConstantBufferData> cbuffers, ShaderData dxshader, D3DC.ShaderReflection shaderReflection)
        {
            dxshader._cbuffers = new int[shaderReflection.Description.ConstantBuffers];
            for (int i = 0; i < shaderReflection.Description.ConstantBuffers; i++)
            {
                D3DC.ConstantBuffer d3dcbuffer = shaderReflection.GetConstantBuffer(i);
                ConstantBufferData cbufferData = ShaderProfileDX11.CreateConstantBufferData(d3dcbuffer);

                // Look for a duplicate cbuffer in the list.
                for (int c = 0; c < cbuffers.Count; c++)
                {
                    if (cbufferData.SameAs(cbuffers[c]))
                    {
                        cbufferData = null;
                        dxshader._cbuffers[i] = c;
                        break;
                    }
                }

                // Add a new cbuffer.
                if (cbufferData != null)
                {
                    dxshader._cbuffers[i] = cbuffers.Count;
                    cbuffers.Add(cbufferData);
                }
            }
        }


        private static ConstantBufferData CreateConstantBufferData(D3DC.ConstantBuffer d3dcbuffer)
        {
            ConstantBufferData cbufferData = new ConstantBufferData();

            cbufferData.Name = string.Empty;
            cbufferData.Size = d3dcbuffer.Description.Size;

            cbufferData.ParameterIndex = new List<int>();

            List<EffectObject.EffectParameterContent> parameters = new List<EffectObject.EffectParameterContent>();

            // Gather all the parameters.
            for (int i = 0; i < d3dcbuffer.Description.VariableCount; i++)
            {
                D3DC.ShaderReflectionVariable vdesc = d3dcbuffer.GetVariable(i);

                EffectObject.EffectParameterContent param = GetParameterFromType(vdesc.GetVariableType());

                param.name = vdesc.Description.Name;
                param.semantic = string.Empty;
                param.bufferOffset = vdesc.Description.StartOffset;

                uint size = param.columns * param.rows * 4;
                byte[] data = new byte[size];

                if (vdesc.Description.DefaultValue != IntPtr.Zero)
                    Marshal.Copy(vdesc.Description.DefaultValue, data, 0, (int)size);

                param.data = data;

                parameters.Add(param);
            }

            // Sort them by the offset for some consistent results.
            IEnumerable<EffectObject.EffectParameterContent> sortedParameters = parameters.OrderBy(e => e.bufferOffset);
            cbufferData.Parameters = sortedParameters.ToList();

            // Store the parameter offsets.
            cbufferData.ParameterOffset = new List<int>();
            foreach (EffectObject.EffectParameterContent param in cbufferData.Parameters)
                cbufferData.ParameterOffset.Add(param.bufferOffset);

            return cbufferData;
        }

        private static EffectObject.EffectParameterContent GetParameterFromType(D3DC.ShaderReflectionType type)
        {
            EffectObject.EffectParameterContent param = new EffectObject.EffectParameterContent();
            param.rows = (uint)type.Description.RowCount;
            param.columns = (uint)type.Description.ColumnCount;
            param.name = type.Description.Name ?? string.Empty;
            param.semantic = string.Empty;
            param.bufferOffset = type.Description.Offset;

            switch (type.Description.Class)
            {
                case D3DC.ShaderVariableClass.Scalar:
                    param.class_ = EffectObject.PARAMETER_CLASS.SCALAR;
                    break;

                case D3DC.ShaderVariableClass.Vector:
                    param.class_ = EffectObject.PARAMETER_CLASS.VECTOR;
                    break;

                case D3DC.ShaderVariableClass.MatrixColumns:
                    param.class_ = EffectObject.PARAMETER_CLASS.MATRIX_COLUMNS;
                    break;

                default:
                    throw new Exception("Unsupported parameter class!");
            }

            switch (type.Description.Type)
            {
                case D3DC.ShaderVariableType.Bool:
                    param.type = EffectObject.PARAMETER_TYPE.BOOL;
                    break;

                case D3DC.ShaderVariableType.Float:
                    param.type = EffectObject.PARAMETER_TYPE.FLOAT;
                    break;

                case D3DC.ShaderVariableType.Int:
                    param.type = EffectObject.PARAMETER_TYPE.INT;
                    break;

                default:
                    throw new Exception("Unsupported parameter type!");
            }

            param.member_count = (uint)type.Description.MemberCount;
            param.element_count = (uint)type.Description.ElementCount;

            if (param.member_count > 0)
            {
                param.member_handles = new EffectObject.EffectParameterContent[param.member_count];
                for (int i = 0; i < param.member_count; i++)
                {
                    EffectObject.EffectParameterContent mparam = GetParameterFromType(type.GetMemberType(i));
                    mparam.name = type.GetMemberTypeName(i) ?? string.Empty;
                    param.member_handles[i] = mparam;
                }
            }
            else
            {
                param.member_handles = new EffectObject.EffectParameterContent[param.element_count];
                for (int i = 0; i < param.element_count; i++)
                {
                    EffectObject.EffectParameterContent mparam = new EffectObject.EffectParameterContent();

                    mparam.name = string.Empty;
                    mparam.semantic = string.Empty;
                    mparam.type = param.type;
                    mparam.class_ = param.class_;
                    mparam.rows = param.rows;
                    mparam.columns = param.columns;
                    mparam.data = new byte[param.columns * param.rows * 4];

                    param.member_handles[i] = mparam;
                }
            }

            return param;
        }
    }
}
