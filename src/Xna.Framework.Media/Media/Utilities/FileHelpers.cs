﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Xna.Platform.Media.Utilities
{
    internal static class FileHelpers
    {
        public static readonly char BackwardSlash = '\\';
        public static readonly char ForwardSlash = '/';

        public static readonly string ForwardSlashString = new string(ForwardSlash, 1);
        private static readonly char[] UrlSafeChars = new[] { '.', '_', '-', ';', '/', '?', '\\', ':' };

#if UAP || WINUI
        public static readonly char Separator = BackwardSlash;
        public static readonly char NotSeparator = ForwardSlash;
#else
        public static readonly char Separator = Path.DirectorySeparatorChar;
        public static readonly char NotSeparator = (Path.DirectorySeparatorChar == BackwardSlash) ? ForwardSlash : BackwardSlash;
#endif

        public static string NormalizeFilePathSeparators(string name)
        {
            return name.Replace(NotSeparator, Separator);
        }

        /// <summary>
        /// Combines the filePath and relativeFile based on relativeFile being a file in the same location as filePath.
        /// Relative directory operators (..) are also resolved
        /// </summary>
        /// <example>"A\B\C.txt","D.txt" becomes "A\B\D.txt"</example>
        /// <example>"A\B\C.txt","..\D.txt" becomes "A\D.txt"</example>
        /// <param name="filePath">Path to the file we are starting from</param>
        /// <param name="relativeFile">Relative location of another file to resolve the path to</param>
        public static string ResolveRelativePath(string filePath, string relativeFile)
        {
            // Uri accepts forward slashes
            filePath = filePath.Replace(BackwardSlash, ForwardSlash);
            relativeFile = relativeFile.Replace(BackwardSlash, ForwardSlash);

            // Sanitize the path of double slashes, they confuse Uri
            while (filePath.Contains("//"))
                filePath = filePath.Replace("//", "/");

            bool hasForwardSlash = filePath.StartsWith(ForwardSlashString);
            if (!hasForwardSlash)
                filePath = ForwardSlashString + filePath;

            // Get a uri for filePath using the file:// schema and no host.
            Uri src = new Uri("file://" + UrlEncode(filePath));

            Uri dst = new Uri(src, UrlEncode(relativeFile));
            // The uri now contains the path to the relativeFile with 
            // relative addresses resolved... get the local path.
            string localPath = dst.LocalPath;

            if (!hasForwardSlash && localPath.StartsWith("/"))
                localPath = localPath.Substring(1);

            // Convert the directory separator characters to the 
            // correct platform specific separator.
            return TrimPath(NormalizeFilePathSeparators(localPath));
        }

        internal static string UrlEncode(string url)
        {
            UTF8Encoding encoder = new UTF8Encoding();
            StringBuilder safeline = new StringBuilder(encoder.GetByteCount(url) * 3);

            foreach (char c in url)
            {
                if ((c >= 48 && c <= 57) || (c >= 65 && c <= 90) || (c >= 97 && c <= 122) || Array.IndexOf(UrlSafeChars, c) != -1)
                    safeline.Append(c);
                else
                {
                    byte[] bytes = encoder.GetBytes(c.ToString());

                    foreach (byte num in bytes)
                    {
                        safeline.Append("%");
                        safeline.Append(num.ToString("X"));
                    }
                }
            }

            return safeline.ToString();
        }

        private static string TrimPath(string filePath)
        {
            // Remove . in filePath

            while (filePath.Contains("/./"))
                filePath = filePath.Replace("/./", "/");

            while (filePath.Contains(@"\.\"))
                filePath = filePath.Replace(@"\.\", @"\");

            filePath = Regex.Replace(filePath, @"^\.(\/|\\)", string.Empty);

            // Remove .. in filePath

            filePath = Regex.Replace(filePath, @"[^\/\\]+(\/|\\)\.\.(\/|\\)", string.Empty);

            return filePath;
        }
    }
}
