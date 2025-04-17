﻿// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace Kni.Tests.Graphics
{
    [TestFixture]
    internal class BlendStateTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void ShouldNotBeAbleToSetNullBlendState()
        {
            Assert.Throws<ArgumentNullException>(() => game.GraphicsDevice.BlendState = null);
        }

        [Test]
        public void ShouldNotBeAbleToMutateStateObjectAfterBindingToGraphicsDevice()
        {
            BlendState blendState = new BlendState();

            // Can mutate before binding.
            DoAsserts(blendState, Assert.DoesNotThrow);

            // Can't mutate after binding.
            game.GraphicsDevice.BlendState = blendState;
            DoAsserts(blendState, d => Assert.Throws<InvalidOperationException>(d));

            // Even after changing to different BlendState, you still can't mutate a previously-bound object.
            game.GraphicsDevice.BlendState = BlendState.Opaque;
            DoAsserts(blendState, d => Assert.Throws<InvalidOperationException>(d));

            blendState.Dispose();
        }

        [Test]
        public void ShouldNotBeAbleToMutateDefaultStateObjects()
        {
            DoAsserts(BlendState.Additive, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(BlendState.AlphaBlend, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(BlendState.NonPremultiplied, d => Assert.Throws<InvalidOperationException>(d));
            DoAsserts(BlendState.Opaque, d => Assert.Throws<InvalidOperationException>(d));
        }

        private static void DoAsserts(BlendState blendState, Action<TestDelegate> assertMethod)
        {
            assertMethod(() => blendState.AlphaBlendFunction = BlendFunction.Add);
            assertMethod(() => blendState.AlphaDestinationBlend = Blend.BlendFactor);
            assertMethod(() => blendState.AlphaSourceBlend = Blend.BlendFactor);
            assertMethod(() => blendState.BlendFactor = Color.White);
            assertMethod(() => blendState.ColorBlendFunction = BlendFunction.Add);
            assertMethod(() => blendState.ColorDestinationBlend = Blend.BlendFactor);
            assertMethod(() => blendState.ColorSourceBlend = Blend.BlendFactor);
            assertMethod(() => blendState.ColorWriteChannels = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels1 = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels2 = ColorWriteChannels.All);
            assertMethod(() => blendState.ColorWriteChannels3 = ColorWriteChannels.All);
            assertMethod(() => blendState.MultiSampleMask = 0);

#if WINDOWSDX
            // that is invalid for Reach profile
            //assertMethod(() => blendState.IndependentBlendEnable = true);
            //for (int i = 0; i < 4; i++)
            //{
            //    assertMethod(() => blendState[0].AlphaBlendFunction = BlendFunction.Add);
            //    assertMethod(() => blendState[0].AlphaDestinationBlend = Blend.BlendFactor);
            //    assertMethod(() => blendState[0].AlphaSourceBlend = Blend.BlendFactor);
            //    assertMethod(() => blendState[0].ColorBlendFunction = BlendFunction.Add);
            //    assertMethod(() => blendState[0].ColorDestinationBlend = Blend.BlendFactor);
            //    assertMethod(() => blendState[0].ColorSourceBlend = Blend.BlendFactor);
            //    assertMethod(() => blendState[0].ColorWriteChannels = ColorWriteChannels.All);
            //}
#endif

        }

        [Test]
        public void VisualTests()
        {
            Blend[] blends = new[]
            {
                Blend.One,
                Blend.Zero,
                Blend.SourceColor,
                Blend.InverseSourceColor,
                Blend.SourceAlpha,
                Blend.InverseSourceAlpha,
                Blend.DestinationColor,
                Blend.InverseDestinationColor,
                Blend.DestinationAlpha,
                Blend.InverseDestinationAlpha,
                Blend.BlendFactor,
                Blend.InverseBlendFactor,
                Blend.SourceAlphaSaturation,
            };

            SpriteBatch spriteBatch = new SpriteBatch(gd);
            Texture2D texture = content.Load<Texture2D>(Paths.Texture("MonoGameIcon"));

            Vector2 size = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);
            Vector2 offset = new Vector2(10, 10);

            PrepareFrameCapture();

            gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1.0f, 0);

            for (int y = 0; y < blends.Length; y++)
            {
                for (int x = 0; x < blends.Length; x++)
                {
                    Vector2 pos = offset + new Vector2(x*size.X, y*size.Y);

                    BlendState blendState = new BlendState();
                    blendState.ColorSourceBlend = blends[y];
                    blendState.AlphaSourceBlend = blends[y];
                    blendState.ColorDestinationBlend = blends[x];
                    blendState.AlphaDestinationBlend = blends[x];
                    blendState.BlendFactor = new Color(0.3f, 0.5f, 0.7f);

                    spriteBatch.Begin(SpriteSortMode.Deferred, blendState);
                    spriteBatch.Draw(texture, new Rectangle((int) pos.X, (int) pos.Y, (int) size.X, (int) size.Y),
                        Color.White);
                    spriteBatch.End();

                    blendState.Dispose();
                }
            }

            CheckFrames();

            spriteBatch.Dispose();
        }
    }
}
