// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler;
using Microsoft.Xna.Framework.Graphics;

namespace EffectCompiler
{

    public class Options
    {
        [CommandLineParser.Required]
        public string SourceFile;

        [CommandLineParser.Required]
        public string OutputFile = string.Empty;

        [CommandLineParser.Name("Platform", "\t - Specify the shader target Platform. Allowed values: (Windows,WindowsStoreApp,DesktopGL,Android,iOS,BlazorGL).")]
        public TargetPlatform Platform = (TargetPlatform)(-1);

        [CommandLineParser.Name("GraphicsProfile", "\t - Specify the shader target GraphicsProfile.")]
        public GraphicsProfile GraphicsProfile = GraphicsProfile.Reach;

        [CommandLineParser.Name("Config", "\t - BuildConfiguration. Set to 'Debug' to include extra debug information in the compiled effect.")]
        public string Config;

        [CommandLineParser.Name("Defines", "\t - Semicolon-delimited define assignments")]
        public string Defines;
    }
}
