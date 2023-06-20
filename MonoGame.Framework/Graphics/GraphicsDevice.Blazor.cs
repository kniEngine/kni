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


        private ShaderProgramCache _programCache;


        private bool _supportsInvalidateFramebuffer;
        private bool _supportsBlitFramebuffer;

        private const WebGLFramebuffer _glDefaultFramebuffer = null;

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int ShaderProgramHash
        {
            get
            {
                if (_mainContext.Strategy._vertexShader == null && _mainContext.Strategy._pixelShader == null)
                    throw new InvalidOperationException("There is no shader bound!");
                if (_mainContext.Strategy._vertexShader == null)
                    return _mainContext.Strategy._pixelShader.HashKey;
                if (_mainContext.Strategy._pixelShader == null)
                    return _mainContext.Strategy._vertexShader.HashKey;
                return _mainContext.Strategy._vertexShader.HashKey ^ _mainContext.Strategy._pixelShader.HashKey;
            }
        }

        private int ShaderProgramHash2
        {
            get { return _mainContext.Strategy._vertexShader.HashKey ^ _mainContext.Strategy._pixelShader.HashKey; }
        }

        internal void SetVertexAttributeArray(bool[] attrs)
        {
            for (var x = 0; x < attrs.Length; x++)
            {
                if (attrs[x])
                {
                    if (((ConcreteGraphicsContext)_mainContext.Strategy)._enabledVertexAttributes.Add(x))
                    {
                        GL.EnableVertexAttribArray(x);
                        GraphicsExtensions.CheckGLError();
                    }
                }
                else
                {
                    if (((ConcreteGraphicsContext)_mainContext.Strategy)._enabledVertexAttributes.Remove(x))
                    {
                        GL.DisableVertexAttribArray(x);
                        GraphicsExtensions.CheckGLError();
                    }
                }
            }
        }

        private void PlatformApplyVertexBuffersAttribs(Shader shader, int baseVertex)
        {
            var programHash = ShaderProgramHash;
            var bindingsChanged = false;

            for (var slot = 0; slot < _mainContext.Strategy._vertexBuffers.Count; slot++)
            {
                var vertexBufferBinding = _mainContext.Strategy._vertexBuffers.Get(slot);
                var vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

                var vertexStride = vertexDeclaration.VertexStride;
                var offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!((ConcreteGraphicsContext)_mainContext.Strategy)._attribsDirty &&
                    slot < ((ConcreteGraphicsContext)_mainContext.Strategy)._activeBufferBindingInfosCount &&
                    ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].AttributeInfo, attrInfo) &&
                    ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                    ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer.vbo)
                    continue;

                bindingsChanged = true;

                GL.BindBuffer(WebGLBufferType.ARRAY, vertexBufferBinding.VertexBuffer.vbo);
                GraphicsExtensions.CheckGLError();

                for (int e = 0; e < attrInfo.Elements.Count; e++)
                {
                    var element = attrInfo.Elements[e];
                    GL.VertexAttribPointer(element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        ((IntPtr)(offset.ToInt64() + element.Offset)).ToInt32());
                    GraphicsExtensions.CheckGLError();

                    // only set the divisor if instancing is supported
                    if (Capabilities.SupportsInstancing)
                    {
                        throw new NotImplementedException();
                        //GL2.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
                        //GraphicsExtensions.CheckGLError();
                    }
                    else // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                    {
                        if (vertexBufferBinding.InstanceFrequency > 0)
                            throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");
                    }
                }

                ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].VertexOffset = offset;
                ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].AttributeInfo = attrInfo;
                ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer.vbo;
            }

            ((ConcreteGraphicsContext)_mainContext.Strategy)._attribsDirty = false;

            if (bindingsChanged)
            {
                for (int eva = 0; eva < ((ConcreteGraphicsContext)_mainContext.Strategy)._newEnabledVertexAttributes.Length; eva++)
                    ((ConcreteGraphicsContext)_mainContext.Strategy)._newEnabledVertexAttributes[eva] = false;
                for (var slot = 0; slot < _mainContext.Strategy._vertexBuffers.Count; slot++)
                {
                    for (int e = 0; e< ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].AttributeInfo.Elements.Count; e++)
                    {
                        var element = ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[slot].AttributeInfo.Elements[e];
                        ((ConcreteGraphicsContext)_mainContext.Strategy)._newEnabledVertexAttributes[element.AttributeLocation] = true;
                    }
                }
                ((ConcreteGraphicsContext)_mainContext.Strategy)._activeBufferBindingInfosCount = _mainContext.Strategy._vertexBuffers.Count;
            }
            SetVertexAttributeArray(((ConcreteGraphicsContext)_mainContext.Strategy)._newEnabledVertexAttributes);
        }

        private void PlatformApplyUserVertexDataAttribs(VertexDeclaration vertexDeclaration, Shader shader, int baseVertex)
        {
            var programHash = ShaderProgramHash;
            var attrInfo = vertexDeclaration.GetAttributeInfo(shader, programHash);

            // Apply the vertex attribute info
            for (int i = 0; i < attrInfo.Elements.Count; i++)
            {
                var element = attrInfo.Elements[i];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    vertexDeclaration.VertexStride,
                    (baseVertex + element.Offset));
                GraphicsExtensions.CheckGLError();

                if (Capabilities.SupportsInstancing)
                {
                    throw new NotImplementedException();
                    //GL2.VertexAttribDivisor(element.AttributeLocation, 0);
                    //GraphicsExtensions.CheckGLError();
                }
            }
            SetVertexAttributeArray(attrInfo.EnabledAttributes);
            ((ConcreteGraphicsContext)_mainContext.Strategy)._attribsDirty = true;
        }

        private void PlatformSetup()
        {
            _programCache = new ShaderProgramCache(this);

            var handle = PresentationParameters.DeviceWindowHandle;
            var gameWindow = BlazorGameWindow.FromHandle(handle);
            var canvas = gameWindow._canvas;

            // create context.
            _glContext = canvas.GetContext<IWebGLRenderingContext>();
            var contextStrategy = new ConcreteGraphicsContext(this);
            _mainContext = new GraphicsContext(this, contextStrategy);
            GraphicsExtensions.GL = _glContext; // for GraphicsExtensions.CheckGLError()
            //_glContext = new LogContent(_glContext);

            Capabilities = new GraphicsCapabilities();
            Capabilities.PlatformInitialize(this);


            ((ConcreteGraphicsContext)_mainContext.Strategy)._newEnabledVertexAttributes = new bool[Capabilities.MaxVertexBufferSlots];
        }

        private void PlatformInitialize()
        {
            // set actual backbuffer size
            PresentationParameters.BackBufferWidth = _glContext.Canvas.Width;
            PresentationParameters.BackBufferHeight = _glContext.Canvas.Height;

            _viewport = new Viewport(0, 0, PresentationParameters.BackBufferWidth, PresentationParameters.BackBufferHeight);

            // Ensure the vertex attributes are reset
            ((ConcreteGraphicsContext)_mainContext.Strategy)._enabledVertexAttributes.Clear();

            // Free all the cached shader programs. 
            _programCache.DisposePrograms();
            ((ConcreteGraphicsContext)_mainContext.Strategy)._shaderProgram = null;

            // TODO: check for FramebufferObjectARB
            //if (graphicsDevice.Capabilities.SupportsFramebufferObjectARB
            //|| graphicsDevice.Capabilities.SupportsFramebufferObjectEXT)
            if (true)
            {
                this._supportsBlitFramebuffer = false; // GL.BlitFramebuffer != null;
                this._supportsInvalidateFramebuffer = false; // GL.InvalidateFramebuffer != null;
            }
            else
            {
                throw new PlatformNotSupportedException(
                    "MonoGame requires either ARB_framebuffer_object or EXT_framebuffer_object." +
                    "Try updating your graphics drivers.");
            }

            // Force resetting states
            this._mainContext.Strategy._actualBlendState.PlatformApplyState((ConcreteGraphicsContext)_mainContext.Strategy, true);
            this._mainContext.Strategy._actualDepthStencilState.PlatformApplyState((ConcreteGraphicsContext)_mainContext.Strategy, true);
            this._mainContext.Strategy._actualRasterizerState.PlatformApplyState((ConcreteGraphicsContext)_mainContext.Strategy, true);

            ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos = new ConcreteGraphicsContext.BufferBindingInfo[Capabilities.MaxVertexBufferSlots];
            for (int i = 0; i < ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos.Length; i++)
                ((ConcreteGraphicsContext)_mainContext.Strategy)._bufferBindingInfos[i] = new ConcreteGraphicsContext.BufferBindingInfo(null, IntPtr.Zero, 0,  null);
        }
        
        private void PlatformClear(ClearOptions options, Vector4 color, float depth, int stencil)
        {
            // TODO: We need to figure out how to detect if we have a
            // depth stencil buffer or not, and clear options relating
            // to them if not attached.

            // Unlike with XNA and DirectX...  GL.Clear() obeys several
            // different render states:
            //
            //  - The color write flags.
            //  - The scissor rectangle.
            //  - The depth/stencil state.
            //
            // So overwrite these states with what is needed to perform
            // the clear correctly and restore it afterwards.
            //
		    var prevScissorRect = ScissorRectangle;
		    var prevDepthStencilState = DepthStencilState;
            var prevBlendState = BlendState;
            ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            DepthStencilState = ((ConcreteGraphicsContext)_mainContext.Strategy)._clearDepthStencilState;
		    BlendState = BlendState.Opaque;
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();

            WebGLBufferBits bb = default(WebGLBufferBits);
            if ((options & ClearOptions.Target) != 0)
            {
                GL.ClearColor(color.X, color.Y, color.Z, color.W);
                GraphicsExtensions.CheckGLError();
                bb |= WebGLBufferBits.COLOR;
            }
            if ((options & ClearOptions.DepthBuffer) != 0)
            {
                GL.ClearDepth(depth);
                GraphicsExtensions.CheckGLError();
                bb |= WebGLBufferBits.DEPTH;
            }
            if ((options & ClearOptions.Stencil) != 0)
            {
                GL.ClearStencil(stencil);
                GraphicsExtensions.CheckGLError();
                bb |= WebGLBufferBits.STENCIL;
            }

            GL.Clear(bb);
            GraphicsExtensions.CheckGLError();

            // Restore the previous render state.
		    ScissorRectangle = prevScissorRect;
		    DepthStencilState = prevDepthStencilState;
		    BlendState = prevBlendState;
        }

        private void PlatformDispose()
        {
        }

        internal void PlatformPresent()
        {
        }

        private void PlatformApplyDefaultRenderTarget()
        {
            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, _glDefaultFramebuffer);
            GraphicsExtensions.CheckGLError();

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _mainContext.Strategy._rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            _mainContext.Strategy.Textures.Dirty();
        }

        internal void PlatformCreateRenderTarget(IRenderTarget renderTarget, int width, int height, bool mipMap, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, RenderTargetUsage usage)
        {
            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;
            
            if (preferredMultiSampleCount > 0 && _supportsBlitFramebuffer)
            {
                throw new NotImplementedException();
            }

            if (preferredDepthFormat != DepthFormat.None)
            {
                var depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                var stencilInternalFormat = (WebGLRenderbufferInternalFormat)0;
                switch (preferredDepthFormat)
                {
                    case DepthFormat.Depth16: 
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        break;
                    case DepthFormat.Depth24:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        break;
                    case DepthFormat.Depth24Stencil8:
                        depthInternalFormat = WebGLRenderbufferInternalFormat.DEPTH_COMPONENT16;
                        stencilInternalFormat = WebGLRenderbufferInternalFormat.STENCIL_INDEX8;
                        break;
                }

                if (depthInternalFormat != 0)
                {
                    depth = GL.CreateRenderbuffer();
                    GraphicsExtensions.CheckGLError();
                    GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, depth);
                    GraphicsExtensions.CheckGLError();
                    if (preferredMultiSampleCount > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                        throw new NotImplementedException();
                    else
                        GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, depthInternalFormat, width, height);
                    GraphicsExtensions.CheckGLError();
                    if (preferredDepthFormat == DepthFormat.Depth24Stencil8)
                    {
                        stencil = depth;
                        if (stencilInternalFormat != 0)
                        {
                            stencil = GL.CreateRenderbuffer();
                            GraphicsExtensions.CheckGLError();
                            GL.BindRenderbuffer(WebGLRenderbufferType.RENDERBUFFER, stencil);
                            GraphicsExtensions.CheckGLError();
                            if (preferredMultiSampleCount > 0 /*&& GL.RenderbufferStorageMultisample != null*/)
                                throw new NotImplementedException();
                            else
                                GL.RenderbufferStorage(WebGLRenderbufferType.RENDERBUFFER, stencilInternalFormat, width, height);
                            GraphicsExtensions.CheckGLError();
                        }
                    }
                }
            }

            var renderTargetGL = (IRenderTargetGL)renderTarget;
            if (color != null)
                renderTargetGL.GLColorBuffer = color;
            else
                renderTargetGL.GLColorBuffer = renderTargetGL.GLTexture;
            renderTargetGL.GLDepthBuffer = depth;
            renderTargetGL.GLStencilBuffer = stencil;
        }

        internal void PlatformDeleteRenderTarget(IRenderTarget renderTarget)
        {
            WebGLTexture color = null;
            WebGLRenderbuffer depth = null;
            WebGLRenderbuffer stencil = null;

            var renderTargetGL = (IRenderTargetGL)renderTarget;
            color = renderTargetGL.GLColorBuffer;
            depth = renderTargetGL.GLDepthBuffer;
            stencil = renderTargetGL.GLStencilBuffer;
            bool colorIsRenderbuffer = renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture;

            if (color != null)
            {
                if (colorIsRenderbuffer)
                {
                    throw new NotImplementedException();
                }
                if (stencil != null && stencil != depth)
                {
                    stencil.Dispose();
                    GraphicsExtensions.CheckGLError();
                }
                if (depth != null)
                {
                    depth.Dispose();
                    GraphicsExtensions.CheckGLError();
                }

                var bindingsToDelete = new List<RenderTargetBinding[]>();
                foreach (var bindings in ((ConcreteGraphicsContext)_mainContext.Strategy)._glFramebuffers.Keys)
                {
                    foreach (var binding in bindings)
                    {
                        if (binding.RenderTarget == renderTarget)
                        {
                            bindingsToDelete.Add(bindings);
                            break;
                        }
                    }
                }

                foreach (var bindings in bindingsToDelete)
                {
                    WebGLFramebuffer fbo = null;
                    if (((ConcreteGraphicsContext)_mainContext.Strategy)._glFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        fbo.Dispose();
                        GraphicsExtensions.CheckGLError();
                        ((ConcreteGraphicsContext)_mainContext.Strategy)._glFramebuffers.Remove(bindings);
                    }
                    if (((ConcreteGraphicsContext)_mainContext.Strategy)._glResolveFramebuffers.TryGetValue(bindings, out fbo))
                    {
                        fbo.Dispose();
                        GraphicsExtensions.CheckGLError();

                        ((ConcreteGraphicsContext)_mainContext.Strategy)._glResolveFramebuffers.Remove(bindings);
                    }
                }
            }
        }

        private void PlatformResolveRenderTargets()
        {
            if (!this._mainContext.Strategy.IsRenderTargetBound)
                return;

            var renderTargetBinding = _mainContext.Strategy._currentRenderTargetBindings[0];
            var renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0 && _supportsBlitFramebuffer)
            {
                throw new NotImplementedException();
            }

            for (var i = 0; i < _mainContext.Strategy._currentRenderTargetCount; i++)
            {
                renderTargetBinding = _mainContext.Strategy._currentRenderTargetBindings[i];
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;
                    GL.BindTexture(renderTargetGL.GLTarget, renderTargetGL.GLTexture);
                    GraphicsExtensions.CheckGLError();
                    GL.GenerateMipmap(renderTargetGL.GLTarget);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        private IRenderTarget PlatformApplyRenderTargets()
        {
            WebGLFramebuffer glFramebuffer = null;
            if (!((ConcreteGraphicsContext)_mainContext.Strategy)._glFramebuffers.TryGetValue(_mainContext.Strategy._currentRenderTargetBindings, out glFramebuffer))
            {
                glFramebuffer = GL.CreateFramebuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
                GraphicsExtensions.CheckGLError();
                var renderTargetBinding = _mainContext.Strategy._currentRenderTargetBindings[0];
                var renderTargetGL = (IRenderTargetGL)renderTargetBinding.RenderTarget;             
                GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLDepthBuffer);
                GraphicsExtensions.CheckGLError();
                GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLStencilBuffer);
                GraphicsExtensions.CheckGLError();

                for (var i = 0; i < _mainContext.Strategy._currentRenderTargetCount; i++)
                {
                    renderTargetBinding = _mainContext.Strategy._currentRenderTargetBindings[i];
                    var renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    renderTargetGL = renderTargetBinding.RenderTarget as IRenderTargetGL;
                    var attachement = (WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0 + i);
                    if (renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        WebGLTextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(WebGLFramebufferType.FRAMEBUFFER, attachement, target, renderTargetGL.GLTexture);
                        GraphicsExtensions.CheckGLError();
                    }
                }

                GraphicsExtensions.CheckFramebufferStatus();

                ((ConcreteGraphicsContext)_mainContext.Strategy)._glFramebuffers.Add((RenderTargetBinding[])_mainContext.Strategy._currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
                GraphicsExtensions.CheckGLError();
            }
#if !GLES
            //GL.DrawBuffers(_currentRenderTargetCount, _drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _mainContext.Strategy._rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            _mainContext.Strategy.Textures.Dirty();

            return _mainContext.Strategy._currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            // Lookup the shader program.
            var shaderProgram = _programCache.GetProgram(_mainContext.Strategy.VertexShader, _mainContext.Strategy.PixelShader, ShaderProgramHash2);
            if (shaderProgram.Program == null)
                return;

            // Set the new program if it has changed.
            if (((ConcreteGraphicsContext)_mainContext.Strategy)._shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
                ((ConcreteGraphicsContext)_mainContext.Strategy)._shaderProgram = shaderProgram;
            }

            var posFixupLoc = shaderProgram.GetUniformLocation("posFixup");
            if (posFixupLoc == null)
                return;

            // Apply vertex shader fix:
            // The following two lines are appended to the end of vertex shaders
            // to account for rendering differences between OpenGL and DirectX:
            //
            // gl_Position.y = gl_Position.y * posFixup.y;
            // gl_Position.xy += posFixup.zw * gl_Position.ww;
            //
            // (the following paraphrased from wine, wined3d/state.c and wined3d/glsl_shader.c)
            //
            // - We need to flip along the y-axis in case of offscreen rendering.
            // - D3D coordinates refer to pixel centers while GL coordinates refer
            //   to pixel corners.
            // - D3D has a top-left filling convention. We need to maintain this
            //   even after the y-flip mentioned above.
            // In order to handle the last two points, we translate by
            // (63.0 / 128.0) / VPw and (63.0 / 128.0) / VPh. This is equivalent to
            // translating slightly less than half a pixel. We want the difference to
            // be large enough that it doesn't get lost due to rounding inside the
            // driver, but small enough to prevent it from interfering with any
            // anti-aliasing.
            //
            // OpenGL coordinates specify the center of the pixel while d3d coords specify
            // the corner. The offsets are stored in z and w in posFixup. posFixup.y contains
            // 1.0 or -1.0 to turn the rendering upside down for offscreen rendering. PosFixup.x
            // contains 1.0 to allow a mad.

            ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.X = 1.0f;
            ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Y = 1.0f;
            if (!UseHalfPixelOffset)
            {
                ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Z = 0f;
                ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.W = 0f;
            }
            else
            {
                ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Z =  (63.0f/64.0f)/Viewport.Width;
                ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.W = -(63.0f/64.0f)/Viewport.Height;
            }

            //If we have a render target bound (rendering offscreen)
            if (_mainContext.Strategy.IsRenderTargetBound)
            {
                //flip vertically
                ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Y = -((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Y;
                ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.W = -((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.W;
            }
            
            GL.Uniform4f(posFixupLoc, ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.X, ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Y, ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.Z, ((ConcreteGraphicsContext)_mainContext.Strategy)._posFixup.W);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformApplyViewport()
        {
            if (_mainContext.Strategy.IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GraphicsExtensions.CheckGLError(); // GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            //GraphicsExtensions.CheckGLError(); // GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "posFixup"
            // vertex shader uniform if the viewport changes.
            _mainContext.Strategy._vertexShaderDirty = true;
        }

        private void PlatformApplyShaders()
        {
            if (_mainContext.Strategy._vertexShader == null)
                throw new InvalidOperationException("A vertex shader must be set!");
            if (_mainContext.Strategy._pixelShader == null)
                throw new InvalidOperationException("A pixel shader must be set!");

            if (_mainContext.Strategy._vertexShaderDirty || _mainContext.Strategy._pixelShaderDirty)
            {
                ActivateShaderProgram();

                if (_mainContext.Strategy._vertexShaderDirty)
                {
                    unchecked { CurrentContext._graphicsMetrics._vertexShaderCount++; }
                }

                if (_mainContext.Strategy._pixelShaderDirty)
                {
                    unchecked { CurrentContext._graphicsMetrics._pixelShaderCount++; }
                }

                _mainContext.Strategy._vertexShaderDirty = false;
                _mainContext.Strategy._pixelShaderDirty = false;
            }

            _mainContext.Strategy._vertexConstantBuffers.Apply(_mainContext.Strategy);
            _mainContext.Strategy._pixelConstantBuffers.Apply(_mainContext.Strategy);

            _mainContext.Strategy.VertexTextures.PlatformApply(_mainContext.Strategy);
            _mainContext.Strategy.VertexSamplerStates.PlatformApply(_mainContext.Strategy);
            _mainContext.Strategy.Textures.PlatformApply(_mainContext.Strategy);
            _mainContext.Strategy.SamplerStates.PlatformApply(_mainContext.Strategy);
        }

        private void PlatformDrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyIndexBuffer();
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            var shortIndices = _mainContext.Strategy._indexBuffer.IndexElementSize == IndexElementSize.SixteenBits;

			var indexElementType = shortIndices ? WebGLDataType.USHORT : WebGLDataType.UINT;
            var indexElementSize = shortIndices ? 2 : 4;
			var indexOffsetInBytes = (startIndex * indexElementSize);
			var indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
			var target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            PlatformApplyVertexBuffersAttribs(_mainContext.Strategy._vertexShader, baseVertex);

            GL.DrawElements(target,
                                     indexElementCount,
                                     indexElementType,
                                     indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount) where T : struct
        {
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyIndexBuffer();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            var vbo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GraphicsExtensions.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _mainContext.Strategy._vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData(WebGLBufferType.ARRAY, 0, vertexData, vertexData.Length);
            GraphicsExtensions.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            PlatformApplyUserVertexDataAttribs(vertexDeclaration, _mainContext.Strategy._vertexShader, vertexOffset);

            var target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                          vertexOffset,
                          vertexCount);
            GraphicsExtensions.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GraphicsExtensions.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GraphicsExtensions.CheckGLError();

            vbo.Dispose();
        }

        private void PlatformDrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyIndexBuffer();
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            PlatformApplyVertexBuffersAttribs(_mainContext.Strategy._vertexShader, 0);

            if (vertexStart < 0)
                vertexStart = 0;

            GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
			              vertexStart,
			              vertexCount);
            GraphicsExtensions.CheckGLError();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyIndexBuffer();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            var vbo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GraphicsExtensions.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _mainContext.Strategy._vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData(WebGLBufferType.ARRAY, 0, vertexData, vertexData.Length);
            GraphicsExtensions.CheckGLError();

            // create and bind index buffer
            var ibo = GL.CreateBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
            GraphicsExtensions.CheckGLError();
            GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                          (indexData.Length * sizeof(short)),
                          (false) ? WebGLBufferUsageHint.STREAM_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GraphicsExtensions.CheckGLError();
            // mark the default index buffer for rebinding
            _mainContext.Strategy._indexBufferDirty = true;

            // set index buffer
            GL.BufferSubData(WebGLBufferType.ELEMENT_ARRAY, 0, indexData, indexData.Length);
            GraphicsExtensions.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            vertexDeclaration.GraphicsDevice = this;
            PlatformApplyUserVertexDataAttribs(vertexDeclaration, _mainContext.Strategy._vertexShader, vertexOffset);


            var indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            var indexOffsetInBytes = (indexOffset * sizeof(short));
            var target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            GL.DrawElements(target,
                                     indexElementCount,
                                     WebGLDataType.USHORT,
                                     indexOffsetInBytes);
            GraphicsExtensions.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GraphicsExtensions.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GraphicsExtensions.CheckGLError();

            ibo.Dispose();
            vbo.Dispose();
        }

        private void PlatformDrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration) where T : struct
        {
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyIndexBuffer();
            //((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            throw new NotImplementedException();
        }

        private void PlatformDrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (!Capabilities.SupportsInstancing)
                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");

            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyState();
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyIndexBuffer();
            ((ConcreteGraphicsContext)_mainContext.Strategy).PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            throw new NotImplementedException();
        }

        private void PlatformGetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int count) where T : struct
        {
            throw new NotImplementedException();
        }

        private static Rectangle PlatformGetTitleSafeArea(int x, int y, int width, int height)
        {
            return new Rectangle(x, y, width, height);
        }
        
        internal void PlatformSetMultiSamplingToMaximum(PresentationParameters presentationParameters, out int quality)
        {
            presentationParameters.MultiSampleCount = 0;
            quality = 0;
        }

        internal void OnPresentationChanged()
        {
            ApplyRenderTargets(null);
        }

    }
}
