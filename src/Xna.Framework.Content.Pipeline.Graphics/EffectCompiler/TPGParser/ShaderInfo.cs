// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser
{
    public class ShaderInfo
	{
		public readonly List<TechniqueInfo> Techniques = new List<TechniqueInfo>();

        public readonly Dictionary<string, SamplerStateInfo> SamplerStates = new Dictionary<string, SamplerStateInfo>();
	}
}
