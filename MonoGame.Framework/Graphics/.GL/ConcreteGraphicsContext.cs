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

        internal ConcreteGraphicsContextGL(GraphicsContext context)
            : base(context)
        {

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

            ClearBufferMask bufferMask = 0;
            if ((options & ClearOptions.Target) == ClearOptions.Target)
            {
                if (color != _lastClearColor)
                {
                    GL.ClearColor(color.X, color.Y, color.Z, color.W);
                    GraphicsExtensions.CheckGLError();
                    _lastClearColor = color;
                }
                bufferMask = bufferMask | ClearBufferMask.ColorBufferBit;
            }
            if ((options & ClearOptions.Stencil) == ClearOptions.Stencil)
            {
                if (stencil != _lastClearStencil)
                {
                    GL.ClearStencil(stencil);
                    GraphicsExtensions.CheckGLError();
                    _lastClearStencil = stencil;
                }
                bufferMask = bufferMask | ClearBufferMask.StencilBufferBit;
            }

            if ((options & ClearOptions.DepthBuffer) == ClearOptions.DepthBuffer)
            {
                if (depth != _lastClearDepth)
                {
                    GL.ClearDepth(depth);
                    GraphicsExtensions.CheckGLError();
                    _lastClearDepth = depth;
                }
                bufferMask = bufferMask | ClearBufferMask.DepthBufferBit;
            }

#if IOS || TVOS
            if (GL.CheckFramebufferStatus(FramebufferTarget.FramebufferExt) == FramebufferErrorCode.FramebufferComplete)
            {
#endif
            GL.Clear(bufferMask);
            GraphicsExtensions.CheckGLError();
#if IOS || TVOS
            }
#endif

            // Restore the previous render state.
            ScissorRectangle = prevScissorRect;
            DepthStencilState = prevDepthStencilState;
            BlendState = prevBlendState;
        }

        private void PlatformApplyState()
        {
            Threading.EnsureUIThread();

            {
                PlatformApplyBlend();
            }

            if (_depthStencilStateDirty)
            {
                _actualDepthStencilState.PlatformApplyState(this);
                _depthStencilStateDirty = false;
            }

            if (_rasterizerStateDirty)
            {
                _actualRasterizerState.PlatformApplyState(this);
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
                _actualBlendState.PlatformApplyState(this);
                _blendStateDirty = false;
            }

            if (_blendFactorDirty)
            {
                GL.BlendColor(
                    this.BlendFactor.R/255.0f,
                    this.BlendFactor.G/255.0f,
                    this.BlendFactor.B/255.0f,
                    this.BlendFactor.A/255.0f);
                GraphicsExtensions.CheckGLError();
                _blendFactorDirty = false;
            }
        }

        private void PlatformApplyScissorRectangle()
        {
            Rectangle scissorRect = _scissorRectangle;
            if (!IsRenderTargetBound)
                scissorRect.Y = this.Context.DeviceStrategy.PresentationParameters.BackBufferHeight - (scissorRect.Y + scissorRect.Height);
            GL.Scissor(scissorRect.X, scissorRect.Y, scissorRect.Width, scissorRect.Height);
            GraphicsExtensions.CheckGLError();
            _scissorRectangleDirty = false;
        }

        internal void PlatformApplyViewport()
        {
            if (this.IsRenderTargetBound)
                GL.Viewport(_viewport.X, _viewport.Y, _viewport.Width, _viewport.Height);
            else
                GL.Viewport(_viewport.X, this.Context.DeviceStrategy.PresentationParameters.BackBufferHeight - _viewport.Y - _viewport.Height, _viewport.Width, _viewport.Height);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.Viewport");

            GL.DepthRange(_viewport.MinDepth, _viewport.MaxDepth);
            GraphicsExtensions.LogGLError("GraphicsDevice.Viewport_set() GL.DepthRange");

            // In OpenGL we have to re-apply the special "_posFixup"
            // vertex shader uniform if the viewport changes.
            _vertexShaderDirty = true;
        }

        private void PlatformApplyIndexBuffer()
        {
            if (_indexBufferDirty)
            {
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, Indices.Strategy.ToConcrete<ConcreteIndexBuffer>().GLIndexBuffer);
                GraphicsExtensions.CheckGLError();
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
                    unchecked { this.Context._graphicsMetrics._vertexShaderCount++; }
                }

                if (_pixelShaderDirty)
                {
                    unchecked { this.Context._graphicsMetrics._pixelShaderCount++; }
                }

                _vertexShaderDirty = false;
                _pixelShaderDirty = false;
            }


            // Apply Constant Buffers
            _vertexConstantBuffers.Apply(this);
            _pixelConstantBuffers.Apply(this);


            // Apply Shader Buffers
            this.VertexTextures.Strategy.ToConcrete<ConcreteTextureCollection>().PlatformApply();
            this.VertexSamplerStates.Strategy.ToConcrete<ConcreteSamplerStateCollection>().PlatformApply();
            this.Textures.Strategy.ToConcrete<ConcreteTextureCollection>().PlatformApply();
            this.SamplerStates.Strategy.ToConcrete<ConcreteSamplerStateCollection>().PlatformApply();
        }

        private int GetCurrentShaderProgramHash2()
        {
            return VertexShader.HashKey ^ PixelShader.HashKey;
        }

        /// <summary>
        /// Activates the Current Vertex/Pixel shader pair into a program.         
        /// </summary>
        private unsafe void ActivateShaderProgram()
        {
            // Lookup the shader program.
            ShaderProgram shaderProgram;
            int shaderProgramHash = GetCurrentShaderProgramHash2();
            if (!this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().ProgramCache.TryGetValue(shaderProgramHash, out shaderProgram))
            {
                // the key does not exist so we need to link the programs
                shaderProgram = CreateProgram(this.VertexShader, this.PixelShader);
                this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>().ProgramCache.Add(shaderProgramHash, shaderProgram);
            }

            if (shaderProgram.Program == -1)
                return;

            // Set the new program if it has changed.
            if (_shaderProgram != shaderProgram)
            {
                GL.UseProgram(shaderProgram.Program);
                GraphicsExtensions.CheckGLError();
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
            if (!this.Context.DeviceStrategy.UseHalfPixelOffset)
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
            GraphicsExtensions.CheckGLError();
        }

        private ShaderProgram CreateProgram(VertexShader vertexShader, PixelShader pixelShader)
        {
            int program = GL.CreateProgram();
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, ((ConcreteVertexShader)vertexShader.Strategy).GetVertexShaderHandle());
            GraphicsExtensions.CheckGLError();

            GL.AttachShader(program, ((ConcretePixelShader)pixelShader.Strategy).GetPixelShaderHandle());
            GraphicsExtensions.CheckGLError();

            //vertexShader.BindVertexAttributes(program);

            GL.LinkProgram(program);
            GraphicsExtensions.CheckGLError();

            GL.UseProgram(program);
            GraphicsExtensions.CheckGLError();

            ((ConcreteVertexShader)vertexShader.Strategy).GetVertexAttributeLocations(program);

            ((ConcretePixelShader)pixelShader.Strategy).ApplySamplerTextureUnits(program);

            int linkStatus;
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out linkStatus);
            GraphicsExtensions.LogGLError("VertexShaderCache.Link(), GL.GetProgram");

            if (linkStatus == (int)Bool.True)
            {
                return new ShaderProgram(program);
            }
            else
            {
                string log = GL.GetProgramInfoLog(program);
                Console.WriteLine(log);
                GL.DetachShader(program, ((ConcreteVertexShader)vertexShader.Strategy).GetVertexShaderHandle());
                GL.DetachShader(program, ((ConcretePixelShader)pixelShader.Strategy).GetPixelShaderHandle());

                if (GL.IsProgram(program))
                {
                    GL.DeleteProgram(program);
                    GraphicsExtensions.CheckGLError();
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
            GraphicsExtensions.CheckGLError();

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
                        GraphicsExtensions.CheckGLError();
                    }
                }
                else
                {
                    if (_enabledVertexAttributes.Remove(x))
                    {
                        GL.DisableVertexAttribArray(x);
                        GraphicsExtensions.CheckGLError();
                    }
                }
            }
        }

        // Get a hashed value based on the currently bound shaders
        // throws an exception if no shaders are bound
        private int GetCurrentShaderProgramHash()
        {
            if (VertexShader == null && PixelShader == null)
                throw new InvalidOperationException("There is no shader bound!");
            if (VertexShader == null)
                return PixelShader.HashKey;
            if (PixelShader == null)
                return VertexShader.HashKey;

            return VertexShader.HashKey ^ PixelShader.HashKey;
        }

        private void PlatformApplyVertexBuffersAttribs(Shader vertexShader, int baseVertex)
        {
            int programHash = GetCurrentShaderProgramHash();
            bool bindingsChanged = false;

            for (int slot = 0; slot < _vertexBuffers.Count; slot++)
            {
                VertexBufferBinding vertexBufferBinding = _vertexBuffers.Get(slot);
                VertexDeclaration vertexDeclaration = vertexBufferBinding.VertexBuffer.VertexDeclaration;
                int maxVertexBufferSlots = this.Context.DeviceStrategy.Capabilities.MaxVertexBufferSlots;
                VertexDeclarationAttributeInfo attrInfo = vertexDeclaration.GetAttributeInfo(vertexShader, programHash, maxVertexBufferSlots);

                int vertexStride = vertexDeclaration.VertexStride;
                IntPtr offset = (IntPtr)(vertexDeclaration.VertexStride * (baseVertex + vertexBufferBinding.VertexOffset));

                if (!_attribsDirty &&
                    slot < _activeBufferBindingInfosCount &&
                    _bufferBindingInfos[slot].VertexOffset == offset &&
                    ReferenceEquals(_bufferBindingInfos[slot].AttributeInfo, attrInfo) &&
                    _bufferBindingInfos[slot].InstanceFrequency == vertexBufferBinding.InstanceFrequency &&
                    _bufferBindingInfos[slot].Vbo == vertexBufferBinding.VertexBuffer.Strategy.ToConcrete<ConcreteVertexBuffer>().GLVertexBuffer)
                    continue;

                bindingsChanged = true;

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferBinding.VertexBuffer.Strategy.ToConcrete<ConcreteVertexBuffer>().GLVertexBuffer);
                GraphicsExtensions.CheckGLError();

                for (int e = 0; e < attrInfo.Elements.Count; e++)
                {
                    VertexDeclarationAttributeInfoElement element = attrInfo.Elements[e];
                    GL.VertexAttribPointer(element.AttributeLocation,
                        element.NumberOfElements,
                        element.VertexAttribPointerType,
                        element.Normalized,
                        vertexStride,
                        (IntPtr)(offset.ToInt64() + element.Offset));
                    GraphicsExtensions.CheckGLError();

                    // only set the divisor if instancing is supported
                    if (this.Context.DeviceStrategy.Capabilities.SupportsInstancing)
                    {
                        GL.VertexAttribDivisor(element.AttributeLocation, vertexBufferBinding.InstanceFrequency);
                        GraphicsExtensions.CheckGLError();
                    }
                    else // If instancing is not supported, but InstanceFrequency of the buffer is not zero, throw an exception
                    {
                        if (vertexBufferBinding.InstanceFrequency > 0)
                            throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics drivers.");
                    }
                }

                _bufferBindingInfos[slot].VertexOffset = offset;
                _bufferBindingInfos[slot].AttributeInfo = attrInfo;
                _bufferBindingInfos[slot].InstanceFrequency = vertexBufferBinding.InstanceFrequency;
                _bufferBindingInfos[slot].Vbo = vertexBufferBinding.VertexBuffer.Strategy.ToConcrete<ConcreteVertexBuffer>().GLVertexBuffer;
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

        internal void PlatformApplyUserVertexDataAttribs(VertexDeclaration vertexDeclaration, Shader vertexShader, IntPtr baseVertex)
        {
            int programHash = GetCurrentShaderProgramHash();
            int maxVertexBufferSlots = this.Context.DeviceStrategy.Capabilities.MaxVertexBufferSlots;
            VertexDeclarationAttributeInfo attrInfo = vertexDeclaration.GetAttributeInfo(vertexShader, programHash, maxVertexBufferSlots);

            // Apply the vertex attribute info
            for (int i = 0; i < attrInfo.Elements.Count; i++)
            {
                VertexDeclarationAttributeInfoElement element = attrInfo.Elements[i];
                GL.VertexAttribPointer(element.AttributeLocation,
                    element.NumberOfElements,
                    element.VertexAttribPointerType,
                    element.Normalized,
                    vertexDeclaration.VertexStride,
                    (IntPtr)(baseVertex.ToInt64() + element.Offset));
                GraphicsExtensions.CheckGLError();

#if DESKTOPGL
                if (this.Context.DeviceStrategy.Capabilities.SupportsInstancing)
                {
                    GL.VertexAttribDivisor(element.AttributeLocation, 0);
                    GraphicsExtensions.CheckGLError();
                }
#endif
            }

            SetVertexAttributeArray(attrInfo.EnabledAttributes);
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

        public override void DrawPrimitives(PrimitiveType primitiveType, int vertexStart, int vertexCount)
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            PlatformApplyVertexBuffersAttribs(VertexShader, 0);

            if (vertexStart < 0)
                vertexStart = 0;

            GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                          vertexStart,
                          vertexCount);
            GraphicsExtensions.CheckGLError();
        }

        public override void DrawIndexedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount)
        {
            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            DrawElementsType indexElementType = Indices.Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType;
            IntPtr indexOffsetInBytes = (IntPtr)(startIndex * Indices.Strategy.ElementSizeInBytes);
            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            GLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            if (GL.DrawElementsBaseVertex != null)
            {
                PlatformApplyVertexBuffersAttribs(VertexShader, 0);

                GL.DrawElementsBaseVertex(target,
                                  indexElementCount,
                                  indexElementType,
                                  indexOffsetInBytes,
                                  baseVertex);
                GraphicsExtensions.CheckGLError();
            }
            else
            {
                PlatformApplyVertexBuffersAttribs(VertexShader, baseVertex);

                GL.DrawElements(target,
                                indexElementCount,
                                indexElementType,
                                indexOffsetInBytes);
                GraphicsExtensions.CheckGLError();

            }

        }

        public override void DrawInstancedPrimitives(PrimitiveType primitiveType, int baseVertex, int startIndex, int primitiveCount, int baseInstance, int instanceCount)
        {
            if (!this.Context.DeviceStrategy.Capabilities.SupportsInstancing)
                throw new PlatformNotSupportedException("Instanced geometry drawing requires at least OpenGL 3.2 or GLES 3.2. Try upgrading your graphics card drivers.");

            PlatformApplyState();
            PlatformApplyIndexBuffer();
            PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            DrawElementsType indexElementType = Indices.Strategy.ToConcrete<ConcreteIndexBuffer>().DrawElementsType;
            IntPtr indexOffsetInBytes = (IntPtr)(startIndex * Indices.Strategy.ElementSizeInBytes);
            int indexElementCount = GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount);
            GLPrimitiveType target = ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType);

            if (baseInstance > 0)
            {
                if (!this.Context.DeviceStrategy.Capabilities.SupportsBaseIndexInstancing)
                    throw new PlatformNotSupportedException("Instanced geometry drawing with base instance requires at least OpenGL 4.2. Try upgrading your graphics card drivers.");

                PlatformApplyVertexBuffersAttribs(VertexShader, baseVertex);

                GL.DrawElementsInstancedBaseInstance(target,
                                                     indexElementCount,
                                                     indexElementType,
                                                     indexOffsetInBytes,
                                                     instanceCount,
                                                     baseInstance);
            }
            else
            {
                PlatformApplyVertexBuffersAttribs(VertexShader, baseVertex);

                GL.DrawElementsInstanced(target,
                                         indexElementCount,
                                         indexElementType,
                                         indexOffsetInBytes,
                                         instanceCount);
            }

            GraphicsExtensions.CheckGLError();
        }

        public override void DrawUserPrimitives<T>(PrimitiveType primitiveType, T[] vertexData, int vertexOffset, VertexDeclaration vertexDeclaration, int vertexCount)
            //where T : struct
        {
            PlatformApplyState();
            //PlatformApplyIndexBuffer();
            //PlatformApplyVertexBuffers();
            PlatformApplyShaders();

            // Unbind current VBOs.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;

            // Pin the buffers.
            GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            try
            {
                // Setup the vertex declaration to point at the VB data.
                vertexDeclaration.BindGraphicsDevice(this.Context.DeviceStrategy.Device);
                PlatformApplyUserVertexDataAttribs(vertexDeclaration, VertexShader, vbHandle.AddrOfPinnedObject());

                //Draw
                GL.DrawArrays(ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                              vertexOffset,
                              vertexCount);
                GraphicsExtensions.CheckGLError();
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
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try
            {
                IntPtr vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

                // Setup the vertex declaration to point at the VB data.
                vertexDeclaration.BindGraphicsDevice(this.Context.DeviceStrategy.Device);
                PlatformApplyUserVertexDataAttribs(vertexDeclaration, VertexShader, vertexAddr);

                //Draw
                GL.DrawElements(
                    ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                    GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount),
                    DrawElementsType.UnsignedShort,
                    (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(short))));
                GraphicsExtensions.CheckGLError();
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
            GraphicsExtensions.CheckGLError();
            _vertexBuffersDirty = true;
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GraphicsExtensions.CheckGLError();
            _indexBufferDirty = true;

            // Pin the buffers.
            GCHandle vbHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            GCHandle ibHandle = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try
            {
                IntPtr vertexAddr = (IntPtr)(vbHandle.AddrOfPinnedObject().ToInt64() + vertexDeclaration.VertexStride * vertexOffset);

                // Setup the vertex declaration to point at the VB data.
                vertexDeclaration.BindGraphicsDevice(this.Context.DeviceStrategy.Device);
                PlatformApplyUserVertexDataAttribs(vertexDeclaration, VertexShader, vertexAddr);

                //Draw
                GL.DrawElements(
                    ConcreteGraphicsContext.PrimitiveTypeGL(primitiveType),
                    GraphicsContextStrategy.GetElementCountArray(primitiveType, primitiveCount),
                    DrawElementsType.UnsignedInt,
                    (IntPtr)(ibHandle.AddrOfPinnedObject().ToInt64() + (indexOffset * sizeof(int))));
                GraphicsExtensions.CheckGLError();
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


        internal override OcclusionQueryStrategy CreateOcclusionQueryStrategy()
        {
            return new ConcreteOcclusionQuery(this);
        }

        internal override GraphicsDebugStrategy CreateGraphicsDebugStrategy()
        {
            return new ConcreteGraphicsDebug(this);
        }

        internal override TextureCollectionStrategy CreateTextureCollectionStrategy(int capacity)
        {
            return new ConcreteTextureCollection(this, capacity);
        }

        internal override SamplerStateCollectionStrategy CreateSamplerStateCollectionStrategy(int capacity)
        {
            return new ConcreteSamplerStateCollection(this, capacity);
        }

        internal override ITexture2DStrategy CreateTexture2DStrategy(int width, int height, bool mipMap, SurfaceFormat format, int arraySize, bool shared)
        {
            return new ConcreteTexture2D(this, width, height, mipMap, format, arraySize, shared);
        }

        internal override ITexture3DStrategy CreateTexture3DStrategy(int width, int height, int depth, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTexture3D(this, width, height, depth, mipMap, format);
        }

        internal override ITextureCubeStrategy CreateTextureCubeStrategy(int size, bool mipMap, SurfaceFormat format)
        {
            return new ConcreteTextureCube(this, size, mipMap, format);
        }

        internal override IRenderTarget2DStrategy CreateRenderTarget2DStrategy(int width, int height, bool mipMap, int arraySize, bool shared, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount, Texture2D.SurfaceType surfaceType)
        {
            return new ConcreteRenderTarget2D(this, width, height, mipMap, arraySize, shared, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount,
                                              surfaceType: surfaceType);
        }

        internal override IRenderTarget3DStrategy CreateRenderTarget3DStrategy(int width, int height, int depth, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTarget3D(this, width, height, depth, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount);
        }

        internal override IRenderTargetCubeStrategy CreateRenderTargetCubeStrategy(int size, bool mipMap, RenderTargetUsage usage, SurfaceFormat preferredSurfaceFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount)
        {
            return new ConcreteRenderTargetCube(this, size, mipMap, usage, preferredSurfaceFormat, preferredDepthFormat, preferredMultiSampleCount);
        }

        internal override ITexture2DStrategy CreateTexture2DStrategy(Stream stream)
        {
            return new ConcreteTexture2D(this, stream);
        }

        internal override ShaderStrategy CreateVertexShaderStrategy(byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
        {
            return new ConcreteVertexShader(this, shaderBytecode, samplers, cBuffers, attributes, profile);
        }
        internal override ShaderStrategy CreatePixelShaderStrategy(byte[] shaderBytecode, SamplerInfo[] samplers, int[] cBuffers, VertexAttribute[] attributes, ShaderProfileType profile)
        {
            return new ConcretePixelShader(this, shaderBytecode, samplers, cBuffers, attributes, profile);
        }
        internal override ConstantBufferStrategy CreateConstantBufferStrategy(string name, int[] parameterIndexes, int[] parameterOffsets, int sizeInBytes, ShaderProfileType profile)
        {
            return new ConcreteConstantBuffer(this, name, parameterIndexes, parameterOffsets, sizeInBytes, profile);
        }

        internal override IndexBufferStrategy CreateIndexBufferStrategy(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
        {
            return new ConcreteIndexBuffer(this, indexElementSize, indexCount, usage);
        }
        internal override VertexBufferStrategy CreateVertexBufferStrategy(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
        {
            return new ConcreteVertexBuffer(this, vertexDeclaration, vertexCount, usage);
        }
        internal override IndexBufferStrategy CreateDynamicIndexBufferStrategy(IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
        {
            return new ConcreteDynamicIndexBuffer(this, indexElementSize, indexCount, usage);
        }
        internal override VertexBufferStrategy CreateDynamicVertexBufferStrategy(VertexDeclaration vertexDeclaration, int vertexCount, BufferUsage usage)
        {
            return new ConcreteDynamicVertexBuffer(this, vertexDeclaration, vertexCount, usage);
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

        internal void PlatformResolveRenderTargets()
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
                    GraphicsExtensions.CheckGLError();
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, glResolveFramebuffer);
                    GraphicsExtensions.CheckGLError();

                    for (int i = 0; i < _currentRenderTargetCount; i++)
                    {
                        IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)_currentRenderTargetBindings[i].RenderTarget;

                        FramebufferAttachment attachement = (FramebufferAttachment.ColorAttachment0 + i);
                        TextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, renderTargetGL.GLTexture, 0);
                        GraphicsExtensions.CheckGLError();
                    }
                    _glResolveFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glResolveFramebuffer);
                }
                else
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, glResolveFramebuffer);
                    GraphicsExtensions.CheckGLError();
                }

                // The only fragment operations which affect the resolve are the pixel ownership test, the scissor test, and dithering.
                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Disable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }

                int glFramebuffer = _glFramebuffers[_currentRenderTargetBindings];
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, glFramebuffer);
                GraphicsExtensions.CheckGLError();

                for (int i = 0; i < _currentRenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    renderTarget = renderTargetBinding.RenderTarget as IRenderTarget;

                    GL.ReadBuffer(ReadBufferMode.ColorAttachment0 + i);
                    GraphicsExtensions.CheckGLError();
                    GL.DrawBuffer(DrawBufferMode.ColorAttachment0 + i);
                    GraphicsExtensions.CheckGLError();
                    Debug.Assert(this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsBlitFramebuffer);
                    GL.BlitFramebuffer(0, 0, renderTarget.Width, renderTarget.Height,
                                       0, 0, renderTarget.Width, renderTarget.Height,
                                       ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
                    GraphicsExtensions.CheckGLError();
                }

                if (renderTarget.RenderTargetUsage == RenderTargetUsage.DiscardContents && this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsInvalidateFramebuffer)
                {
                    Debug.Assert(this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._supportsInvalidateFramebuffer);
                    GL.InvalidateFramebuffer(FramebufferTarget.Framebuffer, 3, InvalidateFramebufferAttachements);
                    GraphicsExtensions.CheckGLError();
                }

                if (_lastRasterizerState.ScissorTestEnable)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GraphicsExtensions.CheckGLError();
                }
            }

            for (int i = 0; i < _currentRenderTargetCount; i++)
            {
                renderTargetBinding = _currentRenderTargetBindings[i];
                if (renderTargetBinding.RenderTarget.LevelCount > 1)
                {
                    IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)renderTargetBinding.RenderTarget.GetTextureStrategy<ITextureStrategy>();
                    GL.BindTexture(renderTargetGL.GLTarget, renderTargetGL.GLTexture);
                    GraphicsExtensions.CheckGLError();
                    GL.GenerateMipmap(renderTargetGL.GLTarget);
                    GraphicsExtensions.CheckGLError();
                }
            }
        }

        internal void PlatformApplyDefaultRenderTarget()
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.Context.DeviceStrategy.ToConcrete<ConcreteGraphicsDevice>()._glDefaultFramebuffer);
            GraphicsExtensions.CheckGLError();

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            _pixelTextures.Strategy.Dirty();
        }

        internal IRenderTarget PlatformApplyRenderTargets()
        {
            int glFramebuffer = 0;
            if (!_glFramebuffers.TryGetValue(_currentRenderTargetBindings, out glFramebuffer))
            {
                glFramebuffer = GL.GenFramebuffer();
                GraphicsExtensions.CheckGLError();
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
                GraphicsExtensions.CheckGLError();
                RenderTargetBinding renderTargetBinding = _currentRenderTargetBindings[0];
                IRenderTargetStrategyGL renderTargetGL = (IRenderTargetStrategyGL)renderTargetBinding.RenderTarget.GetTextureStrategy<ITextureStrategy>();

                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderTargetGL.GLDepthBuffer);
                GraphicsExtensions.CheckGLError();
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.StencilAttachment, RenderbufferTarget.Renderbuffer, renderTargetGL.GLStencilBuffer);
                GraphicsExtensions.CheckGLError();

                for (int i = 0; i < _currentRenderTargetCount; i++)
                {
                    renderTargetBinding = _currentRenderTargetBindings[i];
                    IRenderTarget renderTarget = (IRenderTarget)renderTargetBinding.RenderTarget;
                    renderTargetGL = renderTargetBinding.RenderTarget.GetTextureStrategy<ITextureStrategy>() as IRenderTargetStrategyGL;
                    FramebufferAttachment attachement = (FramebufferAttachment.ColorAttachment0 + i);

                    if (renderTargetGL.GLColorBuffer != renderTargetGL.GLTexture)
                    {
                        GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachement, RenderbufferTarget.Renderbuffer, renderTargetGL.GLColorBuffer);
                        GraphicsExtensions.CheckGLError();
                    }
                    else
                    {
                        TextureTarget target = renderTargetGL.GetFramebufferTarget(renderTargetBinding.ArraySlice);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachement, target, renderTargetGL.GLTexture, 0);
                        GraphicsExtensions.CheckGLError();
                    }
                }

                CheckFramebufferStatus();

                _glFramebuffers.Add((RenderTargetBinding[])_currentRenderTargetBindings.Clone(), glFramebuffer);
            }
            else
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, glFramebuffer);
                GraphicsExtensions.CheckGLError();
            }

