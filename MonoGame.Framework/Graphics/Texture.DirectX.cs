// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Platform.Graphics;
using SharpDX.Direct3D11;


namespace Microsoft.Xna.Framework.Graphics
{
    public abstract partial class Texture
    {


        /// <summary>
        /// Returns a handle to internal device object. Valid only on DirectX platforms.
        /// For usage, convert this to SharpDX.Direct3D11.Resource.
        /// </summary>
        [Obsolete("Use GetD3D11Resource() method.")]
        public object Handle
        {
            get { return this.GetD3D11Resource(); }
        }

        internal abstract Resource CreateTexture();

        internal Resource GetTexture()
        {
            if (GetTextureStrategy<ConcreteTexture>()._texture != null)
                return GetTextureStrategy<ConcreteTexture>()._texture;

            GetTextureStrategy<ConcreteTexture>()._texture = CreateTexture();
            return GetTextureStrategy<ConcreteTexture>()._texture;
        }

        internal ShaderResourceView GetShaderResourceView()
        {
            if (GetTextureStrategy<ConcreteTexture>()._resourceView != null)
                return GetTextureStrategy<ConcreteTexture>()._resourceView;

            GetTextureStrategy<ConcreteTexture>()._resourceView = new ShaderResourceView(GraphicsDevice.Strategy.ToConcrete<ConcreteGraphicsDevice>().D3DDevice, GetTexture());
            return GetTextureStrategy<ConcreteTexture>()._resourceView;
        }

        private void PlatformGraphicsDeviceResetting()
        {
            SharpDX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._resourceView);
            SharpDX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._texture);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SharpDX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._resourceView);
                SharpDX.Utilities.Dispose(ref GetTextureStrategy<ConcreteTexture>()._texture);
            }

            base.Dispose(disposing);
        }
    }
}

