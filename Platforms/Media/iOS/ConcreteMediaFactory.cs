// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using UIKit;


namespace Microsoft.Xna.Platform.Media
{
    public sealed class ConcreteMediaFactory : MediaFactory
    {
        public override MediaLibraryStrategy CreateMediaLibraryStrategy()
        {
            return new ConcreteMediaLibraryStrategy();
        }

        public override MediaLibraryStrategy CreateMediaLibraryStrategy(MediaSource mediaSource)
        {
            return new ConcreteMediaLibraryStrategy(mediaSource);
        }

        public override MediaPlayerStrategy CreateMediaPlayerStrategy()
        {
            return new ConcreteMediaPlayerStrategy();
        }

        public override SongStrategy CreateSongStrategy(string name, Uri streamSource)
        {
            return new ConcreteSongStrategy(name, streamSource);
        }

        public override VideoPlayerStrategy CreateVideoPlayerStrategy()
        {
            return new ConcreteVideoPlayerStrategy();
        }

        public override VideoStrategy CreateVideoStrategy(GraphicsDevice graphicsDevice, string fileName, TimeSpan timeSpan)
        {
            return new ConcreteVideoStrategy(graphicsDevice, fileName, timeSpan);
        }

        public override IList<MediaSource> GetAvailableMediaSources()
        {
            MediaSource[] result = { base.CreateMediaSource(UIDevice.CurrentDevice.SystemName, MediaSourceType.LocalDevice) };
            return result;
        }

    }
}
