// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.XR
{
    public struct CardboardHeadsetState
    {
        public CardboardEyeState LeftEye;
        public CardboardEyeState RightEye;
    }
        
    public struct CardboardEyeState
    {
        public Matrix View;
        
        public Matrix Projection;
        public Viewport Viewport;
    }
}