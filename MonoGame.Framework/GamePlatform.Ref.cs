// Copyright (C)2023 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        public ConcreteGame(Game game) : base(game)
        {
            // register factories
            try { Microsoft.Xna.Platform.TitleContainerFactory.RegisterTitleContainerFactory(new Microsoft.Xna.Platform.ConcreteTitleContainerFactory()); }
            catch (InvalidOperationException) { }
            try { Microsoft.Xna.Platform.Graphics.GraphicsFactory.RegisterGraphicsFactory(new Microsoft.Xna.Platform.Graphics.ConcreteGraphicsFactory()); }
            catch (InvalidOperationException) { }
            try { Microsoft.Xna.Platform.Audio.AudioFactory.RegisterAudioFactory(new Microsoft.Xna.Platform.Audio.ConcreteAudioFactory()); }
            catch (InvalidOperationException) { }
            try { Microsoft.Xna.Platform.Media.MediaFactory.RegisterMediaFactory(new Microsoft.Xna.Platform.Media.ConcreteMediaFactory()); }
            catch (InvalidOperationException) { }

        }

        internal override void Run()
        {
            throw new PlatformNotSupportedException();
        }

        public override void Tick()
        {
            throw new PlatformNotSupportedException();
        }

        public override void BeforeInitialize()
        {
            throw new PlatformNotSupportedException();
        }

        public override void TickExiting()
        {
            throw new PlatformNotSupportedException();
        }

        public override bool BeforeUpdate()
        {
            throw new PlatformNotSupportedException();
        }

        internal override void OnPresentationChanged(PresentationParameters pp)
        {
            throw new PlatformNotSupportedException();
        }
    }
}
