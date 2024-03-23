#region File Description
//-----------------------------------------------------------------------------
// Program.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

#if IOS || TVOS
using MonoTouch.Foundation;
using MonoTouch.UIKit;
#endif


namespace MonoTest
{
	[Register ("AppDelegate")]
	class Program : UIApplicationDelegate 
	{
		public override void FinishedLaunching (UIApplication app)
		{
			// Fun begins..
			using (Game1 game = new Game1())
            {
                game.Run();
            }
			
			//MediaLibrary lib = new MediaLibrary();
			//object result = lib.Playlists;
		}

		static void Main (string [] args)
		{
			UIApplication.Main (args,null,"AppDelegate");
		}
	}
}

