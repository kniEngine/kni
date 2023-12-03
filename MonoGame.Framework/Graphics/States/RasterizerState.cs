// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class RasterizerState : GraphicsResource
    {
        internal IRasterizerStateStrategy _strategy;

        public static readonly RasterizerState CullClockwise;
        public static readonly RasterizerState CullCounterClockwise;
        public static readonly RasterizerState CullNone;

        static RasterizerState()
        {
            CullClockwise = new RasterizerState("RasterizerState.CullClockwise", CullMode.CullClockwiseFace);
            CullCounterClockwise = new RasterizerState("RasterizerState.CullCounterClockwise", CullMode.CullCounterClockwiseFace);
            CullNone = new RasterizerState("RasterizerState.CullNone", CullMode.None);
        }

        public CullMode CullMode
        {
            get { return _strategy.CullMode; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.CullMode = value;
            }
        }

        public float DepthBias
        {
            get { return _strategy.DepthBias; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.DepthBias = value;
            }
        }

        public FillMode FillMode
        {
            get { return _strategy.FillMode; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.FillMode = value;
            }
        }

        public bool MultiSampleAntiAlias
        {
            get { return _strategy.MultiSampleAntiAlias; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.MultiSampleAntiAlias = value;
            }
        }

        public bool ScissorTestEnable
        {
            get { return _strategy.ScissorTestEnable; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.ScissorTestEnable = value;
            }
        }

        public float SlopeScaleDepthBias
        {
            get { return _strategy.SlopeScaleDepthBias; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.SlopeScaleDepthBias = value;
            }
        }

        public bool DepthClipEnable
        {
            get { return _strategy.DepthClipEnable; }
            set
            {
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the rasterizer state after it has been bound to the graphics device!");

                _strategy.DepthClipEnable = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_strategy is ReadonlyRasterizerStateStrategy)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (this.GraphicsDevice != device)
            {
                if (this.GraphicsDevice == null)
                {
                    System.Diagnostics.Debug.Assert(device != null);
                    BindGraphicsDevice(device.Strategy);
                }
                else
                    throw new InvalidOperationException("This rasterizer state is already bound to a different graphics device.");
            }
        }

        public RasterizerState()
            : base()
        {
            _strategy = new RasterizerStateStrategy();
        }

        private RasterizerState(string name, CullMode cullMode)
            : base()
        {
            Name = name;
            _strategy = new ReadonlyRasterizerStateStrategy(cullMode);
        }

        internal RasterizerState(RasterizerState source)
            : base()
        {
            Name = source.Name;
            _strategy = new RasterizerStateStrategy(source._strategy);
        }

        partial void PlatformDispose(bool disposing);

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            PlatformDispose(disposing);
            base.Dispose(disposing);
        }
    }
}
