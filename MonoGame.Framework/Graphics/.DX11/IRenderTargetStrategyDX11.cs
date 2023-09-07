// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using D3D11 = SharpDX.Direct3D11;

namespace Microsoft.Xna.Platform.Graphics
{
    internal interface IRenderTargetStrategyDX11
    {
        /// <summary>
        /// Gets the <see cref="SharpDX.Direct3D11.RenderTargetView"/> for the specified array slice.
        /// </summary>
        /// <param name="arraySlice">The array slice.</param>
        /// <returns>The <see cref="SharpDX.Direct3D11.RenderTargetView"/>.</returns>
        /// <remarks>
        /// For texture cubes: The array slice is the index of the cube map face.
        /// </remarks>
        D3D11.RenderTargetView GetRenderTargetView(int arraySlice);

        /// <summary>
        /// Gets the <see cref="SharpDX.Direct3D11.DepthStencilView"/>.
        /// </summary>
        /// <param name="arraySlice">The array slice.</param>
        /// <returns>The <see cref="SharpDX.Direct3D11.DepthStencilView"/>. Can be <see langword="null"/>.</returns>
        D3D11.DepthStencilView GetDepthStencilView(int arraySlice);
    }
}
