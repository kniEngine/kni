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
    public class Preprocessor : PreprocessorListener, VirtualFileSystem
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
            _pp.setFileSystem(this);
        }

        internal void AddMacro(string name, string value)
        {
            _pp.addMacro(name, value);
        }

        public string Preprocess()
        {
            _pp.setQuoteIncludePath(new List<string> { Path.GetDirectoryName(_fullFilePath) });

            string effectCode = _input.EffectCode;
            effectCode = effectCode.Replace("#line", "//--WORKAROUND#line");

            _pp.addInput(new PPStringLexerSource(effectCode, true, _fullFilePath));

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
            foreach (string dep in _dependencies)
                _context.AddDependency(dep);

            return result.ToString();
        }

        private void AddDependency(string filename)
        {
            if (!_dependencies.Contains(filename))
                _dependencies.Add(filename);
        }

        #region VirtualFileSystem

        private readonly List<string> _dependencies = new List<string>();

        VirtualFile VirtualFileSystem.getFile(string path)
        {
            return new PPVirtualFile((VirtualFileSystem)this, path);
        }

        VirtualFile VirtualFileSystem.getFile(string dir, string name)
        {
            return new PPVirtualFile((VirtualFileSystem)this, Path.Combine(dir, name));
        }

        #endregion VirtualFileSystem

        private class PPVirtualFile : VirtualFile
        {
            VirtualFileSystem _virtualFileSystem;
            private readonly string _path;

            public PPVirtualFile(VirtualFileSystem virtualFileSystem, string path)
            {
                this._virtualFileSystem = virtualFileSystem;
                this._path = Path.GetFullPath(path);
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
                return new PPVirtualFile(_virtualFileSystem, Path.GetDirectoryName(_path));
            }

            public VirtualFile getChildFile(string name)
            {
                return new PPVirtualFile(_virtualFileSystem, Path.Combine(_path, name));
            }

            public Source getSource()
            {
                ((Preprocessor)_virtualFileSystem).AddDependency(_path);

                return new PPStringLexerSource(AppendNewlineIfNonePresent(File.ReadAllText(_path)), true, _path);
            }

            private static string AppendNewlineIfNonePresent(string text)
            {
                if (!text.EndsWith("\n"))
                    return text + "\n";
                return text;
            }
        }

        private class PPStringLexerSource : StringLexerSource
        {
            public string Path { get; private set; }

            public PPStringLexerSource(string str, bool ppvalid, string fileName)
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
            string file = ((PPStringLexerSource)source).Path;
            return new ContentIdentity(file, null, line + "," + column);
        }

        void PreprocessorListener.handleSourceChange(Source source, string ev)
        {

        }

        #endregion PreprocessorListener

    }
}
