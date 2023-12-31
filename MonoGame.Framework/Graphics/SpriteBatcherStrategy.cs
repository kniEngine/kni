// Copyright (C)2023 Nick Kastellanos

using System;

namespace Microsoft.Xna.Framework.Graphics
{
    internal abstract class SpriteBatcherStrategy : IDisposable
    {

        public abstract int BatchItemCount { get; }

        public abstract SpriteBatchItem CreateBatchItem();
        public abstract void SortBatch(SpriteSortMode sortMode);
        public abstract void DrawBatch(Effect effect);


        #region IDisposable Members

        ~SpriteBatcherStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);

        #endregion IDisposable Members
    }
}