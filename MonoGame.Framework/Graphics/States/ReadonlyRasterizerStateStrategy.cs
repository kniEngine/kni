// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ReadonlyRasterizerStateStrategy : RasterizerStateStrategy
    {

        public override CullMode CullMode
        {
            get { return base.CullMode; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override float DepthBias
        {
            get { return base.DepthBias; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override FillMode FillMode
        {
            get { return base.FillMode; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override bool MultiSampleAntiAlias
        {
            get { return base.MultiSampleAntiAlias; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override bool ScissorTestEnable
        {
            get { return base.ScissorTestEnable; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override float SlopeScaleDepthBias
        {
            get { return base.SlopeScaleDepthBias; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override bool DepthClipEnable
        {
            get { return base.DepthClipEnable; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public ReadonlyRasterizerStateStrategy(CullMode cullMode)
            : base()
        {
            base.CullMode = cullMode;
        }
    }
}
