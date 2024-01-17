// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Foundation;
using Microsoft.Xna.Framework;
using UIKit;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        private string _location = string.Empty;

        public override string Location { get { return _location; } }

        private bool _supportRetina;
        private int _retinaScale;

        public ConcreteTitleContainer() : base()
        {
            _location = NSBundle.MainBundle.ResourcePath;

            _supportRetina = UIScreen.MainScreen.Scale >= 2.0f;
            _retinaScale = (int)Math.Round(UIScreen.MainScreen.Scale);
        }

        public override Stream PlatformOpenStream(string name)
        {
            string absolutePath = Path.Combine(_location, name);

            if (_supportRetina)
            {
                for (int scale = _retinaScale; scale >= 2; scale--)
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
        }
    }
}

