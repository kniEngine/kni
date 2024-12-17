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
                    Matrix headTransform = HeadTransform;
                    return headTransform;
                case XREye.Left:
                    Matrix lEyeTransform = LEyeTransform;
                    return lEyeTransform;
                case XREye.Right:
                    Matrix rEyeTransform = REyeTransform;
                    return rEyeTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetEyeView(XREye eyeIndex)
        {
            switch (eyeIndex)
            {
                case XREye.None:
                    Matrix headTransform = HeadTransform;
                    return Matrix.Invert(headTransform);
                case XREye.Left:
                    Matrix lEyeTransform = LEyeTransform;
                    return Matrix.Invert(lEyeTransform);
                case XREye.Right:
                    Matrix rEyeTransform = REyeTransform;
                    return Matrix.Invert(rEyeTransform);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
