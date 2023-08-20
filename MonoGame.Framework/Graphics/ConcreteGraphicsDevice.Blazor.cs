// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {
        internal ShaderProgramCache _programCache;

        internal bool _supportsInvalidateFramebuffer;
        internal bool _supportsBlitFramebuffer;

        internal WebGLFramebuffer _glDefaultFramebuffer = null;


        internal ConcreteGraphicsDevice(GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            PresentationParameters = presentationParameters;
            Reset();
        }

        public override void Reset()
        {
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new NotImplementedException();
        }

        public override void Present()
        {
            base.Present();
        }

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            throw new NotImplementedException();
        }

        internal ShaderProgram GetProgram(Shader vertexShader, Shader pixelShader, int shaderProgramHash)
        {
            return _programCache.GetProgram(vertexShader, pixelShader, shaderProgramHash);
        }

        internal void ClearProgramCache()
        {
            _programCache.Clear();
        }

        internal void DisposeProgramCache()
        {
            _programCache.Dispose();
        }


        internal override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            IntPtr handle = PresentationParameters.DeviceWindowHandle;
            GameWindow gameWindow = BlazorGameWindow.FromHandle(handle);
            Canvas canvas = ((BlazorGameWindow)gameWindow)._canvas;

            IWebGLRenderingContext glContext = canvas.GetContext<IWebGLRenderingContext>();

            return new ConcreteGraphicsContext(context, glContext);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
