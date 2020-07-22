using System;

namespace $ext_safeprojectname$
{
#if WINDOWS || LINUX
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
#endif
}
