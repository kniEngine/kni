// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        private IWebGLRenderingContext GL { get { return _device._glContext; } }

        private WebGLTextureTarget[] _targets;

        void PlatformInit(int capacity)
        {
            _targets = new WebGLTextureTarget[capacity];
        }

        void PlatformClear()
        {
            for (var i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }

        internal void PlatformApply()
        {
            for (var i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                var tex = _textures[i];

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

                    unchecked { _device.CurrentContext._graphicsMetrics._textureCount++; }
                }

                // clear texture bit
                _dirty &= ~mask;
            }
        }
    }
}
