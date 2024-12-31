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

        public Vector3 LHandLinearVelocity;
        public Vector3 RHandLinearVelocity;

        public Vector3 GetLinearVelocity(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    return LHandLinearVelocity;
                case 1:
                    return RHandLinearVelocity;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetHandTransform(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    return Matrix.CreateFromPose(LHandPose);
                case 1:
                    return Matrix.CreateFromPose(RHandPose);
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetGripTransform(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    return Matrix.CreateFromPose(LGripPose);
                case 1:
                    return Matrix.CreateFromPose(RGripPose);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
