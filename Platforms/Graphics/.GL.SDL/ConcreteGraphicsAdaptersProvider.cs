// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Graphics.OpenGL;


namespace Microsoft.Xna.Platform.Graphics
{
    class ConcreteGraphicsAdaptersProvider : GraphicsAdaptersProviderStrategy
    {
        private ReadOnlyCollection<GraphicsAdapter> _adapters;

        public ConcreteGraphicsAdaptersProvider()
        {
            List<GraphicsAdapter> adapterList = CreateAdapterList();
            _adapters = new ReadOnlyCollection<GraphicsAdapter>(adapterList);
        }

        private List<GraphicsAdapter> CreateAdapterList()
        {
            List<GraphicsAdapter> adapterList = new List<GraphicsAdapter>(1);

            ConcreteGraphicsAdapter.InitOpenGL(Sdl.Current,
                out OGL _gl, out GLVersion _glVersion,
                out string _description, out int _capMaxTextureSize, out int _capMaxMultiSampleCount,
                out int _capMaxTextureSlots, out int _capMaxVertexTextureSlots, out int _capMaxVertexAttribs,
                out int _capMaxDrawBuffers
                );

            int displayCount = Sdl.Current.DISPLAY.GetNumVideoDisplays();

            //TODO: get all adapters
            {
                ConcreteGraphicsAdapter adapterStrategy = new ConcreteGraphicsAdapter(
                    _gl, _glVersion,
                    _description, _capMaxTextureSize, _capMaxMultiSampleCount,
                    _capMaxTextureSlots, _capMaxVertexTextureSlots, _capMaxVertexAttribs,
                    _capMaxDrawBuffers);
                GraphicsAdapter adapter = base.CreateGraphicsAdapter(adapterStrategy);

                adapterList.Add(adapter);
            }

            // The first adapter is considered the default.
            ((IPlatformGraphicsAdapter)adapterList[0]).Strategy.Platform_IsDefaultAdapter = true;

            return adapterList;
        }

        public override ReadOnlyCollection<GraphicsAdapter> Platform_Adapters
        {
            get { return _adapters; }
        }

        public override GraphicsAdapter Platform_DefaultAdapter
        {
            get { return _adapters[0]; }
        }
        
    }
}
