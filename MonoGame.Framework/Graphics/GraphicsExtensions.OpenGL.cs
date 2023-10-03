// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using MonoGame.OpenGL;


namespace Microsoft.Xna.Framework.Graphics
{
    static partial class GraphicsExtensions
    {
        public static ComparisonFunc ToGLComparisonFunction(CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return ComparisonFunc.Always;
                case CompareFunction.Equal:
                    return ComparisonFunc.Equal;
                case CompareFunction.Greater:
                    return ComparisonFunc.Greater;
                case CompareFunction.GreaterEqual:
                    return ComparisonFunc.Gequal;
                case CompareFunction.Less:
                    return ComparisonFunc.Less;
                case CompareFunction.LessEqual:
                    return ComparisonFunc.Lequal;
                case CompareFunction.Never:
                    return ComparisonFunc.Never;
                case CompareFunction.NotEqual:
                    return ComparisonFunc.Notequal;

                default:
                    return ComparisonFunc.Always;
            }
        }

        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void CheckGLError()
        {
            ErrorCode error = GL.GetError();
            //Console.WriteLine(error);
            if (error != ErrorCode.NoError)
                throw new MonoGameGLException("GL.GetError() returned " + error.ToString());
        }

        [Conditional("DEBUG")]
        public static void LogGLError(string location)
        {
            try
            {
                GraphicsExtensions.CheckGLError();
            }
            catch (MonoGameGLException ex)
            {
#if ANDROID
                // Todo: Add generic MonoGame logging interface
                Android.Util.Log.Debug("MonoGame", "MonoGameGLException at " + location + " - " + ex.Message);
#else
                Debug.WriteLine("MonoGameGLException at " + location + " - " + ex.Message);
#endif
            }
        }

        [Conditional("DEBUG")]
        public static void CheckFramebufferStatus()
        {
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            switch (status)
            {
                case FramebufferErrorCode.FramebufferComplete:
                    return;
                case FramebufferErrorCode.FramebufferIncompleteAttachment:
                    throw new InvalidOperationException("Not all framebuffer attachment points are framebuffer attachment complete.");
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachment:
                    throw new InvalidOperationException("No images are attached to the framebuffer.");
                case FramebufferErrorCode.FramebufferUnsupported:
                    throw new InvalidOperationException("The combination of internal formats of the attached images violates an implementation-dependent set of restrictions.");
                case FramebufferErrorCode.FramebufferIncompleteMultisample:
                    throw new InvalidOperationException("Not all attached images have the same number of samples.");

                default:
                    throw new InvalidOperationException("Framebuffer Incomplete.");
            }
        }

    }
        
    internal class MonoGameGLException : Exception
    {
        public MonoGameGLException(string message)
            : base(message)
        {
        }
    }
}
