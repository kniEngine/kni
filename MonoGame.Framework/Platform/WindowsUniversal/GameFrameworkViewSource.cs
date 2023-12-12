// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Windows.ApplicationModel.Core;


namespace MonoGame.Framework
{
    public class GameFrameworkViewSource<T> : IFrameworkViewSource
        where T : Game, new()
    {
        public GameFrameworkViewSource()
        {
        }

        [CLSCompliant(false)]
        public IFrameworkView CreateView()
        {
            return new UAPFrameworkView<T>();
        }
    }
}
