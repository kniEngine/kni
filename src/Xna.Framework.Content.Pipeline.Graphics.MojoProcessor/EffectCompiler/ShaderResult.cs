// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    public class ShaderResult
    {
        public ShaderInfo ShaderInfo { get; private set; }

        public ShaderResult(ShaderInfo ShaderInfo)
        {
            this.ShaderInfo = ShaderInfo;
        }

    }
}
