// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Platform.Graphics
{
    public interface IPlatformVertexDeclaration
    {
        VertexElement[] InternalVertexElements { get; }
    }
}
