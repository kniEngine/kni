// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.XR
{
    public struct HandsState
    {
        public Matrix LHandTransform;
        public Matrix RHandTransform;
        
        public Matrix LGripTransform;
        public Matrix RGripTransform;

        public Matrix GetHandTransform(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    Matrix lHandTransform = LHandTransform;
                    return lHandTransform;
                case 1:
                    Matrix rHandTransform = RHandTransform;
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
                    Matrix lGripTransform = LGripTransform;
                    return lGripTransform;
                case 1:
                    Matrix rGripTransform = RGripTransform;
                    return rGripTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
