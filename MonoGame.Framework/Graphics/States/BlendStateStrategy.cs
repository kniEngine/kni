// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class BlendStateStrategy : IBlendStateStrategy
    {
        private Color _blendFactor;
        private int _multiSampleMask;

        private bool _independentBlendEnable;
        private readonly TargetBlendState[] _targetBlendState;

        public virtual bool IndependentBlendEnable
        {
            get { return _independentBlendEnable; }
            set { _independentBlendEnable = value; }
        }

        public TargetBlendState[] Targets
        {
            get { return _targetBlendState; }
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
            get { return _targetBlendState[0].AlphaBlendFunction; }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend AlphaDestinationBlend
        {
            get { return _targetBlendState[0].AlphaDestinationBlend; }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend AlphaSourceBlend
        {
            get { return _targetBlendState[0].AlphaSourceBlend; }
            set { throw new InvalidOperationException(); }
        }

        public virtual BlendFunction ColorBlendFunction
        {
            get { return _targetBlendState[0].ColorBlendFunction; }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend ColorDestinationBlend
        {
            get { return _targetBlendState[0].ColorDestinationBlend; }
            set { throw new InvalidOperationException(); }
        }

        public virtual Blend ColorSourceBlend
        {
            get { return _targetBlendState[0].ColorSourceBlend; }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels
        {
            get { return _targetBlendState[0].ColorWriteChannels; }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels1
        {
            get { return _targetBlendState[1].ColorWriteChannels; }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels2
        {
            get { return _targetBlendState[2].ColorWriteChannels; }
            set { throw new InvalidOperationException(); }
        }

        public virtual ColorWriteChannels ColorWriteChannels3
        {
            get { return _targetBlendState[3].ColorWriteChannels; }
            set { throw new InvalidOperationException(); }
        }

        public BlendStateStrategy()
        {
            _blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;

            _independentBlendEnable = false;
            _targetBlendState = new TargetBlendState[4];

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
            _targetBlendState = new TargetBlendState[4];

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
