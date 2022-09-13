// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Linq;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    partial class EffectObject
    {
        public static byte[] CompileHLSL(ShaderResult shaderResult, string shaderFunction, string shaderProfile, ref string errorsAndWarnings)
        {
            SharpDX.D3DCompiler.ShaderBytecode shaderByteCode;
            try
            {
                SharpDX.D3DCompiler.ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.NoPreshader;

                if (shaderResult.Profile.ProfileType == ShaderProfileType.DirectX_11)
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;
                }

                // force D3DCompiler to generate legacy bytecode that is compatible with MojoShader.
                if (shaderResult.Profile.ProfileType == ShaderProfileType.OpenGL_Mojo)
                {
                    shaderProfile = shaderProfile.Replace("s_4_0_level_9_1", "s_2_0");
                    shaderProfile = shaderProfile.Replace("s_4_0_level_9_3", "s_3_0");
                }

                if (shaderResult.Debug == Processors.EffectProcessorDebugMode.Debug)
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.SkipOptimization;
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.Debug;
                }
                else
                {
                    shaderFlags |= SharpDX.D3DCompiler.ShaderFlags.OptimizationLevel3;
                }

                // Compile the shader into bytecode.                
                var result = SharpDX.D3DCompiler.ShaderBytecode.Compile(
                    shaderResult.FileContent,
                    shaderFunction,
                    shaderProfile,
                    shaderFlags,
                    0,
                    null,
                    null,
                    shaderResult.FilePath);

                // Store all the errors and warnings to log out later.
                errorsAndWarnings += result.Message;

                if (result.Bytecode == null)
                    throw new ShaderCompilerException();
                
                shaderByteCode = result.Bytecode;
                //var source = shaderByteCode.Disassemble();
            }
            catch (SharpDX.CompilationException ex)
            {
                errorsAndWarnings += ex.Message;
                throw new ShaderCompilerException();
            }

            // Return a copy of the shader bytecode.
            return shaderByteCode.Data.ToArray();
        }
    }
}
