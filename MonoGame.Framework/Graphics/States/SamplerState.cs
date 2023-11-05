// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;

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

        private readonly bool _isDefaultStateObject;

        private TextureAddressMode _addressU;
        private TextureAddressMode _addressV;
        private TextureAddressMode _addressW;
        private Color _borderColor;
        private TextureFilter _filter;
        private int _maxAnisotropy;
        private int _maxMipLevel;
        private float _mipMapLevelOfDetailBias;
        private TextureFilterMode _filterMode;
        private CompareFunction _comparisonFunction;

        public TextureAddressMode AddressU
        {
            get { return _addressU; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _addressU = value;
            }
        }

        public TextureAddressMode AddressV
        {
            get { return _addressV; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _addressV = value;
            }
        }

        public TextureAddressMode AddressW
        {
            get { return _addressW; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _addressW = value;
            }
        }

        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _borderColor = value;
            }
        }

        public TextureFilter Filter
        {
            get { return _filter; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _filter = value;
            }
        }

        public int MaxAnisotropy
        {
            get { return _maxAnisotropy; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _maxAnisotropy = value;
            }
        }

        public int MaxMipLevel
        {
            get { return _maxMipLevel; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _maxMipLevel = value;
            }
        }

        public float MipMapLevelOfDetailBias
        {
            get { return _mipMapLevelOfDetailBias; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _mipMapLevelOfDetailBias = value;
            }
        }

        /// <summary>
        /// When using comparison sampling, also set <see cref="FilterMode"/> to <see cref="TextureFilterMode.Comparison"/>.
        /// </summary>
        public CompareFunction ComparisonFunction
        {
            get { return _comparisonFunction; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _comparisonFunction = value;
            }
        }

        public TextureFilterMode FilterMode
        {
            get { return _filterMode; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default sampler state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the sampler state after it has been bound to the graphics device!");

                _filterMode = value;
            }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_isDefaultStateObject)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (this.GraphicsDevice != device)
            {
                if (this.GraphicsDevice == null)
                {
                    System.Diagnostics.Debug.Assert(device != null);
                    BindGraphicsDevice(device.Strategy);
                }
                else
                    throw new InvalidOperationException("This sampler state is already bound to a different graphics device.");
            }
        }

        public SamplerState()
            : base()
        {
            _strategy = new SamplerStateStrategy();

            Filter = TextureFilter.Linear;
            AddressU = TextureAddressMode.Wrap;
            AddressV = TextureAddressMode.Wrap;
            AddressW = TextureAddressMode.Wrap;
            BorderColor = Color.White;
            MaxAnisotropy = 4;
            MaxMipLevel = 0;
            MipMapLevelOfDetailBias = 0.0f;
            ComparisonFunction = CompareFunction.Never;
            FilterMode = TextureFilterMode.Default;
        }

        private SamplerState(string name, TextureFilter filter, TextureAddressMode addressMode)
            : this()
        {
            Name = name;
            _filter = filter;
            _addressU = addressMode;
            _addressV = addressMode;
            _addressW = addressMode;
            _isDefaultStateObject = true;
        }

        private SamplerState(SamplerState cloneSource)
        {
            Name = cloneSource.Name;

            _strategy = new SamplerStateStrategy();

            _filter = cloneSource._filter;
            _addressU = cloneSource._addressU;
            _addressV = cloneSource._addressV;
            _addressW = cloneSource._addressW;
            _borderColor = cloneSource._borderColor;
            _maxAnisotropy = cloneSource._maxAnisotropy;
            _maxMipLevel = cloneSource._maxMipLevel;
            _mipMapLevelOfDetailBias = cloneSource._mipMapLevelOfDetailBias;
            _comparisonFunction = cloneSource._comparisonFunction;
            _filterMode = cloneSource._filterMode;
        }

        internal SamplerState Clone()
        {
            return new SamplerState(this);
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
