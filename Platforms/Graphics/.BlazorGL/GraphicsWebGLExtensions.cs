// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    static public class GraphicsWebGLExtensions
    {
        /// <summary>
        /// Returns the internal WebGL rendering context. Valid only on Blazor platforms.
        /// </summary>
        public static IWebGLRenderingContext GetWebGLContext(this GraphicsDevice device)
        {
            GraphicsContext context = ((IPlatformGraphicsDevice)device).Strategy.CurrentContext;
            IWebGLRenderingContext glContext = ((IPlatformGraphicsContext)context).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;
            return glContext;
        }

        /// <summary>
        /// Returns the internal WebGL texture object. Valid only on Blazor platforms.
        /// </summary>
        public static WebGLTexture GetWebGLTexture(this Texture texture)
        {
            WebGLTexture glTexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTexture;
            return glTexture;
        }
    }
}
