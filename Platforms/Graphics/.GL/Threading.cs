// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Threading;


namespace Microsoft.Xna.Platform.Graphics.OpenGL
{
    internal class Threading
    {
        static int _mainThreadId;

        static Threading()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Set main game thread.
        /// </summary>
        internal static void MakeMainThread()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Throws an exception if the code is not currently running on the main thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the code is not currently running on the main thread.</exception>
        [DebuggerHidden]
        public static void EnsureMainThread()
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
                return;
            
            throw new InvalidOperationException("Operation not called on main thread.");
        }

    }
}
