// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    static partial class GraphicsExtensions
    {
        internal static IWebGLRenderingContext GL;

        [Conditional("DEBUG")]
        internal static void CheckGLError()
        {
            WebGLErrorCode error = GL.GetError();
            if (error != WebGLErrorCode.NO_ERROR)
            {
                Console.WriteLine(error);
                throw new InvalidOperationException("GL.GetError() returned " + error.ToString());
            }
        }

    }
}
