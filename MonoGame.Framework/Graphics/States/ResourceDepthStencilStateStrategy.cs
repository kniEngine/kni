// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public class ResourceDepthStencilStateStrategy : DepthStencilStateStrategy
    {
        public override bool DepthBufferEnable
        {
            get { return base.DepthBufferEnable; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override bool DepthBufferWriteEnable
        {
            get { return base.DepthBufferWriteEnable; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override StencilOperation CounterClockwiseStencilDepthBufferFail
        {
            get { return base.CounterClockwiseStencilDepthBufferFail; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override StencilOperation CounterClockwiseStencilFail
        {
            get { return base.CounterClockwiseStencilFail; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override CompareFunction CounterClockwiseStencilFunction
        {
            get { return base.CounterClockwiseStencilFunction; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override StencilOperation CounterClockwiseStencilPass
        {
            get { return base.CounterClockwiseStencilPass; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override CompareFunction DepthBufferFunction
        {
            get { return base.DepthBufferFunction; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override int ReferenceStencil
        {
            get { return base.ReferenceStencil; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override StencilOperation StencilDepthBufferFail
        {
            get { return base.StencilDepthBufferFail; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override bool StencilEnable
        {
            get { return base.StencilEnable; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override StencilOperation StencilFail
        {
            get { return base.StencilFail; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override CompareFunction StencilFunction
        {
            get { return base.StencilFunction; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override int StencilMask
        {
            get { return base.StencilMask; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override StencilOperation StencilPass
        {
            get { return base.StencilPass; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override int StencilWriteMask
        {
            get { return base.StencilWriteMask; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override bool TwoSidedStencilMode
        {
            get { return base.TwoSidedStencilMode; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public ResourceDepthStencilStateStrategy(GraphicsContextStrategy contextStrategy, IDepthStencilStateStrategy source)
            : base(contextStrategy, source)
        {
        }
    }
}