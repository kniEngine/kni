// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Oculus
{
    public struct HeadsetState
    {
        public Matrix HeadTransform;
        public Matrix LEyeTransform;
        public Matrix REyeTransform;

        public Matrix GetEyeTransform(int eyeIndex)
        {
            switch (eyeIndex)
            {
                case 0:
                    return LEyeTransform;
                case 1:
                    return REyeTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetEyeView(int eyeIndex)
        {
            switch (eyeIndex)
            {
                case 0:
                    return Matrix.Invert(LEyeTransform);
                case 1:
                    return Matrix.Invert(REyeTransform);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
