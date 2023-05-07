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
        private Android.Media.MediaPlayer _player;

        internal Android.Media.MediaPlayer Player { get { return _player; } }


        internal ConcreteVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan duration)
            : base(graphicsDevice, fileName, duration)
        {
            _player = new Android.Media.MediaPlayer();

            var afd = AndroidGameWindow.Activity.Assets.OpenFd(FileName);
            if (afd != null)
            {
                _player.SetDataSource(afd.FileDescriptor, afd.StartOffset, afd.Length);
                _player.Prepare();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_player != null)
                {
                    _player.Dispose();
                    _player = null;
                }

            }

            base.Dispose(disposing);
        }
    }
}
