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
using Microsoft.Xna.Platform.Graphics.Utilities;

namespace Microsoft.Xna.Framework.Content.Pipeline.Processors
{
    /// <summary>
    /// Processes a string representation to a platform-specific compiled effect.
    /// </summary>
    [ContentProcessor(DisplayName = "Effect - KNI")]
    public class EffectProcessor : ContentProcessor<EffectContent, CompiledEffectContent>
    {
        private EffectProcessorDebugMode _debugMode;
        private string _defines;

        /// <summary>
        /// The debug mode for compiling effects.
        /// </summary>
        /// <value>The debug mode to use when compiling effects.</value>
        public virtual EffectProcessorDebugMode DebugMode { get { return _debugMode; } set { _debugMode = value; } }

        /// <summary>
        /// Define assignments for the effect.
        /// </summary>
        /// <value>A list of define assignments delimited by semicolons.</value>
        public virtual string Defines { get { return _defines; } set { _defines = value; } }
        

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
                // TODO: Extract good line numbers from fx parser!
                throw new InvalidContentException(ex.Message, input.Identity, ex);
            }

            // Write out the effect to a runtime format.
            try
            {
                using (MemoryStream stream = new MemoryStream())
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    // Write a very simple header for identification and versioning.
                    writer.Write(KNIFXWriter11.KNIFXSignature.ToCharArray());
                    writer.Write((byte)KNIFXWriter11.Version);

                    // Write an simple identifier for DX11 vs GLSL
                    // so we can easily detect the correct shader type.
                    writer.Write((byte)shaderProfile.ProfileType);

                    using (MemoryStream fxStream = new MemoryStream())
                    using (KNIFXWriter11 fxWriter = new KNIFXWriter11(fxStream))
                    {
                        fxWriter.WriteEffect(effectObject);

                        // Calculate a hash code from memory stream and write it to the header.
                        int effectKey = HashHelper.ComputeHash(fxStream);
                        writer.Write((Int32)effectKey);

                        //write content from memory stream to final stream.
                        fxStream.WriteTo(writer.BaseStream);
                    }

                    byte[] effectBytecode = stream.ToArray();
                    return new CompiledEffectContent(effectBytecode);
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
            Preprocessor pp = new Preprocessor(input, context, fullFilePath);

            pp.AddMacro("__KNIFX__", "1");

            // deprecated macro. Left for backward compatibility with MonoGame.
            pp.AddMacro("MGFX", "1");

            // If we're building shaders for debug set that flag too.
            if (DebugMode == EffectProcessorDebugMode.Debug)
            {
                pp.AddMacro("__DEBUG__", "1");

                // deprecated macro. Left for backward compatibility with MonoGame.
                pp.AddMacro("DEBUG", "1");
            }

            pp.AddMacros(shaderProfile.GetMacros());

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

            string effectCode = pp.Preprocess();

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
                effectObject = EffectProcessor.CompileEffect(input, context, shaderProfile, shaderInfo, fullFilePath, cleanFile, this.DebugMode, out shaderErrorsAndWarnings);
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

        private static EffectObject CompileEffect(EffectContent input, ContentProcessorContext context, ShaderProfile shaderProfile, ShaderInfo shaderInfo, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, out string errorsAndWarnings)
        {
            errorsAndWarnings = string.Empty;

            EffectObject effect = new EffectObject();
            // These are filled out as we process stuff.
            effect.ConstantBuffers = new List<ConstantBufferData>();
            effect.Shaders = new List<ShaderData>();

            // Go thru the techniques and that will find all the 
            // shaders and constant buffers.
            effect.Techniques = new EffectObject.EffectTechniqueContent[shaderInfo.Techniques.Count];
            for (int t = 0; t < shaderInfo.Techniques.Count; t++)
            {
                TechniqueInfo tinfo = shaderInfo.Techniques[t];

                EffectObject.EffectTechniqueContent technique = new EffectObject.EffectTechniqueContent();
                technique.name = tinfo.name;
                technique.pass_count = (uint)tinfo.Passes.Count;
                technique.pass_handles = new EffectObject.EffectPassContent[tinfo.Passes.Count];

                for (int p = 0; p < tinfo.Passes.Count; p++)
                {
                    PassInfo pinfo = tinfo.Passes[p];

                    EffectObject.EffectPassContent pass = new EffectObject.EffectPassContent();
                    pass.name = pinfo.name ?? string.Empty;

                    pass.blendState = pinfo.blendState;
                    pass.depthStencilState = pinfo.depthStencilState;
                    pass.rasterizerState = pinfo.rasterizerState;

                    pass.state_count = 0;
                    EffectObject.EffectStateContent[] tempstate = new EffectObject.EffectStateContent[3];

                    if (!string.IsNullOrEmpty(pinfo.psFunction))
                    {
                        ShaderVersion psShaderVersion = ShaderVersion.ParsePixelShaderModel(pinfo.psModel);
                        shaderProfile.ValidateShaderModels(pinfo, pinfo.psFunction, pinfo.psModel, ShaderStage.Pixel, psShaderVersion);

                        pass.state_count += 1;
                        tempstate[pass.state_count - 1] = EffectProcessor.CreateShader(input, context, effect, shaderInfo, shaderProfile, fullFilePath, fileContent, debugMode, pinfo.psFunction, pinfo.psModel, ShaderStage.Pixel, psShaderVersion, ref errorsAndWarnings);
                    }

                    if (!string.IsNullOrEmpty(pinfo.vsFunction))
                    {
                        ShaderVersion vsShaderVersion = ShaderVersion.ParseVertexShaderModel(pinfo.vsModel);
                        shaderProfile.ValidateShaderModels(pinfo, pinfo.vsFunction, pinfo.vsModel, ShaderStage.Pixel, vsShaderVersion);

                        pass.state_count += 1;
                        tempstate[pass.state_count - 1] = EffectProcessor.CreateShader(input, context, effect, shaderInfo, shaderProfile, fullFilePath, fileContent, debugMode, pinfo.vsFunction, pinfo.vsModel, ShaderStage.Vertex, vsShaderVersion, ref errorsAndWarnings);
                    }

                    if (!string.IsNullOrEmpty(pinfo.csFunction))
                    {
                        ShaderVersion csShaderVersion = ShaderVersion.ParseComputeShaderModel(pinfo.csModel);
                        shaderProfile.ValidateShaderModels(pinfo, pinfo.csFunction, pinfo.csModel, ShaderStage.Compute, csShaderVersion);

                        pass.state_count += 1;
                        tempstate[pass.state_count - 1] = EffectProcessor.CreateShader(input, context, effect, shaderInfo, shaderProfile, fullFilePath, fileContent, debugMode, pinfo.csFunction, pinfo.csModel, ShaderStage.Compute, csShaderVersion, ref errorsAndWarnings);
                    }

                    pass.states = new EffectObject.EffectStateContent[pass.state_count];
                    for (int s = 0; s < pass.state_count; s++)
                        pass.states[s] = tempstate[s];

                    technique.pass_handles[p] = pass;
                }

                effect.Techniques[t] = technique;
            }

            // Make the list of parameters by combining all the
            // constant buffers ignoring the buffer offsets.
            List<EffectObject.EffectParameterContent> parameters = new List<EffectObject.EffectParameterContent>();
            for (int c = 0; c < effect.ConstantBuffers.Count; c++)
            {
                ConstantBufferData cb = effect.ConstantBuffers[c];

                for (int i = 0; i < cb.Parameters.Count; i++)
                {
                    EffectObject.EffectParameterContent param = cb.Parameters[i];

                    int match = parameters.FindIndex(e => e.name == param.name);
                    if (match == -1)
                    {
                        cb.ParameterIndex.Add(parameters.Count);
                        parameters.Add(param);
                    }
                    else
                    {
                        // TODO: Make sure the type and size of 
                        // the parameter match up!
                        cb.ParameterIndex.Add(match);
                    }
                }
            }

            // Add the texture parameters from the samplers.
            foreach (ShaderData shader in effect.Shaders)
            {
                for (int s = 0; s < shader._samplers.Length; s++)
                {
                    SamplerInfo samplerInfo = shader._samplers[s];

                    int textureParameterIdx = parameters.FindIndex((paramContent) => paramContent.name == samplerInfo.textureName);
                    if (textureParameterIdx != -1)
                    {
                        // TODO: Make sure the type and size of
                        // the parameter match up!
                        shader._samplers[s].textureParameter = textureParameterIdx;
                    }
                    else if (samplerInfo.textureName != null)
                    {
                        EffectObject.EffectParameterContent param = new EffectObject.EffectParameterContent();
                        param.class_ = EffectObject.PARAMETER_CLASS.OBJECT;
                        param.name = samplerInfo.textureName;
                        param.semantic = string.Empty;
                        param.type = SamplerTypeToParameterType(samplerInfo.type);
                        parameters.Add(param);

                        // Store the index for runtime lookup.
                        textureParameterIdx = parameters.Count-1;
                        shader._samplers[s].textureParameter = textureParameterIdx;
                    }
                }
            }

            // TODO: Annotations are part of the .FX format and
            // not a part of shaders... we need to implement them
            // in our fx parser if we want them back.

            effect.Parameters = parameters.ToArray();

            return effect;
        }

        private static EffectObject.PARAMETER_TYPE SamplerTypeToParameterType(MojoShader.SamplerType samplerType)
        {
            switch (samplerType)
            {
                case MojoShader.SamplerType.SAMPLER_1D:
                    return EffectObject.PARAMETER_TYPE.TEXTURE1D;

                case MojoShader.SamplerType.SAMPLER_2D:
                    return EffectObject.PARAMETER_TYPE.TEXTURE2D;

                case MojoShader.SamplerType.SAMPLER_VOLUME:
                    return EffectObject.PARAMETER_TYPE.TEXTURE3D;

                case MojoShader.SamplerType.SAMPLER_CUBE:
                    return EffectObject.PARAMETER_TYPE.TEXTURECUBE;

                default:
                    throw new InvalidOperationException();
            }
        }

        internal static EffectObject.EffectStateContent CreateShader(EffectContent input, ContentProcessorContext context, EffectObject effect, ShaderInfo shaderInfo, ShaderProfile shaderProfile, string fullFilePath, string fileContent, EffectProcessorDebugMode debugMode, string shaderFunction, string shaderProfileName, ShaderStage shaderStage, ShaderVersion shaderVersion, ref string errorsAndWarnings)
        {
            // Check if this shader has already been created.

            // The index of the shader in the shared list.
            int sharedIndex = effect.Shaders.FindIndex(shader => shader.ShaderFunctionName == shaderFunction && shader.ShaderProfile == shaderProfileName);
            if (sharedIndex == -1)
            {
                // Compile and create the shader.
                ShaderData shaderData = shaderProfile.CreateShader(input, context, effect, shaderInfo, fullFilePath, fileContent, debugMode, shaderFunction, shaderProfileName, shaderStage, ref errorsAndWarnings);
                shaderData.ShaderFunctionName = shaderFunction;
                shaderData.ShaderProfile = shaderProfileName;
                shaderData.ShaderVersion = shaderVersion;

                sharedIndex = effect.Shaders.Count;
                effect.Shaders.Add(shaderData);
            }

            EffectObject.EffectStateContent state = new EffectObject.EffectStateContent();
            state.index = 0;
            state.type = EffectObject.STATE_TYPE.CONSTANT;
            state.parameter = new EffectObject.EffectParameterContent();
            state.parameter.name = string.Empty;
            state.parameter.semantic = string.Empty;
            state.parameter.class_ = EffectObject.PARAMETER_CLASS.OBJECT;
            state.parameter.rows = 0;
            state.parameter.columns = 0;
            state.parameter.data = sharedIndex;
            switch (shaderStage)
            {
                case ShaderStage.Vertex:
                    state.operation = (uint)146;
                    state.parameter.type = EffectObject.PARAMETER_TYPE.VERTEXSHADER;
                    break;
                case ShaderStage.Pixel:
                    state.operation = (uint)147;
                    state.parameter.type = EffectObject.PARAMETER_TYPE.PIXELSHADER;
                    break;
                case ShaderStage.Compute:
                    state.operation = (uint)148;
                    state.parameter.type = EffectObject.PARAMETER_TYPE.COMPUTESHADER;
                    break;
                default:
                    throw new InvalidOperationException("Unsupported shader stage: " + shaderStage);
            }

            return state;
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
