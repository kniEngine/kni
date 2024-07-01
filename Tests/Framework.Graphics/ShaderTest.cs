// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Kni.Tests.ContentPipeline;
using NUnit.Framework;

namespace Kni.Tests.Graphics
{
	[TestFixture]
	class ShaderTest : GraphicsDeviceTestFixtureBase
    {
		[TestCase("Bevels")]
		[TestCase("BlackOut")]
		[TestCase("ColorFlip")]
		[TestCase("Grayscale")]
		[TestCase("HighContrast")]
		[TestCase("Invert")]
		[TestCase("NoEffect")]
        [TestCase("RainbowH")]
        public void Shader(string effectName)
		{
            PrepareFrameCapture();

#if WINDOWSDX
            Effect effect = AssetTestUtility.CompileEffect(gd, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            SpriteBatch spriteBatch = new SpriteBatch(gd);
            // A background texture to test that the effect doesn't
            // mess up other textures
            Texture2D background = content.Load<Texture2D>(Paths.Texture("fun-background"));
            // The texture to apply the effect to
            Texture2D surge = content.Load<Texture2D>(Paths.Texture("Surge"));

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
            spriteBatch.Draw(background, Vector2.Zero, Color.White);

            effect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.Draw(surge, new Vector2(300, 200), null, Color.White,
                             0f, Vector2.Zero, 2.0f, SpriteEffects.None, 0f);
            spriteBatch.End();

            CheckFrames();

            spriteBatch.Dispose();
            effect.Dispose();
            background.Dispose();
            surge.Dispose();
		}
	}
}
