// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    public sealed class ConcreteTitleContainerFactory : TitleContainerFactory
    {
        public override TitleContainerStrategy CreateTitleContainerStrategy()
        {
            return new ConcreteTitleContainer();
        }
    }
}
