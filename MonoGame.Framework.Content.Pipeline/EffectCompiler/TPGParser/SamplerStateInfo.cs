// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content.Pipeline.EffectCompiler.TPGParser
{
    public class SamplerStateInfo
    {
        private SamplerState _state;
        
        private bool _dirty;

        private TextureFilterTypeContent _minFilter;
        private TextureFilterTypeContent _magFilter;
        private TextureFilterTypeContent _mipFilter;

        private TextureAddressModeContent _addressU;
        private TextureAddressModeContent _addressV;
        private TextureAddressModeContent _addressW;

        private Color _borderColor;

        private int _maxAnisotropy;
        private int _maxMipLevel;
        private float _mipMapLevelOfDetailBias;

        public SamplerStateInfo()
        {
            // NOTE: These match the defaults of SamplerState.
            _minFilter = TextureFilterTypeContent.Linear;
            _magFilter = TextureFilterTypeContent.Linear;
            _mipFilter = TextureFilterTypeContent.Linear;
            _addressU = TextureAddressModeContent.Wrap;
            _addressV = TextureAddressModeContent.Wrap;
            _addressW = TextureAddressModeContent.Wrap;
            _borderColor = Color.White;
            _maxAnisotropy = 4;
            _maxMipLevel = 0;
            _mipMapLevelOfDetailBias = 0.0f;
        }

        public string Name { get; set; }

        public string TextureName { get; set; }

        public TextureFilterTypeContent MinFilter
        {
            set
            {
                _minFilter = value;
                _dirty = true;
            }
        }

        public TextureFilterTypeContent MagFilter
        {
            set
            {
                _magFilter = value;
                _dirty = true;
            }
        }

        public TextureFilterTypeContent MipFilter
        {
            set
            {
                _mipFilter = value;
                _dirty = true;
            }
        }

        public TextureFilterTypeContent Filter
        {
            set
            {
                _minFilter = _magFilter = _mipFilter = value;
                _dirty = true;
            }
        }

        public TextureAddressModeContent AddressU
        {
            set
            {
                _addressU = value;
                _dirty = true;
            }
        }

        public TextureAddressModeContent AddressV
        {
            set
            {
                _addressV = value;
                _dirty = true;
            }
        }

        public TextureAddressModeContent AddressW
        {
            set
            {
                _addressW = value;
                _dirty = true;
            }
        }

        public Color BorderColor
        {
            set
            {
                _borderColor = value;
                _dirty = true;
            }
        }

        public int MaxAnisotropy
        {
            set
            {
                _maxAnisotropy = value;
                _dirty = true;
            }
        }

        public int MaxMipLevel
        {
            set
            {
                _maxMipLevel = value;
                _dirty = true;
            }
        }

        public float MipMapLevelOfDetailBias
        {
            set
            {
                _mipMapLevelOfDetailBias = value;
                _dirty = true;
            }
        }

        private void UpdateSamplerState()
        {
            // Get the existing state or create it.
            if (_state == null)
                _state = new SamplerState();

            _state.AddressU = EffectObject.ToXNATextureAddressMode(_addressU);
            _state.AddressV = EffectObject.ToXNATextureAddressMode(_addressV);
            _state.AddressW = EffectObject.ToXNATextureAddressMode(_addressW);

            _state.BorderColor = _borderColor;

            _state.MaxAnisotropy = _maxAnisotropy;
            _state.MaxMipLevel = _maxMipLevel;
            _state.MipMapLevelOfDetailBias = _mipMapLevelOfDetailBias;

            // Figure out what kind of filter to set based on each
            // individual min, mag, and mip filter settings.
            //
            // NOTE: We're treating "None" and "Point" the same here
            // and disabling mipmapping further below.
            //
            if (_minFilter == TextureFilterTypeContent.Anisotropic)
                _state.Filter = TextureFilter.Anisotropic;
            else if (_minFilter == TextureFilterTypeContent.Linear && _magFilter == TextureFilterTypeContent.Linear && _mipFilter == TextureFilterTypeContent.Linear)
                _state.Filter = TextureFilter.Linear;
            else if (_minFilter == TextureFilterTypeContent.Linear && _magFilter == TextureFilterTypeContent.Linear && _mipFilter <= TextureFilterTypeContent.Point)
                _state.Filter = TextureFilter.LinearMipPoint;
            else if (_minFilter == TextureFilterTypeContent.Linear && _magFilter <= TextureFilterTypeContent.Point && _mipFilter == TextureFilterTypeContent.Linear)
                _state.Filter = TextureFilter.MinLinearMagPointMipLinear;
            else if (_minFilter == TextureFilterTypeContent.Linear && _magFilter <= TextureFilterTypeContent.Point && _mipFilter <= TextureFilterTypeContent.Point)
                _state.Filter = TextureFilter.MinLinearMagPointMipPoint;
            else if (_minFilter <= TextureFilterTypeContent.Point && _magFilter == TextureFilterTypeContent.Linear && _mipFilter == TextureFilterTypeContent.Linear)
                _state.Filter = TextureFilter.MinPointMagLinearMipLinear;
            else if (_minFilter <= TextureFilterTypeContent.Point && _magFilter == TextureFilterTypeContent.Linear && _mipFilter <= TextureFilterTypeContent.Point)
                _state.Filter = TextureFilter.MinPointMagLinearMipPoint;
            else if (_minFilter <= TextureFilterTypeContent.Point && _magFilter <= TextureFilterTypeContent.Point && _mipFilter <= TextureFilterTypeContent.Point)
                _state.Filter = TextureFilter.Point;
            else if (_minFilter <= TextureFilterTypeContent.Point && _magFilter <= TextureFilterTypeContent.Point && _mipFilter == TextureFilterTypeContent.Linear)
                _state.Filter = TextureFilter.PointMipLinear;

            // Do we need to disable mipmapping?
            if (_mipFilter == TextureFilterTypeContent.None)
            {
                // TODO: This is the only option we have right now for 
                // disabling mipmapping.  We should add support for MinLod
                // and MaxLod which potentially does a better job at this.
                _state.MipMapLevelOfDetailBias = -16.0f;
                _state.MaxMipLevel = 0;
            }

            _dirty = false;
        }
        
        public SamplerState State
        {
            get
            {
                if (_dirty)
                    UpdateSamplerState();

                return _state;
            }
        }
    }
}
