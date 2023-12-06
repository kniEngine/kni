// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Platform.Graphics
{
    internal class ConcreteBlendState : ResourceBlendStateStrategy
    {
        private D3D11.BlendState _state;

        internal ConcreteBlendState(GraphicsContextStrategy contextStrategy, IBlendStateStrategy source)
            : base(contextStrategy, source)
        {
        }


        internal D3D11.BlendState GetDxState()
        {
            if (_state == null)
            {
                _state = CreateDXState(this.GraphicsDevice.Strategy);
            }

            return _state;
        }

        internal D3D11.BlendState CreateDXState(GraphicsDeviceStrategy deviceStrategy)
        {
            // Build the description.
            D3D11.BlendStateDescription blendStateDesc = new D3D11.BlendStateDescription();
            ConcreteBlendState.GetDXBlendDescription(this.Targets[0], ref blendStateDesc.RenderTarget[0]);
            ConcreteBlendState.GetDXBlendDescription(this.Targets[1], ref blendStateDesc.RenderTarget[1]);
            ConcreteBlendState.GetDXBlendDescription(this.Targets[2], ref blendStateDesc.RenderTarget[2]);
            ConcreteBlendState.GetDXBlendDescription(this.Targets[3], ref blendStateDesc.RenderTarget[3]);
            blendStateDesc.IndependentBlendEnable = this.IndependentBlendEnable;

            // This is a new DX11 feature we should consider 
            // exposing as part of the extended MonoGame API.
            blendStateDesc.AlphaToCoverageEnable = false;

            // Create the state.
            return new D3D11.BlendState(deviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, blendStateDesc);
        }

        static private void GetDXBlendDescription(TargetBlendState blendStateTarget, ref D3D11.RenderTargetBlendDescription desc)
        {
            // We're blending if we're not in the opaque state.
            desc.IsBlendEnabled = !(blendStateTarget.ColorSourceBlend == Blend.One &&
                                    blendStateTarget.ColorDestinationBlend == Blend.Zero &&
                                    blendStateTarget.AlphaSourceBlend == Blend.One &&
                                    blendStateTarget.AlphaDestinationBlend == Blend.Zero);

            desc.BlendOperation = GetDXBlendOperation(blendStateTarget.ColorBlendFunction);
            desc.SourceBlend = GetDXBlendOption(blendStateTarget.ColorSourceBlend, false);
            desc.DestinationBlend = GetDXBlendOption(blendStateTarget.ColorDestinationBlend, false);

            desc.AlphaBlendOperation = GetDXBlendOperation(blendStateTarget.AlphaBlendFunction);
            desc.SourceAlphaBlend = GetDXBlendOption(blendStateTarget.AlphaSourceBlend, true);
            desc.DestinationAlphaBlend = GetDXBlendOption(blendStateTarget.AlphaDestinationBlend, true);

            desc.RenderTargetWriteMask = GetDXColorWriteMaskFlags(blendStateTarget.ColorWriteChannels);
        }

        static private D3D11.BlendOperation GetDXBlendOperation(BlendFunction blend)
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

        static private D3D11.BlendOption GetDXBlendOption(Blend blend, bool alpha)
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

        static private D3D11.ColorWriteMaskFlags GetDXColorWriteMaskFlags(ColorWriteChannels mask)
        {
            return ((mask & ColorWriteChannels.Red)   != 0 ? D3D11.ColorWriteMaskFlags.Red : 0) |
                   ((mask & ColorWriteChannels.Green) != 0 ? D3D11.ColorWriteMaskFlags.Green : 0) |
                   ((mask & ColorWriteChannels.Blue)  != 0 ? D3D11.ColorWriteMaskFlags.Blue : 0) |
                   ((mask & ColorWriteChannels.Alpha) != 0 ? D3D11.ColorWriteMaskFlags.Alpha : 0);
        }


        internal override void PlatformGraphicsContextLost()
        {
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }

            DX.Utilities.Dispose(ref _state);

            base.Dispose(disposing);
        }
    }

}
