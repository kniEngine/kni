// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework;
using System.Threading.Tasks;

namespace Microsoft.Xna.Platform
{
    internal sealed class ConcreteTitleContainer : TitleContainerStrategy
    {
        private string _location = string.Empty;

        private TitlePlatform _platform = TitlePlatform.UAP;

        public override string Location { get { return _location; } }

        public override TitlePlatform Platform { get { return _platform; } }

        public ConcreteTitleContainer() : base()
        {
            _location = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
        }

        public override Stream PlatformOpenStream(string name)
        {
            return Task.Run(() => PlatformOpenStreamAsync(name).Result).Result;
        }

        private async Task<Stream> PlatformOpenStreamAsync(string name)
        {
            try
            {
                Windows.Storage.StorageFile file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///" + name));
                var randomAccessStream = await file.OpenReadAsync();
                return randomAccessStream.AsStreamForRead();
            }
            catch (IOException)
            {
                // The file must not exist... return a null stream.
                return null;
            }
        }
    }
}

