// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    internal class ResourceBlendStateStrategy : BlendStateStrategy
    {
        public override bool IndependentBlendEnable
        {
            get { return base.IndependentBlendEnable; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override Color BlendFactor
        {
            get { return base.BlendFactor; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override int MultiSampleMask
        {
            get { return base.MultiSampleMask; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override BlendFunction AlphaBlendFunction
        {
            get { return base.AlphaBlendFunction; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override Blend AlphaDestinationBlend
        {
            get { return base.AlphaDestinationBlend; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override Blend AlphaSourceBlend
        {
            get { return base.AlphaSourceBlend; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override BlendFunction ColorBlendFunction
        {
            get { return base.ColorBlendFunction; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override Blend ColorDestinationBlend
        {
            get { return base.ColorDestinationBlend; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override Blend ColorSourceBlend
        {
            get { return base.ColorSourceBlend; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override ColorWriteChannels ColorWriteChannels
        {
            get { return base.ColorWriteChannels; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override ColorWriteChannels ColorWriteChannels1
        {
            get { return base.ColorWriteChannels1; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override ColorWriteChannels ColorWriteChannels2
        {
            get { return base.ColorWriteChannels2; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override ColorWriteChannels ColorWriteChannels3
        {
            get { return base.ColorWriteChannels3; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public ResourceBlendStateStrategy(IBlendStateStrategy source)
            : base(source)
        {
        }
    }
}