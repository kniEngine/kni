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

        [Test]
        public void UnloadAssetDoesNotLeakResources()
        {
            MethodInfo unloadAssetMethod = typeof(ContentManager).GetMethod("UnloadAsset",
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
               null, new[] { typeof(string) }, null);
            if (unloadAssetMethod == null)
                Assert.Ignore("ContentManager.UnloadAsset(string) is not implemented.");

            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));

            // This should remove modelA from the cache,
            // and release any associated resources (Textures, VertexBuffers, etc).
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("Spaceship") });

            Texture2D textureA = ((BasicEffect)modelA.Meshes[0].MeshParts[0].Effect).Texture;
            Assert.IsTrue(textureA.IsDisposed);

            Model modelB = content.Load<Model>(Paths.Model("Spaceship"));

            Texture2D textureB = ((BasicEffect)modelB.Meshes[0].MeshParts[0].Effect).Texture;

            Assert.AreNotSame(textureA, textureB);
            Assert.IsFalse(textureB.IsDisposed);

            content.Unload();
        }

        [Test]
        public void UnloadAssetDoesNotDisposeSharedResources()
        {
            MethodInfo unloadAssetMethod = typeof(ContentManager).GetMethod("UnloadAsset",
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
               null, new[] { typeof(string) }, null);
            if (unloadAssetMethod == null)
                Assert.Ignore("ContentManager.UnloadAsset(string) is not implemented.");

            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));
            Model modelB = content.Load<Model>(Paths.Model("Spaceship2"));

            Texture2D textureA = ((BasicEffect)modelA.Meshes[0].MeshParts[0].Effect).Texture;
            Texture2D textureB = ((BasicEffect)modelB.Meshes[0].MeshParts[0].Effect).Texture;

            Assert.AreSame(textureA, textureB);

            // This should remove modelA from the cache,
            // and release any associated resources (Textures, VertexBuffers, etc),
            // but not its texture if it is shared with another model.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("Spaceship") });

            Assert.IsFalse(textureB.IsDisposed);

            // This should remove modelB from the cache,
            // and release any associated resources (Textures, VertexBuffers, etc).
            // including its texture, since it is not shared with another model.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("Spaceship2") });

            Assert.IsTrue(textureB.IsDisposed);

            //content.Unload();
        }

        [Test]
        public void UnloadAssetDoesNotDisposeSharedAsset()
        {
            MethodInfo unloadAssetMethod = typeof(ContentManager).GetMethod("UnloadAsset",
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
               null, new[] { typeof(string) }, null);
            if (unloadAssetMethod == null)
                Assert.Ignore("ContentManager.UnloadAsset(string) is not implemented.");

            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));
            Texture2D textureB = content.Load<Texture2D>(Paths.Model("enemy_0"));

            // This should remove modelA from the cache,
            // and release any associated resources (Textures, VertexBuffers, etc),
            // but not the texture if it was loaded as a separate asset.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("Spaceship") });

            Assert.IsFalse(textureB.IsDisposed);

            // This should remove textureB from the cache,
            // and release it.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("enemy_0") });

            Assert.IsTrue(textureB.IsDisposed);

            //content.Unload();
        }

        [Test]
        public void UnloadAssetDoesNotDisposeSharedAsset2()
        {
            MethodInfo unloadAssetMethod = typeof(ContentManager).GetMethod("UnloadAsset",
               BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
               null, new[] { typeof(string) }, null);
            if (unloadAssetMethod == null)
                Assert.Ignore("ContentManager.UnloadAsset(string) is not implemented.");

            Model modelA = content.Load<Model>(Paths.Model("Spaceship"));
            Texture2D textureB = content.Load<Texture2D>(Paths.Model("enemy_0"));

            Texture2D textureA = ((BasicEffect)modelA.Meshes[0].MeshParts[0].Effect).Texture;

            Assert.AreSame(textureA, textureB);

            // This should remove textureB from the cache,
            // but not release it if it is shared with modelA.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("enemy_0") });

            Assert.IsFalse(textureB.IsDisposed);

            // subsequent unload of textureB should not release it.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("enemy_0") });

            Assert.IsFalse(textureB.IsDisposed);

            // This should remove modelA from the cache,
            // and release any associated resources (Textures, VertexBuffers, etc),
            // including its texture.
            unloadAssetMethod.Invoke(content, new object[] { Paths.Model("Spaceship") });

            Assert.IsTrue(textureB.IsDisposed);

            //content.Unload();
        }

    }
}