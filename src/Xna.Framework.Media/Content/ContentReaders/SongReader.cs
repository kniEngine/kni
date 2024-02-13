// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Media.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class SongReader : ContentTypeReader<Song>
    {
        protected override Song Read(ContentReader input, Song existingInstance)
        {
            string path = input.ReadString();
            int durationMs = input.ReadObject<int>();

            if (String.IsNullOrEmpty(path))
                throw new InvalidOperationException();

            {
                // Add the ContentManager's RootDirectory
                string assetsLocationFullPath = ((ITitleContainer)TitleContainer.Current).Location;
                string rootDirectoryFullPath = Path.Combine(assetsLocationFullPath, input.ContentManager.RootDirectory);
                string dirPath = Path.Combine(rootDirectoryFullPath, input.AssetName);

                // Resolve the relative path
                path = FileHelpers.ResolveRelativePath(dirPath, path);
            }

            string name = Path.GetFileNameWithoutExtension(path);
            Uri streamSource = new Uri(path, UriKind.RelativeOrAbsolute);
            Song result = existingInstance ?? new Song(name, streamSource, durationMs);
            return result;
        }
    }
}
