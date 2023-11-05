// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ReadonlyDepthStencilStateStrategy : DepthStencilStateStrategy
    {
        public override bool DepthBufferEnable
        {
            get { return base.DepthBufferEnable; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override bool DepthBufferWriteEnable
        {
            get { return base.DepthBufferWriteEnable; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get { return base.CounterClockwiseStencilDepthBufferFail; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override StencilOperation CounterClockwiseStencilFail
        {
            get { return base.CounterClockwiseStencilFail; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override CompareFunction CounterClockwiseStencilFunction
        {
            get { return base.CounterClockwiseStencilFunction; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override StencilOperation CounterClockwiseStencilPass
        {
            get { return base.CounterClockwiseStencilPass; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override CompareFunction DepthBufferFunction
        {
            get { return base.DepthBufferFunction; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override int ReferenceStencil
        {
            get { return base.ReferenceStencil; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override StencilOperation StencilDepthBufferFail
        {
            get { return base.StencilDepthBufferFail; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override bool StencilEnable
        {
            get { return base.StencilEnable; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override StencilOperation StencilFail
        {
            get { return base.StencilFail; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override CompareFunction StencilFunction
        {
            get { return base.StencilFunction; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override int StencilMask
        {
            get { return base.StencilMask; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override StencilOperation StencilPass
        {
            get { return base.StencilPass; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override int StencilWriteMask
        {
            get { return base.StencilWriteMask; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override bool TwoSidedStencilMode
        {
            get { return base.TwoSidedStencilMode; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public ReadonlyDepthStencilStateStrategy(bool depthBufferEnable, bool depthBufferWriteEnable)
        {
            base.DepthBufferEnable = depthBufferEnable;
            base.DepthBufferWriteEnable = depthBufferWriteEnable;
        }
    }
}