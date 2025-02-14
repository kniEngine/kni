﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline
{
    /// <summary>
    /// Provides methods for reporting informational messages or warnings from content importers and processors.
    /// Do not use this class to report errors. Instead, report errors by throwing a PipelineException or InvalidContentException.
    /// </summary>
    public abstract class ContentBuildLogger
    {
        readonly Stack<string> _filenames = new Stack<string>();


        /// <summary>
        /// Initializes a new instance of ContentBuildLogger.
        /// </summary>
        protected ContentBuildLogger()
        {
        }

        /// <summary>
        /// Returns the relative path to the filename from the root directory.
        /// </summary>
        /// <param name="filename">The target filename.</param>
        /// <returns>The relative path.</returns>
        string GetRelativePath(string filename)
        {
            var currentDirectory = Path.GetFullPath(".");
            filename = Path.GetFullPath(filename);
            if (filename.StartsWith(currentDirectory))
                filename = filename.Substring(currentDirectory.Length);
            return filename;
        }

        /// <summary>
        /// Gets the filename currently being processed, for use in warning and error messages.
        /// </summary>
        /// <param name="contentIdentity">Identity of a content item. If specified, GetCurrentFilename uses this value to refine the search. If no value is specified, the current PushFile state is used.</param>
        /// <returns>Name of the file being processed.</returns>
        protected string GetCurrentFilename(ContentIdentity contentIdentity)
        {
            string filename = null;

            if ((contentIdentity != null) && !string.IsNullOrEmpty(contentIdentity.SourceFilename))
                filename = contentIdentity.SourceFilename;
            else if (_filenames.Count > 0)
                filename = _filenames.Peek();

            // This convert's filepaths to relative if they are rooted in the current directory.
            // TODO: Move this out to concrete classes.
            if (filename != null)                
                filename = GetRelativePath(filename);

            return filename;
        }

        /// <summary>
        /// Outputs a high-priority status message from a content importer or processor.
        /// </summary>
        /// <param name="message">Message being reported.</param>
        /// <param name="messageArgs">Arguments for the reported message.</param>
        public abstract void LogImportantMessage(
            string message,
            params Object[] messageArgs
            );

        /// <summary>
        /// Outputs a low priority status message from a content importer or processor.
        /// </summary>
        /// <param name="message">Message being reported.</param>
        /// <param name="messageArgs">Arguments for the reported message.</param>
        public abstract void LogMessage(
            string message,
            params Object[] messageArgs
            );

        /// <summary>
        /// Outputs a warning message from a content importer or processor.
        /// </summary>
        /// <param name="helpLink">Link to an existing online help topic containing related information.</param>
        /// <param name="contentIdentity">Identity of the content item that generated the message.</param>
        /// <param name="message">Message being reported.</param>
        /// <param name="messageArgs">Arguments for the reported message.</param>
        public abstract void LogWarning(
            string helpLink,
            ContentIdentity contentIdentity,
            string message,
            params Object[] messageArgs
            );

        /// <summary>
        /// Outputs a message indicating that a content asset has begun processing.
        /// All logger warnings or error exceptions from this time forward to the next PopFile call refer to this file.
        /// </summary>
        /// <param name="filename">Name of the file containing future messages.</param>
        public virtual void PushFile(string filename)
        {
            _filenames.Push(filename);
        }

        /// <summary>
        /// Outputs a message indicating that a content asset has completed processing.
        /// </summary>
        public virtual void PopFile()
        {
            _filenames.Pop();
        }

    }
}
