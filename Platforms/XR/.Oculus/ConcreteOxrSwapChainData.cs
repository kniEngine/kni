// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.XR;
using Microsoft.Xna.Platform.XR;
using Silk.NET.OpenXR;

namespace Microsoft.Xna.Platform.Graphics
{
    // Concrete SharpDX.Direct3D11 
    internal sealed class ConcreteOxrSwapChainData : ConcreteXRDevice.OxrSwapChainDataBase
    {

        //private OvrTextureSwapChain _swapChain;
        //public D3D11.Texture2D[] _buckBuffers;
        private RenderTarget2D _renderTarget;

        //internal override OvrTextureSwapChain SwapChain { get { return _swapChain; } }

        internal override RenderTarget2D GetRenderTarget(int eye)
        {
            return _renderTarget;
        }

        //internal static int CreateSwapChain(
        //    GraphicsDevice graphicsDevice, OvrSession ovrSession,
        //    int w, int h,
        //    SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat, int preferredMultiSampleCount,
        //    out OvrDevice.OvrSwapChainDataBase outSwapChainData)
        //{
        //    int ovrResult = 0;

        //    outSwapChainData = null;
        //    ConcreteOvrSwapChainData swapChainData = new ConcreteOvrSwapChainData();

        //    OvrTextureSwapChainDesc desc = default(OvrTextureSwapChainDesc);
        //    desc.Type = OvrTextureType.Texture2D;
        //    desc.Format = OvrTextureFormat.R8G8B8A8_UNORM_SRGB;
        //    //desc.Format = OVR_FORMAT_R8G8B8A8_UNORM;
        //    desc.ArraySize = 1;
        //    desc.Width = w;
        //    desc.Height = h;
        //    desc.MipLevels = 1;
        //    desc.SampleCount = 1;
        //    desc.SampleCount = 1;
        //    desc.StaticImage = OvrBool.False;
        //    desc.MiscFlags = OvrTextureMiscFlags.None;
        //    desc.BindFlags = OvrTextureBindFlags.DX_RenderTarget;

        //    OvrTextureSwapChain swapChain;

        //    D3D11.Device d3dDevice = (D3D11.Device)graphicsDevice.GetD3D11Device();
        //    ovrResult = ovrSession.CreateTextureSwapChainDX(d3dDevice.NativePointer, desc, out swapChain);
        //    if (ovrResult < 0)
        //        return ovrResult;
        //    swapChainData._swapChain = swapChain;

        //    // create backBuffers
        //    int texSwapChainCount;
        //    ovrResult = swapChain.GetTextureSwapChainLength(out texSwapChainCount);

        //    swapChainData._renderTarget = new RenderTarget2D(
        //        graphicsDevice, w, h, false,
        //        preferredFormat, preferredDepthFormat, preferredMultiSampleCount,
        //        RenderTargetUsage.DiscardContents);

        //    swapChainData._buckBuffers = new D3D11.Texture2D[texSwapChainCount];
        //    for (int i = 0; i < texSwapChainCount; i++)
        //    {
        //        IntPtr pDxTexture3D;
        //        ovrResult = swapChain.GetTextureSwapChainBufferDX(i, IID_ID3D11Texture2D, out pDxTexture3D);
        //        swapChainData._buckBuffers[i] = new D3D11.Texture2D(pDxTexture3D);
        //    }

        //    outSwapChainData = swapChainData;

        //    return 0;
        //}

        internal override int SubmitRenderTarget(GraphicsDevice graphicsDevice, RenderTarget2D rt)
        {   
            int ovrResult;
            int index = 0;
            //ovrResult = SwapChain.GetCurrentIndex(out index);
            //if (ovrResult < 0)
            //    return ovrResult;

            //D3D11.Texture2D dstResource = _buckBuffers[index];
            //D3D11.Resource srcResource  = (D3D11.Resource)rt.GetD3D11Resource();
//
            //D3D11.Device d3dDevice  = (D3D11.Device)graphicsDevice.GetD3D11Device();
            //D3D11.DeviceContext d3dContext = d3dDevice.ImmediateContext;
            //
            //d3dContext.CopyResource(srcResource, dstResource);

            return 0;
        }
    }
}

