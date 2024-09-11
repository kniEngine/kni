// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Kni.Tests.ContentPipeline;
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
            effect.Parameters["DiffuseColor"].SetValue(Color.HotPink.ToVector4());
            effect.Parameters["FogColor"].SetValue(Color.Honeydew.ToVector3());

            int DiffuseColorIndex = 0;
            int FogColorIndex = 14;

#if XNA
            DiffuseColorIndex = 1;
            FogColorIndex = 15;
#elif DESKTOPGL
            DiffuseColorIndex = 1;
            FogColorIndex = 2;
#endif

            Assert.That(effect.Parameters[DiffuseColorIndex].GetValueVector4().Equals(Color.HotPink.ToVector4()));
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


        private bool IsSamplerEqual(SamplerState s0, SamplerState s1)
        {
            if (s0 == null && s1 == null)
                return true;

            return s0.Filter == s1.Filter
                && s0.AddressU == s1.AddressU
                && s0.AddressV == s1.AddressV
                ;
        }


        [Test]
        public void OldSyntaxEffectPassShouldNotOverrideSamplersAndNotOverrideTextures()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.LinearClamp;
            SamplerState sampler1 = SamplerState.PointClamp;

            device.SamplerStates[0] = sampler0;
            device.SamplerStates[1] = sampler1;

            string effectName = "OldSyntax_x";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            // Samplers shouldn't be exposed as parameters.
            Assert.That(effect.Parameters.Count == 0);

            device.Textures[0] = texture0;
            device.Textures[1] = texture1;

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

        [Test]
        public void OldSyntaxEffectPassShouldOverrideSamplersAndNotOverrideTextures()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.PointWrap;
            SamplerState sampler1 = SamplerState.PointClamp;
#if XNA
            sampler0 = null;
            sampler1 = null;
#endif

            string effectName = "OldSyntax_s0s1";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            // Samplers shouldn't be exposed as parameters.
            Assert.That(effect.Parameters.Count == 0);

            device.Textures[0] = texture0;
            device.Textures[1] = texture1;

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

        [Test]
        public void OldSyntaxEffectPassShouldSetTexturesAndNotSamplers()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.LinearWrap;
            SamplerState sampler1 = SamplerState.LinearWrap;

            string effectName = "OldSyntax_t0t1";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            effect.Parameters["Texture0"].SetValue(texture0);
            effect.Parameters["Texture1"].SetValue(texture1);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

        [Test]
        public void OldSyntaxEffectPassShouldSetTexturesAndSamplers()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.PointWrap;
            SamplerState sampler1 = SamplerState.PointClamp;
#if XNA
            sampler0 = null;
            sampler1 = null;
#endif

            string effectName = "OldSyntax_s0s1t0t1";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            effect.Parameters["Texture0"].SetValue(texture0);
            effect.Parameters["Texture1"].SetValue(texture1);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0],sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1],sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

        [Test]
        public void DeferredBasicEffectPassShouldSetTexturesAndSamplers()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.LinearWrap;
            SamplerState sampler1 = SamplerState.LinearWrap;
#if XNA
            sampler0 = null;
            sampler1 = null;
#endif

            string effectName = "DeferredBasicEffect";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            effect.Parameters["Diffuse"].SetValue(texture0);
            effect.Parameters["SpecularMap"].SetValue(texture1);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

#if WINDOWSDX || DESKTOPGL

        [Test]
        public void NewSyntaxEffectPassShouldSetTexturesAndNotSamplers()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.LinearWrap;
            SamplerState sampler1 = SamplerState.LinearWrap;

            string effectName = "NewSyntax_t0t1";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            effect.Parameters["Texture0"].SetValue(texture0);
            effect.Parameters["Texture1"].SetValue(texture1);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

        [Test]
        public void NewSyntaxEffectPassShouldSetTexturesAndSamplers()
        {
            GraphicsDevice device = game.GraphicsDevice;

            // Reset Samplers and Textures
            for (int i = 0; i < 16; i++)
            {
                device.Textures[i] = null;
                device.SamplerStates[i] = SamplerState.LinearWrap;
            }

            Texture2D texture0 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);
            Texture2D texture1 = new Texture2D(device, 1, 1, false, SurfaceFormat.Color);

            SamplerState sampler0 = SamplerState.PointWrap;
            SamplerState sampler1 = SamplerState.PointClamp;
#if XNA
            sampler0 = null;
            sampler1 = null;
#endif

            string effectName = "NewSyntax_s0s1t0t1";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            effect.Parameters["Texture0"].SetValue(texture0);
            effect.Parameters["Texture1"].SetValue(texture1);

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));

            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }
#endif

    }
}
