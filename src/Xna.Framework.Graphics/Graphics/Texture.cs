// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Platform.Graphics;

namespace Microsoft.Xna.Framework.Graphics
{
    public abstract class Texture : GraphicsResource
        , IPlatformTexture
    {
        protected ITextureStrategy _strategyTexture;
        
        private readonly int _sortingKey = Interlocked.Increment(ref _lastSortingKey);
        private static int _lastSortingKey;

        /// <summary>
        /// Gets a unique identifier of this texture for sorting purposes.
        /// </summary>
        /// <remarks>
        /// <para>For example, this value is used by <see cref="SpriteBatch"/> when drawing with <see cref="SpriteSortMode.Texture"/>.</para>
        /// <para>The value is an implementation detail and may change between application launches or MonoGame versions.
        /// It is only guaranteed to stay consistent during application lifetime.</para>
        /// </remarks>
        internal int SortingKey { get { return _sortingKey; } }

        public SurfaceFormat Format { get { return _strategyTexture.Format; } }		
        public int LevelCount { get { return _strategyTexture.LevelCount; } }


        protected Texture()
            : base()
        {
        }

        
        T IPlatformTexture.GetTextureStrategy<T>()
        {
            return (T)_strategyTexture;
        }

    }
}

