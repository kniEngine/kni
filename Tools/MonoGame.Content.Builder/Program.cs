﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;


namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    class Program
    {
        static int Main(string[] args)
        {
            // We force all stderr to redirect to stdout
            // to avoid any out of order console output.
            Console.SetError(Console.Out);

            if (!Environment.Is64BitProcess && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                Console.Error.WriteLine("The KNI content Builder tool only work on a 64bit OS.");
                return -1;
            }

            var content = new BuildContent();

            // Parse the command line.
            var parser = new CommandLineParser(content)
            {
                Title = "KNI Content Builder\n" +
                        "Builds optimized game content for KNI projects."
            };

            if (!parser.Parse(args))
                return -1;
            
            // Launch debugger if requested.
            if (content.LaunchDebugger)
            {
                try
                {
                    System.Diagnostics.Debugger.Launch();
                }
                catch (NotImplementedException)
                {
                    // not implemented under Mono
                    Console.Error.WriteLine("The debugger is not implemented under Mono and thus is not supported on your platform.");
                }
            }

            // Print a startup message.
            var buildStarted = DateTime.Now;
            if (!content.Quiet)
                Console.WriteLine("Build started {0}\n", buildStarted);

            // Let the content build.
            content.Build();

            // Print the finishing info.
            if (!content.Quiet)
            {
                Console.WriteLine("\nBuild {0} succeeded, {1} failed.\n", content.SuccessCount, content.ErrorCount);
                Console.WriteLine("Time elapsed {0:hh\\:mm\\:ss\\.ff}.", DateTime.Now - buildStarted);
            }

            // Return the error count.
            return content.ErrorCount;
        }
    }
}
