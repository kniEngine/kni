// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.Utilities;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class ConcreteIndexBufferGL : IndexBufferStrategy
    {
        private readonly BufferUsageHint _usageHint;
        private readonly DrawElementsType _drawElementsType;
        private int _ibo;


        internal DrawElementsType DrawElementsType { get { return _drawElementsType; } }
        internal int GLIndexBuffer { get { return _ibo; } }

        internal ConcreteIndexBufferGL(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage, bool isDynamic)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            Debug.Assert(isDynamic == true);
            _usageHint = BufferUsageHint.DynamicDraw;

            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits:   this._drawElementsType = DrawElementsType.UnsignedShort; break;
                case IndexElementSize.ThirtyTwoBits: this._drawElementsType = DrawElementsType.UnsignedInt; break;
                default: throw new InvalidOperationException();
            }
        }

        internal ConcreteIndexBufferGL(GraphicsContextStrategy contextStrategy, IndexElementSize indexElementSize, int indexCount, BufferUsage usage)
            : base(contextStrategy, indexElementSize, indexCount, usage)
        {
            _usageHint = BufferUsageHint.StaticDraw;

            switch (indexElementSize)
            {
                case IndexElementSize.SixteenBits: this._drawElementsType = DrawElementsType.UnsignedShort; break;
                case IndexElementSize.ThirtyTwoBits: this._drawElementsType = DrawElementsType.UnsignedInt; break;
                default: throw new InvalidOperationException();
            }

            PlatformConstructIndexBuffer(contextStrategy);
        }

        internal void PlatformConstructIndexBuffer(GraphicsContextStrategy contextStrategy)
        {
            contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();

            Debug.Assert(_ibo == 0);

            var GL = contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int sizeInBytes = this.IndexCount * base.ElementSizeInBytes;

            _ibo = GL.GenBuffer();
            GL.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GL.CheckGLError();
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._indexBufferDirty = true;

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          (IntPtr)sizeInBytes, IntPtr.Zero, _usageHint);
            GL.CheckGLError();
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().EnsureContextCurrentThread();

            Debug.Assert(GLIndexBuffer != 0);

            var GL = ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy.ToConcrete<ConcreteGraphicsContextGL>().GL;

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementSizeInByte * elementCount;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();
                dataPtr = (IntPtr)(dataPtr.ToInt64() + startIndex * elementSizeInByte);

                int bufferSize = IndexCount * base.ElementSizeInBytes;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLIndexBuffer);
                GL.CheckGLError();
                ((IPlatformGraphicsContext)base.GraphicsDeviceStrategy.CurrentContext).Strategy._indexBufferDirty = true;

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(
                        BufferTarget.ElementArrayBuffer,
                        (IntPtr)bufferSize,
                        IntPtr.Zero,
                        _usageHint);
                    GL.CheckGLError();
                }

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
                GL.CheckGLError();
            }
            finally
            {
                dataHandle.Free();
            }
        }

        protected override void PlatformGraphicsContextLost()
        {
            _ibo = 0;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            if (_ibo != 0)
            {
                if (GraphicsDevice != null && !GraphicsDevice.IsDisposed)
                {
                    _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().BindDisposeContext();
                    try
                    {
                        var GL = _contextStrategy.ToConcrete<ConcreteGraphicsContext>().GL;

                        GL.DeleteBuffer(_ibo);
                        GL.CheckGLError();
                        _ibo = 0;
                    }
                    finally
                    {
                        _contextStrategy.ToConcrete<ConcreteGraphicsContextGL>().UnbindDisposeContext();
                    }
                }
            }

            base.Dispose(disposing);
        }
    }

}
