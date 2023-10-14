using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3DC = SharpDX.D3DCompiler;

// Copyright (C)2022 Nick Kastellanos

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
	internal partial class ShaderData
	{
        public static ShaderData CreateGLSL(D3DC.ShaderBytecode shaderBytecodeDX9, bool isVertexShader, List<ConstantBufferData> cbuffers, int sharedIndex, Dictionary<string, SamplerStateInfo> samplerStates, EffectProcessorDebugMode debugMode)
		{

            ShaderData dxshader = new ShaderData(isVertexShader, sharedIndex);

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
				throw new Exception(errors [0].error);
			}

			// Conver the attributes.
			//
			// TODO: Could this be done using DX shader reflection?
			//
			{
                MojoShader.Attribute[] attributes = MarshalHelper.UnmarshalArray<MojoShader.Attribute>(
						parseData.attributes, parseData.attribute_count);

				dxshader._attributes = new Attribute[attributes.Length];
				for (int i = 0; i < attributes.Length; i++)
                {
					dxshader._attributes [i].name = attributes [i].name;
					dxshader._attributes [i].index = attributes [i].index;
					dxshader._attributes [i].usage = EffectObject.ToXNAVertexElementUsage (attributes [i].usage);
				}
			}

            MojoShader.Symbol[] symbols = MarshalHelper.UnmarshalArray<MojoShader.Symbol>(
					parseData.symbols, parseData.symbol_count);

			//try to put the symbols in the order they are eventually packed into the uniform arrays
			//this /should/ be done by pulling the info from mojoshader
			Array.Sort(symbols, delegate(MojoShader.Symbol a, MojoShader.Symbol b)
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
					switch (symbols [i].register_set)
                    {
					    case MojoShader.SymbolRegisterSet.BOOL:
						    symbols [i].register_index = bool_index;
						    bool_index += symbols [i].register_count;
						    break;

					    case MojoShader.SymbolRegisterSet.FLOAT4:
						    symbols [i].register_index = float4_index;
						    float4_index += symbols[i].register_count;
						    break;

					    case MojoShader.SymbolRegisterSet.INT4:
						    symbols [i].register_index = int4_index;
						    int4_index += symbols [i].register_count;
						    break;
					}
				}
			}

            // Get the samplers.
            MojoShader.Sampler[] samplers = MarshalHelper.UnmarshalArray<MojoShader.Sampler>(
					parseData.samplers, parseData.sampler_count);
			dxshader._samplers = new Sampler[samplers.Length];
			for (int i = 0; i < samplers.Length; i++)
            {
                // We need the original sampler name... look for that in the symbols.
                string samplerName = symbols.First(e => e.register_set == MojoShader.SymbolRegisterSet.SAMPLER &&
                                                     e.register_index == samplers[i].index
                                               ).name;

                Sampler sampler = new Sampler
                {
                    //sampler mapping to parameter is unknown atm
                    parameter = -1,
                                      
                    // GLSL needs the MojoShader mangled sampler name.
                    samplerName = samplers[i].name,

                    // By default use the original sampler name for the parameter name.
                    parameterName = samplerName,

                    textureSlot = samplers[i].index,
                    samplerSlot = samplers[i].index,
                    type = samplers[i].type,
                };

                string textureName = null;
                if (samplerName.Contains('+'))
                {
                    int plusIndex = samplerName.IndexOf('+');
                    textureName = samplerName.Substring(plusIndex+1);
                    samplerName = samplerName.Substring(0, plusIndex);
                }

                SamplerStateInfo state;
                if (samplerStates.TryGetValue(samplerName, out state))
                {
                    sampler.state = state.State;

                    if (textureName != null)
                        sampler.parameterName = textureName;
                    else if (state.TextureName != null)
                        sampler.parameterName = state.TextureName;
                }

                // Store the sampler.
			    dxshader._samplers[i] = sampler;
			}

			// Gather all the parameters used by this shader.
            AddConstantBuffers(cbuffers, dxshader, symbols);

			string glslCode = parseData.output;

			// TODO: This sort of sucks... why does MojoShader not produce
			// code valid for GLES out of the box?

			// GLES platforms do not like this.
			glslCode = glslCode.Replace("#version 110", "");

			// Add the required precision specifiers for GLES.

            string floatPrecision = dxshader.IsVertexShader ? "precision highp float;\r\n" : "precision mediump float;\r\n";

			glslCode = "#ifdef GL_ES\r\n" +
                 floatPrecision +
				"precision mediump int;\r\n" +
				"#endif\r\n" +
				glslCode;

			// Enable standard derivatives extension as necessary
			if (glslCode.IndexOf("dFdx", StringComparison.InvariantCulture) >= 0
			||  glslCode.IndexOf("dFdy", StringComparison.InvariantCulture) >= 0)
			{
				glslCode = "#extension GL_OES_standard_derivatives : enable\r\n" + glslCode;
			}

			// Store the code for serialization.
			dxshader.ShaderCode = Encoding.ASCII.GetBytes(glslCode);

            return dxshader;
		}

        private static void AddConstantBuffers(List<ConstantBufferData> cbuffers, ShaderData dxshader, MojoShader.Symbol[] symbols)
        {
            var symbol_types = new []
            {
                new { name = dxshader.IsVertexShader ? "vs_uniforms_bool" : "ps_uniforms_bool", set = MojoShader.SymbolRegisterSet.BOOL, },
                new { name = dxshader.IsVertexShader ? "vs_uniforms_ivec4" : "ps_uniforms_ivec4", set = MojoShader.SymbolRegisterSet.INT4, },
                new { name = dxshader.IsVertexShader ? "vs_uniforms_vec4" : "ps_uniforms_vec4", set = MojoShader.SymbolRegisterSet.FLOAT4, },
            };

            List<int> cbuffer_index = new List<int>();
            for (int i = 0; i < symbol_types.Length; i++)
            {
                ConstantBufferData cbuffer = new ConstantBufferData(symbol_types[i].name,
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
	}
}
