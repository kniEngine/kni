using System;

namespace $ext_safeprojectname$
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
            using (var game = new $ext_safeprojectname$Game())
                game.Run();
        }
    }
}
