// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class KNIFXWriter11 : BinaryWriter
    {
        internal const string KNIFXSignature = "KNIF";
        internal const int Version = 11;

        public KNIFXWriter11(Stream output) : base(output)
        {
        }

        protected void WritePackedInt(int value)
        {
            // write zigzag encoded int
            int zzint = ((value << 1) ^ (value >> 31));
            Write7BitEncodedInt(zzint);
        }

        internal void WriteEffect(EffectObject effectObject, bool integersAsFloats)
        {
            Write((bool)integersAsFloats);
            WriteConstantBuffers(effectObject.ConstantBuffers);
            WriteShaders(effectObject.Shaders);
            WriteParameters(effectObject.Parameters, effectObject.Parameters.Length);
            WriteTechniques(effectObject.Techniques);
        }

        private void WriteConstantBuffers(ICollection<ConstantBufferData> constantBuffers)
        {
            WritePackedInt(constantBuffers.Count);
            foreach (ConstantBufferData cbuffer in constantBuffers)
                WriteConstantBuffer(cbuffer);
        }

        private void WriteConstantBuffer(ConstantBufferData cbuffer)
        {
            Write(cbuffer.Name);

            WritePackedInt(cbuffer.Size);

            WritePackedInt(cbuffer.ParameterIndex.Count);
            for (int i = 0; i < cbuffer.ParameterIndex.Count; i++)
            {
                WritePackedInt(cbuffer.ParameterIndex[i]);
                Write((ushort)cbuffer.ParameterOffset[i]);
            }
        }

        private void WriteShaders(ICollection<ShaderData> shaders)
        {
            WritePackedInt(shaders.Count);
            foreach (ShaderData shader in shaders)
                WriteShader(shader);
        }

        private void WriteShader(ShaderData shader)
        {
            Write((byte)shader.Stage);
            WriteShaderVersion(shader.ShaderVersion);

            Write(shader.ShaderCode.Length);
            Write(shader.ShaderCode);

            Debug.WriteLine("Write Samplers ("+ shader._samplers.Length + ")");
            foreach (SamplerInfo sampler in shader._samplers)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine(" GLsamplerName: " + sampler.GLsamplerName);
                Debug.WriteLine(" textureName: //" + sampler.textureName);
                Debug.WriteLine(" type: " + sampler.type);
                Debug.WriteLine(" textureSlot: #" + sampler.textureSlot);
                Debug.WriteLine(" samplerSlot: #" + sampler.samplerSlot);
                bool hasState = sampler.state != null;
                Debug.WriteLine(" hasState: " + hasState);
                if (hasState)
                {
                    Debug.WriteLine("  Filter: " + sampler.state.Filter);
                    Debug.WriteLine("  AddressU: " + sampler.state.AddressU);
                    Debug.WriteLine("  AddressV: " + sampler.state.AddressV);
                }
            }
            Debug.WriteLine("");

            WritePackedInt(shader._samplers.Length);
            foreach (SamplerInfo sampler in shader._samplers)
            {
                Write((byte)sampler.type);
                Write((byte)sampler.textureSlot);
                Write((byte)sampler.samplerSlot);

                if (sampler.state != null)
                {
                    Write(true);
                    WriteSamplerState(sampler.state);
                }
                else
                    Write(false);

                Write(sampler.GLsamplerName);

                WritePackedInt(sampler.textureParameter);
            }

            WritePackedInt(shader._cbuffers.Length);
            foreach (int cb in shader._cbuffers)
                WritePackedInt((byte)cb);


            Debug.WriteLine("Write _attributes (" + shader._attributes.Length + ")");
            foreach (ShaderData.Attribute attrib in shader._attributes)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine(" name: " + attrib.name);
                Debug.WriteLine(" usage: " + attrib.usage);
                Debug.WriteLine(" index: " + attrib.index);
                Debug.WriteLine(" location: " + attrib.location);
            }

            WritePackedInt(shader._attributes.Length);
            foreach (ShaderData.Attribute attrib in shader._attributes)
            {
                Write(attrib.name);
                Write((byte)attrib.usage);
                WritePackedInt(attrib.index);
                Write((short)attrib.location);
            }
        }

        private void WriteShaderVersion(ShaderVersion shaderVersion)
        {
            WritePackedInt(checked((ushort)shaderVersion.Major));
            WritePackedInt(checked((ushort)shaderVersion.Minor));
        }

        private void WriteSamplerState(SamplerState state)
        {
            Write((byte)state.AddressU);
            Write((byte)state.AddressV);
            Write((byte)state.AddressW);
            Write(state.BorderColor.R);
            Write(state.BorderColor.G);
            Write(state.BorderColor.B);
            Write(state.BorderColor.A);
            Write((byte)state.Filter);
            WritePackedInt(state.MaxAnisotropy);
            WritePackedInt(state.MaxMipLevel);
            Write(state.MipMapLevelOfDetailBias);
        }

        private void WriteParameters(EffectObject.EffectParameterContent[] parameters, int count)
        {
            WritePackedInt(count);
            for (int i = 0; i < count; i++)
                WriteParameter(parameters[i]);
        }

        private void WriteParameter(EffectObject.EffectParameterContent param)
        {
            EffectParameterClass paramClass = EffectObject.ToXNAParameterClass(param.class_);
            EffectParameterType paramType = EffectObject.ToXNAParameterType(param.type);
            Write((byte)paramClass);
            Write((byte)paramType);

            Write(param.name);
            Write(param.semantic);
            WriteAnnotations(param.annotation_handles);

            Write((byte)param.rows);
            Write((byte)param.columns);
            Write((byte)param.columnsActual);

            // Write the elements or struct members.
            WriteParameters(param.member_handles, (int)param.element_count);
            WriteParameters(param.member_handles, (int)param.member_count);

            if (param.element_count == 0 && param.member_count == 0)
            {
                switch (paramType)
                {
                    case EffectParameterType.Bool:
                    case EffectParameterType.Int32:
                    case EffectParameterType.Single:
                        Write((byte[])param.data);
                        break;
                }
            }
        }

        private void WriteTechniques(EffectObject.EffectTechniqueContent[] techniques)
        {
            WritePackedInt(techniques.Length);
            foreach (EffectObject.EffectTechniqueContent technique in techniques)
            {
                Write(technique.name);
                WriteAnnotations(technique.annotation_handles);

                // Write the passes.
                WritePasses(technique);
            }
        }

        private void WritePasses(EffectObject.EffectTechniqueContent technique)
        {
            WritePackedInt((int)technique.pass_count);
            for (int p = 0; p < technique.pass_count; p++)
            {
                EffectObject.EffectPassContent pass = technique.pass_handles[p];
                WriteEffectPass(pass);
            }
        }

        private void WriteEffectPass(EffectObject.EffectPassContent pass)
        {
            Write(pass.name);
            WriteAnnotations(pass.annotation_handles);

            // Write the index for the vertex and pixel shaders.
            int vertexShaderIndex  = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.VERTEXSHADER, pass.states);
            int pixelShaderIndex   = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.PIXELSHADER, pass.states);
            int computeShaderIndex = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.COMPUTESHADER, pass.states);
            WritePackedInt(vertexShaderIndex);
            WritePackedInt(pixelShaderIndex);
            WritePackedInt(computeShaderIndex);

            // Write the state objects too!
            if (pass.blendState != null)
            {
                Write(true);
                WriteBlendState(pass.blendState);
            }
            else
                Write(false);

            if (pass.depthStencilState != null)
            {
                Write(true);
                WriteDepthStencilState(pass.depthStencilState);
            }
            else
                Write(false);

            if (pass.rasterizerState != null)
            {
                Write(true);
                WriteRasterizerState(pass.rasterizerState);
            }
            else
                Write(false);
        }

        private void WriteBlendState(BlendState blendState)
        {
            Write((byte)blendState.AlphaBlendFunction);
            Write((byte)blendState.AlphaDestinationBlend);
            Write((byte)blendState.AlphaSourceBlend);
            Write(blendState.BlendFactor.R);
            Write(blendState.BlendFactor.G);
            Write(blendState.BlendFactor.B);
            Write(blendState.BlendFactor.A);
            Write((byte)blendState.ColorBlendFunction);
            Write((byte)blendState.ColorDestinationBlend);
            Write((byte)blendState.ColorSourceBlend);
            Write((byte)blendState.ColorWriteChannels);
            Write((byte)blendState.ColorWriteChannels1);
            Write((byte)blendState.ColorWriteChannels2);
            Write((byte)blendState.ColorWriteChannels3);
            Write(blendState.MultiSampleMask);
        }

        private void WriteDepthStencilState(DepthStencilState depthStencilState)
        {
            Write((byte)depthStencilState.CounterClockwiseStencilDepthBufferFail);
            Write((byte)depthStencilState.CounterClockwiseStencilFail);
            Write((byte)depthStencilState.CounterClockwiseStencilFunction);
            Write((byte)depthStencilState.CounterClockwiseStencilPass);
            Write(depthStencilState.DepthBufferEnable);
            Write((byte)depthStencilState.DepthBufferFunction);
            Write(depthStencilState.DepthBufferWriteEnable);
            Write(depthStencilState.ReferenceStencil);
            Write((byte)depthStencilState.StencilDepthBufferFail);
            Write(depthStencilState.StencilEnable);
            Write((byte)depthStencilState.StencilFail);
            Write((byte)depthStencilState.StencilFunction);
            Write(depthStencilState.StencilMask);
            Write((byte)depthStencilState.StencilPass);
            Write(depthStencilState.StencilWriteMask);
            Write(depthStencilState.TwoSidedStencilMode);
        }

        private void WriteRasterizerState(RasterizerState rasterizerState)
        {
            Write((byte)rasterizerState.CullMode);
            Write(rasterizerState.DepthBias);
            Write((byte)rasterizerState.FillMode);
            Write(rasterizerState.MultiSampleAntiAlias);
            Write(rasterizerState.ScissorTestEnable);
            Write(rasterizerState.SlopeScaleDepthBias);
        }

        private void WriteAnnotations(EffectObject.EffectParameterContent[] annotations)
        {
            int count = annotations == null ? 0 : annotations.Length;
            WritePackedInt(count);

            // TODO: Annotations are not implemented!
            System.Diagnostics.Debug.Assert(count == 0);

            //for (int i = 0; i < count; i++)
            //    WriteParameter(writer, annotations[i]);
        }

        //protected void Write7BitEncodedInt(int value)
        //{
        //    unchecked
        //    {
        //        do
        //        {
        //            byte value7bit = (byte)(value & 0x7f);
        //            value = (int)((uint)value >> 7);
        //            if (value != 0)
        //                value7bit |= 0x80;
        //            Write(value7bit);
        //        }
        //        while (value != 0);
        //    }
        //}
    }
}
