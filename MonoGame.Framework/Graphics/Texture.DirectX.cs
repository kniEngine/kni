// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using DX = SharpDX;
using D3D11 = SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {


        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to D3D11.Resource.
        /// </summary>
        [Obsolete("Use GetD3D11Resource() method.")]
        public object Handle
        {
            get { return this.GetD3D11Resource(); }
        }

        protected abstract D3D11.Resource CreateTexture();

        private void PlatformGraphicsDeviceResetting()
        {
            DX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._resourceView);
            DX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._texture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._resourceView);
                DX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._texture);
            }

            base.Dispose(disposing);
        }
    }
}

