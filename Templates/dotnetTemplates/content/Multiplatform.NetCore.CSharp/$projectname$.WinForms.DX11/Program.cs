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
//-:cnd:noEmit
#if OculusOVR
            // Enable VR with the nkast.Kni.Platform.WinForms.DX11.OculusOVR package.
            Microsoft.Xna.Platform.XR.XRFactory.RegisterXRFactory(new Microsoft.Xna.Platform.XR.LibOVR.ConcreteXRFactory());
#endif
//+:cnd:noEmit
            using (Game game = new Platform$safeprojectname$Game())
                game.Run();
        }
    }
}
