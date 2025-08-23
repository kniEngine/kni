// Copyright (C)2025 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal class GLSLBytecode
    {
        internal readonly byte Major;
        internal readonly byte Minor;
        internal readonly bool ES;
        internal readonly byte[] Bytecode;

        public GLSLBytecode(byte major, byte minor, bool es, byte[] bytecode)
        {
            this.Major = major;
            this.Minor = minor;
            this.ES = es;
            this.Bytecode = bytecode;
        }

    }
}
