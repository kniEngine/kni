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
        private TextureTarget[] _targets;

        internal ConcreteTextureCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
            _targets = new TextureTarget[capacity];
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
            var GL = OGL.Current;

            for (int i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                Texture tex = _textures[i];

                // Clear the previous binding if the 
                // target is different from the new one.
                if (_targets[i] != 0 && (tex == null || _targets[i] != tex.GetTextureStrategy<ConcreteTexture>()._glTarget))
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    GL.CheckGLError();
                    GL.BindTexture(_targets[i], 0);
                    _targets[i] = 0;
                    GL.CheckGLError();
                }

                if (tex != null)
                {
                    GL.ActiveTexture(TextureUnit.Texture0 + i);
                    GL.CheckGLError();
                    _targets[i] = tex.GetTextureStrategy<ConcreteTexture>()._glTarget;
                    GL.BindTexture(tex.GetTextureStrategy<ConcreteTexture>()._glTarget, tex.GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GL.CheckGLError();

                    unchecked { _contextStrategy.Context._graphicsMetrics._textureCount++; }
                }

                // clear texture bit
                _dirty &= ~mask;
            }
        }

    }
}
