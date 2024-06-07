// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteTextureCollection : TextureCollectionStrategy
    {
        internal TextureTarget[] _targets;

        internal uint InternalDirty
        {
            get { return base._dirty; }
            set { base._dirty = value; }
        }


        internal ConcreteTextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
            _targets = new TextureTarget[capacity];
            for (int i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }


        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }

    }
}
