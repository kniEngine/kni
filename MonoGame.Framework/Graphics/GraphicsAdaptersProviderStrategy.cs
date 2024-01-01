// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework.Graphics;


namespace Microsoft.Xna.Platform.Graphics
{
    public abstract class GraphicsAdaptersProviderStrategy
    {
        private static GraphicsAdaptersProviderStrategy _current;

        internal static GraphicsAdaptersProviderStrategy Current
        {
            get
            {
                lock (typeof(GraphicsAdaptersProviderStrategy))
                {
                    if (_current == null)
                        _current = GraphicsFactory.Current.CreateGraphicsAdaptersProviderStrategy();

                    return _current;
                }
            }
        }

        public abstract ReadOnlyCollection<GraphicsAdapter> Platform_Adapters { get; }
        public abstract GraphicsAdapter Platform_DefaultAdapter { get; }
        public virtual bool Platform_UseReferenceDevice { get; set; }
        

        internal T ToConcrete<T>() where T : GraphicsAdaptersProviderStrategy
        {
            return (T)this;
        }

    }
}
