// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Media;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Content
{
    internal class SongReader : ContentTypeReader<Song>
    {
        protected internal override Song Read(ContentReader input, Song existingInstance)
        {
            string path = input.ReadString();
            int durationMs = input.ReadObject<int>();

            if (!String.IsNullOrEmpty(path))
            {
                // Add the ContentManager's RootDirectory
                string dirPath = Path.Combine(input.ContentManager.RootDirectoryFullPath, input.AssetName);

                // Resolve the relative path
                path = FileHelpers.ResolveRelativePath(dirPath, path);
            }

            string name = Path.GetFileNameWithoutExtension(path);
            Uri streamSource = new Uri(path, UriKind.RelativeOrAbsolute);
            return existingInstance ?? new Song(name, streamSource, durationMs);
        }
    }
}
