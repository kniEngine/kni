// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    internal class SamplerStateStrategy : GraphicsResourceStrategy
        , ISamplerStateStrategy
    {
        private TextureFilter _filter;
        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;
        private Color _borderColor;
        private int _maxAnisotropy;
        private int _maxMipLevel;
        private float _mipMapLevelOfDetailBias;
        private TextureFilterMode _filterMode;
        private CompareFunction _comparisonFunction;

        public virtual TextureFilter Filter
        {
            get { return _filter; }
            set { _filter = value; }
        }

        public virtual TextureAddressMode AddressU
        {
            get { return _addressU; }
            set { _addressU = value; }
        }

        public virtual TextureAddressMode AddressV
        {
            get { return _addressV; }
            set { _addressV = value; }
        }

        public virtual TextureAddressMode AddressW
        {
            get { return _addressW; }
            set { _addressW = value; }
        }

        public virtual Color BorderColor

        {
            get { return _borderColor; }
            set { _borderColor = value; }
        }

        public virtual int MaxAnisotropy
        {
            get { return _maxAnisotropy; }
            set { _maxAnisotropy = value; }
        }
        public virtual int MaxMipLevel
        {
            get { return _maxMipLevel; }
            set { _maxMipLevel = value; }
        }

        public virtual float MipMapLevelOfDetailBias
        {
            get { return _mipMapLevelOfDetailBias; }
            set { _mipMapLevelOfDetailBias = value; }
        }

        public virtual CompareFunction ComparisonFunction
        {
            get { return _comparisonFunction; }
            set { _comparisonFunction = value; }
        }
        public virtual TextureFilterMode FilterMode
        {
            get { return _filterMode; }
            set { _filterMode = value; }
        }

        public SamplerStateStrategy()
            : base()
        {
            _filter = TextureFilter.Linear;
            _addressU = TextureAddressMode.Wrap;
            _addressV = TextureAddressMode.Wrap;
            _addressW = TextureAddressMode.Wrap;
            _borderColor = Color.White;
            _maxAnisotropy = 4;
            _maxMipLevel = 0;
            _mipMapLevelOfDetailBias = 0.0f;
            _comparisonFunction = CompareFunction.Never;
            _filterMode = TextureFilterMode.Default;
        }
        
        internal SamplerStateStrategy(GraphicsContextStrategy contextStrategy, ISamplerStateStrategy source)
            : base(contextStrategy)
        {
            _filter = source.Filter;
            _addressU = source.AddressU;
            _addressV = source.AddressV;
            _addressW = source.AddressW;
            _borderColor = source.BorderColor;
            _maxAnisotropy = source.MaxAnisotropy;
            _maxMipLevel = source.MaxMipLevel;
            _mipMapLevelOfDetailBias = source.MipMapLevelOfDetailBias;
            _comparisonFunction = source.ComparisonFunction;
            _filterMode = source.FilterMode;
        }

        internal SamplerStateStrategy(ISamplerStateStrategy source)
            : base()
        {
            _filter = source.Filter;
            _addressU = source.AddressU;
            _addressV = source.AddressV;
            _addressW = source.AddressW;
            _borderColor = source.BorderColor;
            _maxAnisotropy = source.MaxAnisotropy;
            _maxMipLevel = source.MaxMipLevel;
            _mipMapLevelOfDetailBias = source.MipMapLevelOfDetailBias;
            _comparisonFunction = source.ComparisonFunction;
            _filterMode = source.FilterMode;
        }
    }
}
