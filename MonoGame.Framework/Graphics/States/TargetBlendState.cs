// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
#if DIRECTX
using D3D11 = SharpDX.Direct3D11;
#endif

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
            AlphaBlendFunction = BlendFunction.Add;
            AlphaDestinationBlend = Blend.Zero;
            AlphaSourceBlend = Blend.One;
            ColorBlendFunction = BlendFunction.Add;
            ColorDestinationBlend = Blend.Zero;
            ColorSourceBlend = Blend.One;
            ColorWriteChannels = ColorWriteChannels.All;
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
                if (_parent._isDefaultStateObject)
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
                if (_parent._isDefaultStateObject)
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
                if (_parent._isDefaultStateObject)
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
                if (_parent._isDefaultStateObject)
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
                if (_parent._isDefaultStateObject)
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
                if (_parent._isDefaultStateObject)
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
                if (_parent._isDefaultStateObject)
                    throw new InvalidOperationException("You cannot modify a default blend state object.");
                if (_parent.GraphicsDevice != null)
                    throw new InvalidOperationException("You cannot modify the blend state after it has been bound to the graphics device!");

                _colorWriteChannels = value;
            }
        }

#if DIRECTX

        internal void GetState(ref D3D11.RenderTargetBlendDescription desc)
        {
            // We're blending if we're not in the opaque state.
            desc.IsBlendEnabled =   !(  ColorSourceBlend == Blend.One &&
                                        ColorDestinationBlend == Blend.Zero &&
                                        AlphaSourceBlend == Blend.One &&
                                        AlphaDestinationBlend == Blend.Zero);

            desc.BlendOperation = GetBlendOperation(ColorBlendFunction);
            desc.SourceBlend = GetBlendOption(ColorSourceBlend, false);
            desc.DestinationBlend = GetBlendOption(ColorDestinationBlend, false);

            desc.AlphaBlendOperation = GetBlendOperation(AlphaBlendFunction);
            desc.SourceAlphaBlend = GetBlendOption(AlphaSourceBlend, true);
            desc.DestinationAlphaBlend = GetBlendOption(AlphaDestinationBlend, true);

            desc.RenderTargetWriteMask = GetColorWriteMask(ColorWriteChannels);
        }

        static private D3D11.BlendOperation GetBlendOperation(BlendFunction blend)
        {
            switch (blend)
            {
                case BlendFunction.Add:
                    return D3D11.BlendOperation.Add;

                case BlendFunction.Max:
                    return D3D11.BlendOperation.Maximum;

                case BlendFunction.Min:
                    return D3D11.BlendOperation.Minimum;

                case BlendFunction.ReverseSubtract:
                    return D3D11.BlendOperation.ReverseSubtract;

                case BlendFunction.Subtract:
                    return D3D11.BlendOperation.Subtract;

                default:
                    throw new ArgumentException("Invalid blend function!");
            }
        }

        static private D3D11.BlendOption GetBlendOption(Blend blend, bool alpha)
        {
            switch (blend)
            {
                case Blend.BlendFactor:
                    return D3D11.BlendOption.BlendFactor;

                case Blend.DestinationAlpha:
                    return D3D11.BlendOption.DestinationAlpha;

                case Blend.DestinationColor:
                    return alpha ? D3D11.BlendOption.DestinationAlpha : D3D11.BlendOption.DestinationColor;

                case Blend.InverseBlendFactor:
                    return D3D11.BlendOption.InverseBlendFactor;

                case Blend.InverseDestinationAlpha:
                    return D3D11.BlendOption.InverseDestinationAlpha;

                case Blend.InverseDestinationColor:
                    return alpha ? D3D11.BlendOption.InverseDestinationAlpha : D3D11.BlendOption.InverseDestinationColor;

                case Blend.InverseSourceAlpha:
                    return D3D11.BlendOption.InverseSourceAlpha;

                case Blend.InverseSourceColor:
                    return alpha ? D3D11.BlendOption.InverseSourceAlpha : D3D11.BlendOption.InverseSourceColor;

                case Blend.One:
                    return D3D11.BlendOption.One;

                case Blend.SourceAlpha:
                    return D3D11.BlendOption.SourceAlpha;

                case Blend.SourceAlphaSaturation:
                    return D3D11.BlendOption.SourceAlphaSaturate;

                case Blend.SourceColor:
                    return alpha ? D3D11.BlendOption.SourceAlpha : D3D11.BlendOption.SourceColor;

                case Blend.Zero:
                    return D3D11.BlendOption.Zero;

                default:
                    throw new ArgumentException("Invalid blend!");
            }
        }

        static private D3D11.ColorWriteMaskFlags GetColorWriteMask(ColorWriteChannels mask)
        {
            return  ((mask & ColorWriteChannels.Red) != 0 ? D3D11.ColorWriteMaskFlags.Red : 0) |
                    ((mask & ColorWriteChannels.Green) != 0 ? D3D11.ColorWriteMaskFlags.Green : 0) |
                    ((mask & ColorWriteChannels.Blue) != 0 ? D3D11.ColorWriteMaskFlags.Blue : 0) |
                    ((mask & ColorWriteChannels.Alpha) != 0 ? D3D11.ColorWriteMaskFlags.Alpha : 0);
        }
#endif

    }
}

