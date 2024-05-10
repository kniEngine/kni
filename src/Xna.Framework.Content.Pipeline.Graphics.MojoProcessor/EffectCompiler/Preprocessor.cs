// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using CppNet;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    public class Preprocessor : PreprocessorListener
    {
        private EffectContent _input;
        private ContentProcessorContext _context;
        private string _fullFilePath;
        private CppNet.Preprocessor _pp;

        public Preprocessor(EffectContent input, ContentProcessorContext context, string fullFilePath)
        {
            this._input = input;
            this._context = context;
            this._fullFilePath = fullFilePath;

            _pp = new CppNet.Preprocessor();
            _pp.EmitExtraLineInfo = false;
            _pp.addFeature(Feature.LINEMARKERS);
            _pp.setListener(this);
        }

        internal void AddMacro(string name, string value)
        {
            _pp.addMacro(name, value);
        }

        public string Preprocess()
        {
            List<string> dependencies = new List<string>();

            _pp.setFileSystem(new MGFileSystem(dependencies));
            _pp.setQuoteIncludePath(new List<string> { Path.GetDirectoryName(_fullFilePath) });

            string effectCode = _input.EffectCode;
            effectCode = effectCode.Replace("#line", "//--WORKAROUND#line");

            _pp.addInput(new MGStringLexerSource(effectCode, true, _fullFilePath));

            StringBuilder result = new StringBuilder();

            bool endOfStream = false;
            while (!endOfStream)
            {
                Token token = _pp.token();
                switch (token.getType())
                {
                    case CppNet.Token.EOF:
                        {
                            endOfStream = true;
                        }
                        break;

                    case CppNet.Token.CPPCOMMENT:
                        {
                            string tokenText = token.getText();
                            if (tokenText.StartsWith("//--WORKAROUND#line"))
                            {
                                result.Append(tokenText.Replace("//--WORKAROUND#line", "#line"));
                            }
                        }
                        break;

                    case CppNet.Token.CCOMMENT:
                        {
                            string tokenText = token.getText();
                            if (tokenText != null)
                            {
                                // Need to preserve line breaks so that line numbers are correct.
                                foreach (char c in tokenText)
                                    if (c == '\n')
                                        result.Append(c);
                            }
                        }
                        break;

                    default:
                        {
                            string tokenText = token.getText();
                            if (tokenText != null)
                                result.Append(tokenText);
                        }
                        break;
                }
            }

            // Add the include dependencies so that if they change
            // it will trigger a rebuild of this effect.
            foreach (string dep in dependencies)
                _context.AddDependency(dep);

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

        #region PreprocessorListener
    
        void PreprocessorListener.handleWarning(Source source, int line, int column, string msg)
        {
            _context.Logger.LogWarning(null, CreateContentIdentity(source, line, column), msg);
        }

        void PreprocessorListener.handleError(Source source, int line, int column, string msg)
        {
            throw new InvalidContentException(msg, CreateContentIdentity(source, line, column));
        }

        private static ContentIdentity CreateContentIdentity(Source source, int line, int column)
        {
            string file = ((MGStringLexerSource)source).Path;
            return new ContentIdentity(file, null, line + "," + column);
        }

        void PreprocessorListener.handleSourceChange(Source source, string ev)
        {

        }

        #endregion PreprocessorListener

    }
}
