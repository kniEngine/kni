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
    class DirectX11ShaderProfile : ShaderProfile
    {
        private static readonly Regex HlslPixelShaderRegex = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_3))?$", RegexOptions.Compiled);
        private static readonly Regex HlslVertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(9_1|9_3))?$", RegexOptions.Compiled);

        public DirectX11ShaderProfile()
            : base("DirectX_11", ShaderProfileType.DirectX_11)
        {
        }

        internal override IEnumerable<KeyValuePair<string, string>> GetMacros()
        {
            yield return new KeyValuePair<string, string>("__DIRECTX__", "1");

            // deprecated macros. Left for backward compatibility with MonoGame.
            yield return new KeyValuePair<string, string>("HLSL", "1");
            yield return new KeyValuePair<string, string>("SM4", "1");
        }

        internal override void ValidateShaderModels(PassInfo pass)
        {
            int major, minor;

            if (!string.IsNullOrEmpty(pass.vsFunction))
            {
                ParseShaderModel(pass.vsModel, HlslVertexShaderRegex, out major, out minor);
                if (major <= 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Vertex shader '{1}' must be SM 4.0 level 9.1 or higher!", pass.vsModel, pass.vsFunction));
            }

            if (!string.IsNullOrEmpty(pass.psFunction))
            {
                ParseShaderModel(pass.psModel, HlslPixelShaderRegex, out major, out minor);
                if (major <= 3)
                    throw new Exception(String.Format("Invalid profile '{0}'. Pixel shader '{1}' must be SM 4.0 level 9.1 or higher!", pass.vsModel, pass.psFunction));
            }
        }

        internal override ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings)
        {
            ShaderInfo shaderInfo = shaderResult.ShaderInfo;

            System.Diagnostics.Debug.Assert(shaderResult.Profile.ProfileType == ShaderProfileType.DirectX_11);

            byte[] bytecodeDX11 = EffectObject.CompileHLSL(shaderResult, shaderFunction,shaderProfile, true, ref errorsAndWarnings);

            ShaderData shaderDataDX11 = ShaderData.CreateHLSL(bytecodeDX11, isVertexShader, effect.ConstantBuffers, effect.Shaders.Count, shaderInfo.SamplerStates, shaderResult.Debug);
            return shaderDataDX11;
        }

        internal override bool Supports(TargetPlatform platform)
        {
            switch (platform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsStoreApp:
                    return true;
                default:
                    return false;
            }
        }
    }
}
