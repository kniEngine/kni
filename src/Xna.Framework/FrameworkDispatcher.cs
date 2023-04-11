// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform;

namespace Microsoft.Xna.Platform
{
    public interface IFrameworkDispatcher
    {
        event Action OnUpdate;
        void Update();
    }
}

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Helper class for processing internal framework events.
    /// </summary>
    /// <remarks>
    /// If you use <see cref="Game"/> class, <see cref="Update()"/> is called automatically.
    /// Otherwise you must call it as part of your game loop.
    /// </remarks>
    public sealed class FrameworkDispatcher : IFrameworkDispatcher
    {
        private static FrameworkDispatcher _current;

        /// <summary>
        /// Returns the current FrameworkDispatcher instance.
        /// </summary> 
        public static FrameworkDispatcher Current
        {
            get
            {
                lock (typeof(FrameworkDispatcher))
                {
                    if (_current == null)
                        _current = new FrameworkDispatcher();
                    return _current;
                }
            }
        }

        /// <summary>
        /// Processes framework events.
        /// </summary>
        public static void Update()
        {
            ((IFrameworkDispatcher)FrameworkDispatcher.Current).Update();
        }


        private Action _onUpdate;

        private FrameworkDispatcher()
        {

        }

        event Action IFrameworkDispatcher.OnUpdate
        {
            add { _onUpdate += value; }
            remove { _onUpdate -= value; }
        }

        void IFrameworkDispatcher.Update()
        {
            var updateHandler = _onUpdate;
            if (updateHandler != null)
                updateHandler();
        }
    }
}