#if DESKTOPGL
            GL.DrawBuffers(_currentRenderTargetCount, this.ToConcrete<ConcreteGraphicsContext>()._drawBuffers);
#endif

            // Reset the raster state because we flip vertices
            // when rendering offscreen and hence the cull direction.
            _rasterizerStateDirty = true;

            // Textures will need to be rebound to render correctly in the new render target.
            _pixelTextures.Strategy.Dirty();

            return _currentRenderTargetBindings[0].RenderTarget as IRenderTarget;
        }

        internal void PlatformUnbindRenderTarget(IRenderTarget renderTarget)
        {
            var bindingsToDelete = new List<RenderTargetBinding[]>();
            foreach (RenderTargetBinding[] bindings in _glFramebuffers.Keys)
            {
                foreach (RenderTargetBinding binding in bindings)
                {
                    if (binding.RenderTarget == renderTarget)
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
                    GraphicsExtensions.CheckGLError();
                    _glFramebuffers.Remove(bindings);
                }
                if (_glResolveFramebuffers.TryGetValue(bindings, out fbo))
                {
                    GL.DeleteFramebuffer(fbo);
                    GraphicsExtensions.CheckGLError();

                    _glResolveFramebuffers.Remove(bindings);
                }
            }
        }


        [Conditional("DEBUG")]
        public static void CheckFramebufferStatus()
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
