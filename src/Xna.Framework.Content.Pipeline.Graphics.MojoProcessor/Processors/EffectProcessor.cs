// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes a string representation to a platform-specific compiled effect.
    /// </summary>
    [ContentProcessor(DisplayName = "Effect - MonoGame")]
    public class EffectProcessor : ContentProcessor<EffectContent, CompiledEffectContent>
    {
        EffectProcessorDebugMode debugMode;
        string defines;

        /// <summary>
        /// The debug mode for compiling effects.
        /// </summary>
        /// <value>The debug mode to use when compiling effects.</value>
        public virtual EffectProcessorDebugMode DebugMode { get { return debugMode; } set { debugMode = value; } }

        /// <summary>
        /// Define assignments for the effect.
        /// </summary>
        /// <value>A list of define assignments delimited by semicolons.</value>
        public virtual string Defines { get { return defines; } set { defines = value; } }
        

        /// <summary>
        /// Initializes a new instance of EffectProcessor.
        /// </summary>
        public EffectProcessor()
        {
        }

        /// <summary>
        /// Processes the string representation of the specified effect into a platform-specific binary format using the specified context.
        /// </summary>
        /// <param name="input">The effect string to be processed.</param>
        /// <param name="context">Context for the specified processor.</param>
        /// <returns>A platform-specific compiled binary effect.</returns>
        /// <remarks>If you get an error during processing, compilation stops immediately. The effect processor displays an error message. Once you fix the current error, it is possible you may get more errors on subsequent compilation attempts.</remarks>
        public override CompiledEffectContent Process(EffectContent input, ContentProcessorContext context)
        {
            if (DebugMode == EffectProcessorDebugMode.Auto)
            {
                if (String.Equals(context.BuildConfiguration, "Debug", StringComparison.OrdinalIgnoreCase))
                    DebugMode = EffectProcessorDebugMode.Debug;
            }

            ShaderProfile shaderProfile = EffectProcessor.FromPlatform(context.TargetPlatform);
            if (shaderProfile == null)
                throw new InvalidContentException(string.Format("{0} effects are not supported.", context.TargetPlatform), input.Identity);
            
            EffectObject effectObject;
            try
            {
                string fullFilePath = Path.GetFullPath(input.Identity.SourceFilename);

                // Preprocess the FX file expanding includes and macros.
                string effectCode = Preprocess(input, context, shaderProfile, fullFilePath);

                effectObject = ProcessTechniques(input, context, shaderProfile, fullFilePath, effectCode);
            }
            catch (InvalidContentException)
            {
                throw;
            }
            catch (Exception ex)
            {
                // TODO: Extract good line numbers from mgfx parser!
                throw new InvalidContentException(ex.Message, input.Identity, ex);
            }

            // Write out the effect to a runtime format.
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        Write(effectObject, writer, shaderProfile.ProfileType);
                        byte[] effectBytecode = stream.ToArray();
                        CompiledEffectContent result = new CompiledEffectContent(effectBytecode);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidContentException("Failed to serialize the effect!", input.Identity, ex);
            }
        }

        /// <summary>
        /// Returns the correct profile for the named platform or
        /// null if no supporting profile is found.
        /// </summary>
        public static ShaderProfile FromPlatform(TargetPlatform platform)
        {
            switch (platform)
            {
                case TargetPlatform.Windows:
                case TargetPlatform.WindowsStoreApp:
                    return ShaderProfile.DirectX_11;

                case TargetPlatform.iOS:
                case TargetPlatform.Android:
                case TargetPlatform.BlazorGL:
                case TargetPlatform.DesktopGL:
                case TargetPlatform.MacOSX:
                case TargetPlatform.RaspberryPi:
                    return ShaderProfile.OpenGL_Mojo;

                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Returns the profile by type or null if no match is found.
        /// </summary>
        public static ShaderProfile FromType(ShaderProfileType profileType)
        {
            switch (profileType)
            {
                case ShaderProfileType.DirectX_11:
                    return ShaderProfile.DirectX_11;

                case ShaderProfileType.OpenGL_Mojo:
                    return ShaderProfile.OpenGL_Mojo;

                default:
                    throw new InvalidOperationException();
            }
        }

        // Pre-process the file,
        // resolving all #includes and macros.
        private string Preprocess(EffectContent input, ContentProcessorContext context, ShaderProfile shaderProfile, string fullFilePath)
        {
            Preprocessor pp = new Preprocessor();

            // deprecated macro. Left for backward compatibility with MonoGame.
            pp.AddMacro("MGFX", "1");

            // If we're building shaders for debug set that flag too.
            if (DebugMode == EffectProcessorDebugMode.Debug)
            {
                pp.AddMacro("__DEBUG__", "1");

                // deprecated macro. Left for backward compatibility with MonoGame.
                pp.AddMacro("DEBUG", "1");
            }

            foreach (KeyValuePair<string,string> macro in shaderProfile.GetMacros())
                pp.AddMacro(macro.Key, macro.Value);

            if (!string.IsNullOrEmpty(Defines))
            {
                string[] defines = Defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string define in defines)
                {
                    string name = define;
                    string value = "1";
                    if (define.Contains("="))
                    {
                        string[] parts = define.Split('=');

                        if (parts.Length > 0)
                            name = parts[0].Trim();

                        if (parts.Length > 1)
                            value = parts[1].Trim();
                    }

                    pp.AddMacro(name, value);
                }
            }

            string effectCode = pp.Preprocess(input, context, fullFilePath);

            return effectCode;
        }


        private EffectObject ProcessTechniques(EffectContent input, ContentProcessorContext context, ShaderProfile shaderProfile, string fullFilePath, string effectCode)
        {
            // Parse the resulting file for techniques and passes.
            ParseTree tree = new Parser(new Scanner()).Parse(effectCode, fullFilePath);
            if (tree.Errors.Count > 0)
            {
                string errors = String.Empty;
                foreach (ParseError error in tree.Errors)
                {

                    errors += string.Format("{0}({1},{2}) : {3}\r\n", error.File, error.Line, error.Column, error.Message);
                }

                throw new InvalidContentException(errors, input.Identity);
            }

            // Evaluate the results of the parse tree.
            ShaderInfo shaderInfo = tree.Eval() as ShaderInfo;

            // Remove the samplers and techniques so that the shader compiler
            // gets a clean file without any FX file syntax in it.
            string cleanFile = effectCode;
            WhitespaceNodes(TokenType.Technique_Declaration, tree.Nodes, ref cleanFile);
            WhitespaceNodes(TokenType.Sampler_Declaration_States, tree.Nodes, ref cleanFile);


            // Remove empty techniques.
            for (int i = 0; i < shaderInfo.Techniques.Count; i++)
            {
                TechniqueInfo tech = shaderInfo.Techniques[i];
                if (tech.Passes.Count <= 0)
                {
                    shaderInfo.Techniques.RemoveAt(i);
                    i--;
                }
            }

            // We must have at least one technique.
            if (shaderInfo.Techniques.Count <= 0)
                throw new InvalidContentException("The effect must contain at least one technique and pass!",
                    input.Identity);


            // Create the effect object.
            EffectObject effectObject = null;
            string shaderErrorsAndWarnings = String.Empty;
            try
            {
                effectObject = EffectObject.CompileEffect(shaderProfile, shaderInfo, fullFilePath, cleanFile, this.DebugMode, out shaderErrorsAndWarnings);
            }
            catch (ShaderCompilerException)
            {
                // This will log any warnings and errors and throw.
                ProcessErrorsAndWarnings(true, shaderErrorsAndWarnings, input, context);
            }

            // Process any warning messages that the shader compiler might have produced.
            ProcessErrorsAndWarnings(false, shaderErrorsAndWarnings, input, context);

            return effectObject;
        }

        private static void WhitespaceNodes(TokenType type, List<ParseNode> nodes, ref string sourceFile)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                ParseNode n = nodes[i];
                if (n.Token.Type != type)
                {
                    WhitespaceNodes(type, n.Nodes, ref sourceFile);
                    continue;
                }

                // Get the full content of this node.
                int start = n.Token.StartPos;
                int end = n.Token.EndPos;
                int length = end - n.Token.StartPos;
                string content = sourceFile.Substring(start, length);

                // Replace the content of this node with whitespace.
                for (int c = 0; c < length; c++)
                {
                    if (!char.IsWhiteSpace(content[c]))
                        content = content.Replace(content[c], ' ');
                }

                // Add the whitespace back to the source file.
                string newfile = sourceFile.Substring(0, start);
                newfile += content;
                newfile += sourceFile.Substring(end);
                sourceFile = newfile;
            }
        }

        private const string MGFXHeader = "MGFX";
        private const int Version = 10;

        /// <summary>
        /// Writes the effect for loading later.
        /// </summary>
        private void Write(EffectObject effect, BinaryWriter writer, ShaderProfileType profileType)
        {
            // Write a very simple header for identification and versioning.
            writer.Write(MGFXHeader.ToCharArray());
            writer.Write((byte)Version);

            // Write an simple identifier for DX11 vs GLSL
            // so we can easily detect the correct shader type.
            writer.Write((byte)profileType);

            // Write the rest to a memory stream.
            using (MemoryStream memStream = new MemoryStream())
            using (EffectObjectWriter memWriter = new EffectObjectWriter(memStream, Version, profileType))
            {
                memWriter.WriteEffect(effect);

                // Calculate a hash code from memory stream
                // and write it to the header.
                int effectKey = MonoGame.Framework.Utilities.Hash.ComputeHash(memStream);
                writer.Write((Int32)effectKey);

                //write content from memory stream to final stream.
                memStream.WriteTo(writer.BaseStream);
            }

            // Write a tail to be used by the reader for validation.
            if (Version >= 10)
                writer.Write(MGFXHeader.ToCharArray());
        }

        private static void ProcessErrorsAndWarnings(bool buildFailed, string shaderErrorsAndWarnings, EffectContent input, ContentProcessorContext context)
        {
            // Split the errors and warnings into individual lines.
            string[] errorsAndWarningArray = shaderErrorsAndWarnings.Split(new[] { "\n", "\r", Environment.NewLine },
                                                                      StringSplitOptions.RemoveEmptyEntries);

            Regex errorOrWarning = new Regex(@"(?<Filename>.*)\((?<LineAndColumn>[0-9]*(,([0-9]+)(-[0-9]+)?)?)\)\s*:\s*(?<ErrorType>error|warning)\s*(?<ErrorCode>[A-Z0-9]*)\s*:\s*(?<Message>.*)", RegexOptions.Compiled);
            ContentIdentity identity = null;
            string allErrorsAndWarnings = string.Empty;

            // Process all the lines.
            for (int i = 0; i < errorsAndWarningArray.Length; i++)
            {
                Match match = errorOrWarning.Match(errorsAndWarningArray[i]);
                if (!match.Success) // || match.Groups.Count != 8)
                {
                    // Just log anything we don't recognize as a warning.
                    context.Logger.LogWarning(string.Empty, input.Identity, errorsAndWarningArray[i]);

                    continue;
                }

                string fileName = match.Groups["Filename"].Value;
                string lineAndColumn = match.Groups["LineAndColumn"].Value;
                string errorType = match.Groups["ErrorType"].Value;
                string errorCode = match.Groups["ErrorCode"].Value;
                string message = match.Groups["Message"].Value;

                // Try to ensure a good file name for the error message.
                if (string.IsNullOrEmpty(fileName))
                    fileName = input.Identity.SourceFilename;
                else if (!File.Exists(fileName))
                {
                    string folder = Path.GetDirectoryName(input.Identity.SourceFilename);
                    fileName = Path.Combine(folder, fileName);
                }

                identity = new ContentIdentity(fileName, input.Identity.SourceTool, lineAndColumn);
                string errorCodeAndMessage = string.Format("{0}:{1}", errorCode, message);

                switch (errorType)
                {
                    case "warning":
                        context.Logger.LogWarning(string.Empty, identity, errorCodeAndMessage);
                        break;
                    case "error":
                        throw new InvalidContentException(errorCodeAndMessage, identity);
                    default:
                        // log anything we didn't recognize as a warning.
                        if (allErrorsAndWarnings != string.Empty)
                            context.Logger.LogWarning(string.Empty, input.Identity, errorsAndWarningArray[i]);
                        break;
                }
            }

            if (buildFailed)
                throw new InvalidContentException(allErrorsAndWarnings, identity ?? input.Identity);
        }
    }
}
