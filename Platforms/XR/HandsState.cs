// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Oculus
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
                    return LHandTransform;
                case 1:
                    return RHandTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public Matrix GetGripTransform(int handIndex)
        {
            switch (handIndex)
            {
                case 0:
                    return LGripTransform;
                case 1:
                    return RGripTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
