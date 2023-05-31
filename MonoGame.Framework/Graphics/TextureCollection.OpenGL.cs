// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class TextureCollection
    {
        private TextureTarget[] _targets;

        void PlatformInit()
        {
            _targets = new TextureTarget[_textures.Length];
        }

        void PlatformClear()
        {
            for (int i = 0; i < _targets.Length; i++)
                _targets[i] = 0;
        }

        internal void PlatformApply()
        {
            for (int i = 0; _dirty != 0 && i < _textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((_dirty & mask) == 0)
                    continue;

                Texture tex = _textures[i];

                GL.ActiveTexture(TextureUnit.Texture0 + i);
                GraphicsExtensions.CheckGLError();

                // Clear the previous binding if the 
                // target is different from the new one.
                if (_targets[i] != 0 && (tex == null || _targets[i] != tex._glTarget))
                {
                    GL.BindTexture(_targets[i], 0);
                    _targets[i] = 0;
                    GraphicsExtensions.CheckGLError();
                }

                if (tex != null)
                {
                    _targets[i] = tex._glTarget;
                    GL.BindTexture(tex._glTarget, tex._glTexture);
                    GraphicsExtensions.CheckGLError();

                    unchecked { _device.CurrentContext._graphicsMetrics._textureCount++; }
                }

                // clear texture bit
                _dirty &= ~mask;
            }
        }
    }
}
