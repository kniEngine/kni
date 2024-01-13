// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformSamplerStateCollection
    {
        SamplerStateCollectionStrategy Strategy { get; }
    }

    public abstract class SamplerStateCollectionStrategy
    {
        protected readonly GraphicsContextStrategy _contextStrategy;

        private readonly SamplerState _samplerStateAnisotropicClamp;
        private readonly SamplerState _samplerStateAnisotropicWrap;
        private readonly SamplerState _samplerStateLinearClamp;
        private readonly SamplerState _samplerStateLinearWrap;
        private readonly SamplerState _samplerStatePointClamp;
        private readonly SamplerState _samplerStatePointWrap;

        private readonly SamplerState[] _samplers;
        protected internal readonly SamplerState[] _actualSamplers;


        internal SamplerStateCollectionStrategy(GraphicsContextStrategy contextStrategy, int capacity)
        {
            // hard limit of 32 because of _d3dDirty flags being 32bits.
            if (capacity > 32)
                throw new ArgumentOutOfRangeException("capacity");

            _contextStrategy = contextStrategy;

            _samplerStateAnisotropicClamp = new SamplerState(SamplerState.AnisotropicClamp);
            _samplerStateAnisotropicWrap = new SamplerState(SamplerState.AnisotropicWrap);
            _samplerStateLinearClamp = new SamplerState(SamplerState.LinearClamp);
            _samplerStateLinearWrap = new SamplerState(SamplerState.LinearWrap);
            _samplerStatePointClamp = new SamplerState(SamplerState.PointClamp);
            _samplerStatePointWrap = new SamplerState(SamplerState.PointWrap);

            _samplers = new SamplerState[capacity];
            _actualSamplers = new SamplerState[capacity];

        }

        public virtual SamplerState this[int index]
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
                SamplerState newSamplerState = value;
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

                newSamplerState.BindToGraphicsDevice(((IPlatformGraphicsContext)_contextStrategy.Context).DeviceStrategy);

                _actualSamplers[index] = newSamplerState;
            }
        }

        public virtual void Clear()
        {
            for (int i = 0; i < _samplers.Length; i++)
            {
                _samplers[i] = SamplerState.LinearWrap;

                _samplerStateLinearWrap.BindToGraphicsDevice(((IPlatformGraphicsContext)_contextStrategy.Context).DeviceStrategy);
                _actualSamplers[i] = _samplerStateLinearWrap;
            }
        }

        /// <summary>
        /// Mark all the sampler slots as dirty.
        /// </summary>
        public virtual void Dirty()
        {
        }

        public T ToConcrete<T>() where T : SamplerStateCollectionStrategy
        {
            return (T)this;
        }

    }
}
