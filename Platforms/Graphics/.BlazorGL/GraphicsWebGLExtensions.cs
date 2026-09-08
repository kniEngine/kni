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
        /// Returns a handle to the internal WebGL rendering context. Valid only on Blazor platforms.
        /// For usage, convert this to nkast.Wasm.Canvas.WebGL.IWebGLRenderingContext.
        /// </summary>
        public static object GetWebGLContext(this GraphicsDevice device)
        {
            GraphicsContext context = ((IPlatformGraphicsDevice)device).Strategy.CurrentContext;
            IWebGLRenderingContext glContext = ((IPlatformGraphicsContext)context).Strategy.ToConcrete<ConcreteGraphicsContext>().GL;
            return glContext;
        }

        /// <summary>
        /// Returns a handle to the internal WebGL texture object. Valid only on Blazor platforms.
        /// For usage, convert this to nkast.Wasm.Canvas.WebGL.WebGLTexture.
        /// </summary>
        public static object GetWebGLTexture(this Texture texture)
        {
            WebGLTexture glTexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>()._glTexture;
            return glTexture;
        }
    }
}
