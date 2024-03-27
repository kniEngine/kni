// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    [TypeConverter(typeof(StringConverter))]
    public abstract class ShaderProfile
    {
        protected ShaderProfile()
        {
        }

        public static readonly ShaderProfile DirectX_11 = new ShaderProfileDX11();

        public static readonly ShaderProfile OpenGL_Mojo = new ShaderProfileGL();


        /// <summary>
        /// Returns the name of the shader profile.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Returns the format identifier used in the MGFX file format.
        /// </summary>
        public abstract ShaderProfileType ProfileType { get; }

        internal abstract IEnumerable<KeyValuePair<string,string>> GetMacros();

        internal abstract void ValidateShaderModels(PassInfo pass);

        internal abstract ShaderData CreateShader(EffectContent input, ContentProcessorContext context, EffectObject effect, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderStage shaderStage, ref string errorsAndWarnings);

        internal static D3DC.ShaderBytecode CompileHLSL(EffectContent input, ContentProcessorContext context, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, bool backwardsCompatibility, ref string errorsAndWarnings)
        {
            try
            {
                D3DC.ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= D3DC.ShaderFlags.NoPreshader;

                if (backwardsCompatibility)
                    shaderFlags |= D3DC.ShaderFlags.EnableBackwardsCompatibility;

                if (debugMode == Processors.EffectProcessorDebugMode.Debug)
                {
                    shaderFlags |= D3DC.ShaderFlags.SkipOptimization;
                    shaderFlags |= D3DC.ShaderFlags.Debug;
                }
                else
                {
                    shaderFlags |= D3DC.ShaderFlags.OptimizationLevel3;
                }

                // Compile the shader into bytecode.                
                D3DC.CompilationResult result = D3DC.ShaderBytecode.Compile(
                    fileContent,
                    shaderFunction,
                    shaderProfileName,
                    shaderFlags,
                    0,
                    null,
                    null,
                    fullFilePath);

                // Store all the errors and warnings to log out later.
                errorsAndWarnings += result.Message;

                if (result.Bytecode == null)
                    throw new ShaderCompilerException();

                D3DC.ShaderBytecode shaderBytecode = result.Bytecode;
                //string source = shaderByteCode.Disassemble();

                return shaderBytecode;
            }
            catch (SharpDX.CompilationException ex)
            {
                errorsAndWarnings += ex.Message;
                throw new ShaderCompilerException();
            }
        }
        
        [Conditional("DEBUG")]
        internal static void LogDX11ShaderReflection(D3DC.ShaderReflection d3dcShaderReflection)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("LogShaderReflection ");

            for (int r = 0; r < d3dcShaderReflection.Description.BoundResources; r++)
            {
                D3DC.InputBindingDescription ibDesc = d3dcShaderReflection.GetResourceBindingDescription(r);

                sb.AppendLine("");
                sb.AppendLine("ResourceBindingDescription: #" + r);
                sb.AppendLine("Name: '" + ibDesc.Name + "'");
                sb.AppendLine("Type: " + ibDesc.Type);
                sb.AppendLine("BindPoint: " + ibDesc.BindPoint);
                sb.AppendLine("BindCount: " + ibDesc.BindCount);
                sb.AppendLine("Flags: " + ibDesc.Flags);
                sb.AppendLine("ReturnType: " + ibDesc.ReturnType);
                sb.AppendLine("Dimension: " + ibDesc.Dimension);
            }

            for (int i = 0; i < d3dcShaderReflection.Description.InputParameters; i++)
            {
                D3DC.ShaderParameterDescription paramDesc = d3dcShaderReflection.GetInputParameterDescription(i);

                sb.AppendLine("");
                sb.AppendLine("InputParameterDescription: #" + i);
                sb.AppendLine("SemanticName: '" + paramDesc.SemanticName + "'");
                sb.AppendLine("SemanticIndex: " + paramDesc.SemanticIndex);
                sb.AppendLine("Register: " + paramDesc.Register);
                sb.AppendLine("SystemValueType: " + paramDesc.SystemValueType);
                sb.AppendLine("UsageMask: " + paramDesc.UsageMask);
            }

            for (int o = 0; o < d3dcShaderReflection.Description.OutputParameters; o++)
            {
                D3DC.ShaderParameterDescription paramDesc = d3dcShaderReflection.GetOutputParameterDescription(o);

                sb.AppendLine("");
                sb.AppendLine("OutputParameterDescription: #" + o);
                sb.AppendLine("SemanticName: '" + paramDesc.SemanticName + "'");
                sb.AppendLine("SemanticIndex: " + paramDesc.SemanticIndex);
                sb.AppendLine("Register: " + paramDesc.Register);
                sb.AppendLine("SystemValueType: " + paramDesc.SystemValueType);
                sb.AppendLine("UsageMask: " + paramDesc.UsageMask);
            }

            for (int c = 0; c < d3dcShaderReflection.Description.ConstantBuffers; c++)
            {
                D3DC.ConstantBuffer cbuffer = d3dcShaderReflection.GetConstantBuffer(c);

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
                    sb.Append("  DefaultValue: ");
                    if (variable.Description.DefaultValue == IntPtr.Zero)
                    {
                        sb.Append("(null)");
                    }
                    else
                    {
                        int size = type.Description.ColumnCount * type.Description.RowCount;
                        switch (type.Description.Type)
                        {
                            case D3DC.ShaderVariableType.Float:
                                float[] fdata = new float[size];
                                Marshal.Copy(variable.Description.DefaultValue, fdata, 0, (int)size);
                                for (int d = 0; d < fdata.Length; d++)
                                    sb.Append(fdata[d].ToString(CultureInfo.InvariantCulture) + " ");
                                break;
                            case D3DC.ShaderVariableType.Int:
                            case D3DC.ShaderVariableType.Bool:
                                int[] idata = new int[size];
                                Marshal.Copy(variable.Description.DefaultValue, idata, 0, (int)size);
                                for(int d = 0; d<idata.Length; d++)
                                    sb.Append(idata[d].ToString(CultureInfo.InvariantCulture) + " ");
                                break;
                        }
                    }
                    sb.AppendLine("");
                }
            }

            for (int p = 0; p < d3dcShaderReflection.Description.PatchConstantParameters; p++)
            {
                D3DC.ShaderParameterDescription paramDesc = d3dcShaderReflection.GetPatchConstantParameterDescription(p);

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

        internal static ConstantBufferData[] GetDX11ConstantBuffers(D3DC.ShaderReflection d3dcShaderReflection)
        {
            ConstantBufferData[] cbuffersData = new ConstantBufferData[d3dcShaderReflection.Description.ConstantBuffers];
            for (int i = 0; i < d3dcShaderReflection.Description.ConstantBuffers; i++)
            {
                D3DC.ConstantBuffer d3dcbuffer = d3dcShaderReflection.GetConstantBuffer(i);
                ConstantBufferData cbufferData = ShaderProfileDX11.CreateDX11ConstantBufferData(d3dcbuffer);
                cbuffersData[i] = cbufferData;
            }

            return cbuffersData;
        }

        private static ConstantBufferData CreateDX11ConstantBufferData(D3DC.ConstantBuffer d3dcbuffer)
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

        private class StringConverter : TypeConverter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string name = value as string;

                    if (ShaderProfile.DirectX_11.Name == name)
                        return ShaderProfile.DirectX_11;

                    if (ShaderProfile.OpenGL_Mojo.Name == name)
                        return ShaderProfile.OpenGL_Mojo;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
