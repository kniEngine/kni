// Copyright (C)2022 Nick Kastellanos

using System;
using System.Diagnostics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    static partial class GraphicsExtensions
    {
        internal static IWebGLRenderingContext GL;

        public static WebGLDepthComparisonFunc ToGLComparisonFunction(CompareFunction compare)
        {
            switch (compare)
            {
                case CompareFunction.Always:
                    return WebGLDepthComparisonFunc.ALWAYS;
                case CompareFunction.Equal:
                    return WebGLDepthComparisonFunc.EQUAL;
                case CompareFunction.Greater:
                    return WebGLDepthComparisonFunc.GREATER;
                case CompareFunction.GreaterEqual:
                    return WebGLDepthComparisonFunc.GEQUAL;
                case CompareFunction.Less:
                    return WebGLDepthComparisonFunc.LESS;
                case CompareFunction.LessEqual:
                    return WebGLDepthComparisonFunc.LEQUAL;
                case CompareFunction.Never:
                    return WebGLDepthComparisonFunc.NEVER;
                case CompareFunction.NotEqual:
                    return WebGLDepthComparisonFunc.NOTEQUAL;

                default:
                    throw new ArgumentOutOfRangeException("compare");
            }
        }

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

        [Conditional("DEBUG")]
        public static void CheckFramebufferStatus()
        {
            WebGLFramebufferStatus status = GL.CheckFramebufferStatus(WebGLFramebufferType.FRAMEBUFFER);
            switch (status)
            {
                case WebGLFramebufferStatus.FRAMEBUFFER_COMPLETE:
                    return;
                case WebGLFramebufferStatus.FRAMEBUFFER_INCOMPLETE_ATTACHMENT:
                    throw new InvalidOperationException("Not all framebuffer attachment points are framebuffer attachment complete.");
                case WebGLFramebufferStatus.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT:
                    throw new InvalidOperationException("No images are attached to the framebuffer.");
                case WebGLFramebufferStatus.FRAMEBUFFER_UNSUPPORTED:
                    throw new InvalidOperationException("The combination of internal formats of the attached images violates an implementation-dependent set of restrictions.");
                case WebGLFramebufferStatus.FRAMEBUFFER_INCOMPLETE_DIMENSIONS:
                    throw new InvalidOperationException("Not all attached images have the same dimensions.");

                default:
                    throw new InvalidOperationException("Framebuffer Incomplete.");
            }
        }

    }
}
