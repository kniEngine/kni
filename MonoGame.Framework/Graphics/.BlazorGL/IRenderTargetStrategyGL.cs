// Copyright (C)2023 Nick Kastellanos

using nkast.Wasm.Canvas.WebGL;

namespace Microsoft.Xna.Platform.Graphics
{
    internal interface IRenderTargetStrategyGL
    {
        WebGLTexture GLTexture { get; }
        WebGLTextureTarget GLTarget { get; }
        WebGLTexture GLColorBuffer { get; set; }
        WebGLRenderbuffer GLDepthBuffer { get; set; }
        WebGLRenderbuffer GLStencilBuffer { get; set; }

        WebGLTextureTarget GetFramebufferTarget(int arraySlice);
    }
}
