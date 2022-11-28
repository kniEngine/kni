// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;

#if IOS || TVOS
using MonoGame.OpenGL;
using Foundation;
using OpenGLES;
#endif


namespace Microsoft.Xna.Framework
{
    internal class Threading
    {
        static int _mainThreadId;

#if IOS || TVOS
        public static EAGLContext BackgroundContext;
#endif

        static Threading()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

#if ANDROID
        internal static void ResetThread(int id)
        {
            _mainThreadId = id;
        }
#endif

        /// <summary>
        /// Checks if the code is currently running on the UI thread.
        /// </summary>
        /// <returns>true if the code is currently running on the UI thread.</returns>
        public static bool IsOnUIThread()
        {
            return _mainThreadId == Thread.CurrentThread.ManagedThreadId;
        }

        /// <summary>
        /// Throws an exception if the code is not currently running on the UI thread.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the code is not currently running on the UI thread.</exception>
        public static void EnsureUIThread()
        {
            if (_mainThreadId == Thread.CurrentThread.ManagedThreadId)
                return;
            
            throw new InvalidOperationException("Operation not called on UI thread.");
        }

        /// <summary>
        /// Runs all pending actions. Must be called from the UI thread.
        /// </summary>
        internal static void Run()
        {
#if IOS || TVOS
            EnsureUIThread();

            lock (BackgroundContext)
            {
                // Make the context current on this thread if it is not already
                if (!Object.ReferenceEquals(EAGLContext.CurrentContext, BackgroundContext))
                    EAGLContext.SetCurrentContext(BackgroundContext);

                // Must flush the GL calls so the GPU asset is ready for the main context to use it
                GL.Flush();
                GraphicsExtensions.CheckGLError();
            }
#endif
        }
    }
}
