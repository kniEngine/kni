// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public class BlendState : GraphicsResource
    {
        internal IBlendStateStrategy _strategy;

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

        internal T GetStrategy<T>() where T : IBlendStateStrategy
        {
            return (T)_strategy;
        }


        /// <summary>
        /// Enables use of the per-target blend states.
        /// </summary>
        public bool IndependentBlendEnable
        {
            get { return _strategy.IndependentBlendEnable; }
            set {  _strategy.IndependentBlendEnable = value; }
        }

        /// <summary>
        /// Returns the target specific blend state.
        /// </summary>
        /// <param name="index">The 0 to 3 target blend state index.</param>
        /// <returns>A target blend state.</returns>
        public TargetBlendState this[int index]
        {
            get { return _strategy.Targets[index]; }
        }

        public BlendFunction AlphaBlendFunction
        {
            get { return _strategy.AlphaBlendFunction; }
            set { _strategy.AlphaBlendFunction = value; }
        }

        public Blend AlphaDestinationBlend
        {
            get { return _strategy.AlphaDestinationBlend; }
            set { _strategy.AlphaDestinationBlend = value; }
        }

        public Blend AlphaSourceBlend
        {
            get { return _strategy.AlphaSourceBlend; }
            set { _strategy.AlphaSourceBlend = value; }
        }

        public BlendFunction ColorBlendFunction
        {
            get { return _strategy.ColorBlendFunction; }
            set { _strategy.ColorBlendFunction = value; }
        }

        public Blend ColorDestinationBlend
        {
            get { return _strategy.ColorDestinationBlend; }
            set { _strategy.ColorDestinationBlend = value; }
        }

        public Blend ColorSourceBlend
        {
            get { return _strategy.ColorSourceBlend; }
            set { _strategy.ColorSourceBlend = value; }
        }

        public ColorWriteChannels ColorWriteChannels
        {
            get { return _strategy.ColorWriteChannels; }
            set { _strategy.ColorWriteChannels = value; }
        }

        public ColorWriteChannels ColorWriteChannels1
        {
            get { return _strategy.ColorWriteChannels1; }
            set { _strategy.ColorWriteChannels1 = value; }
        }

        public ColorWriteChannels ColorWriteChannels2
        {
            get { return _strategy.ColorWriteChannels2; }
            set { _strategy.ColorWriteChannels2 = value; }
        }

        public ColorWriteChannels ColorWriteChannels3
        {
            get { return _strategy.ColorWriteChannels3; }
            set { _strategy.ColorWriteChannels3 = value; }
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
            get { return _strategy.BlendFactor; }
            set { _strategy.BlendFactor = value; }
        }

        public int MultiSampleMask
        {
            get { return _strategy.MultiSampleMask; }
            set { _strategy.MultiSampleMask = value; }
        }

        internal void BindToGraphicsDevice(GraphicsDevice device)
        {
            if (_strategy is ReadonlyBlendStateStrategy)
                throw new InvalidOperationException("You cannot bind a default state object.");

            if (this.GraphicsDevice != device)
            {
                if (this.GraphicsDevice == null)
                {
                    System.Diagnostics.Debug.Assert(device != null);

                    _strategy = device.CurrentContext.Strategy.CreateBlendStateStrategy(_strategy);
                    SetResourceStrategy((IGraphicsResourceStrategy)_strategy);
                }
                else
                    throw new InvalidOperationException("This blend state is already bound to a different graphics device.");
            }
        }

        public BlendState()
            : base()
        {
            _strategy = new BlendStateStrategy();
        }

        private BlendState(string name, Blend sourceBlend, Blend destinationBlend)
            : base()
        {
            Name = name;
            _strategy = new ReadonlyBlendStateStrategy(sourceBlend, destinationBlend);
        }

        internal BlendState(BlendState source)
            : base()
        {
            Name = source.Name;
            _strategy = new BlendStateStrategy(source._strategy);
        }


        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
            }

            for (int i = 0; i < _strategy.Targets.Length; i++)
                _strategy.Targets[i] = null;

            base.Dispose(disposing);
        }
    }
}

