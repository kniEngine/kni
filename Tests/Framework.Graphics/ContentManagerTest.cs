// Copyright (C)2026 Nick Kastellanos

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace Kni.Tests.Graphics
{
    [TestFixture]
    class ContentManagerTest : GraphicsDeviceTestFixtureBase
    {
        [Test]
        public void LoadSharedResources()
        {
            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));

            Assert.AreSame(modelA.Meshes[0].Effects[0],
                           modelA.Meshes[0].MeshParts[0].Effect);

            content.Unload();
        }

        [Test]
        public void LoadExternalResources()
        {
            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));
            Model modelB = content.Load<Model>(Paths.Model("Spaceship2"));

            Texture2D textureA = ((BasicEffect)modelA.Meshes[0].MeshParts[0].Effect).Texture;
            Texture2D textureB = ((BasicEffect)modelB.Meshes[0].MeshParts[0].Effect).Texture;

            Assert.AreSame(textureA, textureB);

            content.Unload();
        }

        [Test]
        public void LoadExternalResources2()
        {
            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));
            Texture2D textureB = content.Load<Texture2D>(Paths.Model("enemy_0"));

            Texture2D textureA = ((BasicEffect)modelA.Meshes[0].MeshParts[0].Effect).Texture;

            Assert.AreSame(textureA, textureB);

            content.Unload();
        }

    }
}