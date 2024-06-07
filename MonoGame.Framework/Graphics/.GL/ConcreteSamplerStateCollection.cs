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
    internal sealed class ConcreteSamplerStateCollection : SamplerStateCollectionStrategy
    {

        internal ConcreteSamplerStateCollection(GraphicsContextStrategy contextStrategy, int capacity)
            : base(contextStrategy, capacity)
        {
        }


        public override SamplerState this[int index]
        {
            get { return base[index]; }
            set
            {
                base[index] = value;
            }
        }

        public override void Clear()
        {
            base.Clear();
        }

        public override void Dirty()
        {
            base.Dirty();
        }

        internal static  void PlatformApply(ConcreteGraphicsContextGL cgraphicsContext, ConcreteSamplerStateCollection csamplerStateCollection)
        {
            var GL = cgraphicsContext.GL;

            for (int i = 0; i < csamplerStateCollection._actualSamplers.Length; i++)
            {
                SamplerState sampler = csamplerStateCollection._actualSamplers[i];
                Texture texture = cgraphicsContext.Textures[i];

                if (sampler != null && texture != null)
                {
                    ConcreteTexture ctexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>();

                    if (sampler != ctexture._glLastSamplerState)
                    {
                        // TODO: Avoid doing this redundantly (see TextureCollection.Apply())
                        // However, I suspect that rendering from the same texture with different sampling modes
                        // is a relatively rare occurrence...
                        GL.ActiveTexture(TextureUnit.Texture0 + i);
                        GL.CheckGLError();

                        // NOTE: We don't have to bind the texture here because it is already bound in
                        // TextureCollection.Apply(). This, of course, assumes that Apply() is called
                        // before this method is called. If that ever changes this code will misbehave.
                        // GL.BindTexture(ctexture._glTarget, texture._glTexture);
                        // GL.CheckGLError();

                        ConcreteSamplerState csamplerState = ((IPlatformSamplerState)sampler).GetStrategy<ConcreteSamplerState>();

                        csamplerState.PlatformApplyState(cgraphicsContext.Context, ctexture._glTarget, ctexture.LevelCount > 1);
                        ctexture._glLastSamplerState = sampler;
                    }
                }
            }
        }

    }
}
