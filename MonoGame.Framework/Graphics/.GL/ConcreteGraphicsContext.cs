// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;

#if ANDROID //FromStream
//using Android.Graphics;
#endif

#if IOS || TVOS //FromStream
using UIKit;
using CoreGraphics;
using Foundation;
#endif

namespace Microsoft.Xna.Platform.Graphics
{
    internal abstract class ConcreteGraphicsContextGL : GraphicsContextStrategy
    {
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
        internal readonly HashSet<int> _enabledVertexAttributes = new HashSet<int>();
        private bool _attribsDirty;

        // Keeps track of last applied state to avoid redundant OpenGL calls
        private Vector4 _lastClearColor = Vector4.Zero;
        private float _lastClearDepth = 1.0f;
        private int _lastClearStencil = 0;

        private DepthStencilState _clearDepthStencilState = new DepthStencilState { StencilEnable = true };

        // FBO cache, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], int> _glFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());
        // FBO cache used to resolve MSAA rendertargets, we create 1 FBO per RenderTargetBinding combination
        internal Dictionary<RenderTargetBinding[], int> _glResolveFramebuffers = new Dictionary<RenderTargetBinding[], int>(new RenderTargetBindingArrayComparer());

        internal ShaderProgram ShaderProgram { get { return _shaderProgram; } }

        public override Viewport Viewport
        {
            get { return base.Viewport; }
            set
            {
                base.Viewport = value;
                PlatformApplyViewport();
            }
        }

        internal OGL GL { get { return OGL.Current; } }

        internal ConcreteGraphicsContextGL(GraphicsContext context)
            : base(context)
        {

        }

        public abstract void BindDisposeContext();
        public abstract void UnbindDisposeContext();

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

            ClearBufferMask bufferMask = 0;
            if ((options & ClearOptions.Target) == ClearOptions.Target)
            {
                if (color != _lastClearColor)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    GL.CheckGLError();
                    _lastClearColor = color;
                }
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
            }
            if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
                if (stencil != _lastClearStencil)
                {
                    GL.ClearStencil(stencil);
                    GL.CheckGLError();
                    _lastClearStencil = stencil;
                }
                bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
            }

            if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
            {
                if (depth != _lastClearDepth)
                {
                    GL.ClearDepth(depth);
                    GL.CheckGLError();
                    _lastClearDepth = depth;
                }
                bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
            }

#if IOS || TVOS
            if (GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt) == FramebufferErrorCode.FramebufferComplete)
            {
#endif
            GL.Clear(bufferMask);
            GL.CheckGLError();
#if IOS || TVOS
            }
