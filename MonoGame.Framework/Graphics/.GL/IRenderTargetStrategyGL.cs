// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using MonoGame.OpenGL;

namespace Microsoft.Xna.Platform.Graphics
{
    internal interface IRenderTargetGL
    {
        int GLTexture { get; }
        TextureTarget GLTarget { get; }
        int GLColorBuffer { get; set; }
        int GLDepthBuffer { get; set; }
        int GLStencilBuffer { get; set; }

        TextureTarget GetFramebufferTarget(int arraySlice);
    }
}
