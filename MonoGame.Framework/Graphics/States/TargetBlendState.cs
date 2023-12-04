// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class TargetBlendState
    {
        private readonly IBlendStateStrategy _parentStrategy;

        private BlendFunction _alphaBlendFunction;
        private Blend _alphaDestinationBlend;
        private Blend _alphaSourceBlend;
        private BlendFunction _colorBlendFunction;
        private Blend _colorDestinationBlend;
        private Blend _colorSourceBlend;
        private ColorWriteChannels _colorWriteChannels;

        internal TargetBlendState(IBlendStateStrategy parentStrategy)
        {
            _parentStrategy = parentStrategy;

            _alphaBlendFunction = BlendFunction.Add;
            _alphaDestinationBlend = Blend.Zero;
            _alphaSourceBlend = Blend.One;
            _colorBlendFunction = BlendFunction.Add;
            _colorDestinationBlend = Blend.Zero;
            _colorSourceBlend = Blend.One;
            _colorWriteChannels = ColorWriteChannels.All;
        }

        internal TargetBlendState(IBlendStateStrategy parentStrategy, Blend sourceBlend, Blend destinationBlend) 
            : this(parentStrategy)
        {
            _colorSourceBlend = sourceBlend;
            _alphaSourceBlend = sourceBlend;
            _colorDestinationBlend = destinationBlend;
            _alphaDestinationBlend = destinationBlend;
        }

        internal TargetBlendState(TargetBlendState source, IBlendStateStrategy parentStrategy)
        {
            _parentStrategy = parentStrategy;

            this._alphaBlendFunction = source.AlphaBlendFunction;
            this._alphaDestinationBlend = source.AlphaDestinationBlend;
            this._alphaSourceBlend = source.AlphaSourceBlend;
            this._colorBlendFunction = source.ColorBlendFunction;
            this._colorDestinationBlend = source.ColorDestinationBlend;
            this._colorSourceBlend = source.ColorSourceBlend;
            this._colorWriteChannels = source.ColorWriteChannels;
        }

        public BlendFunction AlphaBlendFunction
        {
            get { return _alphaBlendFunction; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _alphaBlendFunction = value;
            }
        }

        public Blend AlphaDestinationBlend
        {
            get { return _alphaDestinationBlend; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _alphaDestinationBlend = value;
            }
        }

        public Blend AlphaSourceBlend
        {
            get { return _alphaSourceBlend; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _alphaSourceBlend = value;
            }
        }

        public BlendFunction ColorBlendFunction
        {
            get { return _colorBlendFunction; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _colorBlendFunction = value;
            }
        }

        public Blend ColorDestinationBlend
        {
            get { return _colorDestinationBlend; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _colorDestinationBlend = value;
            }
        }

        public Blend ColorSourceBlend
        {
            get { return _colorSourceBlend; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _colorSourceBlend = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _colorWriteChannels; }
            set
            {
                if (_parentStrategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("The state object is readonly.");
                if (_parentStrategy is ResourceBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify the state after it has been bound to the graphics device!");

                _colorWriteChannels = value;
            }
        }

    }
}

