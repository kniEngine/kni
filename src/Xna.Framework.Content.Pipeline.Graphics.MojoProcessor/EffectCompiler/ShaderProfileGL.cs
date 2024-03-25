// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class ShaderProfileGL : ShaderProfile
    {
        public override ShaderProfileType ProfileType { get { return ShaderProfileType.OpenGL_Mojo; } }
        public override string Name { get { return "OpenGL"; } }


        private static readonly Regex GlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);
        private static readonly Regex GlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);

        public ShaderProfileGL()
        {
        }

        internal override IEnumerable<KeyValuePair<string, string>> GetMacros()
        {
            yield return new KeyValuePair<string, string>("__OPENGL__", "1");
            yield return new KeyValuePair<string, string>("__MOJOSHADER__", "1");

            // deprecated macros. Left for backward compatibility with MonoGame.
            yield return new KeyValuePair<string, string>("GLSL", "1");
            yield return new KeyValuePair<string, string>("OPENGL", "1");
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                ParseShaderModel(pass.vsModel, GlslVertexShaderRegex, out major, out minor);
                if (major > 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 3.0 or lower!", pass.vsModel, pass.vsFunction));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                ParseShaderModel(pass.psModel, GlslPixelShaderRegex, out major, out minor);
                if (major > 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 3.0 or lower!", pass.vsModel, pass.psFunction));
            }
        }

        internal override ShaderData CreateShader(EffectContent input, ContentProcessorContext context, EffectObject effect, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderStage shaderStage, ref string errorsAndWarnings)
        {
            // For now GLSL is only supported via translation
            // using MojoShader which works from DX9 HLSL bytecode.
            string dx9ShaderProfileName = shaderProfileName;
            dx9ShaderProfileName = dx9ShaderProfileName.Replace("s_4_0_level_9_1", "s_2_0");
            dx9ShaderProfileName = dx9ShaderProfileName.Replace("s_4_0_level_9_3", "s_3_0");
            using (D3DC.ShaderBytecode shaderBytecodeDX9 = ShaderProfile.CompileHLSL(input, context, fullFilePath, fileContent, debugMode, shaderFunction, dx9ShaderProfileName, false, ref errorsAndWarnings))
            {
                ShaderData shaderDataDX9 = ShaderProfileGL.CreateGLSL(input, context, shaderInfo, shaderBytecodeDX9, shaderStage, effect.ConstantBuffers, effect.Shaders.Count, debugMode);
                return shaderDataDX9;
            }
        }

        private static ShaderData CreateGLSL(EffectContent input, ContentProcessorContext context, ShaderInfo shaderInfo, D3DC.ShaderBytecode shaderBytecodeDX9, ShaderStage shaderStage, List<ConstantBufferData> cbuffers, int sharedIndex, EffectProcessorDebugMode debugMode)
        {
            ShaderData dxshader = new ShaderData(shaderStage, sharedIndex);

            // Use MojoShader to convert the HLSL bytecode to GLSL.

            IntPtr parseDataPtr = MojoShader.NativeMethods.Parse(
                MojoShader.NativeConstants.PROFILE_GLSL,
                shaderBytecodeDX9.Data,
                shaderBytecodeDX9.Data.Length,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                0,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            MojoShader.ParseData parseData = MarshalHelper.Unmarshal<MojoShader.ParseData>(parseDataPtr);
            if (parseData.error_count > 0)
            {
                MojoShader.Error[] errors = MarshalHelper.UnmarshalArray<MojoShader.Error>(
                    parseData.errors,
                    parseData.error_count
                );
                throw new Exception(errors[0].error);
            }

            // Conver the attributes.
            //
            // TODO: Could this be done using DX shader reflection?
            //
            {
                MojoShader.Attribute[] attributes = MarshalHelper.UnmarshalArray<MojoShader.Attribute>(
                        parseData.attributes, parseData.attribute_count);

                dxshader._attributes = new ShaderData.Attribute[attributes.Length];
                for (int i = 0; i < attributes.Length; i++)
                {
                    dxshader._attributes[i].name = attributes[i].name;
                    dxshader._attributes[i].index = attributes[i].index;
                    dxshader._attributes[i].usage = EffectObject.ToXNAVertexElementUsage(attributes[i].usage);
                }
            }

            MojoShader.Symbol[] symbols = MarshalHelper.UnmarshalArray<MojoShader.Symbol>(
                    parseData.symbols, parseData.symbol_count);

            //try to put the symbols in the order they are eventually packed into the uniform arrays
            //this /should/ be done by pulling the info from mojoshader
            Array.Sort(symbols, delegate (MojoShader.Symbol a, MojoShader.Symbol b)
            {
                uint va = a.register_index;
                if (a.info.elements == 1)
                    va += 1024; //hax. mojoshader puts array objects first
                uint vb = b.register_index;
                if (b.info.elements == 1)
                    vb += 1024;
                return va.CompareTo(vb);
            }
            );//(a, b) => ((int)(a.info.elements > 1))a.register_index.CompareTo(b.register_index));

            // NOTE: It seems the latest versions of MojoShader only 
            // output vec4 register sets.  We leave the code below, but
            // the runtime has been optimized for this case.

            // For whatever reason the register indexing is 
            // incorrect from MojoShader.
            {
                uint bool_index = 0;
                uint float4_index = 0;
                uint int4_index = 0;

                for (int i = 0; i < symbols.Length; i++)
                {
                    switch (symbols[i].register_set)
                    {
                        case MojoShader.SymbolRegisterSet.BOOL:
                            symbols[i].register_index = bool_index;
                            bool_index += symbols[i].register_count;
                            break;

                        case MojoShader.SymbolRegisterSet.FLOAT4:
                            symbols[i].register_index = float4_index;
                            float4_index += symbols[i].register_count;
                            break;

                        case MojoShader.SymbolRegisterSet.INT4:
                            symbols[i].register_index = int4_index;
                            int4_index += symbols[i].register_count;
                            break;
                    }
                }
            }

            // Get the samplers.
            MojoShader.Sampler[] samplers = MarshalHelper.UnmarshalArray<MojoShader.Sampler>(
                    parseData.samplers, parseData.sampler_count);
            dxshader._samplers = new SamplerInfo[samplers.Length];
            for (int i = 0; i < samplers.Length; i++)
            {
                // We need the original sampler name... look for that in the symbols.
                MojoShader.Symbol symbol = symbols.First(e => e.register_set == MojoShader.SymbolRegisterSet.SAMPLER &&
                                                  e.register_index == samplers[i].index);
                string samplerName = symbol.name;

                SamplerInfo samplerInfo = new SamplerInfo();
                samplerInfo.textureSlot = samplers[i].index;
                samplerInfo.samplerSlot = samplers[i].index;

                // GLSL needs the uniform sampler name.
                samplerInfo.GLsamplerName = samplers[i].name;

                //sampler mapping to parameter is unknown atm
                samplerInfo.textureParameter = -1;
                // By default use the original sampler name for the parameter name.
                samplerInfo.textureName = samplerName;

                samplerInfo.type = samplers[i].type;


                string textureName = null;
                if (samplerName.Contains("+"))
                {
                    int plusIndex = samplerName.IndexOf('+');
                    textureName = samplerName.Substring(plusIndex + 1);
                    samplerName = samplerName.Substring(0, plusIndex);
                }

                SamplerStateInfo state;
                if (shaderInfo.SamplerStates.TryGetValue(samplerName, out state))
                {
                    samplerInfo.state = state.State;

                    if (textureName != null)
                        samplerInfo.textureName = textureName;
                    else if (state.TextureName != null)
                        samplerInfo.textureName = state.TextureName;
                }

                // Store the sampler.
                dxshader._samplers[i] = samplerInfo;
            }

            // Gather all the parameters used by this shader.
            AddConstantBuffers(cbuffers, dxshader, symbols);

            string glslCode = parseData.output;

            // TODO: This sort of sucks... why does MojoShader not produce
            // code valid for GLES out of the box?

            // GLES platforms do not like this.
            glslCode = glslCode.Replace("#version 110", "");

            // Add the required precision specifiers for GLES.

            string floatPrecision = dxshader.Stage == ShaderStage.Vertex ? "precision highp float;\r\n" : "precision mediump float;\r\n";

            glslCode = "#ifdef GL_ES\r\n" +
                 floatPrecision +
                "precision mediump int;\r\n" +
                "#endif\r\n" +
                glslCode;

            // Enable standard derivatives extension as necessary
            if (glslCode.IndexOf("dFdx", StringComparison.InvariantCulture) >= 0
            || glslCode.IndexOf("dFdy", StringComparison.InvariantCulture) >= 0)
            {
                glslCode = "#extension GL_OES_standard_derivatives : enable\r\n" + glslCode;
            }

            // Store the code for serialization.
            dxshader.ShaderCode = Encoding.ASCII.GetBytes(glslCode);

            return dxshader;
        }

        private static void AddConstantBuffers(List<ConstantBufferData> cbuffers, ShaderData dxshader, MojoShader.Symbol[] symbols)
        {
            var symbol_types = new[]
            {
                new { name = (dxshader.Stage == ShaderStage.Vertex) ? "vs_uniforms_bool"  : "ps_uniforms_bool",  set = MojoShader.SymbolRegisterSet.BOOL, },
                new { name = (dxshader.Stage == ShaderStage.Vertex) ? "vs_uniforms_ivec4" : "ps_uniforms_ivec4", set = MojoShader.SymbolRegisterSet.INT4, },
                new { name = (dxshader.Stage == ShaderStage.Vertex) ? "vs_uniforms_vec4"  : "ps_uniforms_vec4",  set = MojoShader.SymbolRegisterSet.FLOAT4, },
            };

            List<int> cbuffer_index = new List<int>();
            for (int i = 0; i < symbol_types.Length; i++)
            {
                ConstantBufferData cbuffer = ShaderProfileGL.CreateConstantBufferData(symbol_types[i].name,
                                                                                          symbol_types[i].set,
                                                                                          symbols);

                if (cbuffer.Size == 0)
                    continue;

                int match = cbuffers.FindIndex(e => e.SameAs(cbuffer));
                if (match == -1)
                {
                    cbuffer_index.Add(cbuffers.Count);
                    cbuffers.Add(cbuffer);
                }
                else
                    cbuffer_index.Add(match);
            }
            dxshader._cbuffers = cbuffer_index.ToArray();
        }

        private static ConstantBufferData CreateConstantBufferData(string name, MojoShader.SymbolRegisterSet set, MojoShader.Symbol[] symbols)
        {
            ConstantBufferData cbuffer = new ConstantBufferData();

            cbuffer.Name = name ?? string.Empty;

            cbuffer.ParameterIndex = new List<int>();
            cbuffer.ParameterOffset = new List<int>();
            cbuffer.Parameters = new List<EffectObject.EffectParameterContent>();

            int minRegister = short.MaxValue;
            int maxRegister = 0;

            int registerSize = (set == MojoShader.SymbolRegisterSet.BOOL ? 1 : 4) * 4;

            foreach (MojoShader.Symbol symbol in symbols)
            {
                if (symbol.register_set != set)
                    continue;

                // Create the parameter.
                EffectObject.EffectParameterContent parm = GetParameterFromSymbol(symbol);

                int offset = (int)symbol.register_index * registerSize;
                parm.bufferOffset = offset;

                cbuffer.Parameters.Add(parm);
                cbuffer.ParameterOffset.Add(offset);

                minRegister = Math.Min(minRegister, (int)symbol.register_index);
                maxRegister = Math.Max(maxRegister, (int)(symbol.register_index + symbol.register_count));
            }

            cbuffer.Size = Math.Max(maxRegister - minRegister, 0) * registerSize;

            return cbuffer;
        }

        private static EffectObject.EffectParameterContent GetParameterFromSymbol(MojoShader.Symbol symbol)
        {
            EffectObject.EffectParameterContent param = new EffectObject.EffectParameterContent();
            param.rows = symbol.info.rows;
            param.columns = symbol.info.columns;
            param.name = symbol.name ?? string.Empty;
            param.semantic = string.Empty; // TODO: How do i do this with only MojoShader?

            int registerSize = (symbol.register_set == MojoShader.SymbolRegisterSet.BOOL ? 1 : 4) * 4;
            int offset = (int)symbol.register_index * registerSize;
            param.bufferOffset = offset;

            switch (symbol.info.parameter_class)
            {
                case MojoShader.SymbolClass.SCALAR:
                    param.class_ = EffectObject.PARAMETER_CLASS.SCALAR;
                    break;

                case MojoShader.SymbolClass.VECTOR:
                    param.class_ = EffectObject.PARAMETER_CLASS.VECTOR;
                    break;

                case MojoShader.SymbolClass.MATRIX_COLUMNS:
                    param.class_ = EffectObject.PARAMETER_CLASS.MATRIX_COLUMNS;

                    // MojoShader optimizes matrices to occupy less registers.
                    // This effectively convert a Matrix4x4 into Matrix4x3, Matrix4x2 or Matrix4x1.
                    param.columns = Math.Min(param.columns, symbol.register_count);

                    break;

                default:
                    throw new Exception("Unsupported parameter class!");
            }

            switch (symbol.info.parameter_type)
            {
                case MojoShader.SymbolType.BOOL:
                    param.type = EffectObject.PARAMETER_TYPE.BOOL;
                    break;

                case MojoShader.SymbolType.FLOAT:
                    param.type = EffectObject.PARAMETER_TYPE.FLOAT;
                    break;

                case MojoShader.SymbolType.INT:
                    param.type = EffectObject.PARAMETER_TYPE.INT;
                    break;

                default:
                    throw new Exception("Unsupported parameter type!");
            }

            // HACK: We don't have real default parameters from mojoshader! 
            param.data = new byte[param.rows * param.columns * 4];

            param.member_count = symbol.info.member_count;
            param.element_count = symbol.info.elements > 1 ? symbol.info.elements : 0;

            if (param.member_count > 0)
            {
                param.member_handles = new EffectObject.EffectParameterContent[param.member_count];

                MojoShader.Symbol[] members = MarshalHelper.UnmarshalArray<MojoShader.Symbol>(
                    symbol.info.members, (int)symbol.info.member_count);

                for (int i = 0; i < param.member_count; i++)
                {
                    EffectObject.EffectParameterContent mparam = GetParameterFromSymbol(members[i]);
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
