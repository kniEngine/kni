// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class Texture3D : Texture
	{
        private void PlatformConstructTexture3D(
            GraphicsDevice graphicsDevice, 
            int width,
            int height, 
            int depth, 
            bool mipMap, 
            SurfaceFormat format, 
            bool renderTarget)
        {
            throw new NotImplementedException();
        }

	}
}

