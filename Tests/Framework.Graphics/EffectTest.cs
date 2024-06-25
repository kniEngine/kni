// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace Kni.Tests.Graphics
{
    [TestFixture]
    internal class EffectTest : GraphicsDeviceTestFixtureBase
    {

        [Test]
        public void EffectParameterShouldBeSetIfSetByNameAndGetByIndex()
        {
            // This relies on the parameters permanently being on the same index.
            // Should be no problem except when adding parameters.
            Texture2D texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = texture;

            BasicEffect effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;
            effect.Parameters["DiffuseColor"].SetValue(Color.HotPink.ToVector3());
            effect.Parameters["FogColor"].SetValue(Color.Honeydew.ToVector3());

            int DiffuseColorIndex = 0;
            int FogColorIndex = 14;

            Assert.That(effect.Parameters[DiffuseColorIndex].GetValueVector3().Equals(Color.HotPink.ToVector3()));
            Assert.That(effect.Parameters[FogColorIndex].GetValueVector3().Equals(Color.Honeydew.ToVector3()));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTexture()
        {
            Texture2D texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = null;

            BasicEffect effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTextureOnSubsequentCalls()
        {
            Texture2D texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = null;

            BasicEffect effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = texture;

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            game.GraphicsDevice.Textures[0] = null;

            effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldSetTextureEvenIfNull()
        {
            Texture2D texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = texture;

            BasicEffect effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.Texture = null;

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            texture.Dispose();
            effect.Dispose();
        }

        [Test]
        public void EffectPassShouldOverrideTextureIfNotExplicitlySet()
        {
            Texture2D texture = new Texture2D(game.GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            game.GraphicsDevice.Textures[0] = texture;

            BasicEffect effect = new BasicEffect(game.GraphicsDevice);
            effect.TextureEnabled = true;

            Assert.That(game.GraphicsDevice.Textures[0], Is.SameAs(texture));

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(game.GraphicsDevice.Textures[0], Is.Null);

            texture.Dispose();
            effect.Dispose();
        }

    }
}
