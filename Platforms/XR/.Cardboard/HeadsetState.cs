// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.XR
{
    internal struct CardboardHeadsetState
    {
        public CardboardEyeState LeftEye;
        public CardboardEyeState RightEye;
    }

    internal struct CardboardEyeState
    {
        public Matrix View;
        
        public Matrix Projection;
        public Viewport Viewport;
    }
}