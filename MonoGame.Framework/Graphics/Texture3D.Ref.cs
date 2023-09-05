// Copyright (C)2022 Nick Kastellanos

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

        private void PlatformSetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                                        T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTexture3D.SetData<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

        private void PlatformGetData<T>(int level, int left, int top, int right, int bottom, int front, int back,
                                        T[] data, int startIndex, int elementCount)
            where T : struct
        {
            _strategyTexture3D.GetData<T>(level, left, top, right, bottom, front, back, data, startIndex, elementCount);
        }

	}
}

