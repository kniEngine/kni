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
                    return Matrix.CreateFromPose(LEyePose);
                case XREye.Right:
                    return Matrix.CreateFromPose(REyePose);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetEyeView(XREye eyeIndex)
        {
            switch (eyeIndex)
            {
                case XREye.None:
                    Pose3 invhpose = Pose3.Inverse(HeadPose);
                    return Matrix.CreateFromPose(invhpose);
                case XREye.Left:
                    Pose3 invlpose = Pose3.Inverse(LEyePose);
                    return Matrix.CreateFromPose(invlpose);
                case XREye.Right:
                    Pose3 invrpose = Pose3.Inverse(REyePose);
                    return Matrix.CreateFromPose(invrpose);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
