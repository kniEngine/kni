// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {
        // TODO: make private
        internal IWebGLRenderingContext _glContext;

        private IWebGLRenderingContext GL { get { return _glContext; } }


        private void PlatformSetup()
        {
            ((ConcreteGraphicsDevice)_strategy)._programCache = new ShaderProgramCache(this);

            var handle = PresentationParameters.DeviceWindowHandle;
            var gameWindow = BlazorGameWindow.FromHandle(handle);
            var canvas = gameWindow._canvas;

            // create context.
            _glContext = canvas.GetContext<IWebGLRenderingContext>();
            var contextStrategy = new ConcreteGraphicsContext(this);
            _strategy._mainContext = new GraphicsContext(this, contextStrategy);
            GraphicsExtensions.GL = _glContext; // for GraphicsExtensions.CheckGLError()
            //_glContext = new LogContent(_glContext);

            Strategy._capabilities = new GraphicsCapabilities();
            Strategy._capabilities.PlatformInitialize(this);


            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._newEnabledVertexAttributes = new bool[Strategy.Capabilities.MaxVertexBufferSlots];
        }

        private void PlatformInitialize()
        {
            // set actual backbuffer size
            PresentationParameters.BackBufferWidth = _glContext.Canvas.Width;
            PresentationParameters.BackBufferHeight = _glContext.Canvas.Height;

            _strategy._mainContext.Strategy._viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // TODO: check for FramebufferObjectARB
            //if (this.Capabilities.SupportsFramebufferObjectARB
            //||  this.Capabilities.SupportsFramebufferObjectEXT)
            if (true)
            {
                ((ConcreteGraphicsDevice)_strategy)._supportsBlitFramebuffer = false; // GL.BlitFramebuffer != null;
                ((ConcreteGraphicsDevice)_strategy)._supportsInvalidateFramebuffer = false; // GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            _strategy._mainContext.Strategy._actualBlendState.PlatformApplyState((ConcreteGraphicsContext)_strategy._mainContext.Strategy, true);
            _strategy._mainContext.Strategy._actualDepthStencilState.PlatformApplyState((ConcreteGraphicsContext)_strategy._mainContext.Strategy, true);
            _strategy._mainContext.Strategy._actualRasterizerState.PlatformApplyState((ConcreteGraphicsContext)_strategy._mainContext.Strategy, true);

            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos = new ConcreteGraphicsContext.BufferBindingInfo[Strategy.Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos.Length; i++)
                ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._bufferBindingInfos[i] = new ConcreteGraphicsContext.BufferBindingInfo(null, IntPtr.Zero, 0,  null);
        }

        private void PlatformDispose()
        {
        }

        internal void PlatformPresent()
        {
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            throw new NotImplementedException();
        }

        internal void OnPresentationChanged()
        {
            _strategy._mainContext.ApplyRenderTargets(null);
        }

    }
}
