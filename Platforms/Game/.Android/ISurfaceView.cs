// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Platform.Graphics.OpenGL;
using Javax.Microedition.Khronos.Egl;


namespace Microsoft.Xna.Platform
{
    interface ISurfaceView
    {

        public event EventHandler<EventArgs> SurfaceCreated;
        public event EventHandler<EventArgs> SurfaceChanged;
        public event EventHandler<EventArgs> SurfaceDestroyed;
    }
}
