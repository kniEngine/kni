// Copyright (C)2023 Nick Kastellanos

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


        public override void Clear()
        {
            base.Clear();
            for (int i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }

        internal void PlatformApply()
        {
            var GL = _contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

            for (int i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                Texture texture = _textures[i];

                // Clear the previous binding if the 
                // target is different from the new one.
                if (_targets[i] != 0 && (texture == null || _targets[i] != ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTarget))
                {
                    GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + i);
                    GL.CheckGLError();
                    GL.BindTexture(_targets[i], null);
                    _targets[i] = 0;
                    GL.CheckGLError();
                }

                if (texture != null)
                {
                    GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + i);
                    GL.CheckGLError();
                    _targets[i] = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTarget;
                    GL.BindTexture(((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTarget, ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTexture);
                    GL.CheckGLError();

                    unchecked { _contextStrategy.Context._graphicsMetrics._textureCount++; }
                }

                // clear texture bit
                _dirty &= ~mask;
            }
        }

    }
}
