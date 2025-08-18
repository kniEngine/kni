// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace EffectCompiler
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Options options = new Options();
            CommandLineParser parser = new CommandLineParser(options);
            parser.Title = "knifxc - The KNI Effect compiler.";

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
                ContentBuildLogger logger = new BuildLogger();

                ContentImporter<EffectContent> importer = new EffectImporter();
                ContentImporterContext importerContext = new ImporterContext(logger);
                EffectContent content = importer.Import(options.SourceFile, importerContext);

                EffectProcessor processor = new EffectProcessor();
                processor.Defines = options.Defines;

                if (options.Platform == (TargetPlatform)(-1))
                    throw new InvalidOperationException("Missing argument 'Platform'");

                ContentProcessorContext processorContext = new ProcessorContext(logger, options.Platform, options.Profile, options.OutputFile, options.Config);
                CompiledEffectContent output = processor.Process(content, processorContext);

                byte[] effectCode = output.GetEffectCode();
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
