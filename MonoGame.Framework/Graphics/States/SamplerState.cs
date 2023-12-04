// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class SamplerState : GraphicsResource
    {
        internal ISamplerStateStrategy _strategy;

        public static readonly SamplerState AnisotropicClamp;
        public static readonly SamplerState AnisotropicWrap;
        public static readonly SamplerState LinearClamp;
        public static readonly SamplerState LinearWrap;
        public static readonly SamplerState PointClamp;
        public static readonly SamplerState PointWrap;

        static SamplerState()
        {
            AnisotropicClamp = new SamplerState("SamplerState.AnisotropicClamp", TextureFilter.Anisotropic, TextureAddressMode.Clamp);
            AnisotropicWrap = new SamplerState("SamplerState.AnisotropicWrap", TextureFilter.Anisotropic, TextureAddressMode.Wrap);
            LinearClamp = new SamplerState("SamplerState.LinearClamp", TextureFilter.Linear, TextureAddressMode.Clamp);
            LinearWrap = new SamplerState("SamplerState.LinearWrap", TextureFilter.Linear, TextureAddressMode.Wrap);
            PointClamp = new SamplerState("SamplerState.PointClamp", TextureFilter.Point, TextureAddressMode.Clamp);
            PointWrap = new SamplerState("SamplerState.PointWrap", TextureFilter.Point, TextureAddressMode.Wrap);
        }


        public TextureAddressMode AddressU
        {
            get { return _strategy.AddressU; }
            set { _strategy.AddressU = value; }
        }

        public TextureAddressMode AddressV
        {
            get { return _strategy.AddressV; }
            set { _strategy.AddressV = value; }
        }

        public TextureAddressMode AddressW
        {
            get { return _strategy.AddressW; }
            set { _strategy.AddressW = value; }
        }

        public Color BorderColor
        {
            get { return _strategy.BorderColor; }
            set { _strategy.BorderColor = value; }
        }

        public TextureFilter Filter
        {
            get { return _strategy.Filter; }
            set { _strategy.Filter = value; }
        }

        public int MaxAnisotropy
        {
            get { return _strategy.MaxAnisotropy; }
            set { _strategy.MaxAnisotropy = value; }
        }

        public int MaxMipLevel
        {
            get { return _strategy.MaxMipLevel; }
            set { _strategy.MaxMipLevel = value; }
        }

        public float MipMapLevelOfDetailBias
        {
            get { return _strategy.MipMapLevelOfDetailBias; }
            set { _strategy.MipMapLevelOfDetailBias = value; }
        }

        /// <summary>
        /// When using comparison sampling, also set <see cref="FilterMode"/> to <see cref="TextureFilterMode.Comparison"/>.
        /// </summary>
        public CompareFunction ComparisonFunction
        {
            get { return _strategy.ComparisonFunction; }
            set { _strategy.ComparisonFunction = value; }
        }

        public TextureFilterMode FilterMode
        {
            get { return _strategy.FilterMode; }
            set { _strategy.FilterMode = value; }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_strategy is ReadonlySamplerStateStrategy)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (this.GraphicsDevice != device)
            {
                if (this.GraphicsDevice == null)
                {
                    System.Diagnostics.Debug.Assert(device != null);

                    _strategy = new ResourceSamplerStateStrategy(_strategy);
                    GraphicsResourceStrategy resourceStrategy = (GraphicsResourceStrategy)_strategy;
                    resourceStrategy.BindGraphicsDevice(device.Strategy);
                    SetResourceStrategy(resourceStrategy);
                }
                else
                    throw new InvalidOperationException("This sampler state is already bound to a different graphics device.");
            }
        }

        public SamplerState()
            : base()
        {
            _strategy = new SamplerStateStrategy();
        }

        private SamplerState(string name, TextureFilter filter, TextureAddressMode addressMode)
            : base()
        {
            Name = name;
            _strategy = new ReadonlySamplerStateStrategy(filter, addressMode);
        }

        internal SamplerState(SamplerState source)
            : base()
        {
            Name = source.Name;
            _strategy = new SamplerStateStrategy(source._strategy);
        }

        partial void PlatformDispose(bool disposing);

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            PlatformDispose(disposing);
            base.Dispose(disposing);
        }
    }
}
