// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using D3DC = SharpDX.D3DCompiler;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    [TypeConverter(typeof(StringConverter))]
    public abstract class ShaderProfile
    {
        protected ShaderProfile()
        {
        }

        public static readonly ShaderProfile DirectX_11 = new ShaderProfileDX11();

        public static readonly ShaderProfile OpenGL_Mojo = new ShaderProfileGL();


        /// <summary>
        /// Returns the name of the shader profile.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Returns the format identifier used in the MGFX file format.
        /// </summary>
        public abstract ShaderProfileType ProfileType { get; }

        internal abstract IEnumerable<KeyValuePair<string,string>> GetMacros();

        internal abstract void ValidateShaderModels(PassInfo pass);

        internal abstract ShaderData CreateShader(EffectContent input, ContentProcessorContext context, EffectObject effect, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderStage shaderStage, ref string errorsAndWarnings);

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


        internal static D3DC.ShaderBytecode CompileHLSL(EffectContent input, ContentProcessorContext context, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, bool backwardsCompatibility, ref string errorsAndWarnings)
        {
            try
            {
                D3DC.ShaderFlags shaderFlags = 0;

                // While we never allow preshaders, this flag is invalid for
                // the DX11 shader compiler which doesn't allow preshaders
                // in the first place.
                //shaderFlags |= D3DC.ShaderFlags.NoPreshader;

                if (backwardsCompatibility)
                    shaderFlags |= D3DC.ShaderFlags.EnableBackwardsCompatibility;

                if (debugMode == Processors.EffectProcessorDebugMode.Debug)
                {
                    shaderFlags |= D3DC.ShaderFlags.SkipOptimization;
                    shaderFlags |= D3DC.ShaderFlags.Debug;
                }
                else
                {
                    shaderFlags |= D3DC.ShaderFlags.OptimizationLevel3;
                }

                // Compile the shader into bytecode.                
                D3DC.CompilationResult result = D3DC.ShaderBytecode.Compile(
                    fileContent,
                    shaderFunction,
                    shaderProfileName,
                    shaderFlags,
                    0,
                    null,
                    null,
                    fullFilePath);

                // Store all the errors and warnings to log out later.
                errorsAndWarnings += result.Message;

                if (result.Bytecode == null)
                    throw new ShaderCompilerException();

                D3DC.ShaderBytecode shaderBytecode = result.Bytecode;
                //string source = shaderByteCode.Disassemble();

                return shaderBytecode;
            }
            catch (SharpDX.CompilationException ex)
            {
                errorsAndWarnings += ex.Message;
                throw new ShaderCompilerException();
            }
        }
        

        private class StringConverter : TypeConverter
        {
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if (value is string)
                {
                    string name = value as string;

                    if (ShaderProfile.DirectX_11.Name == name)
                        return ShaderProfile.DirectX_11;

                    if (ShaderProfile.OpenGL_Mojo.Name == name)
                        return ShaderProfile.OpenGL_Mojo;
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
