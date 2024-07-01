// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;
using Microsoft.Xna.Framework.Content;
#if DIRECTX || XNA
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endif
using Microsoft.Xna.Framework.Graphics;

namespace Kni.Tests.ContentPipeline
{
    internal static class AssetTestUtility
    {

        public static Effect LoadEffect(ContentManager content, string effectName)
        {
#if DIRECTX
            GraphicsDevice gd = ((IGraphicsDeviceService)content.ServiceProvider.GetService(typeof(IGraphicsDeviceService))).GraphicsDevice;
            
            Effect effect = AssetTestUtility.CompileEffect(gd, Paths.RawEffect(effectName));
            return effect;
#else
            Effect effect = content.Load<Effect>(Paths.CompiledEffect(effectName));
            return effect;
#endif
        }

        public static Effect CompileEffect(GraphicsDevice graphicsDevice, string effectPath)
        {
#if DIRECTX || XNA

            TargetPlatform targetPlatform = TargetPlatform.Windows;

            EffectProcessor effectProcessor = new EffectProcessor();
            ContentProcessorContext context = new TestProcessorContext(targetPlatform, "notused.xnb");
            EffectContent effectContent = new EffectContent();
            effectContent.EffectCode = File.ReadAllText(effectPath);
            effectContent.Identity = new ContentIdentity(effectPath);

            CompiledEffectContent compiledEffect = effectProcessor.Process(effectContent, context);
            byte[] effectCode = compiledEffect.GetEffectCode();
            return new Effect(graphicsDevice, effectCode);
#else // OpenGL
            throw new NotImplementedException();
#endif
        }
    }
}