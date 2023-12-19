// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Framework.Utilities;


namespace Microsoft.Xna.Platform.Graphics
{
    internal sealed class ConcreteGraphicsDevice : GraphicsDeviceStrategy
    {

        internal ConcreteGraphicsDevice(GraphicsDevice device, GraphicsAdapter adapter, GraphicsProfile graphicsProfile, bool preferHalfPixelOffset, PresentationParameters presentationParameters)
            : base(device, adapter, graphicsProfile, preferHalfPixelOffset, presentationParameters)
        {
        }


        public override void Reset(PresentationParameters presentationParameters)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Reset()
        {
            throw new PlatformNotSupportedException();
        }

        public override void Present(Rectangle? sourceRectangle, Rectangle? destinationRectangle, IntPtr overrideWindowHandle)
        {
            throw new PlatformNotSupportedException();
        }

        public override void Present()
        {
            throw new PlatformNotSupportedException();
        }

        public override void GetBackBufferData<T>(Rectangle? rect, T[] data, int startIndex, int elementCount)
        {
            throw new PlatformNotSupportedException();
        }


        protected override void PlatformSetup()
        {
            throw new PlatformNotSupportedException();
        }

        protected override void PlatformInitialize()
        {
            throw new PlatformNotSupportedException();
        }


        internal override GraphicsContextStrategy CreateGraphicsContextStrategy(GraphicsContext context)
        {
            return new ConcreteGraphicsContext(context);
        }

        public override System.Reflection.Assembly ConcreteAssembly
        {
            get { return ReflectionHelpers.GetAssembly(typeof(ConcreteGraphicsDevice)); }
        }

        public override string ResourceNameAlphaTestEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.ref.fxo"; } }
        public override string ResourceNameBasicEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.BasicEffect.ref.fxo"; } }
        public override string ResourceNameDualTextureEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.DualTextureEffect.ref.fxo"; } }
        public override string ResourceNameEnvironmentMapEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.EnvironmentMapEffect.ref.fxo"; } }
        public override string ResourceNameSkinnedEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.SkinnedEffect.ref.fxo"; } }
        public override string ResourceNameSpriteEffect { get { return "Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.ref.fxo"; } }


        internal void OnPresentationChanged()
        {
            throw new PlatformNotSupportedException();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            base.Dispose(disposing);
        }

    }
}
