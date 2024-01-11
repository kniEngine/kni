// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public class ConstantBuffer : GraphicsResource
    {
        private ConstantBufferStrategy _strategy;
        
        internal ConstantBufferStrategy Strategy { get { return _strategy; } }

        public ConstantBuffer(GraphicsDevice device,
                              string name,
                              int[] parameterIndexes,
                              int[] parameterOffsets,
                              int sizeInBytes,
                              ShaderProfileType profile)
            : base()
        {
            _strategy = ((IPlatformGraphicsContext)device.MainContext).Strategy.CreateConstantBufferStrategy(name, parameterIndexes, parameterOffsets, sizeInBytes, profile);
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }

        public ConstantBuffer(ConstantBuffer cloneSource)
            : base()
        {
            _strategy = (ConstantBufferStrategy)cloneSource._strategy.Clone();
            SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_strategy != null)
                    _strategy.Dispose();

                _strategy = null;
            }

            base.Dispose(disposing);
        }
    }
}
