// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        private TitlePlatform _platform = TitlePlatform.Android;

        public override string Location { get { return string.Empty; } }

        public override TitlePlatform Platform { get { return _platform; } }

        public ConcreteTitleContainer() : base()
        {
        }

        public override Stream PlatformOpenStream(string name)
        {
            Stream stream = Android.App.Application.Context.Assets.Open(name, Android.Content.Res.Access.Random);

            // This results in a ~50% reduction in load times on Android due to slow Android asset streams.
            stream = new BufferedStream(stream);

            return stream;
        }
    }
}

