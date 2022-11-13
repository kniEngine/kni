// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

using System;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
    {
        IWebGLRenderingContext GL { get { return _device._glContext; } }

        private void PlatformSetSamplerState(int index)
        {
        }

        private void PlatformClear()
        {
        }

        private void PlatformDirty()
        {
        }

        internal void PlatformApply()
        {
            for (var i = 0; i < _actualSamplers.Length; i++)
            {
                var sampler = _actualSamplers[i];
                var texture = _device.Textures[i];

                if (sampler != null && texture != null && sampler != texture.glLastSamplerState)
                {
                    // TODO: Avoid doing this redundantly (see TextureCollection.Apply())
                    // However, I suspect that rendering from the same texture with different sampling modes
                    // is a relatively rare occurrence...
                    GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + i);
                    GraphicsExtensions.CheckGLError();

                    // NOTE: We don't have to bind the texture here because it is already bound in
                    // TextureCollection.Apply(). This, of course, assumes that Apply() is called
                    // before this method is called. If that ever changes this code will misbehave.
                    // GL.BindTexture(texture.glTarget, texture.glTexture);
                    // GraphicsExtensions.CheckGLError();

                    sampler.Activate(_device, texture.glTarget, texture.LevelCount > 1);
                    texture.glLastSamplerState = sampler;
                }
            }
        }
	}
}
