// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteVideoStrategy : VideoStrategy
    {
        internal Android.Media.MediaPlayer Player { get; private set; }


        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            Player = new Android.Media.MediaPlayer();
            if (Player != null)
            {
                var afd = AndroidGameWindow.Activity.Assets.OpenFd(FileName);
                if (afd != null)
                {
                    Player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                    Player.Prepare();
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            /* PlatformDispose(...) disabled in https://github.com/MonoGame/MonoGame/pull/2406
            if (Player != null)
            {
                Player.Dispose();
                Player = null;
            }
            */

            if (disposing)
            {

            }

            base.Dispose(disposing);
        }
    }
}
