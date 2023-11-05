// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class BlendStateStrategy : IBlendStateStrategy
    {
        private Color _blendFactor;
        private int _multiSampleMask;

        public virtual Color BlendFactor
        {
            get { return _blendFactor; }
            set { _blendFactor = value; }
        }

        public virtual int MultiSampleMask
        {
            get { return _multiSampleMask; }
            set { _multiSampleMask = value; }
        }

        public virtual BlendFunction AlphaBlendFunction
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend AlphaDestinationBlend
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend AlphaSourceBlend
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual BlendFunction ColorBlendFunction
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend ColorDestinationBlend
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend ColorSourceBlend
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels1
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels2
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels3
        {
            get { throw new InvalidOperationException(); }
            set { throw new InvalidOperationException(); }
        }

        public BlendStateStrategy()
        {
        }
    }
}
