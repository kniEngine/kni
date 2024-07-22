using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Input.Cardboard
{
    public struct HeadsetState
    {
        public EyeState LeftEye;
        public EyeState RightEye;
    }

    public struct EyeState
    {
        public Viewport Viewport;
        public Matrix View;
        public Matrix Projection;
    }
}