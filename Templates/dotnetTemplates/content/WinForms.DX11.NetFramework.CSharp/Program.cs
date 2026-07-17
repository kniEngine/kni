using System;
using Microsoft.Xna.Framework;

namespace $safeprojectname$
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (Game game = new $safeprojectname$Game())
                game.Run();
        }
    }
}
