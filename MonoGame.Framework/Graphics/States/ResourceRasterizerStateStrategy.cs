// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    internal class ResourceRasterizerStateStrategy : RasterizerStateStrategy
    {

        public override CullMode CullMode
        {
            get { return base.CullMode; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override float DepthBias
        {
            get { return base.DepthBias; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override FillMode FillMode
        {
            get { return base.FillMode; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override bool MultiSampleAntiAlias
        {
            get { return base.MultiSampleAntiAlias; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override bool ScissorTestEnable
        {
            get { return base.ScissorTestEnable; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override float SlopeScaleDepthBias
        {
            get { return base.SlopeScaleDepthBias; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override bool DepthClipEnable
        {
            get { return base.DepthClipEnable; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public ResourceRasterizerStateStrategy(IRasterizerStateStrategy source) 
            : base(source)
        {
        }
    }
}
