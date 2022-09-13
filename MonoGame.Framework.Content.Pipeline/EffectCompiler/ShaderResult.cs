// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    public class ShaderResult
    {
        public ShaderInfo ShaderInfo { get; private set; }

        public string FilePath { get; private set; }

        public string FileContent { get; private set; }

        public List<string> AdditionalOutputFiles { get; private set; }

        public ShaderProfile Profile { get; private set; }

        public EffectProcessorDebugMode Debug { get; private set; }

        static internal ShaderResult FromString(EffectContent input, ContentProcessorContext context, ShaderProfile profile, EffectProcessorDebugMode debugMode, string effectCode)
        {
            // Parse the resulting file for techniques and passes.
            var fullPath = Path.GetFullPath(input.Identity.SourceFilename);
            var tree = new Parser(new Scanner()).Parse(effectCode, fullPath);
            if (tree.Errors.Count > 0)
            {
                var errors = String.Empty;
                foreach (var error in tree.Errors)
                {
                    
                    errors += string.Format("{0}({1},{2}) : {3}\r\n", error.File, error.Line, error.Column, error.Message);
                }

                throw new InvalidContentException(errors, input.Identity);
            }

            // Evaluate the results of the parse tree.
            var shaderInfo = tree.Eval() as ShaderInfo;

            // Remove the samplers and techniques so that the shader compiler
            // gets a clean file without any FX file syntax in it.
            var cleanFile = effectCode;
            WhitespaceNodes(TokenType.Technique_Declaration, tree.Nodes, ref cleanFile);
            WhitespaceNodes(TokenType.Sampler_Declaration_States, tree.Nodes, ref cleanFile);

            // Setup the rest of the shader info.
            ShaderResult result = new ShaderResult();
            result.ShaderInfo = shaderInfo;
            result.FilePath = fullPath;
            result.FileContent = cleanFile;
            result.AdditionalOutputFiles = new List<string>();

            // Remove empty techniques.
            for (var i = 0; i < shaderInfo.Techniques.Count; i++)
            {
                var tech = shaderInfo.Techniques[i];
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

            result.Profile = profile;
            result.Debug = debugMode;

            return result;
        }

        public static void WhitespaceNodes(TokenType type, List<ParseNode> nodes, ref string sourceFile)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n.Token.Type != type)
                {
                    WhitespaceNodes(type, n.Nodes, ref sourceFile);
                    continue;
                }

                // Get the full content of this node.
                var start = n.Token.StartPos;
                var end = n.Token.EndPos;
                var length = end - n.Token.StartPos;
                var content = sourceFile.Substring(start, length);

                // Replace the content of this node with whitespace.
                for (var c = 0; c < length; c++)
                {
                    if (!char.IsWhiteSpace(content[c]))
                        content = content.Replace(content[c], ' ');
                }

                // Add the whitespace back to the source file.
                var newfile = sourceFile.Substring(0, start);
                newfile += content;
                newfile += sourceFile.Substring(end);
                sourceFile = newfile;
            }
        }
    }
}
