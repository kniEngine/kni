// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using Microsoft.Xna.Platform.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


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
            var GL = OGL.Current;

            ErrorCode error = GL.GetError();
            //Console.WriteLine(error);
            if (error != ErrorCode.NoError)
            {
                throw new OpenGLException("GL.GetError() returned " + error.ToString());
            }
        }

        [Conditional("DEBUG")]
        public static void LogGLError(string location)
        {
            try
            {
                GraphicsExtensions.CheckGLError();
            }
            catch (OpenGLException ex)
            {
#if ANDROID
                // Todo: Add generic logging interface
                Android.Util.Log.Debug("KNI", "OpenGLException at " + location + " - " + ex.Message);
#else
                Debug.WriteLine("OpenGLException at " + location + " - " + ex.Message);
#endif
            }
        }

    }
        
}
