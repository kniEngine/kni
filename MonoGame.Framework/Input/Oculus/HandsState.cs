// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Oculus
{
    public struct HandsState
    {
        public Matrix LHandTransform;
        public Matrix RHandTransform;
        
        public Matrix GetHandTransform(int handIndex)
        {
            switch (eyeIndex)
            {
                case 0:
                    return LHandTransform;
                case 1:
                    return RHandTransform;
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
