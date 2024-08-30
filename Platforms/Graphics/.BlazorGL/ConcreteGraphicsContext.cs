// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsContext : GraphicsContextStrategy
    {
        private IWebGLRenderingContext _glContext;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        internal BlendState _lastBlendState = new BlendState();
        internal bool _lastBlendEnable = false;
        internal DepthStencilState _lastDepthStencilState = new DepthStencilState();
        internal RasterizerState _lastRasterizerState = new RasterizerState();

        internal ShaderProgram _shaderProgram = null;

        private Vector4 _posFixup;

        internal BufferBindingInfo[] _bufferBindingInfos;
        private int _activeBufferBindingInfosCount;
        internal bool[] _newEnabledVertexAttributes;
        private readonly HashSet<int> _enabledVertexAttributesSet = new HashSet<int>();
        private int _lastVertexAttribs; // 0 = dirty, 1 = last set by PlatformApplyVertexBuffers, 2 = last set by PlatformApplyUserVertexData

        private DepthStencilState _clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], WebGLFramebuffer> _glFramebuffers = new Dictionary<RenderTargetBinding[], WebGLFramebuffer>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], WebGLFramebuffer> _glResolveFramebuffers = new Dictionary<RenderTargetBinding[], WebGLFramebuffer>(new RenderTargetBindingArrayComparer());

        internal ShaderProgram ShaderProgram { get { return _shaderProgram; } }

        internal IWebGLRenderingContext GlContext { get { return _glContext; } }
        internal IWebGLRenderingContext GL { get { return _glContext; } }

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal ConcreteGraphicsContext(GraphicsContext context, IWebGLRenderingContext glContext)
            : base(context)
        {
            _glContext = glContext;
        }

        public override void Clear(ClearOptions options, Vector4 color, float depth, int stencil)
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
            Rectangle prevScissorRect = this.ScissorRectangle;
            DepthStencilState prevDepthStencilState = this.DepthStencilState;
            BlendState prevBlendState = this.BlendState;
            this.ScissorRectangle = _viewport.Bounds;
            // DepthStencilState.Default has the Stencil Test disabled; 
            // make sure stencil test is enabled before we clear since
            // some drivers won't clear with stencil test disabled
            this.DepthStencilState = _clearDepthStencilState;
            this.BlendState = BlendState.Opaque;
            PlatformApplyState();

            WebGLBufferBits bb = default(WebGLBufferBits);
            if ((options & ClearOptions.Target) != 0)
            {
                GL.ClearColor(color.X, color.Y, color.Z, color.W);
                GL.CheckGLError();
                bb |= WebGLBufferBits.COLOR;
            }
            if ((options & ClearOptions.DepthBuffer) != 0)
            {
                GL.ClearDepth(depth);
                GL.CheckGLError();
                bb |= WebGLBufferBits.DEPTH;
            }
            if ((options & ClearOptions.Stencil) != 0)
            {
                GL.ClearStencil(stencil);
                GL.CheckGLError();
                bb |= WebGLBufferBits.STENCIL;
            }

            GL.Clear(bb);
            GL.CheckGLError();

            base.Metrics_AddClearCount();

            // Restore the previous render state.
            ScissorRectangle = prevScissorRect;
            DepthStencilState = prevDepthStencilState;
            BlendState = prevBlendState;
        }

        private void PlatformApplyState()
        {
            // Threading.EnsureMainThread();

            {
                PlatformApplyBlend();
            }

            if (_depthStencilStateDirty)
            {
                ((IPlatformDepthStencilState)_actualDepthStencilState).GetStrategy<ConcreteDepthStencilState>().PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                ((IPlatformRasterizerState)_actualRasterizerState).GetStrategy<ConcreteRasterizerState>().PlatformApplyState(this);
                _rasterizerStateDirty = false;
            }

            if (_scissorRectangleDirty)
            {
                PlatformApplyScissorRectangle();
                _scissorRectangleDirty = false;
            }
        }

        private void PlatformApplyBlend()
        {
            if (_blendStateDirty)
            {
                ((IPlatformBlendState)_actualBlendState).GetStrategy<ConcreteBlendState>().PlatformApplyState(this);
                _blendStateDirty = false;
            }

            if (_blendFactorDirty)
            {
                GL.BlendColor(
                    this.BlendFactor.R / 255.0f,
                    this.BlendFactor.G / 255.0f,
                    this.BlendFactor.B / 255.0f,
                    this.BlendFactor.A / 255.0f);
                GL.CheckGLError();
                _blendFactorDirty = false;
            }
        }

        private void PlatformApplyScissorRectangle()
        {
            if (IsRenderTargetBound)
            {
                GL.Scissor(_scissorRectangle.X, _scissorRectangle.Y, _scissorRectangle.Width, _scissorRectangle.Height);
                GL.CheckGLError();
            }
            else
            {
                int backBufferHeight = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.PresentationParameters.BackBufferHeight;
                GL.Scissor(_scissorRectangle.X, backBufferHeight - (_scissorRectangle.Y + _scissorRectangle.Height), _scissorRectangle.Width, _scissorRectangle.Height);
                GL.CheckGLError();
            }

        }

        internal void PlatformApplyViewport()
        {
            if (this.IsRenderTargetBound)
            {
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
                GL.CheckGLError(); // GL.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");
            }
            else
            {
                int backBufferHeight = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.PresentationParameters.BackBufferHeight;
                GL.Viewport(_viewport.X, backBufferHeight - (_viewport.Y + _viewport.Height), _viewport.Width, _viewport.Height);
                GL.CheckGLError(); // GL.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");
            }

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            //GL.CheckGLError(); // GL.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "_posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;
        }

        private void PlatformApplyIndexBuffer()
        {
            if (_indexBufferDirty)
            {
                GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().GLIndexBuffer);
                GL.CheckGLError();
                _indexBufferDirty = false;
            }
        }

        private void PlatformApplyShaders()
        {
            ConcreteVertexShader cvertexShader = ((IPlatformShader)this.VertexShader).Strategy.ToConcrete<ConcreteVertexShader>();
            ConcretePixelShader  cpixelShader  = ((IPlatformShader)this.PixelShader).Strategy.ToConcrete<ConcretePixelShader>();

            if (_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram(cvertexShader, cpixelShader);

                if (_vertexShaderDirty)
                {
                    base.Metrics_AddVertexShaderCount();
                }

                if (_pixelShaderDirty)
                {
                    base.Metrics_AddPixelShaderCount();
                }

                _vertexShaderDirty = false;
                _pixelShaderDirty = false;
            }


            // Apply Constant Buffers
            ((IPlatformConstantBufferCollection)_vertexConstantBuffers).Strategy.ToConcrete<ConcreteConstantBufferCollection>().Apply(this);
            ((IPlatformConstantBufferCollection)_pixelConstantBuffers).Strategy.ToConcrete<ConcreteConstantBufferCollection>().Apply(this);

            // Apply Shader Texture and Samplers
            PlatformApplyTexturesAndSamplers(cvertexShader,
                ((IPlatformTextureCollection)this.VertexTextures).Strategy.ToConcrete<ConcreteTextureCollection>(),
                ((IPlatformSamplerStateCollection)this.VertexSamplerStates).Strategy.ToConcrete<ConcreteSamplerStateCollection>());
            PlatformApplyTexturesAndSamplers(cpixelShader,
                ((IPlatformTextureCollection)this.Textures).Strategy.ToConcrete<ConcreteTextureCollection>(),
                ((IPlatformSamplerStateCollection)this.SamplerStates).Strategy.ToConcrete<ConcreteSamplerStateCollection>());
        }

        private void PlatformApplyTexturesAndSamplers(ConcreteShader cshader, ConcreteTextureCollection ctextureCollection, ConcreteSamplerStateCollection csamplerStateCollection)
        {
            int texturesCount = ctextureCollection.Length;

            // Apply Textures
            for (int slot = 0; ctextureCollection.InternalDirty != 0 && slot < texturesCount; slot++)
            {
                uint mask = ((uint)1) << slot;
                if ((ctextureCollection.InternalDirty & mask) != 0)
                {
                    Texture texture = ctextureCollection[slot];
                    if (texture != null)
                    {
                        ConcreteTexture ctexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>();

                        // Clear the previous binding if the target is different from the new one.
                        WebGLTextureTarget prevTarget = ctextureCollection._targets[slot];
                        if (prevTarget != 0 && prevTarget != ctexture._glTarget)
                        {
                            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + slot);
                            GL.CheckGLError();
                            GL.BindTexture(prevTarget, null);
                            ctextureCollection._targets[slot] = 0;
                            GL.CheckGLError();
                        }

                        GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + slot);
                        GL.CheckGLError();
                        ctextureCollection._targets[slot] = ctexture._glTarget;
                        GL.BindTexture(ctexture._glTarget, ctexture._glTexture);
                        GL.CheckGLError();

                        this.Metrics_AddTextureCount();
                    }
                    else // (texture == null)
                    {
                        // Clear the previous binding if the target is different from the new one.
                        WebGLTextureTarget prevTarget = ctextureCollection._targets[slot];
                        if (prevTarget != 0)
                        {
                            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + slot);
                            GL.CheckGLError();
                            GL.BindTexture(prevTarget, null);
                            ctextureCollection._targets[slot] = 0;
                            GL.CheckGLError();
                        }
                    }

                    // clear texture bit
                    ctextureCollection.InternalDirty &= ~mask;
                }
            }

            // Check Samplers
            GraphicsProfile graphicsProfile = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.GraphicsProfile;
            if (graphicsProfile == GraphicsProfile.Reach)
            {
                for (int i = 0; i < cshader.Samplers.Length; i++)
                {
                    int textureSlot = cshader.Samplers[i].textureSlot;
                    int samplerSlot = cshader.Samplers[i].samplerSlot;

                    Texture2D texture2D = ctextureCollection[textureSlot] as Texture2D;
                    if (texture2D != null)
                    {
                        if (this.SamplerStates[samplerSlot].AddressU != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(texture2D.Width)
                        ||  this.SamplerStates[samplerSlot].AddressV != TextureAddressMode.Clamp && !MathHelper.IsPowerOfTwo(texture2D.Height))
                            throw new NotSupportedException("Reach profile support only Clamp mode for non-power of two Textures.");
                    }
                }
            }

            // Apply Samplers
            for (int slot = 0; slot < texturesCount; slot++)
            {
                Texture texture = ctextureCollection[slot];
                if (texture != null)
                {
                    ConcreteTexture ctexture = ((IPlatformTexture)texture).GetTextureStrategy<ConcreteTexture>();

                    SamplerState sampler = csamplerStateCollection.InternalActualSamplers[slot];
                    if (sampler != null)
                    {
                        if (sampler != ctexture._glLastSamplerState)
                        {
                            // TODO: Avoid doing this redundantly.
                            //       Merge the two loops 'Apply Textures' and 'Apply Samplers'.
                            //       Use information from 'cshader.Samplers' to find active samplers.
                            GL.ActiveTexture(WebGLTextureUnit.TEXTURE0 + slot);
                            GL.CheckGLError();

                            // NOTE: We don't have to bind the texture here because it is already bound in the loop above.
                            // GL.BindTexture(ctexture._glTarget, texture._glTexture);
                            // GL.CheckGLError();

                            ConcreteSamplerState csamplerState = ((IPlatformSamplerState)sampler).GetStrategy<ConcreteSamplerState>();

                            csamplerState.PlatformApplyState(this, ctexture._glTarget, ctexture.LevelCount > 1);
                            ctexture._glLastSamplerState = sampler;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private unsafe void ActivateShaderProgram(ConcreteVertexShader cvertexShader, ConcretePixelShader cpixelShader)
        {
            ConcreteGraphicsDevice deviceStrategy = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>();

            // Lookup the shader program.
            ShaderProgram shaderProgram;
            int shaderProgramHash = (cvertexShader.HashKey ^ cpixelShader.HashKey);
            if (!deviceStrategy.ProgramCache.TryGetValue(shaderProgramHash, out shaderProgram))
            {
                // the key does not exist so we need to link the programs
                shaderProgram = CreateProgram(cvertexShader, cpixelShader);
                deviceStrategy.ProgramCache.Add(shaderProgramHash, shaderProgram);
            }

            if (shaderProgram.Program == null)
                return;

            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GL.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            WebGLUniformLocation posFixupLoc = this.GetUniformLocation(shaderProgram, "posFixup");
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

            _posFixup.X = 1.0f;
            _posFixup.Y = 1.0f;
            if (!((IPlatformGraphicsContext)this.Context).DeviceStrategy.UseHalfPixelOffset)
            {
                _posFixup.Z = 0f;
                _posFixup.W = 0f;
            }
            else
            {
                _posFixup.Z =  (63.0f/64.0f)/Viewport.Width;
                _posFixup.W = -(63.0f/64.0f)/Viewport.Height;
            }

            //If we have a render target bound (rendering offscreen)
            if (this.IsRenderTargetBound)
            {
                //flip vertically
                _posFixup.Y = -_posFixup.Y;
                _posFixup.W = -_posFixup.W;
            }
            
            GL.Uniform4f(posFixupLoc, _posFixup.X, _posFixup.Y, _posFixup.Z, _posFixup.W);
            GL.CheckGLError();
        }

        private ShaderProgram CreateProgram(ConcreteVertexShader cvertexShader, ConcretePixelShader cpixelShader)
        {
            WebGLProgram program = GL.CreateProgram();
            GL.CheckGLError();

            WebGLShader vertexShaderHandle = cvertexShader.ShaderHandle;
            GL.AttachShader(program, vertexShaderHandle);
            GL.CheckGLError();

            WebGLShader pixelShaderHandle = cpixelShader.ShaderHandle;
            GL.AttachShader(program, pixelShaderHandle);
            GL.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GL.CheckGLError();

            GL.UseProgram(program);
            GL.CheckGLError();

            // Get Vertex AttributeLocations
            for (int i = 0; i < cvertexShader.Attributes.Length; i++)
            {
                int attribloc = GL.GetAttribLocation(program, cvertexShader.Attributes[i].name);
                GL.CheckGLError();
                cvertexShader.Attributes[i].location = attribloc;
            }

            // Apply Pixel Sampler TextureUnits
            // Assign the texture unit index to the sampler uniforms.
            for (int i = 0; i < cpixelShader.Samplers.Length; i++)
            {
                WebGLUniformLocation loc = GL.GetUniformLocation(program, cpixelShader.Samplers[i].GLsamplerName);
                GL.CheckGLError();
                if (loc != null)
                {
                    GL.Uniform1i(loc, cpixelShader.Samplers[i].textureSlot);
                    GL.CheckGLError();
                }
            }

            bool linkStatus;
            linkStatus = GL.GetProgramParameter(program, WebGLProgramStatus.LINK);

            if (linkStatus == true)
            {
                return new ShaderProgram(program);
            }
            else
            {
                string log = GL.GetProgramInfoLog(program);
                //GL.DetachShader(program, vertexShaderHandle);
                //GL.DetachShader(program, pixelShaderHandle);
                program.Dispose();

                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        public WebGLUniformLocation GetUniformLocation(ShaderProgram shaderProgram, string name)
        {
            WebGLUniformLocation location;
            if (shaderProgram._uniformLocationCache.TryGetValue(name, out location))
                return location;

            location = GL.GetUniformLocation(shaderProgram.Program, name);
            GL.CheckGLError();

            shaderProgram._uniformLocationCache[name] = location;
            return location;
        }

        private void PlatformApplyVertexBuffers(int baseVertex)
        {
            ConcreteVertexShader vertexShaderStrategy = ((IPlatformShader)this.VertexShader).Strategy.ToConcrete<ConcreteVertexShader>();
            bool bindingsChanged = false;

            for (int slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                VertexBufferBinding vertexBufferBinding = _vertexBuffers.Get(slot);

                VertexDeclaration vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                int vertexStride = vertexDeclaration.VertexStride;
                IntPtr vertexOffset = (IntPtr)(vertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                VertexDeclarationAttributeInfo vertexAttribInfo = vertexShaderStrategy.GetVertexAttribInfo(vertexDeclaration);

                if (_lastVertexAttribs != 1
                ||  _bufferBindingInfos[slot].VertexBuffer != vertexBufferBinding.VertexBuffer
                ||  _bufferBindingInfos[slot].VertexOffset != vertexOffset
                ||  _bufferBindingInfos[slot].InstanceFrequency != vertexBufferBinding.InstanceFrequency
                ||  !ReferenceEquals(_bufferBindingInfos[slot].AttributeInfo, vertexAttribInfo)
                ||  slot >= _activeBufferBindingInfosCount)
                {
                    bindingsChanged = true;

                    GL.BindBuffer(WebGLBufferType.ARRAY, ((IPlatformVertexBuffer)vertexBufferBinding.VertexBuffer).Strategy.ToConcrete<ConcreteVertexBuffer>().GLVertexBuffer);
                    GL.CheckGLError();

                    for (int e = 0; e < vertexAttribInfo.Elements.Count; e++)
                    {
                        VertexDeclarationAttributeInfoElement element = vertexAttribInfo.Elements[e];
                        GL.VertexAttribPointer(element.AttributeLocation,
                            element.NumberOfElements,
                            element.VertexAttribPointerType,
                            element.Normalized,
                            vertexStride,
                            ((IntPtr)(vertexOffset.ToInt64() + element.Offset)).ToInt32());
                        GL.CheckGLError();

                        // only set the divisor if instancing is supported
                        if (((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsInstancing)
                        {
                            throw new NotImplementedException();
                            //GL2.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
                            //GL.CheckGLError();
                        }
                        else // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                        {
                            if (vertexBufferBinding.InstanceFrequency > 0)
                                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");
                        }
                    }

                    _bufferBindingInfos[slot].VertexBuffer = vertexBufferBinding.VertexBuffer;
                    _bufferBindingInfos[slot].AttributeInfo = vertexAttribInfo;
                    _bufferBindingInfos[slot].VertexOffset = vertexOffset;
                    _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                }
            }

            if (bindingsChanged)
            {
                for (int eva = 0; eva < _newEnabledVertexAttributes.Length; eva++)
                    _newEnabledVertexAttributes[eva] = false;

                for (int slot = 0; slot < _vertexBuffers.Count; slot++)
                {
                    for (int e = 0; e < _bufferBindingInfos[slot].AttributeInfo.Elements.Count; e++)
                    {
                        VertexDeclarationAttributeInfoElement element = _bufferBindingInfos[slot].AttributeInfo.Elements[e];
                        _newEnabledVertexAttributes[element.AttributeLocation] = true;
                    }
                }
                _activeBufferBindingInfosCount = _vertexBuffers.Count;
            }

            // SetVertexAttributeArray
            if (bindingsChanged || _lastVertexAttribs != 1)
            {
                for (int x = 0; x < _newEnabledVertexAttributes.Length; x++)
                {
                    if (_newEnabledVertexAttributes[x] == true)
                    {
                        if (_enabledVertexAttributesSet.Add(x))
                        {
                            GL.EnableVertexAttribArray(x);
                            GL.CheckGLError();
                        }
                    }
                    else // (_newEnabledVertexAttributes[x] == false)
                    {
                        if (_enabledVertexAttributesSet.Remove(x))
                        {
                            GL.DisableVertexAttribArray(x);
                            GL.CheckGLError();
                        }
                    }
                }
            }
            _lastVertexAttribs = 1;
        }

        internal void PlatformApplyUserVertexData(VertexDeclaration vertexDeclaration)
        {
            ConcreteVertexShader vertexShaderStrategy = ((IPlatformShader)this.VertexShader).Strategy.ToConcrete<ConcreteVertexShader>();

            int vertexStride = vertexDeclaration.VertexStride;

            VertexDeclarationAttributeInfo vertexAttribInfo = vertexShaderStrategy.GetVertexAttribInfo(vertexDeclaration);
           
            for (int e = 0; e < vertexAttribInfo.Elements.Count; e++)
            {
                VertexDeclarationAttributeInfoElement element = vertexAttribInfo.Elements[e];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    vertexStride,
                    element.Offset);
                GL.CheckGLError();

                if (((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsInstancing)
                {
                    throw new NotImplementedException();
                    //GL2.VertexAttribDivisor(element.AttributeLocation, 0);
                    //GL.CheckGLError();
                }
            }

            // SetVertexAttributeArray
            {
                for (int x = 0; x < vertexAttribInfo.EnabledAttributes.Length; x++)
                {
                    if (vertexAttribInfo.EnabledAttributes[x] == true)
                    {
                        if (_enabledVertexAttributesSet.Add(x))
                        {
                            GL.EnableVertexAttribArray(x);
                            GL.CheckGLError();
                        }
                    }
                    else // (vertexAttribInfo.EnabledAttributes[x] == false)
                    {
                        if (_enabledVertexAttributesSet.Remove(x))
                        {
                            GL.DisableVertexAttribArray(x);
                            GL.CheckGLError();
                        }
                    }
                }
            }
            _lastVertexAttribs = 2;
        }

        private static WebGLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    throw new NotSupportedException();
                case PrimitiveType.LineList:
                    return WebGLPrimitiveType.LINES;
                case PrimitiveType.LineStrip:
                    return WebGLPrimitiveType.LINE_STRIP;
                case PrimitiveType.TriangleList:
                    return WebGLPrimitiveType.TRIANGLES;
                case PrimitiveType.TriangleStrip:
                    return WebGLPrimitiveType.TRIANGLE_STRIP;
                default:
                    throw new ArgumentException();
            }
        }

        public override void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount, int vertexCount)
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            PlatformApplyVertexBuffers(0);

            if (vertexStart < 0)
                vertexStart = 0;

            GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                          vertexStart,
                          vertexCount);
            GL.CheckGLError();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount);
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            WebGLDataType indexElementType = ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType;
            int indexOffsetInBytes = (startIndex * ((IPlatformIndexBuffer)Indices).Strategy.ElementSizeInBytes);
            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            WebGLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            PlatformApplyVertexBuffers(baseVertex);

            GL.DrawElements(target,
                            indexElementCount,
                            indexElementType,
                            indexOffsetInBytes);
            GL.CheckGLError();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount);
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int minVertexIndex, int numVertices, int startIndex, int primitiveCount)
        {
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            WebGLDataType indexElementType = ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType;
            int indexOffsetInBytes = (startIndex * ((IPlatformIndexBuffer)Indices).Strategy.ElementSizeInBytes);
            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            WebGLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            PlatformApplyVertexBuffers(baseVertex);

            if (GL is IWebGL2RenderingContext)
            {
                ((IWebGL2RenderingContext)GL).DrawRangeElements(target,
                                    minVertexIndex,
                                    minVertexIndex + numVertices - 1,
                                    indexElementCount,
                                    indexElementType,
                                    indexOffsetInBytes);
                GL.CheckGLError();
            }
            else
            {
                GL.DrawElements(target,
                                indexElementCount,
                                indexElementType,
                                indexOffsetInBytes);
                GL.CheckGLError();
            }

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount);
        }

        public override void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (!((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsInstancing)
                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");

            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            throw new NotImplementedException();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount * instanceCount);
        }

        public override void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration, int vertexCount)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            WebGLBuffer vbo = GL.CreateBuffer();
            GL.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GL.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GL.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData<T>(WebGLBufferType.ARRAY, 0, vertexData, vertexOffset, vertexCount);
            GL.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            PlatformApplyUserVertexData(vertexDeclaration);

            WebGLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                          0,
                          vertexCount);
            GL.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GL.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GL.CheckGLError();

            vbo.Dispose();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount);
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            WebGLBuffer vbo = GL.CreateBuffer();
            GL.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GL.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GL.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData<T>(WebGLBufferType.ARRAY, 0, vertexData, vertexOffset, numVertices);
            GL.CheckGLError();

            // create and bind index buffer
            WebGLBuffer ibo = GL.CreateBuffer();
            GL.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
            GL.CheckGLError();
            GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                          (indexData.Length * sizeof(short)),
                          (false) ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GL.CheckGLError();
            // mark the default index buffer for rebinding
            _indexBufferDirty = true;

            // set index buffer
            GL.BufferSubData<short>(WebGLBufferType.ELEMENT_ARRAY, 0, indexData, indexData.Length);
            GL.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            PlatformApplyUserVertexData(vertexDeclaration);


            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            int indexOffsetInBytes = (indexOffset * sizeof(short));
            WebGLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            GL.DrawElements(target,
                            indexElementCount,
                            WebGLDataType.USHORT,
                            indexOffsetInBytes);
            GL.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GL.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GL.CheckGLError();

            ibo.Dispose();
            vbo.Dispose();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount);
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyShaders();

            // TODO: reimplement without creating new buffers

            // create and bind vertexBuffer
            WebGLBuffer vbo = GL.CreateBuffer();
            GL.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ARRAY, vbo);
            GL.CheckGLError();
            GL.BufferData(WebGLBufferType.ARRAY,
                          (vertexDeclaration.VertexStride * vertexData.Length),
                          (false) ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GL.CheckGLError();
            // mark the default Vertex buffers for rebinding
            _vertexBuffersDirty = true;

            //set vertex data
            GL.BufferSubData<T>(WebGLBufferType.ARRAY, 0, vertexData, vertexOffset, numVertices);
            GL.CheckGLError();

            // create and bind index buffer
            WebGLBuffer ibo = GL.CreateBuffer();
            GL.CheckGLError();
            GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, ibo);
            GL.CheckGLError();
            GL.BufferData(WebGLBufferType.ELEMENT_ARRAY,
                          (indexData.Length * sizeof(int)),
                          (false) ? WebGLBufferUsageHint.DYNAMIC_DRAW : WebGLBufferUsageHint.STATIC_DRAW);
            GL.CheckGLError();
            // mark the default index buffer for rebinding
            _indexBufferDirty = true;

            // set index buffer
            ((IWebGL2RenderingContext)GL).BufferSubData<int>(WebGLBufferType.ELEMENT_ARRAY, 0, indexData, indexData.Length);
            GL.CheckGLError();

            // Setup the vertex declaration to point at the VB data.
            PlatformApplyUserVertexData(vertexDeclaration);


            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            int indexOffsetInBytes = (indexOffset * sizeof(int));
            WebGLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            GL.DrawElements(target,
                            indexElementCount,
                            WebGLDataType.UINT,
                            indexOffsetInBytes);
            GL.CheckGLError();

            //GL.BindBuffer(WebGLBufferType.ARRAY, null);
            //GL.CheckGLError();
            //GL.BindBuffer(WebGLBufferType.ELEMENT_ARRAY, null);
            //GL.CheckGLError();

            ibo.Dispose();
            vbo.Dispose();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount);
        }

        public override void Flush()
        {
            GL.Flush();
        }


        public override OcclusionQueryStrategy CreateOcclusionQueryStrategy()
        {
            return new ConcreteOcclusionQuery(this);
        }

        public override GraphicsDebugStrategy CreateGraphicsDebugStrategy()
        {
            return new ConcreteGraphicsDebug(this);
        }

        public override ConstantBufferCollectionStrategy CreateConstantBufferCollectionStrategy(int capacity)
        {
            return new ConcreteConstantBufferCollection(capacity);
        }

        public override TextureCollectionStrategy CreateTextureCollectionStrategy(int capacity)
        {
            return new ConcreteTextureCollection(this, capacity);
        }

        public override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(int capacity)
        {
            return new ConcreteSamplerStateCollection(this, capacity);
        }

        public override ITexture2DStrategy CreateTexture2DStrategy(int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
        {
            return new ConcreteTexture2D(this, width, height, mipMap, format, arraySize, shared);
        }

        public override ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTexture3D(this, width, height, depth, mipMap, format);
        }

        public override ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTextureCube(this, size, mipMap, format);
        }

        public override IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTarget2D(this, width, height, mipMap, arraySize, shared, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount,
                                              surfaceType: TextureSurfaceType.RenderTarget);
        }

        public override IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTarget3D(this, width, height, depth, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount);
        }

        public override IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTargetCube(this, size, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount);
        }

        public override ITexture2DStrategy CreateTexture2DStrategy(Stream stream)
        {
            return new ConcreteTexture2D(this, stream);
        }

        public override ShaderStrategy CreateVertexShaderStrategy(byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
        {
            return new ConcreteVertexShader(this, shaderBytecode, samplers, cBuffers, attributes, profile);
        }
        public override ShaderStrategy CreatePixelShaderStrategy(byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
        {
            return new ConcretePixelShader(this, shaderBytecode, samplers, cBuffers, attributes, profile);
        }
        public override ConstantBufferStrategy CreateConstantBufferStrategy(string name, int[] parameterIndexes, int[] parameterOffsets, int sizeInBytes, ShaderProfileType profile)
        {
            return new ConcreteConstantBuffer(this, name, parameterIndexes, parameterOffsets, sizeInBytes, profile);
        }

        public override IndexBufferStrategy CreateIndexBufferStrategy(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
        {
            return new ConcreteIndexBuffer(this, indexElementSize, indexCount, usage);
        }
        public override VertexBufferStrategy CreateVertexBufferStrategy(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
        {
            return new ConcreteVertexBuffer(this, vertexDeclaration, vertexCount, usage);
        }
        public override IndexBufferStrategy CreateDynamicIndexBufferStrategy(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
        {
            return new ConcreteDynamicIndexBuffer(this, indexElementSize, indexCount, usage);
        }
        public override VertexBufferStrategy CreateDynamicVertexBufferStrategy(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
        {
            return new ConcreteDynamicVertexBuffer(this, vertexDeclaration, vertexCount, usage);
        }

        public override IBlendStateStrategy CreateBlendStateStrategy(IBlendStateStrategy source)
        {
            return new ConcreteBlendState(this, source);
        }
        public override IDepthStencilStateStrategy CreateDepthStencilStateStrategy(IDepthStencilStateStrategy source)
        {
            return new ConcreteDepthStencilState(this, source);
        }
        public override IRasterizerStateStrategy CreateRasterizerStateStrategy(IRasterizerStateStrategy source)
        {
            return new ConcreteRasterizerState(this, source);
        }
        public override ISamplerStateStrategy CreateSamplerStateStrategy(ISamplerStateStrategy source)
        {
            return new ConcreteSamplerState(this, source);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

        static readonly WebGLFramebufferAttachmentPoint[] InvalidateFramebufferAttachements =
        {
            WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0,
            WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT,
            WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT,
        };

        protected override void PlatformResolveRenderTargets()
        {
            if (!this.IsRenderTargetBound)
                return;

            RenderTargetBinding renderTargetBinding = base.CurrentRenderTargetBindings[0];
            IRenderTarget renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0)
            {
                throw new NotImplementedException();
            }

            for (int i = 0; i < base.RenderTargetCount; i++)
            {
                renderTargetBinding = base.CurrentRenderTargetBindings[i];

                renderTarget = renderTargetBinding.RenderTarget as RenderTarget2D;
                if (renderTarget != null)
                {
                    ((IRenderTarget)renderTarget).RenderTargetStrategy.ResolveSubresource(this);
                }

                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>();
                    GL.BindTexture(renderTargetGL.GLTarget, renderTargetGL.GLTexture);
                    GL.CheckGLError();
                    GL.GenerateMipmap(renderTargetGL.GLTarget);
                    GL.CheckGLError();
                }
            }
        }

        protected override void PlatformApplyDefaultRenderTarget()
        {
            GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER,((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._glDefaultFramebuffer);
            GL.CheckGLError();

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            ((IPlatformTextureCollection)this.Textures).Strategy.Dirty();
        }

        protected override IRenderTarget PlatformApplyRenderTargets()
        {
            WebGLFramebuffer glFramebuffer = null;
            if (!_glFramebuffers.TryGetValue(base.CurrentRenderTargetBindings, out glFramebuffer))
            {
                glFramebuffer = GL.CreateFramebuffer();
                GL.CheckGLError();
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
                GL.CheckGLError();
                RenderTargetBinding renderTargetBinding = base.CurrentRenderTargetBindings[0];
                IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>();

                if (renderTargetGL.GLDepthBuffer != renderTargetGL.GLStencilBuffer)
                {
                    GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.DEPTH_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLDepthBuffer);
                    GL.CheckGLError();
                    //GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.STENCIL_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLStencilBuffer);
                    //GL.CheckGLError();
                }
                else
                {
                    GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, WebGLFramebufferAttachmentPoint.DEPTH_STENCIL_ATTACHMENT, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLDepthBuffer);
                    GL.CheckGLError();
                }

                for (int i = 0; i < base.RenderTargetCount; i++)
                {
                    renderTargetBinding = base.CurrentRenderTargetBindings[i];
                    renderTargetGL = ((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>() as IRenderTargetStrategyGL;
                    WebGLFramebufferAttachmentPoint attachement = (WebGLFramebufferAttachmentPoint.COLOR_ATTACHMENT0 + i);

                    if (renderTargetGL.GLColorBuffer != null)
                    {
                        GL.FramebufferRenderbuffer(WebGLFramebufferType.FRAMEBUFFER, attachement, WebGLRenderbufferType.RENDERBUFFER, renderTargetGL.GLColorBuffer);
                        GL.CheckGLError();
                    }
                    else
                    {
                        WebGLTextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(WebGLFramebufferType.FRAMEBUFFER, attachement, target, renderTargetGL.GLTexture);
                        GL.CheckGLError();
                    }
                }

                CheckFramebufferStatus();

                _glFramebuffers.Add((RenderTargetBinding[])base.CurrentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                GL.BindFramebuffer(WebGLFramebufferType.FRAMEBUFFER, glFramebuffer);
                GL.CheckGLError();
            }

#if DESKTOPGL
            //GL.DrawBuffers(base.RenderTargetCount, _drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            ((IPlatformTextureCollection)this.Textures).Strategy.Dirty();

            return base.CurrentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        internal void PlatformUnbindRenderTarget(IRenderTargetStrategyGL renderTargetStrategy)
        {
            var bindingsToDelete = new List<RenderTargetBinding[]>();
            foreach (RenderTargetBinding[] bindings in _glFramebuffers.Keys)
            {
                foreach (RenderTargetBinding renderTargetBinding in bindings)
                {
                    if (renderTargetBinding.RenderTarget != null && ((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>() == renderTargetStrategy)
                    {
                        bindingsToDelete.Add(bindings);
                        break;
                    }
                }
            }

            foreach (RenderTargetBinding[] bindings in bindingsToDelete)
            {
                WebGLFramebuffer fbo = null;
                if (_glFramebuffers.TryGetValue(bindings, out fbo))
                {
                    fbo.Dispose();
                    GL.CheckGLError();
                    _glFramebuffers.Remove(bindings);
                }
                if (_glResolveFramebuffers.TryGetValue(bindings, out fbo))
                {
                    fbo.Dispose();
                    GL.CheckGLError();

                    _glResolveFramebuffers.Remove(bindings);
                }
            }
        }


        [Conditional("DEBUG")]
        public void CheckFramebufferStatus()
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
