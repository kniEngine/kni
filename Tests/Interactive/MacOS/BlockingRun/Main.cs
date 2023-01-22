using System;
using System.Drawing;

using Microsoft.Xna.Framework;

using MonoMac.AppKit;
using MonoMac.Foundation;

namespace BlockingRun
{
    class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();

            Console.WriteLine("Starting a blocking game instance...");
            using (var pool = new NSAutoreleasePool())
            using (var game = new BlockingRunGame())
            {
                game.Run();
            }

            Console.WriteLine("Starting a second blocking game instance...");
            using (var pool = new NSAutoreleasePool())
            using (var game = new BlockingRunGame())
            {
                game.Run();
            }

            Console.WriteLine("Fin");
        }
    }
}
