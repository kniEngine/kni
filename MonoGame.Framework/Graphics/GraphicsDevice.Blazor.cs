// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Graphics;
using nkast.Wasm.Canvas.WebGL;


namespace Microsoft.Xna.Framework.Graphics
{
    public partial class GraphicsDevice
    {

        private void PlatformSetup()
        {
            // create context.
            _strategy._mainContext = new GraphicsContext(this);
            GraphicsExtensions.GL = ((ConcreteGraphicsContext)_strategy._mainContext.Strategy).GlContext; // for GraphicsExtensions.CheckGLError()
            //_glContext = new LogContent(_glContext);

            Strategy._capabilities = new GraphicsCapabilities();
            Strategy._capabilities.PlatformInitialize(this);


            ((ConcreteGraphicsContext)_strategy._mainContext.Strategy)._newEnabledVertexAttributes = new bool[Strategy.Capabilities.MaxVertexBufferSlots];
        }


        private void PlatformDispose()
        {
        }

        internal void OnPresentationChanged()
        {
            _strategy._mainContext.ApplyRenderTargets(null);
        }

    }
}
