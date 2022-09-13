// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class EffectObjectWriter : BinaryWriter
    {
        private readonly int Version;
        private readonly ShaderProfile _profile;

        public EffectObjectWriter(Stream output, int version, ShaderProfile profile) : base(output)
        {
            System.Diagnostics.Debug.Assert(version == 10);
            this.Version = version;
            this._profile = profile;
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
            foreach (var cbuffer in constantBuffers)
                WriteConstantBuffer(cbuffer);
        }

        private void WriteConstantBuffer(ConstantBufferData cbuffer)
        {
            Write(cbuffer.Name);

            Write((ushort)cbuffer.Size);

            Write(cbuffer.ParameterIndex.Count);
            for (var i = 0; i < cbuffer.ParameterIndex.Count; i++)
            {
                Write(cbuffer.ParameterIndex[i]);
                Write((ushort)cbuffer.ParameterOffset[i]);
            }
        }

        private void WriteShaders(ICollection<ShaderData> shaders)
        {
            Write(shaders.Count);
            foreach (var shader in shaders)
                WriteShader(shader);
        }

        private void WriteShader(ShaderData shader)
        {
            Write(shader.IsVertexShader);

            Write(shader.ShaderCode.Length);
            Write(shader.ShaderCode);

            Write((byte)shader._samplers.Length);
            foreach (var sampler in shader._samplers)
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

                Write(sampler.samplerName);

                Write((byte)sampler.parameter);
            }

            Write((byte)shader._cbuffers.Length);
            foreach (var cb in shader._cbuffers)
                Write((byte)cb);

            Write((byte)shader._attributes.Length);
            foreach (var attrib in shader._attributes)
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
            for (var i = 0; i < count; i++)
                WriteParameter(parameters[i]);
        }

        private void WriteParameter(EffectObject.EffectParameterContent param)
        {
            var class_ = EffectObject.ToXNAParameterClass(param.class_);
            var type = EffectObject.ToXNAParameterType(param.type);
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
            foreach (var technique in techniques)
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
            for (var p = 0; p < technique.pass_count; p++)
            {
                var pass = technique.pass_handles[p];
                WriteEffectPass(pass);
            }
        }

        private void WriteEffectPass(EffectObject.EffectPassContent pass)
        {
            Write(pass.name);
            WriteAnnotations(pass.annotation_handles);

            // Write the index for the vertex and pixel shaders.
            var vertexShaderIndex = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.VERTEXSHADER, pass.states);
            var pixelShaderIndex  = EffectObject.GetShaderIndex(EffectObject.STATE_CLASS.PIXELSHADER, pass.states);
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
            var count = annotations == null ? 0 : annotations.Length;
            Write(count);

            // TODO: Annotations are not implemented!
            System.Diagnostics.Debug.Assert(count == 0);

            //for (var i = 0; i < count; i++)
            //    WriteParameter(writer, annotations[i]);
        }

        //protected void Write7BitEncodedInt(int value)
        //{
        //    unchecked
        //    {
        //        do
        //        {
        //            var value7bit = (byte)(value & 0x7f);
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
