﻿// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteTextureCollection : TextureCollectionStrategy
    {
        private WebGLTextureTarget[] _targets;

        internal ConcreteTextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
            _targets = new WebGLTextureTarget[capacity];
            for (int i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }


        internal override void Clear()
        {
            base.Clear();
            for (int i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }

        internal void PlatformApply()
        {
            var GL = ((ConcreteGraphicsContext)_contextStrategy).GL;

            for (int i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                Texture tex = _textures[i];

                GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + i);
                GraphicsExtensions.CheckGLError();

                // Clear the previous binding if the 
                // target is different from the new one.
                if (_targets[i] != 0 && (tex == null || _targets[i] != tex.glTarget))
                {
                    GL.BindTexture(_targets[i], null);
                    _targets[i] = 0;
                    GraphicsExtensions.CheckGLError();
                }

                if (tex != null)
                {
                    _targets[i] = tex.glTarget;
                    GL.BindTexture(tex.glTarget, tex.glTexture);
                    GraphicsExtensions.CheckGLError();

                    unchecked { _contextStrategy.Context._graphicsMetrics._textureCount++; }
                }

                // clear texture bit
                _dirty &= ~mask;
            }
        }

    }
}
