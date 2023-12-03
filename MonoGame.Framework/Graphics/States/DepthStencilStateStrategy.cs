// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    internal class DepthStencilStateStrategy : GraphicsResourceStrategy
        , IDepthStencilStateStrategy
    {
        private bool _depthBufferEnable;
        private bool _depthBufferWriteEnable;
        private CompareFunction _depthBufferFunction;
        private StencilOperation _counterClockwiseStencilDepthBufferFail;
        private StencilOperation _counterClockwiseStencilFail;
        private CompareFunction _counterClockwiseStencilFunction;
        private StencilOperation _counterClockwiseStencilPass;
        private int _referenceStencil;
        private StencilOperation _stencilDepthBufferFail;
        private bool _stencilEnable;
        private StencilOperation _stencilFail;
        private CompareFunction _stencilFunction;
        private int _stencilMask;
        private StencilOperation _stencilPass;
        private int _stencilWriteMask;
        private bool _twoSidedStencilMode;

        public virtual bool DepthBufferEnable
        {
            get { return _depthBufferEnable; }
            set { _depthBufferEnable = value; }
        }

        public virtual bool DepthBufferWriteEnable
        {
            get { return _depthBufferWriteEnable; }
            set { _depthBufferWriteEnable = value; }
        }

        public virtual CompareFunction DepthBufferFunction
        {
            get { return _depthBufferFunction; }
            set { _depthBufferFunction = value; }
        }

        public virtual StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get { return _counterClockwiseStencilDepthBufferFail; }
            set { _counterClockwiseStencilDepthBufferFail = value; }
        }

        public virtual StencilOperation CounterClockwiseStencilFail
        {
            get { return _counterClockwiseStencilFail; }
            set { _counterClockwiseStencilFail = value; }
        }

        public virtual CompareFunction CounterClockwiseStencilFunction
        {
            get { return _counterClockwiseStencilFunction; }
            set { _counterClockwiseStencilFunction = value; }
        }

        public virtual StencilOperation CounterClockwiseStencilPass
        {
            get { return _counterClockwiseStencilPass; }
            set { _counterClockwiseStencilPass = value; }
        }

        public virtual int ReferenceStencil
        {
            get { return _referenceStencil; }
            set { _referenceStencil = value; }
        }

        public virtual StencilOperation StencilDepthBufferFail
        {
            get { return _stencilDepthBufferFail; }
            set { _stencilDepthBufferFail = value; }
        }

        public virtual bool StencilEnable
        {
            get { return _stencilEnable; }
            set { _stencilEnable = value; }
        }

        public virtual StencilOperation StencilFail
        {
            get { return _stencilFail; }
            set { _stencilFail = value; }
        }

        public virtual CompareFunction StencilFunction
        {
            get { return _stencilFunction; }
            set { _stencilFunction = value; }
        }

        public virtual int StencilMask
        {
            get { return _stencilMask; }
            set { _stencilMask = value; }
        }

        public virtual StencilOperation StencilPass
        {
            get { return _stencilPass; }
            set { _stencilPass = value; }
        }

        public virtual int StencilWriteMask
        {
            get { return _stencilWriteMask; }
            set { _stencilWriteMask = value; }
        }

        public virtual bool TwoSidedStencilMode
        {
            get { return _twoSidedStencilMode; }
            set { _twoSidedStencilMode = value;
            }
        }

        public DepthStencilStateStrategy()
            : base()
        {
            _depthBufferEnable = true;
            _depthBufferWriteEnable = true;
            _depthBufferFunction = CompareFunction.LessEqual;
            _stencilEnable = false;
            _stencilFunction = CompareFunction.Always;
            _stencilPass = StencilOperation.Keep;
            _stencilFail = StencilOperation.Keep;
            _stencilDepthBufferFail = StencilOperation.Keep;
            _twoSidedStencilMode = false;
            _counterClockwiseStencilFunction = CompareFunction.Always;
            _counterClockwiseStencilPass = StencilOperation.Keep;
            _counterClockwiseStencilFail = StencilOperation.Keep;
            _counterClockwiseStencilDepthBufferFail = StencilOperation.Keep;
            _stencilMask = Int32.MaxValue;
            _stencilWriteMask = Int32.MaxValue;
            _referenceStencil = 0;
        }

        internal DepthStencilStateStrategy(IDepthStencilStateStrategy source)
            : base()
        {
            _depthBufferEnable = source.DepthBufferEnable;
            _depthBufferWriteEnable = source.DepthBufferWriteEnable;
            _depthBufferFunction = source.DepthBufferFunction;
            _stencilEnable = source.StencilEnable;
            _stencilFunction = source.StencilFunction;
            _stencilPass = source.StencilPass;
            _stencilFail = source.StencilFail;
            _stencilDepthBufferFail = source.StencilDepthBufferFail;
            _twoSidedStencilMode = source.TwoSidedStencilMode;
            _counterClockwiseStencilFunction = source.CounterClockwiseStencilFunction;
            _counterClockwiseStencilPass = source.CounterClockwiseStencilPass;
            _counterClockwiseStencilFail = source.CounterClockwiseStencilFail;
            _counterClockwiseStencilDepthBufferFail = source.CounterClockwiseStencilDepthBufferFail;
            _stencilMask = source.StencilMask;
            _stencilWriteMask = source.StencilWriteMask;
            _referenceStencil = source.ReferenceStencil;
        }
    }
}
