// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.XR
{
    public struct HandsState
    {
        public Pose3 LHandPose;
        public Pose3 RHandPose;

        public Pose3 LGripPose;
        public Pose3 RGripPose;

        public Matrix GetHandTransform(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    Matrix lHandTransform = Matrix.CreateFromPose(LHandPose);
                    return lHandTransform;
                case 1:
                    Matrix rHandTransform = Matrix.CreateFromPose(RHandPose);
                    return rHandTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetGripTransform(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    Matrix lGripTransform = Matrix.CreateFromPose(LGripPose);
                    return lGripTransform;
                case 1:
                    Matrix rGripTransform = Matrix.CreateFromPose(RGripPose);
                    return rGripTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
