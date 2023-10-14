// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    class OpenGLShaderProfile : ShaderProfile
    {
        private static readonly Regex GlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);
        private static readonly Regex GlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)$", RegexOptions.Compiled);

        public OpenGLShaderProfile()
            : base("OpenGL", ShaderProfileType.OpenGL_Mojo)
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

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings)
        {
            ShaderInfo shaderInfo = shaderResult.ShaderInfo;

            System.Diagnostics.Debug.Assert(shaderResult.Profile.ProfileType == ShaderProfileType.OpenGL_Mojo);

            // For now GLSL is only supported via translation
            // using MojoShader which works from DX9 HLSL bytecode.
            shaderProfile = shaderProfile.Replace("s_4_0_level_9_1", "s_2_0");
            shaderProfile = shaderProfile.Replace("s_4_0_level_9_3", "s_3_0");
            D3DC.ShaderBytecode shaderBytecodeDX9 = EffectObject.CompileHLSL(shaderResult, shaderFunction, shaderProfile, false, ref errorsAndWarnings);

            ShaderData shaderDataDX9 = ShaderData.CreateGLSL(shaderBytecodeDX9, isVertexShader, effect.ConstantBuffers, effect.Shaders.Count, shaderInfo.SamplerStates, shaderResult.Debug);
            return shaderDataDX9;
        }
    }
}
