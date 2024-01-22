// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Platform;
using Microsoft.Xna.Platform.Content.Utilities;

namespace Microsoft.Xna.Platform
{
    public interface ITitleContainer
    {
        string Location { get; }

        Stream OpenStream(string name);
    }
}

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Provides functionality for opening a stream in the title storage area.
    /// </summary>
    public sealed class TitleContainer : ITitleContainer
    {
        private static TitleContainer _current;

        /// <summary>
        /// Returns the current FrameworkDispatcher instance.
        /// </summary> 
        public static TitleContainer Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(TitleContainer))
                {
                    if (_current == null)
                        _current = new TitleContainer();

                    return _current;
                }
            }
        }

        /// <summary>
        /// Returns an open stream to an existing file in the title storage area.
        /// </summary>
        /// <param name="name">The filepath relative to the title storage area.</param>
        /// <returns>A open stream or null if the file is not found.</returns>
        public static Stream OpenStream(string name)
        {
            return ((ITitleContainer)TitleContainer.Current).OpenStream(name);
        }


        private TitleContainerStrategy _strategy;

        private TitleContainer()
        {
            _strategy = new ConcreteTitleContainer();
        }

        #region ITitleContainer

        string ITitleContainer.Location
        {
            get { return _strategy.Location; }
        }

        Stream ITitleContainer.OpenStream(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // We do not accept absolute paths here.
            if (Path.IsPathRooted(name))
                throw new ArgumentException("Invalid filename. TitleContainer.OpenStream requires a relative path.", name);

            // Normalize the file path.
            string safeName = TitleContainer.NormalizeRelativePath(name);

            // Call the platform code to open the stream.
            // Any errors at this point should result in a file not found.
            try
            {
                Stream stream = _strategy.PlatformOpenStream(safeName);
                if (stream != null)
                    return stream;
                else
                    throw new FileNotFoundException("Error loading \"" + name + "\". File not found.");
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FileNotFoundException(name, ex);
            }
        }

        #endregion ITitleContainer

        private static string NormalizeRelativePath(string name)
        {
            Uri uri = new Uri("file:///" + FileHelpers.UrlEncode(name));
            string path = uri.LocalPath;
            path = path.Substring(1);
            path = path.Replace(FileHelpers.NotSeparator, FileHelpers.Separator);

            return path;
        }
    }
}

