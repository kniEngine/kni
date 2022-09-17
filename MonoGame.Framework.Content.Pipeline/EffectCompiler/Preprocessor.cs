// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System.Collections.Generic;
using System.IO;
using System.Text;
using CppNet;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    public static class Preprocessor
    {
        public static string Preprocess(
            EffectContent input, ContentProcessorContext context, IDictionary<string, string> defines)
        {
            var fullPath = Path.GetFullPath(input.Identity.SourceFilename);
            var dependencies = new List<string>();            

            var pp = new CppNet.Preprocessor();

            pp.EmitExtraLineInfo = false;
            pp.addFeature(Feature.LINEMARKERS);
            pp.setListener(new MGErrorListener(context.Logger));
            pp.setFileSystem(new MGFileSystem(dependencies));
            pp.setQuoteIncludePath(new List<string> { Path.GetDirectoryName(fullPath) });

            foreach (var define in defines)
                pp.addMacro(define.Key, define.Value);

            string effectCode = input.EffectCode;
            effectCode = effectCode.Replace("#line", "//--WORKAROUND#line");

            pp.addInput(new MGStringLexerSource(effectCode, true, fullPath));

            var result = new StringBuilder();

            var endOfStream = false;
            while (!endOfStream)
            {
                var token = pp.token();
                switch (token.getType())
                {
                    case CppNet.Token.EOF:
                        endOfStream = true;
                        break;
                    case CppNet.Token.CPPCOMMENT:
                        if (token.getText().StartsWith("//--WORKAROUND#line"))
                        {
                            result.Append(token.getText().Replace("//--WORKAROUND#line", "#line"));
                        }
                        break;
                    case CppNet.Token.CCOMMENT:
                    {
                        var tokenText = token.getText();
                        if (tokenText != null)
                        {
                            // Need to preserve line breaks so that line numbers are correct.
                            foreach (var c in tokenText)
                                if (c == '\n')
                                    result.Append(c);
                        }
                        break;
                    }
                    default:
                    {
                        var tokenText = token.getText();
                        if (tokenText != null)
                            result.Append(tokenText);
                        break;
                    }
                }
            }

            // Add the include dependencies so that if they change
            // it will trigger a rebuild of this effect.
            foreach (var dep in dependencies)
                context.AddDependency(dep);

            return result.ToString();
        }

        private class MGFileSystem : VirtualFileSystem
        {
            private readonly List<string> _dependencies;

            public MGFileSystem(List<string> dependencies)
            {
                _dependencies = dependencies;
            }

            public VirtualFile getFile(string path)
            {
                return new MGFile(path, _dependencies);
            }

            public VirtualFile getFile(string dir, string name)
            {
                return new MGFile(Path.Combine(dir, name), _dependencies);
            }
        }

        private class MGFile : VirtualFile
        {
            private readonly List<string> _dependencies;
            private readonly string _path;

            public MGFile(string path, List<string> dependencies)
            {
                _dependencies = dependencies;
                _path = Path.GetFullPath(path);
            }

            public bool isFile()
            {
                return File.Exists(_path) && !File.GetAttributes(_path).HasFlag(FileAttributes.Directory);
            }

            public string getPath()
            {
                return _path;
            }

            public string getName()
            {
                return Path.GetFileName(_path);
            }

            public VirtualFile getParentFile()
            {
                return new MGFile(Path.GetDirectoryName(_path), _dependencies);
            }

            public VirtualFile getChildFile(string name)
            {
                return new MGFile(Path.Combine(_path, name), _dependencies);
            }

            public Source getSource()
            {
                if (!_dependencies.Contains(_path))
                    _dependencies.Add(_path);
                return new MGStringLexerSource(AppendNewlineIfNonePresent(File.ReadAllText(_path)), true, _path);
            }

            private static string AppendNewlineIfNonePresent(string text)
            {
                if (!text.EndsWith("\n"))
                    return text + "\n";
                return text;
            }
        }

        private class MGStringLexerSource : StringLexerSource
        {
            public string Path { get; private set; }

            public MGStringLexerSource(string str, bool ppvalid, string fileName)
                : base(str.Replace("\r\n", "\n"), ppvalid, fileName)
            {
                Path = fileName;
            }
        }

        private class MGErrorListener : PreprocessorListener
        {
            private readonly ContentBuildLogger _logger;

            public MGErrorListener(ContentBuildLogger logger)
            {
                _logger = logger;
            }

            public void handleWarning(Source source, int line, int column, string msg)
            {
                _logger.LogWarning(null, CreateContentIdentity(source, line, column), msg);
            }

            public void handleError(Source source, int line, int column, string msg)
            {
                throw new InvalidContentException(msg, CreateContentIdentity(source, line, column));
            }

            private static ContentIdentity CreateContentIdentity(Source source, int line, int column)
            {
                string file = ((MGStringLexerSource)source).Path;
                return new ContentIdentity(file, null, line + "," + column);
            }

            public void handleSourceChange(Source source, string ev)
            {
                
            }
        }
    }
}
