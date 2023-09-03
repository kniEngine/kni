// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using nkast.LibOVR;
using D3D11 = SharpDX.Direct3D11;

namespace Microsoft.Xna.Framework.Graphics
{
    // Concrete SharpDX.Direct3D11 
    internal sealed class ConcreteOvrSwapChainData : OvrDevice.OvrSwapChainDataBase
    {
        public readonly static Guid IID_IUnknown = new Guid(0x00000000, 0x0000, 0x0000, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x46);
        public readonly static Guid IID_ID3D11DeviceChild = new Guid(0x1841e5c8, 0x16b0, 0x489b, 0xbc, 0xc8, 0x44, 0xcf, 0xb0, 0xd5, 0xde, 0xae);
        public readonly static Guid IID_ID3D11Resource = new Guid(0xdc8e63f3, 0xd12b, 0x4952, 0xb4, 0x7b, 0x5e, 0x45, 0x02, 0x6a, 0x86, 0x2d);
        public readonly static Guid IID_ID3D11Texture2D = new Guid(0x6f15aaf2, 0xd208, 0x4e89, 0x9a, 0xb4, 0x48, 0x95, 0x35, 0xd3, 0x4f, 0x9c);

        private OvrTextureSwapChain _swapChain;
        public D3D11.Texture2D[] _buckBuffers;
        private RenderTarget2D _renderTarget;

        internal override OvrTextureSwapChain SwapChain { get { return _swapChain; } }

        internal override RenderTarget2D GetRenderTarget(int eye)
        {
            return _renderTarget;
        }

        internal static int CreateSwapChain(
            GraphicsDevice graphicsDevice, OvrSession ovrSession,
            int w, int h,
            SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
            out OvrDevice.OvrSwapChainDataBase outSwapChainData)
        {
            int ovrResult = 0;

            outSwapChainData = null;
            ConcreteOvrSwapChainData swapChainData = new ConcreteOvrSwapChainData();

            OvrTextureSwapChainDesc desc = default(OvrTextureSwapChainDesc);
            desc.Type = OvrTextureType.Texture2D;
            desc.Format = OvrTextureFormat.R8G8B8A8_UNORM_SRGB;
            //desc.Format = OVR_FORMAT_R8G8B8A8_UNORM;
            desc.ArraySize = 1;
            desc.Width = w;
            desc.Height = h;
            desc.MipLevels = 1;
            desc.SampleCount = 1;
            desc.SampleCount = 1;
            desc.StaticImage = OvrBool.False;
            desc.MiscFlags = OvrTextureMiscFlags.None;
            desc.BindFlags = OvrTextureBindFlags.DX_RenderTarget;

            OvrTextureSwapChain swapChain;

            D3D11.Device d3dDevice = (D3D11.Device)graphicsDevice.Handle; //.GetD3D11Device();
            ovrResult = ovrSession.CreateTextureSwapChainDX(d3dDevice.NativePointer, desc, out swapChain);
            if (ovrResult < 0)
                return ovrResult;
            swapChainData._swapChain = swapChain;

            // create backBuffers
            int texSwapChainCount;
            ovrResult = swapChain.GetTextureSwapChainLength(out texSwapChainCount);

            swapChainData._renderTarget = new RenderTarget2D(
                graphicsDevice, w, h, false,
                preferredFormat, preferredDepthFormat, preferredMultiSampleCount,
                RenderTargetUsage.DiscardContents);

            swapChainData._buckBuffers = new D3D11.Texture2D[texSwapChainCount];
            for (int i = 0; i < texSwapChainCount; i++)
            {
                IntPtr pDxTexture3D;
                ovrResult = swapChain.GetTextureSwapChainBufferDX(i, IID_ID3D11Texture2D, out pDxTexture3D);
                swapChainData._buckBuffers[i] = new D3D11.Texture2D(pDxTexture3D);
            }

            outSwapChainData = swapChainData;

            return 0;
        }

        internal override int SubmitRenderTarget(GraphicsDevice graphicsDevice, RenderTarget2D rt)
        {   
            int ovrResult;
            int index = 0;
            ovrResult = SwapChain.GetCurrentIndex(out index);
            if (ovrResult < 0)
                return ovrResult;

            D3D11.Texture2D dstResource = _buckBuffers[index];
            D3D11.Resource srcResource  = (D3D11.Resource)rt.Handle; //.GetD3D11Resource();

            D3D11.Device d3dDevice  = (D3D11.Device)graphicsDevice.Handle; //.GetD3D11Device();
            D3D11.DeviceContext d3dContext = d3dDevice.ImmediateContext;

            d3dContext.CopyResource(srcResource, dstResource);

            return 0;
        }
    }
}

