// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    [TypeConverter(typeof(StringConverter))]
    public abstract class ShaderProfile
    {
        private static readonly LoadedTypeCollection<ShaderProfile> _profiles = new LoadedTypeCollection<ShaderProfile>();

        protected ShaderProfile(string name, ShaderProfileType profileType)
        {
            Name = name;
            ProfileType = profileType;
        }

        public static readonly ShaderProfile OpenGL_Mojo = FromType(ShaderProfileType.OpenGL_Mojo);

        public static readonly ShaderProfile DirectX_11 = FromType(ShaderProfileType.DirectX_11);

        /// <summary>
        /// Returns all the loaded shader profiles.
        /// </summary>
        public static IEnumerable<ShaderProfile> All
        {
            get { return _profiles; }
        }

        /// <summary>
        /// Returns the name of the shader profile.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Returns the format identifier used in the MGFX file format.
        /// </summary>
        public ShaderProfileType ProfileType { get; private set; }

        internal abstract bool Supports(TargetPlatform platform);
        
        /// <summary>
        /// Returns the correct profile for the named platform or
        /// null if no supporting profile is found.
        /// </summary>
        public static ShaderProfile FromPlatform(TargetPlatform platform)
        {
            return _profiles.FirstOrDefault(p => p.Supports(platform));
        }

        /// <summary>
        /// Returns the profile by type or null if no match is found.
        /// </summary>
        public static ShaderProfile FromType(ShaderProfileType profileType)
        {
            return _profiles.FirstOrDefault(p => p.ProfileType == profileType);
        }

        internal abstract IEnumerable<KeyValuePair<string,string>> GetMacros();

        internal abstract void ValidateShaderModels(PassInfo pass);

        internal abstract ShaderData CreateShader(ShaderResult shaderResult, string shaderFunction, string shaderProfile, bool isVertexShader, EffectObject effect, ref string errorsAndWarnings);

        protected static void ParseShaderModel(string text, Regex regex, out int major, out int minor)
        {
            Match match = regex.Match(text);
            if (!match.Success)
            {
                major = 0;
                minor = 0;
                return;
            }

            major = int.Parse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
            minor = int.Parse(match.Groups["minor"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture);
        }

        private class StringConverter : TypeConverter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string name = value as string;

                    foreach (ShaderProfile e in All)
                    {
                        if (e.Name == name)
                            return e;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
