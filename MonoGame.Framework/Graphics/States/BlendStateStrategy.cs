// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class BlendStateStrategy : IBlendStateStrategy
    {
        private Color _blendFactor;
        private int _multiSampleMask;

        private bool _independentBlendEnable;

        public virtual bool IndependentBlendEnable
        {
            get { return _independentBlendEnable; }
            set { _independentBlendEnable = value; }
        }

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
            _blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;

            _independentBlendEnable = false;

            //_alphaBlendFunction = BlendFunction.Add;
            //_alphaDestinationBlend = Blend.Zero;
            //_alphaSourceBlend = Blend.One;
            //_colorBlendFunction = BlendFunction.Add;
            //_colorDestinationBlend = Blend.Zero;
            //_colorSourceBlend = Blend.One;
            //_colorWriteChannels  = ColorWriteChannels.All;
            //_colorWriteChannels1 = ColorWriteChannels1.All;
            //_colorWriteChannels2 = ColorWriteChannels2.All;
            //_colorWriteChannels3 = ColorWriteChannels3.All;
        }

        internal BlendStateStrategy(IBlendStateStrategy source)
        {
            this._blendFactor = source.BlendFactor;
            this._multiSampleMask = source.MultiSampleMask;

            _independentBlendEnable = source.IndependentBlendEnable;

            //_alphaBlendFunction = source.AlphaBlendFunction;
            //_alphaDestinationBlend = source.AlphaDestinationBlend;
            //_alphaSourceBlend = source.AlphaSourceBlend;
            //_colorBlendFunction = source.ColorBlendFunction;
            //_colorDestinationBlend = source.ColorDestinationBlend;
            //_colorSourceBlend = source.ColorSourceBlend;
            //_colorWriteChannels  = source.ColorWriteChannels;
            //_colorWriteChannels1 = source.ColorWriteChannels1;
            //_colorWriteChannels2 = source.ColorWriteChannels2;
            //_colorWriteChannels3 = source.ColorWriteChannels3;
        }
    }
}
