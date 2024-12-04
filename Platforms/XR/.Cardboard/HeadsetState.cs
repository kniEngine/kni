// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.XR
{
    public struct HeadsetState
    {
        public EyeState LeftEye;
        public EyeState RightEye;
    }
        
    public struct EyeState
    {
        public Matrix View;
        
        public Matrix Projection;
        public Viewport Viewport;
    }
}