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
