// Copyright (C)2026 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;

namespace Kni.Tests.ContentPipeline
{
    // Design-time asset. Refers to the texture through an ExternalReference.
    public class CustomSpriteContent
    {
        public Rectangle SourceRectangle;
        public ExternalReference<TextureContent> Texture;
    }

    // Runtime asset. It doesn't own the texture, so it must not dispose it.
    public class CustomSprite : IDisposable
    {
        public Texture2D Texture { get; private set; }
        public Rectangle SourceRectangle { get; private set; }

        public CustomSprite(Texture2D texture, Rectangle sourceRectangle)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
        }

        public void Dispose()
        {
            // The texture is shared and owned by the ContentManager.
            Texture = null;
        }
    }

    [ContentProcessor(DisplayName = "CustomSprite Processor")]
    public class CustomSpriteProcessor : ContentProcessor<CustomSpriteContent, CustomSpriteContent>
    {
        public override CustomSpriteContent Process(CustomSpriteContent input, ContentProcessorContext context)
        {
            // Build the texture as an external asset. All sprites that point
            // to the same source file share the same built asset.
            ExternalReference<TextureContent> texture =
                context.BuildAsset<TextureContent, TextureContent>(input.Texture, "TextureProcessor");

            return new CustomSpriteContent()
            {
                SourceRectangle = input.SourceRectangle,
                Texture = texture,
            };
        }
    }

    [ContentTypeWriter]
    public class CustomSpriteWriter : ContentTypeWriter<CustomSpriteContent>
    {
        protected override void Write(ContentWriter output, CustomSpriteContent value)
        {
            output.Write(value.SourceRectangle.X);
            output.Write(value.SourceRectangle.Y);
            output.Write(value.SourceRectangle.Width);
            output.Write(value.SourceRectangle.Height);
            output.WriteExternalReference(value.Texture);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(CustomSprite).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(CustomSpriteReader).AssemblyQualifiedName;
        }
    }

    public class CustomSpriteReader : ContentTypeReader<CustomSprite>
    {
        protected override CustomSprite Read(ContentReader input, CustomSprite existingInstance)
        {
            int x = input.ReadInt32();
            int y = input.ReadInt32();
            int w = input.ReadInt32();
            int h = input.ReadInt32();
            // loaded through the ContentManager, so it is cached and shared.
            Texture2D texture = input.ReadExternalReference<Texture2D>();
            return new CustomSprite(texture, new Rectangle(x, y, w, h));
        }
    }

    public static class CustomSpriteExtensions
    {
        public static void Draw(this SpriteBatch spriteBatch, CustomSprite sprite, Vector2 position, Color color)
        {
            spriteBatch.Draw(sprite.Texture, position, sprite.SourceRectangle, color);
        }

        public static void Draw(this SpriteBatch spriteBatch, CustomSprite sprite, Rectangle destinationRectangle, Color color)
        {
            spriteBatch.Draw(sprite.Texture, destinationRectangle, sprite.SourceRectangle, color);
        }
    }

    [TestFixture]
    class CustomSpriteTests : GraphicsDeviceTestFixtureBase
    {
        const string RootDirectory = "CustomSpriteRoot";

        class TestBuildContext : TestProcessorContext
        {
            readonly string _root;
            readonly TargetPlatform _platform;
            public readonly Dictionary<string, byte[]> Output = new Dictionary<string, byte[]>();

            public TestBuildContext(TargetPlatform platform, string root)
                : base(platform, "notused.xnb")
            {
                _platform = platform;
                _root = root;
            }

            public override ExternalReference<TOutput> BuildAsset<TInput, TOutput>(ExternalReference<TInput> sourceAsset, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
            {
                if (string.IsNullOrEmpty(assetName))
                    assetName = Path.GetFileNameWithoutExtension(sourceAsset.Filename);
                string outputFile = Path.Combine(_root, assetName + ".xnb");

                // build once, reuse for every reference to the same asset
                if (!Output.ContainsKey(assetName))
                {
                    TextureContent texture = new TextureImporter().Import(sourceAsset.Filename, new TestImporterContext("TestObj", "TestBin"));
                    TextureContent processed = new TextureProcessor().Process(texture, this);
                    Output[assetName] = Compile(processed, _platform, _root);
                }

                return new ExternalReference<TOutput>(outputFile);
            }
        }

        class MemoryContentManager : ContentManager
        {
            readonly Dictionary<string, byte[]> _files;

            public MemoryContentManager(IServiceProvider services, Dictionary<string, byte[]> files)
                : base(services, "")
            {
                _files = files;
            }

            protected override Stream OpenStream(string assetName)
            {
                return new MemoryStream(_files[assetName], false);
            }
        }

        static byte[] Compile(object content, TargetPlatform platform, string root)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                new ContentCompiler().Compile(stream, content, platform, GraphicsProfile.HiDef,
                    ContentCompression.Uncompressed, root, root);
                return stream.ToArray();
            }
        }

        [Test]
        public void SpritesShareTheSameTexture()
        {
            TargetPlatform platform = TargetPlatform.Windows;
#if DESKTOPGL
            platform = TargetPlatform.DesktopGL;
#endif
            string root = Path.Combine(Path.GetFullPath(RootDirectory), "x");
            root = Path.GetDirectoryName(root) + Path.DirectorySeparatorChar;

            var context = new TestBuildContext(platform, root);
            var files = new Dictionary<string, byte[]>();

            // import (XmlImporter) -> process -> compile
            foreach (string name in new[] { "CustomSprite1", "CustomSprite2" })
            {
                var imported = (CustomSpriteContent)new XmlImporter().Import(
                    Paths.Xml(name + ".xml"), new TestImporterContext("TestObj", "TestBin"));
                imported.Texture.Filename = Path.GetFullPath(imported.Texture.Filename);

                CustomSpriteContent processed = new CustomSpriteProcessor().Process(imported, context);
                files[name] = Compile(processed, platform, root);
            }
            foreach (var kvp in context.Output)
                files[kvp.Key] = kvp.Value;

            using (var manager = new MemoryContentManager(game.Services, files))
            using (CustomSprite sprite1 = manager.Load<CustomSprite>("CustomSprite1"))
            using (CustomSprite sprite2 = manager.Load<CustomSprite>("CustomSprite2"))
            {
                Assert.AreEqual(1, context.Output.Count);
                Assert.AreEqual(new Rectangle(0, 0, 32, 32), sprite1.SourceRectangle);
                Assert.AreEqual(new Rectangle(32, 32, 32, 32), sprite2.SourceRectangle);

                Assert.NotNull(sprite1.Texture);
                Assert.AreSame(sprite1.Texture, sprite2.Texture);

                // the extension draws without error
                using (var spriteBatch = new SpriteBatch(gd))
                {
                    spriteBatch.Begin();
                    spriteBatch.Draw(sprite1, Vector2.Zero, Color.White);
                    spriteBatch.Draw(sprite2, new Rectangle(64, 0, 32, 32), Color.White);
                    spriteBatch.End();
                }
            }
        }
    }
}
