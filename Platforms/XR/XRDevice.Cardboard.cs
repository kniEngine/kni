// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.XR
{
    public class XRDevice
    {
        internal static GameWindow GameWindow;

        public static HeadsetState GetState()
        {
            HeadsetState state;

            var window = GameWindow as AndroidGameWindow;
            window.UpdateHeadsetState(out state);

            return state;
        }


    }
}

