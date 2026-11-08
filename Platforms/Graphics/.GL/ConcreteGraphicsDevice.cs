// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsDeviceGL : GraphicsDeviceStrategy
    {
        private readonly Dictionary<int, ShaderProgram> _programCache = new Dictionary<int, ShaderProgram>();

        internal Dictionary<int, ShaderProgram> ProgramCache { get { return _programCache; } }

        internal int _glDefaultFramebuffer = 0;


        internal ConcreteGraphicsDeviceGL(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset()
        {
            PlatformReset(this.PresentationParameters);
        }

        public override void Reset(PresentationParameters presentationParameters)
        {
            PlatformReset(presentationParameters);
        }

        private void PlatformReset(PresentationParameters presentationParameters)
        {
            this.PresentationParameters = presentationParameters;

#if DESKTOPGL
            ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContext>().MakeCurrent(this.PresentationParameters.DeviceWindowHandle);
            int swapInterval = ConcreteGraphicsContext.ToGLSwapInterval(this.PresentationParameters.PresentationInterval);
            Sdl.Current.OpenGL.SetSwapInterval(swapInterval);
#endif

            ((IPlatformGraphicsContext)_mainContext).Strategy.ApplyRenderTargets(null);
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public unsafe override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            ConcreteGraphicsContextGL context = ((IPlatformGraphicsContext)this.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>();
            bool isSharedContext = context.BindSharedContext();
            try
            {
                var GL = context.GL;

                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, _glDefaultFramebuffer);
                GL.CheckGLError();
                GL.ReadBuffer(ReadBufferMode.Back);
                GL.CheckGLError();

                Rectangle srcRect = rect ?? new Rectangle(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);
                int fSize = PresentationParameters.BackBufferFormat.GetSize();
                int tSize = sizeof(T);
                int flippedY = PresentationParameters.BackBufferHeight - srcRect.Y - srcRect.Height;
                fixed (T* pData = &data[0])
                {
                    ConcreteTexture.ToGLSurfaceFormat(PresentationParameters.BackBufferFormat, context,
                        out PixelInternalFormat glInternalFormat,
                        out PixelFormat glFormat,
                        out PixelType glType,
                        out bool glIsCompressedFormat);

                    GL.PixelStore(PixelStoreParameter.PackAlignment, Math.Min(tSize, 8));
                    GL.CheckGLError();

                    IntPtr dataPtr = (IntPtr)pData + (startIndex * tSize);
                    GL.ReadPixels(srcRect.X, flippedY, srcRect.Width, srcRect.Height, glFormat, glType, dataPtr);
                    GL.CheckGLError();

                    // buffer is returned upside down, so we swap the rows around when copying over
                    int rowSize = srcRect.Width * fSize / tSize;

                    T[] row = new T[rowSize];

                    for (int dy = 0; dy < srcRect.Height / 2; dy++)
                    {
                        int topRow = startIndex + dy * rowSize;
                        int bottomRow = startIndex + (srcRect.Height - dy - 1) * rowSize;

                        // copy the bottom row to buffer
                        Array.Copy(data, bottomRow, row, 0, rowSize);

                        // copy top row to bottom row
                        Array.Copy(data, topRow, data, bottomRow, rowSize);

                        // copy buffer to top row
                        Array.Copy(row, 0, data, topRow, rowSize);
                    }
                }
            }
            finally
            {
                context.UnbindSharedContext();
            }
        }

        public override System.Reflection.Assembly ConcreteAssembly
        {
            get { return ReflectionHelpers.GetAssembly(typeof(ConcreteGraphicsDevice)); }
        }

        protected override void PlatformInitialize()
        {
        }

        private void ClearProgramCache()
        {
            var GL = ((IPlatformGraphicsContext)_mainContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            foreach (ShaderProgram shaderProgram in ProgramCache.Values)
            {
                if (GL.IsProgram(shaderProgram.Program))
                {
                    GL.DeleteProgram(shaderProgram.Program);
                    GL.CheckGLError();
                }
            }
            ProgramCache.Clear();
        }

        internal int GetMaxMultiSampleCount(SurfaceFormat surfaceFormat)
        {
            switch (surfaceFormat)
            {
                case SurfaceFormat.Color:
                case SurfaceFormat.Bgr565:
                case SurfaceFormat.Bgra4444:
                case SurfaceFormat.Bgra5551:
                case SurfaceFormat.Single:
                case SurfaceFormat.HalfSingle:
                case SurfaceFormat.Vector2:
                case SurfaceFormat.HalfVector2:
                case SurfaceFormat.Vector4:
                case SurfaceFormat.HalfVector4:
                    // See: PlatformCreateRenderTarget(...)
                    // for the supported surface types.
                    break;

                default:
                    return 0;
            }

            var contextStrategy = ((IPlatformGraphicsContext)this.MainContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>();
            var GL = contextStrategy.GL;

            int maxMultiSampleCount = 0;
            if (contextStrategy._supportsBlitFramebuffer
            &&  GL.RenderbufferStorageMultisample != null)
                GL.GetInteger(GetPName.MaxSamples, out maxMultiSampleCount);

            return maxMultiSampleCount;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            // Free all the cached shader programs.
            ClearProgramCache();

            base.Dispose(disposing);
        }

    }
}
