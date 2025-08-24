﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

        internal override void ValidateShaderModels(PassInfo pass, string shaderFunction, string shaderModel, ShaderStage shaderStage, ShaderVersion shaderVersion)
        {
            const int MojoMaxShaderVersion = 4;

            if (shaderStage == ShaderStage.Vertex)
            {
                if (shaderVersion.Major == -1)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}'.", shaderModel, shaderFunction));
                if (shaderVersion.Major < 2)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be at least SM 2.0.", shaderModel, shaderFunction));
                if (shaderVersion.Major > MojoMaxShaderVersion)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 3.0 or lower.", shaderModel, shaderFunction));
            }

            if (shaderStage == ShaderStage.Pixel)
            {
                if (shaderVersion.Major == -1)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}'.", shaderModel, pass.psFunction));
                if (shaderVersion.Major < 2)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be at least SM 2.0.", shaderModel, shaderFunction));
                if (shaderVersion.Major > MojoMaxShaderVersion)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 3.0 or lower.", shaderModel, shaderFunction));
            }

            if (shaderStage == ShaderStage.Compute)
            {
                if (shaderVersion.Major == -1)
                    throw new Exception(String.Format("Invalid profile '{0}'. Compute shader '{1}'.", shaderModel, shaderFunction));
                if (shaderVersion.Major < 5)
                    throw new Exception(String.Format("Invalid profile '{0}'. Compute shader '{1}' must be at least SM 5.0.", shaderModel, shaderFunction));
                if (shaderVersion.Major > MojoMaxShaderVersion)
                    throw new Exception(String.Format("Invalid profile '{0}'. Compute shader '{1}' must be SM 3.0 or lower.", shaderModel, shaderFunction));
            }
        }

        internal override ShaderData CreateShader(EffectContent input, ContentProcessorContext context, EffectObject effect, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderVersion shaderVersion, ShaderStage shaderStage, ref string errorsAndWarnings)
        {
            ConstantBufferData[] dx11CBuffersData;
            string dx11ShaderProfileName = shaderProfileName;
            dx11ShaderProfileName = dx11ShaderProfileName.Replace("s_2_0", "s_4_0_level_9_1");
            dx11ShaderProfileName = dx11ShaderProfileName.Replace("s_3_0", "s_4_0_level_9_3");
            using (D3DC.ShaderBytecode shaderBytecodeDX11 = ShaderProfile.CompileHLSL(input, context, fullFilePath, fileContent, debugMode, shaderFunction, dx11ShaderProfileName, true, ref errorsAndWarnings))
            {
                // Use reflection to get details of the shader.
                using (D3DC.ShaderReflection shaderReflection = new D3DC.ShaderReflection(shaderBytecodeDX11.Data))
                {
                    LogDX11ShaderReflection(shaderReflection);
                    dx11CBuffersData = GetDX11ConstantBuffers(shaderReflection);
                }
            }

            // For now GLSL is only supported via translation
            // using MojoShader which works from DX9 HLSL bytecode.
            string dx9ShaderProfileName = shaderProfileName;
            dx9ShaderProfileName = dx9ShaderProfileName.Replace("s_4_0_level_9_1", "s_2_0");
            dx9ShaderProfileName = dx9ShaderProfileName.Replace("s_4_0_level_9_3", "s_3_0");
            dx9ShaderProfileName = dx9ShaderProfileName.Replace("s_4_0", "s_3_0");
            using (D3DC.ShaderBytecode shaderBytecodeDX9 = ShaderProfile.CompileHLSL(input, context, fullFilePath, fileContent, debugMode, shaderFunction, dx9ShaderProfileName, false, ref errorsAndWarnings))
            {
                ShaderData shaderDataDX9 = ShaderProfileGL.CreateGLSL(input, context, shaderInfo, shaderBytecodeDX9, shaderStage, shaderVersion, effect.ConstantBuffers, debugMode, dx11CBuffersData);
                return shaderDataDX9;
            }
        }

        private static ShaderData CreateGLSL(EffectContent input, ContentProcessorContext context, ShaderInfo shaderInfo, D3DC.ShaderBytecode shaderBytecodeDX9, ShaderStage shaderStage, ShaderVersion shaderVersion, List<ConstantBufferData> cbuffers, EffectProcessorDebugMode debugMode, ConstantBufferData[] dx11CBuffersData)
        {
            ShaderData dxshader = new ShaderData(shaderStage);

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
            // TODO: use reflection to map HLSL parameter indices to GLSL
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
                    else
                        samplerInfo.textureName = state.TextureName;
                }

                // Store the sampler.
                dxshader._samplers[i] = samplerInfo;
            }

            // For whatever reason the register indexing is 
            // incorrect from MojoShader.
            // TODO: use reflection to map HLSL sampler & texture indices to GLSL
            {
                uint sampler_index = 0;

                for (int i = 0; i < dxshader._samplers.Length; i++)
                {
                    dxshader._samplers[i].samplerSlot = i;
                    dxshader._samplers[i].textureSlot = i;
                }
            }

            // Gather all the parameters used by this shader.
            AddConstantBuffers(cbuffers, dxshader, symbols, dx11CBuffersData);

            string glsl110Code = parseData.output;
            glsl110Code = glsl110Code.Replace("\r\n", "\n");

            List<GLSLBytecode> glslBytecodes = new List<GLSLBytecode>();

            string glslCode = ConvertGLSL110ToGLSL(dxshader.Stage, glsl110Code);
            GLSLBytecode glsl000 = new GLSLBytecode(0,0,false, Encoding.ASCII.GetBytes(glslCode));
            glslBytecodes.Add(glsl000);

            // GLES 3.00 is required for dFdx/dFdy/gl_FragData
            string glsl300esCode = ConvertGLSL110ToGLSL300es(dxshader.Stage, glsl110Code);
            GLSLBytecode glsl300es = new GLSLBytecode(3, 0, true, Encoding.ASCII.GetBytes(glsl300esCode));
            glslBytecodes.Add(glsl300es);

            using (MemoryStream memoryStream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(memoryStream, Encoding.ASCII))
            {
                writer.Write((short)0); // Reserved

                // write bytecode directory
                writer.Write((short)glslBytecodes.Count);
                const int HeaderSize = 4;
                const int EntrySize = 7;
                int bytecodesOffset = HeaderSize + EntrySize * glslBytecodes.Count;
                for (int i = 0; i < glslBytecodes.Count; i++)
                {
                    writer.Write((byte)glslBytecodes[i].Major);
                    writer.Write((byte)glslBytecodes[i].Minor);
                    writer.Write(glslBytecodes[i].ES);
                    writer.Write((int)(bytecodesOffset));
                    bytecodesOffset += (2 + glslBytecodes[i].Bytecode.Length);
                }
                // write bytecodes
                for (int i = 0; i < glslBytecodes.Count; i++)
                {
                    writer.Write((short)glslBytecodes[i].Bytecode.Length);
                    writer.Write(glslBytecodes[i].Bytecode);
                }

                // Store the code for serialization.
                dxshader.ShaderCode = memoryStream.ToArray();
            }

            return dxshader;
        }

        private static string ConvertGLSL110ToGLSL(ShaderStage shaderStage, string glslCode)
        {
            // remove the opengl 1.10 header. GLES platforms do not like this.
            Debug.Assert(glslCode.StartsWith("#version 110\n"));
            glslCode = glslCode.Replace("#version 110\n", "");

            // Add the required precision specifiers for GLES.
            if (shaderStage == ShaderStage.Pixel)
            {
                glslCode = "#ifdef GL_ES\n"
                         + "precision highp float;\n"
                         + "precision highp int;\n"
                         + "#endif\n"
                         + "\n"
                         + glslCode;
            }

            // Enable standard derivatives extension as necessary
            if (glslCode.IndexOf("dFdx", StringComparison.InvariantCulture) >= 0
            ||  glslCode.IndexOf("dFdy", StringComparison.InvariantCulture) >= 0)
            {
                glslCode = "#extension GL_OES_standard_derivatives : enable\n" + glslCode;
            }

            return glslCode;
        }

        static Regex rgxAttribute = new Regex(
                @"^attribute(?=\s)", RegexOptions.Multiline);
        static Regex rgxVarying = new Regex(
                @"^varying(?=\s)", RegexOptions.Multiline);
        static Regex rgxFragColor = new Regex(
                @"^#define (\w+) gl_FragColor", RegexOptions.Multiline);
        static Regex rgxFragData = new Regex(
                @"^#define (\w+) gl_FragData\[(\d+)\]", RegexOptions.Multiline);
        static Regex rgxTexture = new Regex(
                @"texture(2D|3D|Cube)(?=\()", RegexOptions.Multiline);

        private static string ConvertGLSL110ToGLSL300es(ShaderStage shaderStage, string glslCode)
        {
            // remove the opengl 1.10 header. GLES platforms do not like this.
            Debug.Assert(glslCode.StartsWith("#version 110\n"));
            glslCode = glslCode.Replace("#version 110\n", "");

            // Add the required precision specifiers for GLES.
            if (shaderStage == ShaderStage.Pixel)
            {
                glslCode = "precision highp float;\n"
                         + "precision highp int;\n"
                         + "\n"
                         + glslCode;
            }

            // add the GL ES header.
            glslCode = "#version 300 es\n"
                     + glslCode;

            switch (shaderStage)
            {
                case ShaderStage.Vertex:
                    {
                        glslCode = rgxVarying.Replace(glslCode, "out");
                    }
                    break;

                case ShaderStage.Pixel:
                    {
                        glslCode = rgxVarying.Replace(glslCode, "in");
                        glslCode = rgxFragColor.Replace(glslCode, "out vec4 $1;");
                        glslCode = rgxFragData.Replace(glslCode, "layout(location=$2) out vec4 $1;");
                    }
                    break;
            }

            glslCode = rgxAttribute.Replace(glslCode, "in");
            glslCode = rgxTexture.Replace(glslCode, "texture");

            return glslCode;
        }

        private static void AddConstantBuffers(List<ConstantBufferData> cbuffers, ShaderData dxshader, MojoShader.Symbol[] symbols, ConstantBufferData[] dx11CBuffersData)
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

                    foreach (EffectObject.EffectParameterContent param in cbuffer.Parameters)
                        SetReflectionData(dx11CBuffersData, param);
                }
                else
                {
                    cbuffer_index.Add(match);
                }
            }

            dxshader._cbuffers = cbuffer_index.ToArray();
        }

        private static void SetReflectionData(ConstantBufferData[] dx11CBuffersData, EffectObject.EffectParameterContent param)
        {
            foreach (ConstantBufferData dx11cb in dx11CBuffersData)
            {
                foreach (EffectObject.EffectParameterContent dx11param in dx11cb.Parameters)
                {
                    if (param.name == dx11param.name
                    && param.type == dx11param.type
                    && param.class_ == dx11param.class_)
                    {
                        // set reflection data
                        if (param.data is Array && dx11param.data is Array)
                        {
                            Array paramArray = param.data as Array;
                            Array dx11paramArray = dx11param.data as Array;
                            if (paramArray.Length == dx11paramArray.Length)
                            {
                                Array.Copy(dx11paramArray, paramArray, paramArray.Length);
                            }
                            else
                            {
                                Debug.Assert(paramArray.Length == dx11paramArray.Length);
                            }
                        }
                        return;
                    }
                }
            }
            return;
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
            param.columnsActual = symbol.info.columns;
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
                    param.columnsActual = Math.Min(param.columnsActual, symbol.register_count);

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
                    mparam.columnsActual = param.columnsActual;
                    mparam.data = new byte[param.columns * param.rows * 4];

                    param.member_handles[i] = mparam;
                }
            }

            return param;
        }
    }
}
