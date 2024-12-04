// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.XR
{
    public struct HeadsetState
    {
        public Matrix HeadTransform;
        public Matrix LEyeTransform;
        public Matrix REyeTransform;

        public Matrix GetEyeTransform(XREye eyeIndex)
        {
            switch (eyeIndex)
            {
                case XREye.None:
                    return HeadTransform;
                case XREye.Left:
                    return LEyeTransform;
                case XREye.Right:
                    return REyeTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetEyeView(XREye eyeIndex)
        {
            switch (eyeIndex)
            {
                case XREye.None:
                    return Matrix.Invert(HeadTransform);
                case XREye.Left:
                    return Matrix.Invert(LEyeTransform);
                case XREye.Right:
                    return Matrix.Invert(REyeTransform);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
