// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal partial class ConstantBufferData
    {
        public ConstantBufferData(D3DC.ConstantBuffer cb)
        {
            Name = string.Empty;
            Size = cb.Description.Size;

            ParameterIndex = new List<int>();

            List<EffectObject.EffectParameterContent> parameters = new List<EffectObject.EffectParameterContent>();

            // Gather all the parameters.
            for (int i = 0; i < cb.Description.VariableCount; i++)
            {
                D3DC.ShaderReflectionVariable vdesc = cb.GetVariable(i);

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
            Parameters = parameters.OrderBy(e => e.bufferOffset).ToList();

            // Store the parameter offsets.
            ParameterOffset = new List<int>();
            foreach (EffectObject.EffectParameterContent param in Parameters)
                ParameterOffset.Add(param.bufferOffset);
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
