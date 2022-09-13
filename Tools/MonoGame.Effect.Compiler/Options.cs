// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler;

namespace MonoGame.EffectCompiler
{

    public class Options
    {
        [CommandLineParser.Required]
        public string SourceFile;

        [CommandLineParser.Required]
        public string OutputFile = string.Empty;

        [CommandLineParser.Name("Profile", "\t - Specify the shader target profile. Values: DirectX_11, OpenGL_Mojo. ")]
        public ShaderProfileType Profile = (ShaderProfileType)(-1);

        [CommandLineParser.Name("Config", "\t\t - BuildConfiguration. Set to 'Debug' to include extra debug information in the compiled effect.")]
        public string Config;

        [CommandLineParser.Name("Defines", "\t - Semicolon-delimited define assignments")]
        public string Defines;
    }
}
