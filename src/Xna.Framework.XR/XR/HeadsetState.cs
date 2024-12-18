// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.XR
{
    public struct HeadsetState
    {
        public Pose3 HeadPose;
        public Pose3 LEyePose;
        public Pose3 REyePose;

        public Matrix GetEyeTransform(XREye eyeIndex)
        {
            switch (eyeIndex)
            {
                case XREye.None:
                    return Matrix.CreateFromPose(HeadPose);
                case XREye.Left:
                    Matrix lEyeTransform = Matrix.CreateFromPose(LEyePose);
                    return lEyeTransform;
                case XREye.Right:
                    Matrix rEyeTransform = Matrix.CreateFromPose(REyePose);
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
                    Matrix headTransform = Matrix.CreateFromPose(HeadPose);
                    return Matrix.Invert(headTransform);
                case XREye.Left:
                    Matrix lEyeTransform = Matrix.CreateFromPose(LEyePose);
                    return Matrix.Invert(lEyeTransform);
                case XREye.Right:
                    Matrix rEyeTransform = Matrix.CreateFromPose(REyePose);
                    return Matrix.Invert(rEyeTransform);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
