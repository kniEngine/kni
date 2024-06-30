using System;
using Foundation;
using UIKit;

namespace $safeprojectname$
{
    [Register("AppDelegate")]
    class Program : UIApplicationDelegate
    {
        private static $safeprojectname$Game game;

        internal static void RunGame()
        {
            game = new $safeprojectname$Game();
            game.Run();
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            UIApplication.Main(args, null, "AppDelegate");
        }

        public override void FinishedLaunching(UIApplication app)
        {
            RunGame();
        }
    }
}