#endif

            base.Metrics_AddClearCount();

            // Restore the previous render state.
            ScissorRectangle = prevScissorRect;
            DepthStencilState = prevDepthStencilState;
            BlendState = prevBlendState;
        }

        private void PlatformApplyState()
        {
            Threading.EnsureMainThread();

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
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GL.CheckGLError();
                _blendFactorDirty = false;
            }
        }

        private void PlatformApplyScissorRectangle()
        {
            Rectangle scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GL.CheckGLError();
            _scissorRectangleDirty = false;
        }

        internal void PlatformApplyViewport()
        {
            if (this.IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, ((IPlatformGraphicsContext)this.Context).DeviceStrategy.PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GL.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            GL.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "_posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;
        }

        private void PlatformApplyIndexBuffer()
        {
            if (_indexBufferDirty)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().GLIndexBuffer);
                GL.CheckGLError();
                _indexBufferDirty = false;
            }
        }

        private void PlatformApplyVertexBuffers()
        {
        }

        private void PlatformApplyShaders()
        {
            if (_vertexShaderDirty || _pixelShaderDirty)
            {
                ActivateShaderProgram();

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


            // Apply Shader Buffers
            ((IPlatformTextureCollection)this.VertexTextures).Strategy.ToConcrete<ConcreteTextureCollection>().PlatformApply();
            ((IPlatformSamplerStateCollection)this.VertexSamplerStates).Strategy.ToConcrete<ConcreteSamplerStateCollection>().PlatformApply();
            ((IPlatformTextureCollection)this.Textures).Strategy.ToConcrete<ConcreteTextureCollection>().PlatformApply();
            ((IPlatformSamplerStateCollection)this.SamplerStates).Strategy.ToConcrete<ConcreteSamplerStateCollection>().PlatformApply();
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            // Lookup the shader program.
            ShaderProgram shaderProgram;
            int shaderProgramHash = (this.VertexShader.HashKey ^ this.PixelShader.HashKey);
            if (!((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().ProgramCache.TryGetValue(shaderProgramHash, out shaderProgram))
            {
                // the key does not exist so we need to link the programs
                shaderProgram = CreateProgram(this.VertexShader, this.PixelShader);
               ((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().ProgramCache.Add(shaderProgramHash, shaderProgram);
            }

            if (shaderProgram.Program == -1)
                return;

            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GL.CheckGLError();
                _shaderProgram = shaderProgram;
            }

            int posFixupLoc = this.GetUniformLocation(shaderProgram, "posFixup");
            if (posFixupLoc == -1)
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
            
            GL.Uniform4(posFixupLoc, _posFixup);
            GL.CheckGLError();
        }

        private ShaderProgram CreateProgram(VertexShader vertexShader, PixelShader pixelShader)
        {
            int program = GL.CreateProgram();
            GL.CheckGLError();

            int vertexShaderHandle = ((IPlatformShader)vertexShader).Strategy.ToConcrete<ConcreteVertexShader>().ShaderHandle;
            GL.AttachShader(program, vertexShaderHandle);
            GL.CheckGLError();

            int pixelShaderHandle = ((IPlatformShader)pixelShader).Strategy.ToConcrete<ConcretePixelShader>().ShaderHandle;
            GL.AttachShader(program, pixelShaderHandle);
            GL.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GL.CheckGLError();

            GL.UseProgram(program);
            GL.CheckGLError();

            ((IPlatformShader)vertexShader).Strategy.ToConcrete<ConcreteVertexShader>().GetVertexAttributeLocations(this, program);

            ((IPlatformShader)pixelShader).Strategy.ToConcrete<ConcretePixelShader>().ApplySamplerTextureUnits(this, program);

            int linkStatus;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linkStatus);
            GL.LogGLError("VertexShaderCache.Link(), GL.GetProgram");

            if (linkStatus == (int)Bool.True)
            {
                return new ShaderProgram(program);
            }
            else
            {
                string log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
                GL.DetachShader(program, vertexShaderHandle);
                GL.DetachShader(program, pixelShaderHandle);

                if (GL.IsProgram(program))
                {
                    GL.DeleteProgram(program);
                    GL.CheckGLError();
                }
                throw new InvalidOperationException("Unable to link effect program");
            }
        }

        internal int GetUniformLocation(ShaderProgram shaderProgram, string name)
        {
            int location;
            if (shaderProgram._uniformLocationCache.TryGetValue(name, out location))
                return location;

            location = GL.GetUniformLocation(shaderProgram.Program, name);
            GL.CheckGLError();

            shaderProgram._uniformLocationCache[name] = location;
            return location;
        }

        private void SetVertexAttributeArray(bool[] attrs)
        {
            for (int x = 0; x < attrs.Length; x++)
            {
                if (attrs[x])
                {
                    if (_enabledVertexAttributes.Add(x))
                    {
                        GL.EnableVertexAttribArray(x);
                        GL.CheckGLError();
                    }
                }
                else
                {
                    if (_enabledVertexAttributes.Remove(x))
                    {
                        GL.DisableVertexAttribArray(x);
                        GL.CheckGLError();
                    }
                }
            }
        }

        private void PlatformApplyVertexBuffersAttribs(int baseVertex)
        {
            ConcreteVertexShader vertexShaderStrategy = ((IPlatformShader)this.VertexShader).Strategy.ToConcrete<ConcreteVertexShader>();

            bool bindingsChanged = false;

            for (int slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                VertexBufferBinding vertexBufferBinding = _vertexBuffers.Get(slot);
                VertexDeclaration vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                int vertexStride = vertexDeclaration.VertexStride;
                IntPtr vertexOffset = (IntPtr)(vertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                int maxVertexBufferSlots = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.MaxVertexBufferSlots;
                VertexDeclarationAttributeInfo vertexAttribInfo = vertexShaderStrategy.GetVertexAttribInfo(vertexDeclaration, maxVertexBufferSlots);

                if (_attribsDirty
                ||  _bufferBindingInfos[slot].GLVertexBuffer != ((IPlatformVertexBuffer)vertexBufferBinding.VertexBuffer).Strategy
                ||  !ReferenceEquals(_bufferBindingInfos[slot].AttributeInfo, vertexAttribInfo)
                ||  _bufferBindingInfos[slot].VertexOffset != vertexOffset
                ||  _bufferBindingInfos[slot].InstanceFrequency != vertexBufferBinding.InstanceFrequency
                ||  slot >= _activeBufferBindingInfosCount)
                {
                    bindingsChanged = true;

                    GL.BindBuffer(BufferTarget.ArrayBuffer, ((IPlatformVertexBuffer)vertexBufferBinding.VertexBuffer).Strategy.ToConcrete<ConcreteVertexBuffer>().GLVertexBuffer);
                    GL.CheckGLError();

                    for (int e = 0; e < vertexAttribInfo.Elements.Count; e++)
                    {
                        VertexDeclarationAttributeInfoElement element = vertexAttribInfo.Elements[e];
                        GL.VertexAttribPointer(element.AttributeLocation,
                            element.NumberOfElements,
                            element.VertexAttribPointerType,
                            element.Normalized,
                            vertexStride,
                            (IntPtr)(vertexOffset.ToInt64() + element.Offset));
                        GL.CheckGLError();

                        // only set the divisor if instancing is supported
                        if (((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsInstancing)
                        {
                            GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
                            GL.CheckGLError();
                        }
                        else // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                        {
                            if (vertexBufferBinding.InstanceFrequency > 0)
                                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");
                        }
                    }

                    _bufferBindingInfos[slot].GLVertexBuffer = ((IPlatformVertexBuffer)vertexBufferBinding.VertexBuffer).Strategy;
                    _bufferBindingInfos[slot].AttributeInfo = vertexAttribInfo;
                    _bufferBindingInfos[slot].VertexOffset = vertexOffset;
                    _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                }
            }

            _attribsDirty = false;

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

            SetVertexAttributeArray(_newEnabledVertexAttributes);
        }

        internal void PlatformApplyUserVertexDataAttribs(VertexDeclaration vertexDeclaration, IntPtr baseVertex)
        {
            ConcreteVertexShader vertexShaderStrategy = ((IPlatformShader)this.VertexShader).Strategy.ToConcrete<ConcreteVertexShader>();

            int vertexStride = vertexDeclaration.VertexStride;
            IntPtr vertexOffset = baseVertex;

            int maxVertexBufferSlots = ((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.MaxVertexBufferSlots;
            VertexDeclarationAttributeInfo vertexAttribInfo = vertexShaderStrategy.GetVertexAttribInfo(vertexDeclaration, maxVertexBufferSlots);

            for (int e = 0; e < vertexAttribInfo.Elements.Count; e++)
            {
                VertexDeclarationAttributeInfoElement element = vertexAttribInfo.Elements[e];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    vertexStride,
                    (IntPtr)(vertexOffset.ToInt64() + element.Offset));
                GL.CheckGLError();

#if DESKTOPGL
                if (((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsInstancing)
                {
                    GL.VertexAttribDivisor(element.AttributeLocation, 0);
                    GL.CheckGLError();
                }
#endif
            }

            SetVertexAttributeArray(vertexAttribInfo.EnabledAttributes);
            _attribsDirty = true;
        }

        private static GLPrimitiveType PrimitiveTypeGL(PrimitiveType primitiveType)
        {
            switch (primitiveType)
            {
                case PrimitiveType.PointList:
                    return GLPrimitiveType.Points;
                case PrimitiveType.LineList:
                    return GLPrimitiveType.Lines;
                case PrimitiveType.LineStrip:
                    return GLPrimitiveType.LineStrip;
                case PrimitiveType.TriangleList:
                    return GLPrimitiveType.Triangles;
                case PrimitiveType.TriangleStrip:
                    return GLPrimitiveType.TriangleStrip;
                default:
                    throw new ArgumentException();
            }
        }

        public override void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int primitiveCount, int vertexCount)
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            PlatformApplyVertexBuffersAttribs(0);

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
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            DrawElementsType indexElementType = ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType;
            IntPtr indexOffsetInBytes = (IntPtr)(startIndex * ((IPlatformIndexBuffer)Indices).Strategy.ElementSizeInBytes);
            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            GLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            if (GL.DrawElementsBaseVertex != null)
            {
                PlatformApplyVertexBuffersAttribs(0);

                GL.DrawElementsBaseVertex(target,
                                  indexElementCount,
                                  indexElementType,
                                  indexOffsetInBytes,
                                  baseVertex);
                GL.CheckGLError();
            }
            else
            {
                PlatformApplyVertexBuffersAttribs(baseVertex);

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
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            DrawElementsType indexElementType = ((IPlatformIndexBuffer)Indices).Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType;
            IntPtr indexOffsetInBytes = (IntPtr)(startIndex * ((IPlatformIndexBuffer)Indices).Strategy.ElementSizeInBytes);
            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            GLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            if (baseInstance > 0)
            {
                if (!((IPlatformGraphicsContext)this.Context).DeviceStrategy.Capabilities.SupportsBaseIndexInstancing)
                    throw new PlatformNotSupportedException("Instanced geometry drawing with base instance requires at least OpenGL 4.2. Try upgrading your graphics card drivers.");

                PlatformApplyVertexBuffersAttribs(baseVertex);

                GL.DrawElementsInstancedBaseInstance(target,
                                                     indexElementCount,
                                                     indexElementType,
                                                     indexOffsetInBytes,
                                                     instanceCount,
                                                     baseInstance);
            }
            else
            {
                PlatformApplyVertexBuffersAttribs(baseVertex);

                GL.DrawElementsInstanced(target,
                                         indexElementCount,
                                         indexElementType,
                                         indexOffsetInBytes,
                                         instanceCount);
            }

            GL.CheckGLError();

            base.Metrics_AddDrawCount();
            base.Metrics_AddPrimitiveCount(primitiveCount * instanceCount);
        }

        public override void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int primitiveCount, VertexDeclaration vertexDeclaration, int vertexCount)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.CheckGLError();
            _vertexBuffersDirty = true;

            // Pin the buffers.
            GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            try
            {
                // Setup the vertex declaration to point at the VB data.
                PlatformApplyUserVertexDataAttribs(vertexDeclaration, vbHandle.AddrOfPinnedObject());

                //Draw
                GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                              vertexOffset,
                              vertexCount);
                GL.CheckGLError();

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
            finally
            {
                // Release the handles.
                vbHandle.Free();
            }
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, short[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try
            {
                IntPtr vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

                // Setup the vertex declaration to point at the VB data.
                PlatformApplyUserVertexDataAttribs(vertexDeclaration, vertexAddr);

                //Draw
                GL.DrawElements(
                    ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                    GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount),
                    DrawElementsType.UnsignedShort,
                    (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
                GL.CheckGLError();

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
            finally
            {
                // Release the handles.
                ibHandle.Free();
                vbHandle.Free();
            }
        }

        public override void DrawUserIndexedPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, int numVertices, int[] indexData, int indexOffset, int primitiveCount, VertexDeclaration vertexDeclaration)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try
            {
                IntPtr vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

                // Setup the vertex declaration to point at the VB data.
                PlatformApplyUserVertexDataAttribs(vertexDeclaration, vertexAddr);

                //Draw
                GL.DrawElements(
                    ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                    GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount),
                    DrawElementsType.UnsignedInt,
                    (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(int))));
                GL.CheckGLError();

                base.Metrics_AddDrawCount();
                base.Metrics_AddPrimitiveCount(primitiveCount);
            }
            finally
            {
                // Release the handles.
                ibHandle.Free();
                vbHandle.Free();
            }
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

        static readonly FramebufferAttachment[] InvalidateFramebufferAttachements =
        {
            FramebufferAttachment.ColorAttachment0,
            FramebufferAttachment.DepthAttachment,
            FramebufferAttachment.StencilAttachment,
        };

        protected override void PlatformResolveRenderTargets()
        {
            if (!this.IsRenderTargetBound)
                return;

            RenderTargetBinding renderTargetBinding = _currentRenderTargetBindings[0];
            IRenderTarget renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;
            if (renderTarget.MultiSampleCount > 0)
            {
                int glResolveFramebuffer = 0;
                if (!_glResolveFramebuffers.TryGetValue(_currentRenderTargetBindings, out glResolveFramebuffer))
                {
                    glResolveFramebuffer = GL.GenFramebuffer();
                    GL.CheckGLError();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, glResolveFramebuffer);
                    GL.CheckGLError();

                    for (int i = 0; i < base.RenderTargetCount; i++)
                    {
                        IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)_currentRenderTargetBindings[i].RenderTarget;

                        FramebufferAttachment attachement = (FramebufferAttachment.ColorAttachment0 + i);
                        TextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, renderTargetGL.GLTexture, 0);
                        GL.CheckGLError();
                    }
                    _glResolveFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, glResolveFramebuffer);
                    GL.CheckGLError();
                }

                // The only fragment operations which affect the resolve are the pixel ownership test, the scissor test, and dithering.
                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    GL.CheckGLError();
                }

                int glFramebuffer = _glFramebuffers[_currentRenderTargetBindings];
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, glFramebuffer);
                GL.CheckGLError();

                for (int i = 0; i < base.RenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + i);
                    GL.CheckGLError();
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + i);
                    GL.CheckGLError();
                    Debug.Assert(((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsBlitFramebuffer);
                    GL.BlitFramebuffer(0, 0, renderTarget.Width, renderTarget.Height,
                                       0, 0, renderTarget.Width, renderTarget.Height,
                                       ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                    GL.CheckGLError();
                }

                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents &&((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsInvalidateFramebuffer)
                {
                    Debug.Assert(((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsInvalidateFramebuffer);
                    GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, InvalidateFramebufferAttachements);
                    GL.CheckGLError();
                }

                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GL.CheckGLError();
                }
            }

            for (int i = 0; i < base.RenderTargetCount; i++)
            {
                renderTargetBinding = _currentRenderTargetBindings[i];
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
            GL.BindFramebuffer(FramebufferTarget.Framebuffer,((IPlatformGraphicsContext)this.Context).DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._glDefaultFramebuffer);
            GL.CheckGLError();

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            ((IPlatformTextureCollection)_pixelTextures).Strategy.Dirty();
        }

        protected override IRenderTarget PlatformApplyRenderTargets()
        {
            int glFramebuffer = 0;
            if (!_glFramebuffers.TryGetValue(_currentRenderTargetBindings, out glFramebuffer))
            {
                glFramebuffer = GL.GenFramebuffer();
                GL.CheckGLError();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
                GL.CheckGLError();
                RenderTargetBinding renderTargetBinding = _currentRenderTargetBindings[0];
                IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>();

                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderTargetGL.GLDepthBuffer);
                GL.CheckGLError();
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, renderTargetGL.GLStencilBuffer);
                GL.CheckGLError();

                for (int i = 0; i < base.RenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    renderTargetGL = ((IPlatformTexture)renderTargetBinding.RenderTarget).GetTextureStrategy<ITextureStrategy>() as IRenderTargetStrategyGL;
                    FramebufferAttachment attachement = (FramebufferAttachment.ColorAttachment0 + i);

                    if (renderTargetGL.GLColorBuffer != 0)
                    {
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachement, RenderbufferTarget.Renderbuffer, renderTargetGL.GLColorBuffer);
                        GL.CheckGLError();
                    }
                    else
                    {
                        TextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, renderTargetGL.GLTexture, 0);
                        GL.CheckGLError();
                    }
                }

                CheckFramebufferStatus();

                _glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
                GL.CheckGLError();
            }

