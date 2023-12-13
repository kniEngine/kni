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
    internal class ConcreteSamplerState : ResourceSamplerStateStrategy
    {
        private D3D11.SamplerState _state;

        internal ConcreteSamplerState(GraphicsContextStrategy contextStrategy, ISamplerStateStrategy source)
            : base(contextStrategy, source)
        {
            _state = CreateDXState(this.GraphicsDevice.Strategy);
        }

        internal D3D11.SamplerState GetDxState()
        {
            return _state;
        }

        internal D3D11.SamplerState CreateDXState(GraphicsDeviceStrategy deviceStrategy)
        {
            // Build the description.
            D3D11.SamplerStateDescription samplerStateDesc = new D3D11.SamplerStateDescription();
            samplerStateDesc.AddressU = ToDXTextureAddressMode(AddressU);
            samplerStateDesc.AddressV = ToDXTextureAddressMode(AddressV);
            samplerStateDesc.AddressW = ToDXTextureAddressMode(AddressW);

#if WINDOWS_UAP || WINUI
            samplerStateDesc.BorderColor = new SharpDX.Mathematics.Interop.RawColor4(
                BorderColor.R / 255.0f,
                BorderColor.G / 255.0f,
                BorderColor.B / 255.0f,
                BorderColor.A / 255.0f);
#else
            samplerStateDesc.BorderColor = BorderColor.ToRawColor4();
#endif

            samplerStateDesc.Filter = ToDXTextureFilter(Filter, FilterMode);
            samplerStateDesc.MaximumAnisotropy = Math.Min(MaxAnisotropy, deviceStrategy.Capabilities.MaxTextureAnisotropy);
            samplerStateDesc.MipLodBias = MipMapLevelOfDetailBias;
            samplerStateDesc.ComparisonFunction = ComparisonFunction.ToDXComparisonFunction();

            // TODO: How do i do this?
            samplerStateDesc.MinimumLod = 0.0f;

            // To support feature level 9.1 these must 
            // be set to these exact values.
            samplerStateDesc.MaximumLod = float.MaxValue;

            // Create the state.
            return new D3D11.SamplerState(deviceStrategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, samplerStateDesc);
        }

        private static D3D11.Filter ToDXTextureFilter(TextureFilter filter, TextureFilterMode mode)
        {
            switch (mode)
            {
                case TextureFilterMode.Comparison:
                    switch (filter)
                    {
                        case TextureFilter.Anisotropic:
                            return D3D11.Filter.ComparisonAnisotropic;
                        case TextureFilter.Linear:
                            return D3D11.Filter.ComparisonMinMagMipLinear;
                        case TextureFilter.LinearMipPoint:
                            return D3D11.Filter.ComparisonMinMagLinearMipPoint;
                        case TextureFilter.MinLinearMagPointMipLinear:
                            return D3D11.Filter.ComparisonMinLinearMagPointMipLinear;
                        case TextureFilter.MinLinearMagPointMipPoint:
                            return D3D11.Filter.ComparisonMinLinearMagMipPoint;
                        case TextureFilter.MinPointMagLinearMipLinear:
                            return D3D11.Filter.ComparisonMinPointMagMipLinear;
                        case TextureFilter.MinPointMagLinearMipPoint:
                            return D3D11.Filter.ComparisonMinPointMagLinearMipPoint;
                        case TextureFilter.Point:
                            return D3D11.Filter.ComparisonMinMagMipPoint;
                        case TextureFilter.PointMipLinear:
                            return D3D11.Filter.ComparisonMinMagPointMipLinear;

                        default:
                            throw new ArgumentException("Invalid texture filter!");
                    }
                case TextureFilterMode.Default:
                    switch (filter)
                    {
                        case TextureFilter.Anisotropic:
                            return D3D11.Filter.Anisotropic;
                        case TextureFilter.Linear:
                            return D3D11.Filter.MinMagMipLinear;
                        case TextureFilter.LinearMipPoint:
                            return D3D11.Filter.MinMagLinearMipPoint;
                        case TextureFilter.MinLinearMagPointMipLinear:
                            return D3D11.Filter.MinLinearMagPointMipLinear;
                        case TextureFilter.MinLinearMagPointMipPoint:
                            return D3D11.Filter.MinLinearMagMipPoint;
                        case TextureFilter.MinPointMagLinearMipLinear:
                            return D3D11.Filter.MinPointMagMipLinear;
                        case TextureFilter.MinPointMagLinearMipPoint:
                            return D3D11.Filter.MinPointMagLinearMipPoint;
                        case TextureFilter.Point:
                            return D3D11.Filter.MinMagMipPoint;
                        case TextureFilter.PointMipLinear:
                            return D3D11.Filter.MinMagPointMipLinear;

                        default:
                            throw new ArgumentException("Invalid texture filter!");
                    }
                default:
                    throw new ArgumentException("Invalid texture filter mode!");
            }
        }

        private static D3D11.TextureAddressMode ToDXTextureAddressMode(TextureAddressMode mode)
        {
            switch (mode)
            {
                case TextureAddressMode.Clamp:
                    return D3D11.TextureAddressMode.Clamp;
                case TextureAddressMode.Mirror:
                    return D3D11.TextureAddressMode.Mirror;
                case TextureAddressMode.Wrap:
                    return D3D11.TextureAddressMode.Wrap;
                case TextureAddressMode.Border:
                    return D3D11.TextureAddressMode.Border;

                default:
                    throw new ArgumentException("Invalid texture address mode!");
            }
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
