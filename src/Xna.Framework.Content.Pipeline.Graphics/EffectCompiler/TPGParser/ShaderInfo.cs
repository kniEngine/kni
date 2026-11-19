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

        public readonly Dictionary<string, ShaderArrayInfo> ShaderArrays = new Dictionary<string, ShaderArrayInfo>();

        public readonly Dictionary<string, IntDeclarationInfo> IntDeclarations = new Dictionary<string, IntDeclarationInfo>();

        public void AddIntDeclaration(string text, int startPos, int endPos)
        {
            var m = System.Text.RegularExpressions.Regex.Match(text,
                @"^int\s+(?<name>\w+)\s*(?<array>\[[^\]]*\])?\s*=\s*(?<value>\{[^}]*\}|[+-]?\d+)\s*;$");
            if (!m.Success)
                return;

            var decl = new IntDeclarationInfo();
            decl.name = m.Groups["name"].Value;
            decl.isArray = m.Groups["array"].Success;
            decl.startPos = startPos;
            decl.endPos = endPos;

            var value = System.Text.RegularExpressions.Regex.Replace(m.Groups["value"].Value, @"//[^\r\n]*|/\*.*?\*/", " ", System.Text.RegularExpressions.RegexOptions.Singleline);
            value = value.Trim().Trim('{', '}');
            foreach (var item in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int v;
                if (int.TryParse(item.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out v))
                    decl.values.Add(v);
            }

            // Function-scope declarations can share a name; the last one wins only if no global was found.
            if (!IntDeclarations.ContainsKey(decl.name))
                IntDeclarations[decl.name] = decl;
        }
	}

    public enum ShaderArrayStage
    {
        Vertex,
        Pixel,
    }

    public class ShaderCompileInfo
    {
        public string model;
        public string function;
    }

    public class ShaderArrayInfo
    {
        public string name;
        public ShaderArrayStage stage;
        public int startPos;
        public int endPos;
        public readonly List<ShaderCompileInfo> Shaders = new List<ShaderCompileInfo>();
    }

    public class IntDeclarationInfo
    {
        public string name;
        public bool isArray;
        public int startPos;
        public int endPos;
        public readonly List<int> values = new List<int>();
    }
}
