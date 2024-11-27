// Copyright (C)2024 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Content.Pipeline.Builder
{
    public struct Package
    {
        public string Name;
        public string Version;

        public static Package Parse(string packageReference)
        {
            packageReference.Trim();

            Package package;
            package.Name = packageReference;
            package.Version = String.Empty;

            string[] split = packageReference.Split(' ');
            if (split.Length == 2)
            {
                package.Name = split[0].Trim();
                package.Version = split[1].Trim();
            }

            return package;
        }
    }
}