#if DESKTOPGL
            GL.DrawBuffers(base.RenderTargetCount, this.ToConcrete<ConcreteGraphicsContext>()._drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            ((IPlatformTextureCollection)_pixelTextures).Strategy.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
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
                int fbo = 0;
                if (_glFramebuffers.TryGetValue(bindings, out fbo))
                {
                    GL.DeleteFramebuffer(fbo);
                    GL.CheckGLError();
                    _glFramebuffers.Remove(bindings);
                }
                if (_glResolveFramebuffers.TryGetValue(bindings, out fbo))
                {
                    GL.DeleteFramebuffer(fbo);
                    GL.CheckGLError();
                    _glResolveFramebuffers.Remove(bindings);
                }
            }
        }


        [Conditional("DEBUG")]
        public void CheckFramebufferStatus()
        {
            FramebufferErrorCode status = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            switch (status)
            {
                case FramebufferErrorCode.FramebufferComplete:
                    return;
                case FramebufferErrorCode.FramebufferIncompleteAttachment:
                    throw new InvalidOperationException("Not all framebuffer attachment points are framebuffer attachment complete.");
                case FramebufferErrorCode.FramebufferIncompleteMissingAttachment:
                    throw new InvalidOperationException("No images are attached to the framebuffer.");
                case FramebufferErrorCode.FramebufferUnsupported:
                    throw new InvalidOperationException("The combination of internal formats of the attached images violates an implementation-dependent set of restrictions.");
                case FramebufferErrorCode.FramebufferIncompleteMultisample:
                    throw new InvalidOperationException("Not all attached images have the same number of samples.");

                default:
                    throw new InvalidOperationException("Framebuffer Incomplete.");
            }
        }
    }


    internal class OpenGLException : Exception
    {
        public OpenGLException(string message)
            : base(message)
        {
        }
    }
}
