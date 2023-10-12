// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;

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
            // For now GLSL is only supported via translation
            // using MojoShader which works from HLSL bytecode.
            byte[] bytecode = EffectObject.CompileHLSL(shaderResult, shaderFunction, shaderProfile, ref errorsAndWarnings);

            ShaderInfo shaderInfo = shaderResult.ShaderInfo;
            ShaderData shaderData = ShaderData.CreateGLSL(bytecode, isVertexShader, effect.ConstantBuffers, effect.Shaders.Count, shaderInfo.SamplerStates, shaderResult.Debug);
            effect.Shaders.Add(shaderData);

            return shaderData;
        }
            
        internal override bool Supports(TargetPlatform platform)
        {
            switch(platform)
            {
                case TargetPlatform.iOS:
                case TargetPlatform.Android:
                case TargetPlatform.BlazorGL:
                case TargetPlatform.DesktopGL:
                case TargetPlatform.MacOSX:
                case TargetPlatform.RaspberryPi:
                    return true;
                default:
                    return false;
            }
        }
    }
}
