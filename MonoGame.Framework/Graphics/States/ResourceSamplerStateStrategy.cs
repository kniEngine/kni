// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    internal class ResourceSamplerStateStrategy : SamplerStateStrategy
    {
        public override TextureFilter Filter
        {
            get { return base.Filter; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override TextureAddressMode AddressU
        {
            get { return base.AddressU; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override TextureAddressMode AddressV
        {
            get { return base.AddressV; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override TextureAddressMode AddressW
        {
            get { return base.AddressW; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override Color BorderColor

        {
            get { return base.BorderColor; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override int MaxAnisotropy
        {
            get { return base.MaxAnisotropy; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override int MaxMipLevel
        {
            get { return base.MaxMipLevel; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override float MipMapLevelOfDetailBias
        {
            get { return base.MipMapLevelOfDetailBias; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override CompareFunction ComparisonFunction
        {
            get { return base.ComparisonFunction; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public override TextureFilterMode FilterMode
        {
            get { return base.FilterMode; }
            set { throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device."); }
        }

        public ResourceSamplerStateStrategy(GraphicsContextStrategy contextStrategy, ISamplerStateStrategy source) 
            : base(contextStrategy, source)
        {
        }
    }
}
