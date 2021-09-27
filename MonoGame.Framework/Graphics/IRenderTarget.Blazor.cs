// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

//using MonoGame.OpenGL;
using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Platform.Graphics
{
    internal interface IRenderTargetGL
    {
        WebGLTexture GLTexture { get; }
        WebGLTextureTarget GLTarget { get; }
        WebGLTexture GLColorBuffer { get; set; }
        WebGLRenderbuffer GLDepthBuffer { get; set; }
        WebGLRenderbuffer GLStencilBuffer { get; set; }

        WebGLTextureTarget GetFramebufferTarget(int arraySlice);
    }
}
