// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class DepthStencilState : GraphicsResource
    {
        internal IDepthStencilStateStrategy _strategy;

        public static readonly DepthStencilState Default;
        public static readonly DepthStencilState DepthRead;
        public static readonly DepthStencilState None;

        static DepthStencilState()
        {
            Default = new DepthStencilState("DepthStencilState.Default", true, true);
            DepthRead = new DepthStencilState("DepthStencilState.DepthRead", true, false);
            None = new DepthStencilState("DepthStencilState.None", false, false);
        }

        internal T GetStrategy<T>() where T : IDepthStencilStateStrategy
        {
            return (T)_strategy;
        }


        public bool DepthBufferEnable
        {
            get { return _strategy.DepthBufferEnable; }
            set { _strategy.DepthBufferEnable = value; }
        }

        public bool DepthBufferWriteEnable
        {
            get { return _strategy.DepthBufferWriteEnable; }
            set { _strategy.DepthBufferWriteEnable = value; }
        }

        public StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get { return _strategy.CounterClockwiseStencilDepthBufferFail; }
            set { _strategy.CounterClockwiseStencilDepthBufferFail = value; }
        }

        public StencilOperation CounterClockwiseStencilFail
        {
            get { return _strategy.CounterClockwiseStencilFail; }
            set { _strategy.CounterClockwiseStencilFail = value; }
        }

        public CompareFunction CounterClockwiseStencilFunction
        {
            get { return _strategy.CounterClockwiseStencilFunction; }
            set { _strategy.CounterClockwiseStencilFunction = value; }
        }

        public StencilOperation CounterClockwiseStencilPass
        {
            get { return _strategy.CounterClockwiseStencilPass; }
            set { _strategy.CounterClockwiseStencilPass = value; }
        }

        public CompareFunction DepthBufferFunction
        {
            get { return _strategy.DepthBufferFunction; }
            set { _strategy.DepthBufferFunction = value; }
        }

        public int ReferenceStencil
        {
            get { return _strategy.ReferenceStencil; }
            set { _strategy.ReferenceStencil = value; }
        }

        public StencilOperation StencilDepthBufferFail
        {
            get { return _strategy.StencilDepthBufferFail; }
            set { _strategy.StencilDepthBufferFail = value; }
        }

        public bool StencilEnable
        {
            get { return _strategy.StencilEnable; }
            set { _strategy.StencilEnable = value; }
        }

        public StencilOperation StencilFail
        {
            get { return _strategy.StencilFail; }
            set { _strategy.StencilFail = value; }
        }

        public CompareFunction StencilFunction
        {
            get { return _strategy.StencilFunction; }
            set { _strategy.StencilFunction = value; }
        }

        public int StencilMask
        {
            get { return _strategy.StencilMask; }
            set { _strategy.StencilMask = value; }
        }

        public StencilOperation StencilPass
        {
            get { return _strategy.StencilPass; }
            set {
                _strategy.StencilPass = value; }
        }

        public int StencilWriteMask
        {
            get { return _strategy.StencilWriteMask; }
            set { _strategy.StencilWriteMask = value; }
        }

        public bool TwoSidedStencilMode
        {
            get { return _strategy.TwoSidedStencilMode; }
            set { _strategy.TwoSidedStencilMode = value; }
        }

        internal void BindToGraphicsDevice(GraphicsDeviceStrategy deviceStrategy)
        {
            if (_strategy is ReadonlyDepthStencilStateStrategy)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (this.GraphicsDevice != deviceStrategy.Device)
            {
                if (this.GraphicsDevice == null)
                {
                    System.Diagnostics.Debug.Assert(deviceStrategy.Device != null);

                    _strategy = ((IPlatformGraphicsContext)deviceStrategy.CurrentContext).Strategy.CreateDepthStencilStateStrategy(_strategy);
                    SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
                }
                else
                    throw new InvalidOperationException("This depth stencil state is already bound to a different graphics device.");
            }
        }

        public DepthStencilState()
            : base()
        {
            _strategy = new DepthStencilStateStrategy();
        }

        private DepthStencilState(string name, bool depthBufferEnable, bool depthBufferWriteEnable)
            : base()
        {
            Name = name;
            _strategy = new ReadonlyDepthStencilStateStrategy(depthBufferEnable, depthBufferWriteEnable);
        }

        internal DepthStencilState(DepthStencilState source)
            : base()
        {
            Name = source.Name;
            _strategy = new DepthStencilStateStrategy(source._strategy);
        }


        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            base.Dispose(disposing);
        }
    }
}

