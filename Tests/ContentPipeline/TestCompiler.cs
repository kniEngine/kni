﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace Kni.Tests.ContentPipeline
{
    class TestCompiler
    {
        class TestContentManager : ContentManager
        {
            class FakeGraphicsService : IGraphicsDeviceService
            {
                public GraphicsDevice GraphicsDevice { get; private set; }

#pragma warning disable 67
                public event EventHandler<EventArgs> DeviceCreated;
                public event EventHandler<EventArgs> DeviceDisposing;
                public event EventHandler<EventArgs> DeviceReset;
                public event EventHandler<EventArgs> DeviceResetting;
#pragma warning restore 67
            }

            class FakeServiceProvider : IServiceProvider
            {
                public object GetService(Type serviceType)
                {
                    if (serviceType == typeof(IGraphicsDeviceService))
                        return new FakeGraphicsService();

                    throw new NotImplementedException();
                }
            }

            private readonly byte[] _bufferData;

            public TestContentManager(byte[] bufferData)
                : base(new FakeServiceProvider(), "NONE")
            {
                _bufferData = bufferData;
            }

            protected override Stream OpenStream(string assetName)
            {
                return new MemoryStream(_bufferData, false);
            }
        }

        static readonly IReadOnlyCollection<TargetPlatform> Platforms = new[]
        {
            TargetPlatform.Windows,
            TargetPlatform.Xbox360,
            TargetPlatform.iOS,
            TargetPlatform.Android,
            TargetPlatform.DesktopGL,
            TargetPlatform.MacOSX,
            TargetPlatform.WindowsStoreApp,
            TargetPlatform.NativeClient,

            TargetPlatform.RaspberryPi,
            TargetPlatform.PlayStation4,
            TargetPlatform.PlayStation5,
            TargetPlatform.XboxOne,
            TargetPlatform.Switch,
            TargetPlatform.BlazorGL
        };
        static readonly IReadOnlyCollection<GraphicsProfile> GraphicsProfiles = new[]
        {
            GraphicsProfile.HiDef,
            GraphicsProfile.Reach
        };
        static readonly IReadOnlyCollection<ContentCompression> Compression = new[]
        {
            ContentCompression.LegacyLZ4,
            ContentCompression.Uncompressed,
            ContentCompression.LZ4,
#if NET6_0_OR_GREATER
            ContentCompression.Brotli,
#endif
        };

        public static void CompileAndLoadAssets<T>(T data, Action<T> validation)
        {
            ContentCompiler compiler = new ContentCompiler();

            foreach (TargetPlatform platform in Platforms)
                foreach (GraphicsProfile gfxProfile in GraphicsProfiles)
                    foreach (ContentCompression compression in Compression)
                        using (MemoryStream xnbStream = new MemoryStream())
                        {
                            compiler.Compile(xnbStream, data, platform, gfxProfile, compression, "", "");
                            byte[] bufferData = xnbStream.ToArray();
                            using (ContentManager content = new TestContentManager(bufferData))
                            {
                                T result = content.Load<T>("foo");
                                validation(result);
                            }
                        }
        }
    }
}
