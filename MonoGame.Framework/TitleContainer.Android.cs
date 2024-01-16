// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {

        private void PlatformInit()
        {
        }

        private Stream PlatformOpenStream(string safeName)
        {
            Stream stream = Android.App.Application.Context.Assets.Open(safeName, Android.Content.Res.Access.Random);

            // This results in a ~50% reduction in load times on Android due to slow Android asset streams.
            stream = new BufferedStream(stream);

            return stream;
        }
    }
}

