// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteTextureCollection : TextureCollectionStrategy
    {
        internal WebGLTextureTarget[] _targets;

        internal uint InternalDirty
        {
            get { return base._dirty; }
            set { base._dirty = value; }
        }


        internal ConcreteTextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
            _targets = new WebGLTextureTarget[capacity];
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
