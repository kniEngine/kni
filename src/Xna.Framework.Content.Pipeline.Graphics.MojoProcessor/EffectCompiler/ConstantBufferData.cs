// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler
{
    internal class ConstantBufferData
    {
        public ConstantBufferData()
        {
        }

        public string Name { get; internal set; }

        public int Size { get; internal set; }

        public List<int> ParameterIndex { get; internal set; }

        public List<int> ParameterOffset { get; internal set; }

        public List<EffectObject.EffectParameterContent> Parameters { get; internal set; }

        public ConstantBufferData(string name)
        {
            Name = name;

            ParameterIndex = new List<int>();
            ParameterOffset = new List<int>();
            Parameters = new List<EffectObject.EffectParameterContent>();
            Size = 0;
        }

        public bool SameAs(ConstantBufferData other)
        {
            // If the names of the constant buffers don't
            // match then consider them different right off 
            // the bat... even if their parameters are the same.
            if (Name != other.Name)
                return false;

            // Do we have the same count of parameters and size?
            if (    Size != other.Size ||
                    Parameters.Count != other.Parameters.Count)
                return false;
            
            // Compare the parameters themselves.
            for (int i = 0; i < Parameters.Count; i++)
            {
                EffectObject.EffectParameterContent p1 = Parameters[i];
                EffectObject.EffectParameterContent p2 = other.Parameters[i];

                // Check the importaint bits.
                if (    p1.name != p2.name ||
                        p1.rows != p2.rows ||
                        p1.columns != p2.columns ||
                        p1.class_ != p2.class_ ||
                        p1.type != p2.type ||
                        p1.bufferOffset != p2.bufferOffset)
                    return false;
            }

            // These are equal.
            return true;
        }

    }
}
