// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public class TargetBlendState
    {
        private readonly BlendState _parent;
        private BlendFunction _alphaBlendFunction;
        private Blend _alphaDestinationBlend;
        private Blend _alphaSourceBlend;
        private BlendFunction _colorBlendFunction;
        private Blend _colorDestinationBlend;
        private Blend _colorSourceBlend;
        private ColorWriteChannels _colorWriteChannels;

        internal TargetBlendState(BlendState parent)
        {
            _parent = parent;
            _alphaBlendFunction = BlendFunction.Add;
            _alphaDestinationBlend = Blend.Zero;
            _alphaSourceBlend = Blend.One;
            _colorBlendFunction = BlendFunction.Add;
            _colorDestinationBlend = Blend.Zero;
            _colorSourceBlend = Blend.One;
            _colorWriteChannels = ColorWriteChannels.All;
        }

        public TargetBlendState(BlendState parent, Blend sourceBlend, Blend destinationBlend) 
            : this(parent)
        {
            _colorSourceBlend = sourceBlend;
            _alphaSourceBlend = sourceBlend;
            _colorDestinationBlend = destinationBlend;
            _alphaDestinationBlend = destinationBlend;
        }

        internal TargetBlendState Clone(BlendState parent)
        {
            return new TargetBlendState(parent)
            {
                AlphaBlendFunction = AlphaBlendFunction,
                AlphaDestinationBlend = AlphaDestinationBlend,
                AlphaSourceBlend = AlphaSourceBlend,
                ColorBlendFunction = ColorBlendFunction,
                ColorDestinationBlend = ColorDestinationBlend,
                ColorSourceBlend = ColorSourceBlend,
                ColorWriteChannels = ColorWriteChannels
            };
        }

        public BlendFunction AlphaBlendFunction
        {
            get { return _alphaBlendFunction; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _alphaBlendFunction = value;
            }
        }

        public Blend AlphaDestinationBlend
        {
            get { return _alphaDestinationBlend; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _alphaDestinationBlend = value;
            }
        }

        public Blend AlphaSourceBlend
        {
            get { return _alphaSourceBlend; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _alphaSourceBlend = value;
            }
        }

        public BlendFunction ColorBlendFunction
        {
            get { return _colorBlendFunction; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _colorBlendFunction = value;
            }
        }

        public Blend ColorDestinationBlend
        {
            get { return _colorDestinationBlend; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _colorDestinationBlend = value;
            }
        }

        public Blend ColorSourceBlend
        {
            get { return _colorSourceBlend; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _colorSourceBlend = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _colorWriteChannels; }
            set
            {
                if (_parent._strategy is ReadonlyBlendStateStrategy)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _colorWriteChannels = value;
            }
        }

    }
}

