// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.
//
// Author: Kenneth James Pouncey

// Copyright (C)2023 Nick Kastellanos

using System;


namespace Microsoft.Xna.Framework.Graphics
{
    public sealed partial class SamplerStateCollection
	{
        private readonly GraphicsDevice _device;
        private readonly ShaderStage _stage = ShaderStage.Pixel;

        private readonly SamplerState _samplerStateAnisotropicClamp;
        private readonly SamplerState _samplerStateAnisotropicWrap;
        private readonly SamplerState _samplerStateLinearClamp;
        private readonly SamplerState _samplerStateLinearWrap;
        private readonly SamplerState _samplerStatePointClamp;
        private readonly SamplerState _samplerStatePointWrap;

        private readonly SamplerState[] _samplers;
        private readonly SamplerState[] _actualSamplers;


        internal SamplerStateCollection(GraphicsDevice device, ShaderStage stage, int capacity)
        {
            // hard limit of 32 because of _d3dDirty flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

            _device = device;
            _stage = stage;

            _samplerStateAnisotropicClamp = SamplerState.AnisotropicClamp.Clone();
            _samplerStateAnisotropicWrap = SamplerState.AnisotropicWrap.Clone();
            _samplerStateLinearClamp = SamplerState.LinearClamp.Clone();
            _samplerStateLinearWrap = SamplerState.LinearWrap.Clone();
            _samplerStatePointClamp = SamplerState.PointClamp.Clone();
            _samplerStatePointWrap = SamplerState.PointWrap.Clone();

            _samplers = new SamplerState[capacity];
            _actualSamplers = new SamplerState[capacity];

            Clear();
        }
		
		public SamplerState this[int index] 
        {
			get { return _samplers[index]; }
			set
            {
                if (value == null)
                    throw new ArgumentNullException("value");

                if (_samplers[index] == value)
                    return;

                _samplers[index] = value;

                // Static state properties never actually get bound;
                // instead we use our GraphicsDevice-specific version of them.
                var newSamplerState = value;
                if (ReferenceEquals(value, SamplerState.AnisotropicClamp))
                    newSamplerState = _samplerStateAnisotropicClamp;
                else if (ReferenceEquals(value, SamplerState.AnisotropicWrap))
                    newSamplerState = _samplerStateAnisotropicWrap;
                else if (ReferenceEquals(value, SamplerState.LinearClamp))
                    newSamplerState = _samplerStateLinearClamp;
                else if (ReferenceEquals(value, SamplerState.LinearWrap))
                    newSamplerState = _samplerStateLinearWrap;
                else if (ReferenceEquals(value, SamplerState.PointClamp))
                    newSamplerState = _samplerStatePointClamp;
                else if (ReferenceEquals(value, SamplerState.PointWrap))
                    newSamplerState = _samplerStatePointWrap;

                newSamplerState.BindToGraphicsDevice(_device);

                _actualSamplers[index] = newSamplerState;

                PlatformSetSamplerState(index);
            }
		}

        internal void Clear()
        {
            for (var i = 0; i < _samplers.Length; i++)
            {
                _samplers[i] = SamplerState.LinearWrap;

                _samplerStateLinearWrap.BindToGraphicsDevice(_device);
                _actualSamplers[i] = _samplerStateLinearWrap;
            }

            PlatformClear();
        }

        /// <summary>
        /// Mark all the sampler slots as dirty.
        /// </summary>
        internal void Dirty()
        {
            PlatformDirty();
        }
    }
}
