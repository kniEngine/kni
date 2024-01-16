// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
#if IOS || TVOS
using Foundation;
using UIKit;
#endif

namespace Microsoft.Xna.Framework
{
    partial class TitleContainer
    {
        static partial void PlatformInit()
        {
            Location = NSBundle.MainBundle.ResourcePath;
#if IOS || TVOS
            SupportRetina = UIScreen.MainScreen.Scale >= 2.0f;
            RetinaScale = (int)Math.Round(UIScreen.MainScreen.Scale);
#endif
        }

#if IOS || TVOS
        static internal bool SupportRetina { get; private set; }
        static internal int RetinaScale { get; private set; }
#endif

        private static Stream PlatformOpenStream(string safeName)
        {
#if IOS || TVOS
            string absolutePath = Path.Combine(Location, safeName);
            if (SupportRetina)
            {
                for (int scale = RetinaScale; scale >= 2; scale--)
                {
                    // Insert the @#x immediately prior to the extension. If this file exists
                    // and we are on a Retina device, return this file instead.
                    string absolutePathX = Path.Combine(Path.GetDirectoryName(absolutePath),
                                                        Path.GetFileNameWithoutExtension(absolutePath)
                                                        + "@" + scale + "x" + Path.GetExtension(absolutePath));
                    if (File.Exists(absolutePathX))
                        return File.OpenRead(absolutePathX);
                }
            }
            return File.OpenRead(absolutePath);
#else
            string absolutePath = Path.Combine(Location, safeName);
            return File.OpenRead(absolutePath);
#endif
        }
    }
}

