// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class EffectObjectWriter : BinaryWriter
    {
        private readonly int Version;
        private readonly ShaderProfileType _profile;

        public EffectObjectWriter(Stream output, int version, ShaderProfileType profileType) : base(output)
        {
            System.Diagnostics.Debug.Assert(version == 10);
            this.Version = version;
            this._profile = profileType;
        }

        protected void WritePackedInt(int value)
        {
            // write zigzag encoded int
            int zzint = ((value << 1) ^ (value >> 31));
            Write7BitEncodedInt(zzint);
        }

        internal void WriteEffect(EffectObject effectObject)
        {
            WriteConstantBuffers(effectObject.ConstantBuffers);
            WriteShaders(effectObject.Shaders);
            WriteParameters(effectObject.Parameters, effectObject.Parameters.Length);
            WriteTechniques(effectObject.Techniques);
        }

        private void WriteConstantBuffers(ICollection<ConstantBufferData> constantBuffers)
        {
            Write(constantBuffers.Count);
            foreach (ConstantBufferData cbuffer in constantBuffers)
                WriteConstantBuffer(cbuffer);
        }

        private void WriteConstantBuffer(ConstantBufferData cbuffer)
        {
            Write(cbuffer.Name);

            Write((ushort)cbuffer.Size);

            Write(cbuffer.ParameterIndex.Count);
            for (int i = 0; i < cbuffer.ParameterIndex.Count; i++)
            {
                Write(cbuffer.ParameterIndex[i]);
                Write((ushort)cbuffer.ParameterOffset[i]);
            }
        }

        private void WriteShaders(ICollection<ShaderData> shaders)
        {
            Write(shaders.Count);
            foreach (ShaderData shader in shaders)
                WriteShader(shader);
        }

        private void WriteShader(ShaderData shader)
        {
            Write((byte)shader.Stage);

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

            Write((byte)shader._samplers.Length);
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

                Write((byte)sampler.textureParameter);
            }

            Write((byte)shader._cbuffers.Length);
            foreach (int cb in shader._cbuffers)
                Write((byte)cb);


            Debug.WriteLine("Write _attributes (" + shader._attributes.Length + ")");
            foreach (ShaderData.Attribute attrib in shader._attributes)
            {
                Debug.WriteLine(" ");
                Debug.WriteLine(" name: " + attrib.name);
                Debug.WriteLine(" usage: " + attrib.usage);
                Debug.WriteLine(" index: " + attrib.index);
                Debug.WriteLine(" location: " + attrib.location);
            }

            Write((byte)shader._attributes.Length);
            foreach (ShaderData.Attribute attrib in shader._attributes)
            {
                Write(attrib.name);
                Write((byte)attrib.usage);
                Write((byte)attrib.index);
                Write((short)attrib.location);
            }
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
            Write(state.MaxAnisotropy);
            Write(state.MaxMipLevel);
            Write(state.MipMapLevelOfDetailBias);
        }

        private void WriteParameters(EffectObject.EffectParameterContent[] parameters, int count)
        {
            Write(count);
            for (int i = 0; i < count; i++)
                WriteParameter(parameters[i]);
        }

        private void WriteParameter(EffectObject.EffectParameterContent param)
        {
            EffectParameterClass class_ = EffectObject.ToXNAParameterClass(param.class_);
            EffectParameterTypeContent type = EffectObject.ToXNAParameterType(param.type);
            Write((byte)class_);
            Write((byte)type);

            Write(param.name);
            Write(param.semantic);
            WriteAnnotations(param.annotation_handles);

            Write((byte)param.rows);
            Write((byte)param.columns);

            // Write the elements or struct members.
            WriteParameters(param.member_handles, (int)param.element_count);
            WriteParameters(param.member_handles, (int)param.member_count);

            if (param.element_count == 0 && param.member_count == 0)
            {
                switch (type)
                {
                    case EffectParameterTypeContent.Bool:
                    case EffectParameterTypeContent.Int32:
                    case EffectParameterTypeContent.Single:
                        Write((byte[])param.data);
                        break;
                }
            }
        }

        private void WriteTechniques(EffectObject.EffectTechniqueContent[] techniques)
        {
            Write(techniques.Length);
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
            Write((int)technique.pass_count);
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
            int vertexShaderIndex = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.VERTEXSHADER, pass.states);
            int pixelShaderIndex  = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.PIXELSHADER, pass.states);
            Write(vertexShaderIndex);
            Write(pixelShaderIndex);

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
            Write(count);

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
