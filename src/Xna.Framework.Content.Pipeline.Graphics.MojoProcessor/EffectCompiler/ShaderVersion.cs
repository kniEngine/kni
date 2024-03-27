// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    public struct ShaderVersion
    {
        public readonly int Major;
        public readonly int Minor;

        private static readonly Regex VertexShaderRegex = new Regex(@"^vs_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(?<level>9_1|9_3))?$", RegexOptions.Compiled);
        private static readonly Regex PixelShaderRegex  = new Regex(@"^ps_(?<major>1|2|3|4|5)_(?<minor>0|1|)(_level_(?<level>9_1|9_3))?$", RegexOptions.Compiled);
        

        public ShaderVersion(int major, int minor)
            : this()
        {
            this.Major = major;
            this.Minor = minor;
        }

        private static ShaderVersion ParseShaderModel(string shaderModel, Regex regex)
        {
            Match match = regex.Match(shaderModel);

            if (match.Success)
            {
                int major = int.Parse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
                int minor = int.Parse(match.Groups["minor"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);

                Group levelGroup = match.Groups["level"];
                if (levelGroup.Success)
                {
                    if (major == 4 && minor == 0)
                    {
                        string level = levelGroup.Value;
                        if (level == "9_1")
                            return new ShaderVersion(2, 0);
                        if (level == "9_3")
                            return new ShaderVersion(3, 0);
                    }
                }
                else
                {
                    return new ShaderVersion(major, minor);
                }
            }

            return new ShaderVersion(-1, 0);

        }

        internal static ShaderVersion ParseVertexShaderModel(string vsModel)
        {
            return ParseShaderModel(vsModel, VertexShaderRegex);
        }

        internal static ShaderVersion ParsePixelShaderModel(string vsModel)
        {
            return ParseShaderModel(vsModel, PixelShaderRegex);
        }

        public override string ToString()
        {
            return String.Format("{{Major: {0}, Minor: {1} }}",
                Major, Minor);
        }
    }
}
