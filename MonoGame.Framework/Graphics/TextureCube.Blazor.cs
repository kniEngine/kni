// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
	public partial class TextureCube
	{
        private void PlatformConstructTextureCube(GraphicsDevice graphicsDevice, int size, bool mipMap, SurfaceFormat format, bool renderTarget)
        {
            throw new NotImplementedException();
        }

        private void PlatformGetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTextureCube.GetData<T>(face, level, checkedRect, data, startIndex, elementCount);
        }

        private void PlatformSetData<T>(CubeMapFace face, int level, Rectangle checkedRect, T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTextureCube.SetData<T>(face, level, checkedRect, data, startIndex, elementCount);
        }
	}
}

