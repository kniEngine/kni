// Copyright (C)2023 Nick Kastellanos

using System;
using System.Configuration;

namespace Microsoft.Xna.Framework.Graphics
{
    internal class ReadonlySamplerStateStrategy : SamplerStateStrategy
    {
        public override TextureFilter Filter
        {
            get { return base.Filter; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override TextureAddressMode AddressU
        {
            get { return base.AddressU; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override TextureAddressMode AddressV
        {
            get { return base.AddressV; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override TextureAddressMode AddressW
        {
            get { return base.AddressW; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override Color BorderColor

        {
            get { return base.BorderColor; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override int MaxAnisotropy
        {
            get { return base.MaxAnisotropy; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override int MaxMipLevel
        {
            get { return base.MaxMipLevel; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override float MipMapLevelOfDetailBias
        {
            get { return base.MipMapLevelOfDetailBias; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override CompareFunction ComparisonFunction
        {
            get { return base.ComparisonFunction; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public override TextureFilterMode FilterMode
        {
            get { return base.FilterMode; }
            set { throw new InvalidOperationException("The state object is readonly."); }
        }

        public ReadonlySamplerStateStrategy(TextureFilter filter, TextureAddressMode addressMode)
            :base()
        {
            base.Filter = filter;
            base.AddressU = addressMode;
            base.AddressV = addressMode;
            base.AddressW = addressMode;
        }
    }
}
