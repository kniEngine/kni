// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal partial class ConstantBufferData
    {
        public ConstantBufferData (string name,
                                MojoShader.SymbolRegisterSet set, 
                                MojoShader.Symbol[] symbols)
		{
			Name = name ?? string.Empty;

			ParameterIndex = new List<int>();
			ParameterOffset = new List<int>();
			Parameters = new List<EffectObject.EffectParameterContent>();

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

				Parameters.Add(parm);
                ParameterOffset.Add(offset);

                minRegister = Math.Min(minRegister, (int)symbol.register_index);
                maxRegister = Math.Max(maxRegister, (int)(symbol.register_index + symbol.register_count));
            }

            Size = Math.Max(maxRegister - minRegister, 0) * registerSize;
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
