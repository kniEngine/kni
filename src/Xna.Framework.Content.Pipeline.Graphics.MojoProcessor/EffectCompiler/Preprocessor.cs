// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

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

        internal void AddMacros(IEnumerable<KeyValuePair<string, string>> macros)
        {
            foreach (KeyValuePair<string, string> macro in macros)
                _pp.addMacro(macro.Key, macro.Value);
        }

        public string Preprocess()
        {
            _pp.setQuoteIncludePath(new List<string> { Path.GetDirectoryName(_fullFilePath) });

            string effectCode = _input.EffectCode;
            effectCode = effectCode.Replace("#line", "//--WORKAROUND#line");

            StringLexerSource inputSource = new PPStringLexerSource(effectCode, true, _fullFilePath);
            _pp.addInput(inputSource);

            StringBuilder result = new StringBuilder();

            while (true)
            {
                Token token = _pp.token();
                int tokenType = token.getType();

                switch (tokenType)
                {
                    case CppNet.Token.EOF:
                        return result.ToString();

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

        }


        #region PreprocessorListener
    
        void PreprocessorListener.handleWarning(Source source, int line, int column, string msg)
        {
            string file = ((PPStringLexerSource)source).Path;
            ContentIdentity contentIdentity = new ContentIdentity(file, null, line + "," + column);
            _context.Logger.LogWarning(null, contentIdentity, msg);
        }

        void PreprocessorListener.handleError(Source source, int line, int column, string msg)
        {
            string file = ((PPStringLexerSource)source).Path;
            ContentIdentity contentIdentity = new ContentIdentity(file, null, line + "," + column);
            throw new InvalidContentException(msg, contentIdentity);
        }

        void PreprocessorListener.handleSourceChange(Source source, string ev)
        {

        }

        #endregion PreprocessorListener


        #region VirtualFileSystem

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

            bool VirtualFile.isFile()
            {
                return File.Exists(_path) && !File.GetAttributes(_path).HasFlag(FileAttributes.Directory);
            }

            string VirtualFile.getPath()
            {
                return _path;
            }

            string VirtualFile.getName()
            {
                return Path.GetFileName(_path);
            }

            VirtualFile VirtualFile.getParentFile()
            {
                return new PPVirtualFile(_virtualFileSystem, Path.GetDirectoryName(_path));
            }

            VirtualFile VirtualFile.getChildFile(string name)
            {
                return new PPVirtualFile(_virtualFileSystem, Path.Combine(_path, name));
            }

            Source VirtualFile.getSource()
            {
                // Add the include dependencies so that if they change
                // it will trigger a rebuild of this effect.
                ((Preprocessor)_virtualFileSystem)._context.AddDependency(_path);

                string effectCode = File.ReadAllText(_path);
                // Append new line if none present.
                if (!effectCode.EndsWith("\n"))
                    effectCode += "\n";

                StringLexerSource source = new PPStringLexerSource(effectCode, true, _path);
                return source;
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

    }
}
