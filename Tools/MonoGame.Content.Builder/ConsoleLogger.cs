// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace MonoGame.Content.Builder
{
    internal class ConsoleLogger : ContentBuildLogger
    {
        private int indentCount = 0;

        public void Indent()
        {
            indentCount++;
        }

        public void Unindent()
        {
            indentCount--;
        }

        protected string IndentString { get { return String.Empty.PadLeft(Math.Max(0, indentCount), '\t'); } }


        public override void LogMessage(string message, params object[] messageArgs)
        {
			Console.WriteLine(IndentString + message, messageArgs);
        }

        public override void LogImportantMessage(string message, params object[] messageArgs)
        {
            // TODO: How do i make it high importance?
            Console.WriteLine(IndentString + message, messageArgs);
        }

        public override void LogWarning(string helpLink, ContentIdentity contentIdentity, string message, params object[] messageArgs)
        {
            var warning = string.Empty;
            if (contentIdentity != null && !string.IsNullOrEmpty(contentIdentity.SourceFilename))
            {
                warning = contentIdentity.SourceFilename;
                if (!string.IsNullOrEmpty(contentIdentity.FragmentIdentifier))
                    warning += "(" + contentIdentity.FragmentIdentifier + ")";
                else
                    warning += " ";
            }
            else
            {
                warning = GetCurrentFilename(contentIdentity);
                warning += " ";
            }
            warning += ": warning ";
            
            // extract errorCode from message
            var match = System.Text.RegularExpressions.Regex.Match(message, @"([A-Z]+[0-9]+):(.+)");
            if (match.Success || match.Groups.Count == 2)
            {
                warning += match.Groups[1].Value;
                warning += " ";
                message = match.Groups[2].Value;
            }

            warning += ": ";

            if (messageArgs != null && messageArgs.Length != 0)
                warning += string.Format(message, messageArgs);
            else if (!string.IsNullOrEmpty(message))
                warning += message;

            Console.WriteLine(warning);
        }
    }
}
