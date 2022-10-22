// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;

#if DESKTOPGL || GLES
using MonoGame.OpenGL;
#endif


namespace Microsoft.Xna.Framework.Graphics
{
    static partial class GraphicsExtensions
    {
        public static DepthFunction ToGLComparisonFunction(CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return DepthFunction.Always;
                case CompareFunction.Equal:
                    return DepthFunction.Equal;
                case CompareFunction.Greater:
                    return DepthFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return DepthFunction.Gequal;
                case CompareFunction.Less:
                    return DepthFunction.Less;
                case CompareFunction.LessEqual:
                    return DepthFunction.Lequal;
                case CompareFunction.Never:
                    return DepthFunction.Never;
                case CompareFunction.NotEqual:
                    return DepthFunction.Notequal;
                    
                default:
                    return DepthFunction.Always;
            }
        }

        public static GLStencilFunction ToGLStencilComparisonFunc(CompareFunction function)
        {
            switch (function)
            {
                case CompareFunction.Always:
                    return GLStencilFunction.Always;
                case CompareFunction.Equal:
                    return GLStencilFunction.Equal;
                case CompareFunction.Greater:
                    return GLStencilFunction.Greater;
                case CompareFunction.GreaterEqual:
                    return GLStencilFunction.Gequal;
                case CompareFunction.Less:
                    return GLStencilFunction.Less;
                case CompareFunction.LessEqual:
                    return GLStencilFunction.Lequal;
                case CompareFunction.Never:
                    return GLStencilFunction.Never;
                case CompareFunction.NotEqual:
                    return GLStencilFunction.Notequal;

                default:
                    return GLStencilFunction.Always;
            }
        }

        [Conditional("DEBUG")]
        [DebuggerHidden]
        public static void CheckGLError()
        {
            var error = GL.GetError();
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

    }
        
    internal class MonoGameGLException : Exception
    {
        public MonoGameGLException(string message)
            : base(message)
        {
        }
    }
}
