// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace MonoGame.EffectCompiler
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var options = new Options();
            var parser = new CommandLineParser(options);
            parser.Title = "mgfxc - The MonoGame Effect compiler.";

            if (!parser.ParseCommandLine(args))
                return 1;
            
            // Validate the input file exits.
            if (!File.Exists(options.SourceFile))
            {
                Console.Error.WriteLine("The input file '{0}' was not found!", options.SourceFile);
                return 1;
            }
            
            try
            {
                var logger = new BuildLogger();

                var importer = new EffectImporter();
                var importerContext = new ImporterContext(logger);
                var content = importer.Import(options.SourceFile, importerContext);

                var processor = new EffectProcessor();
                processor.DebugMode = (options.Debug) ? EffectProcessorDebugMode.Debug : EffectProcessorDebugMode.Optimize;
                processor.Defines = options.Defines;
                TargetPlatform targetPlatform;
                if (options.Profile == ShaderProfile.DirectX_11)
                    targetPlatform = TargetPlatform.Windows;
                else if (options.Profile == ShaderProfile.OpenGL)
                    targetPlatform = TargetPlatform.DesktopGL;
                else
                    throw new InvalidOperationException("");

                var processorContext = new ProcessorContext(logger, targetPlatform, options.OutputFile);
                var output = processor.Process(content, processorContext);

                var effectCode = output.GetEffectCode();
                File.WriteAllBytes(options.OutputFile, effectCode);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            
            // We finished succesfully.
            Console.WriteLine("Compiled '{0}' to '{1}'.", options.SourceFile, options.OutputFile);
            return 0;
        }

    }
}
