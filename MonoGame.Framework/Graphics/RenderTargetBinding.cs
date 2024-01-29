//// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

// Copyright (C)2023-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;


namespace Microsoft.Xna.Framework.Graphics
{
    // http://msdn.microsoft.com/en-us/library/ff434403.aspx
    public struct RenderTargetBinding
    {
        private readonly Texture _renderTarget;
        private readonly int _arraySlice;

        public Texture RenderTarget { get { return _renderTarget; } }
        public int ArraySlice { get { return _arraySlice; } }


        public RenderTargetBinding(RenderTarget2D renderTarget)
        {
            if (renderTarget == null) 
                throw new ArgumentNullException("renderTarget");

            _renderTarget = renderTarget;
            _arraySlice = (int)CubeMapFace.PositiveX;
        }

        public RenderTargetBinding(RenderTargetCube renderTarget, CubeMapFace cubeMapFace)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (cubeMapFace < CubeMapFace.PositiveX || cubeMapFace > CubeMapFace.NegativeZ)
                throw new ArgumentOutOfRangeException("cubeMapFace");

            _renderTarget = renderTarget;
            _arraySlice = (int)cubeMapFace;
        }

        public RenderTargetBinding(RenderTarget2D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (arraySlice < 0 || arraySlice >= renderTarget.ArraySize)
                throw new ArgumentOutOfRangeException("arraySlice");
            if (!((IPlatformGraphicsDevice)renderTarget.GraphicsDevice).Strategy.Capabilities.SupportsTextureArrays)
                throw new InvalidOperationException("Texture arrays are not supported on this graphics device");

            _renderTarget = renderTarget;
            _arraySlice = arraySlice;
        }

        public RenderTargetBinding(RenderTarget3D renderTarget, int arraySlice)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");
            if (arraySlice < 0 || arraySlice >= renderTarget.Depth)
                throw new ArgumentOutOfRangeException("arraySlice");

            _renderTarget = renderTarget;
            _arraySlice = arraySlice;
        }

        public RenderTargetBinding(RenderTarget3D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException("renderTarget");

            _renderTarget = renderTarget;
            _arraySlice = 0;
        }

        public static implicit operator RenderTargetBinding(RenderTarget2D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

        public static implicit operator RenderTargetBinding(RenderTarget3D renderTarget)
        {
            return new RenderTargetBinding(renderTarget);
        }

        public override string ToString()
        {
            return String.Format("{{_arraySlice: {0}, _renderTarget: {1} }}",
                _arraySlice,
                (_renderTarget != null) ? _renderTarget.GetHashCode().ToString("X") : "null"
                );
        }
    }
}
