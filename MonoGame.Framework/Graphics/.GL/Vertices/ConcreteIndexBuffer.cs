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
using MonoGame.Framework.Utilities;
using MonoGame.OpenGL;


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

            PlatformConstructIndexBuffer();
        }

        internal void PlatformConstructIndexBuffer()
        {
            Threading.EnsureUIThread();

            Debug.Assert(_ibo == 0);

            int sizeInBytes = this.IndexCount * base.ElementSizeInBytes;

            _ibo = GL.GenBuffer();
            GraphicsExtensions.CheckGLError();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ibo);
            GraphicsExtensions.CheckGLError();
            this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          (IntPtr)sizeInBytes, IntPtr.Zero, _usageHint);
            GraphicsExtensions.CheckGLError();
        }

        public override void SetData<T>(int offsetInBytes, T[] data, int startIndex, int elementCount, SetDataOptions options)
        {
            Threading.EnsureUIThread();

            Debug.Assert(GLIndexBuffer != 0);

            int elementSizeInByte = ReflectionHelpers.SizeOf<T>();
            int sizeInBytes = elementSizeInByte * elementCount;
            GCHandle dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = (IntPtr)(dataHandle.AddrOfPinnedObject().ToInt64() + startIndex * elementSizeInByte);
                int bufferSize = IndexCount * base.ElementSizeInBytes;

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, GLIndexBuffer);
                GraphicsExtensions.CheckGLError();
                this.GraphicsDevice.CurrentContext.Strategy._indexBufferDirty = true;

                if (options == SetDataOptions.Discard)
                {
                    // By assigning NULL data to the buffer this gives a hint
                    // to the device to discard the previous content.
                    GL.BufferData(
                        BufferTarget.ElementArrayBuffer,
                        (IntPtr)bufferSize,
                        IntPtr.Zero,
                        _usageHint);
                    GraphicsExtensions.CheckGLError();
                }

                GL.BufferSubData(BufferTarget.ElementArrayBuffer, (IntPtr)offsetInBytes, (IntPtr)sizeInBytes, dataPtr);
                GraphicsExtensions.CheckGLError();
            }
            finally
            {
                dataHandle.Free();
            }
        }

        internal override void PlatformGraphicsDeviceResetting()
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
                    GL.DeleteBuffer(_ibo);
                    GraphicsExtensions.CheckGLError();
                    _ibo = 0;
                }
            }

            base.Dispose(disposing);
        }
    }

}
