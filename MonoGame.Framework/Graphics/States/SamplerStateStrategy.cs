// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class SamplerStateStrategy : ISamplerStateStrategy
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
        {
        }
    }
}
