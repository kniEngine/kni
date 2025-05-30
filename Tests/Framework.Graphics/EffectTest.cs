﻿// MonoGame - Copyright (C) The MonoGame Team
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
            texture0.Name = "texture0";
            texture1.Name = "texture1";

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
            texture0.Name = "texture0";
            texture1.Name = "texture1";

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
            texture0.Name = "texture0";
            texture1.Name = "texture1";
            texture0.SetData(new Color[] { new Color(0f,0f,1f,1f) });
            texture1.SetData(new Color[] { new Color(0f,1f,0f,1f) });

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

            BasicEffect basicEffect = new BasicEffect(device);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = Matrix.Identity;
            basicEffect.CurrentTechnique.Passes[0].Apply(); // apply basic VertexShader

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            Assert.That(device.Textures[0], Is.SameAs(texture0));
            Assert.That(device.Textures[1], Is.SameAs(texture1));
            Assert.That(device.Textures[2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));


            RenderTarget2D rt = new RenderTarget2D(device, 1, 1);
            device.SetRenderTarget(rt);
            device.Clear(Color.Black);
            VertexPosition[] vertices = new VertexPosition[4];
            vertices[0].Position = new Vector3(-1, -1, 0);
            vertices[1].Position = new Vector3( 1, -1, 0);
            vertices[2].Position = new Vector3(-1,  1, 0);
            vertices[3].Position = new Vector3( 1,  1, 0);
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
            device.SetRenderTarget(null);
            Color[] data = new Color[1];
            rt.GetData(data);
            Assert.That(data[0] == new Color(0f, 0.5f, 1f, 1f));
            rt.Dispose();


            effect.Dispose();
            texture1.Dispose();
            texture0.Dispose();
        }

        [Test]
        public void OldSyntaxEffectPassShouldSetTexturesAndNotSamplers2()
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
            texture0.Name = "texture0";
            texture1.Name = "texture1";
            texture0.SetData(new Color[] { new Color(0f, 0f, 1f, 1f) });
            texture1.SetData(new Color[] { new Color(0f, 1f, 0f, 1f) });

            SamplerState sampler0 = SamplerState.LinearWrap;
            SamplerState sampler1 = SamplerState.LinearWrap;

            string effectName = "OldSyntax_t1t0";

#if WINDOWSDX || DESKTOPGL
            Effect effect = AssetTestUtility.CompileEffect(device, Paths.RawEffect(effectName));
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
#endif

            effect.Parameters["Texture0"].SetValue(texture0);
            effect.Parameters["Texture1"].SetValue(texture1);

            BasicEffect basicEffect = new BasicEffect(device);
            basicEffect.World = Matrix.Identity;
            basicEffect.View = Matrix.Identity;
            basicEffect.Projection = Matrix.Identity;
            basicEffect.CurrentTechnique.Passes[0].Apply(); // apply basic VertexShader

            EffectPass effectPass = effect.CurrentTechnique.Passes[0];
            effectPass.Apply();

            int t0 = 0;
            int t1 = 1;
            int t2 = 2;
#if KNI
            //if (device.Adapter.Backend == GraphicsBackend.DirectX11)
            //{
            //    t0 = 1;
            //    t1 = 0;
            //}
#endif

            Assert.That(device.Textures[t0], Is.SameAs(texture0));
            Assert.That(device.Textures[t1], Is.SameAs(texture1));
            Assert.That(device.Textures[t2], Is.Null);

            Assert.That(IsSamplerEqual(device.SamplerStates[0], sampler0));
            Assert.That(IsSamplerEqual(device.SamplerStates[1], sampler1));
            Assert.That(device.SamplerStates[2], Is.SameAs(SamplerState.LinearWrap));


            RenderTarget2D rt = new RenderTarget2D(device, 1, 1);
            device.SetRenderTarget(rt);
            device.Clear(Color.Black);
            VertexPosition[] vertices = new VertexPosition[4];
            vertices[0].Position = new Vector3(-1, -1, 0);
            vertices[1].Position = new Vector3( 1, -1, 0);
            vertices[2].Position = new Vector3(-1,  1, 0);
            vertices[3].Position = new Vector3( 1,  1, 0);
            device.BlendState = BlendState.Opaque;
            device.DepthStencilState = DepthStencilState.None;
            device.RasterizerState = RasterizerState.CullNone;
            device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 2);
            device.SetRenderTarget(null);
            Color[] data = new Color[1];
            rt.GetData(data);
            //TODO: this currently fails on DX11. The order of texture slots has to be swapped.
            Assert.That(data[0] == new Color(0f, 0.5f, 1f, 1f));
            rt.Dispose();


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
            texture0.Name = "texture0";
            texture1.Name = "texture1";

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
            texture0.Name = "texture0";
            texture1.Name = "texture1";

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

            int t0 = 0;
            int t1 = 1;
            int t2 = 2;
#if KNI
            //if (device.Adapter.Backend == GraphicsBackend.DirectX11)
            //{
            //    t0 = 3;
            //    t1 = 4;
            //    t2 = 5;
            //}
#endif

            Assert.That(device.Textures[t0], Is.SameAs(texture0));
            Assert.That(device.Textures[t1], Is.SameAs(texture1));
            Assert.That(device.Textures[t2], Is.Null);

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
            texture0.Name = "texture0";
            texture1.Name = "texture1";

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
            texture0.Name = "texture0";
            texture1.Name = "texture1";

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
