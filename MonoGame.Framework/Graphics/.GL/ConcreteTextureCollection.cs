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

        internal static void PlatformApplyTextures(ConcreteGraphicsContextGL cgraphicsContext, ConcreteTextureCollection ctextureCollection)
        {
            var GL = cgraphicsContext.GL;

            for (int i = 0; ctextureCollection.InternalDirty != 0 && i < ctextureCollection._textures.Length; i++)
            {
                uint mask = ((uint)1) << i;
                if ((ctextureCollection.InternalDirty & mask) != 0)
                {
                    Texture texture = ctextureCollection._textures[i];

                    // Clear the previous binding if the 
                    // target is different from the new one.
                    if (ctextureCollection._targets[i] != 0 && (texture == null || ctextureCollection._targets[i] != ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTarget))
                    {
                        GL.ActiveTexture(TextureUnit.Texture0 + i);
                        GL.CheckGLError();
                        GL.BindTexture(ctextureCollection._targets[i], 0);
                        ctextureCollection._targets[i] = 0;
                        GL.CheckGLError();
                    }

                    if (texture != null)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0 + i);
                        GL.CheckGLError();
                        ConcreteTexture ctexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>();
                        ctextureCollection._targets[i] = ctexture._glTarget;
                        GL.BindTexture(ctexture._glTarget, ctexture._glTexture);
                        GL.CheckGLError();

                        cgraphicsContext.Metrics_AddTextureCount();
                    }

                    // clear texture bit
                    ctextureCollection.InternalDirty &= ~mask;
                }
            }
        }

    }
}
