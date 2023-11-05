// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    public partial class BlendState : GraphicsResource
    {

        public static readonly BlendState Additive;
        public static readonly BlendState AlphaBlend;
        public static readonly BlendState NonPremultiplied;
        public static readonly BlendState Opaque;

        static BlendState()
        {
            Additive = new BlendState("BlendState.Additive", Blend.SourceAlpha, Blend.One);
            AlphaBlend = new BlendState("BlendState.AlphaBlend", Blend.One, Blend.InverseSourceAlpha);
            NonPremultiplied = new BlendState("BlendState.NonPremultiplied", Blend.SourceAlpha, Blend.InverseSourceAlpha);
            Opaque = new BlendState("BlendState.Opaque", Blend.One, Blend.Zero);
        }

        internal readonly bool _isDefaultStateObject;

        private readonly TargetBlendState[] _targetBlendState;

        private Color _blendFactor;

        private int _multiSampleMask;

        private bool _independentBlendEnable;

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
                    throw new InvalidOperationException("This blend state is already bound to a different graphics device.");
            }
        }

        /// <summary>
        /// Returns the target specific blend state.
        /// </summary>
        /// <param name="index">The 0 to 3 target blend state index.</param>
        /// <returns>A target blend state.</returns>
        public TargetBlendState this[int index]
        {
            get { return _targetBlendState[index]; }
        }

        public BlendFunction AlphaBlendFunction
        {
            get { return _targetBlendState[0].AlphaBlendFunction; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].AlphaBlendFunction = value;
            }
        }

        public Blend AlphaDestinationBlend
        {
            get { return _targetBlendState[0].AlphaDestinationBlend; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].AlphaDestinationBlend = value;
            }
        }

        public Blend AlphaSourceBlend
        {
            get { return _targetBlendState[0].AlphaSourceBlend; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].AlphaSourceBlend = value;
            }
        }

        public BlendFunction ColorBlendFunction
        {
            get { return _targetBlendState[0].ColorBlendFunction; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].ColorBlendFunction = value;
            }
        }

        public Blend ColorDestinationBlend
        {
            get { return _targetBlendState[0].ColorDestinationBlend; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].ColorDestinationBlend = value;
            }
        }

        public Blend ColorSourceBlend
        {
            get { return _targetBlendState[0].ColorSourceBlend; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].ColorSourceBlend = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _targetBlendState[0].ColorWriteChannels; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[0].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels1
        {
            get { return _targetBlendState[1].ColorWriteChannels; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[1].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels2
        {
            get { return _targetBlendState[2].ColorWriteChannels; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[2].ColorWriteChannels = value;
            }
        }

        public ColorWriteChannels ColorWriteChannels3
        {
            get { return _targetBlendState[3].ColorWriteChannels; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _targetBlendState[3].ColorWriteChannels = value;
            }
        }

        /// <summary>
        /// The color used as blend factor when alpha blending.
        /// </summary>
        /// <remarks>
        /// <see cref="P:Microsoft.Xna.Framework.Graphics.GraphicsDevice.BlendFactor"/> is set to this value when this <see cref="BlendState"/>
        /// is bound to a GraphicsDevice.
        /// </remarks>
        public Color BlendFactor
        {
            get { return _blendFactor; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _blendFactor = value;
            }
        }

        public int MultiSampleMask
        {
            get { return _multiSampleMask; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _multiSampleMask = value;
            }
        }

        /// <summary>
        /// Enables use of the per-target blend states.
        /// </summary>
        public bool IndependentBlendEnable
        {
            get { return _independentBlendEnable; }
            set
            {
                if (_isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _independentBlendEnable = value;
            }
        }

        public BlendState()
            : base()
        {
            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = new TargetBlendState(this);
            _targetBlendState[1] = new TargetBlendState(this);
            _targetBlendState[2] = new TargetBlendState(this);
            _targetBlendState[3] = new TargetBlendState(this);

            _blendFactor = Color.White;
            _multiSampleMask = Int32.MaxValue;
            _independentBlendEnable = false;
        }

        private BlendState(string name, Blend sourceBlend, Blend destinationBlend)
            : this()
        {
            Name = name;
            ColorSourceBlend = sourceBlend;
            AlphaSourceBlend = sourceBlend;
            ColorDestinationBlend = destinationBlend;
            AlphaDestinationBlend = destinationBlend;
            _isDefaultStateObject = true;
        }

        private BlendState(BlendState cloneSource)
        {
            Name = cloneSource.Name;

            _targetBlendState = new TargetBlendState[4];
            _targetBlendState[0] = cloneSource[0].Clone(this);
            _targetBlendState[1] = cloneSource[1].Clone(this);
            _targetBlendState[2] = cloneSource[2].Clone(this);
            _targetBlendState[3] = cloneSource[3].Clone(this);

            _blendFactor = cloneSource._blendFactor;
            _multiSampleMask = cloneSource._multiSampleMask;
            _independentBlendEnable = cloneSource._independentBlendEnable;
        }

        internal BlendState Clone()
        {
            return new BlendState(this);
        }

        partial void PlatformDispose(bool disposing);

        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            for (int i = 0; i < _targetBlendState.Length; i++)
                _targetBlendState[i] = null;

            PlatformDispose(disposing);
            base.Dispose(disposing);
        }
    }
}

