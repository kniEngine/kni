// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// This class handles the queueing of batch items into the GPU by creating the triangle tesselations
    /// that are used to draw the sprite textures. This class supports int.MaxValue number of sprites to be
    /// batched and will process them into short.MaxValue groups (strided by 6 for the number of vertices
    /// sent to the GPU). 
    /// </summary>
    internal class SpriteBatcher : IDisposable
    {
        /*
         * Note that this class is fundamental to high performance for SpriteBatch games. Please exercise
         * caution when making changes to this class.
         */

        /// <summary>
        /// Initialization size for the batch item list and queue.
        /// </summary>
        private const int InitialBatchSize = 256;
        /// <summary>
        /// The maximum number of batch items that can be processed per iteration
        /// </summary>
        // the upper limit is the range of 16bit indices, (ushort.MaxValue+1)/4 = 16384 vertices per quad.
        // or (short.MaxValue+1)/4 = 8192 if we are using shorts instead of usigned shorts.
        private const int MaxBatchSize = 4096;

        /// <summary>
        /// The list of batch items to process.
        /// </summary>
        private SpriteBatchItem[] _batchItemList;
        /// <summary>
        /// Index pointer to the next available SpriteBatchItem in _batchItemList.
        /// </summary>
        private int _batchItemCount;

        /// <summary>
        /// The target graphics device.
        /// </summary>
        private readonly GraphicsDevice _device;

        /// <summary>
        /// Vertex index array. The values in this array never change.
        /// </summary>
        private short[] _index;

        private VertexPositionColorTexture[] _vertexArray;

        public SpriteBatcher(GraphicsDevice device, int capacity = 0)
        {
            _device = device;

            if (capacity <= 0)
                capacity = InitialBatchSize;
            else
                capacity = (capacity + 63) & (~63); // ensure chunks of 64.

            _batchItemList = new SpriteBatchItem[capacity];
            _batchItemCount = 0;

            for (int i = 0; i < capacity; i++)
                _batchItemList[i] = new SpriteBatchItem();

            EnsureArrayCapacity(capacity);
        }

        /// <summary>
        /// Reuse a previously allocated SpriteBatchItem from the item pool. 
        /// if there is none available grow the pool and initialize new items.
        /// </summary>
        /// <returns></returns>
        public SpriteBatchItem CreateBatchItem()
        {
            if (_batchItemCount < _batchItemList.Length)
            {
                return _batchItemList[_batchItemCount++];
            }
            else
            {
                var oldSize = _batchItemList.Length;
                var newSize = oldSize + oldSize / 2; // grow by x1.5
                newSize = (newSize + 63) & (~63); // grow in chunks of 64.
                Array.Resize(ref _batchItemList, newSize);
                for (int i = oldSize; i < newSize; i++)
                    _batchItemList[i] = new SpriteBatchItem();

                EnsureArrayCapacity(Math.Min(newSize, MaxBatchSize));
                return _batchItemList[_batchItemCount++];
            }
        }

        /// <summary>
        /// Resize and recreate the missing indices for the index and vertex position color buffers.
        /// </summary>
        /// <param name="numBatchItems"></param>
        private unsafe void EnsureArrayCapacity(int numBatchItems)
        {
            int neededCapacity = 6 * numBatchItems;
            if (_index != null && neededCapacity <= _index.Length)
            {
                // Short circuit out of here because we have enough capacity.
                return;
            }
            short[] newIndex = new short[6 * numBatchItems];
            int start = 0;
            if (_index != null)
            {
                _index.CopyTo(newIndex, 0);
                start = _index.Length / 6;
            }
            fixed (short* indexFixedPtr = newIndex)
            {
                var indexPtr = indexFixedPtr + (start * 6);
                for (var i = start; i < numBatchItems; i++, indexPtr += 6)
                {
                    /*
                     *  TL    TR
                     *   0----1 0,1,2,3 = index offsets for vertex indices
                     *   |   /| TL,TR,BL,BR are vertex references in SpriteBatchItem.
                     *   |  / |
                     *   | /  |
                     *   |/   |
                     *   2----3
                     *  BL    BR
                     */
                    // Triangle 1
                    *(indexPtr + 0) = (short)(i * 4);
                    *(indexPtr + 1) = (short)(i * 4 + 1);
                    *(indexPtr + 2) = (short)(i * 4 + 2);
                    // Triangle 2
                    *(indexPtr + 3) = (short)(i * 4 + 1);
                    *(indexPtr + 4) = (short)(i * 4 + 3);
                    *(indexPtr + 5) = (short)(i * 4 + 2);
                }
            }
            _index = newIndex;

            _vertexArray = new VertexPositionColorTexture[4 * numBatchItems];
        }

        /// <summary>
        /// Sorts the batch items and then groups batch drawing into maximal allowed batch sets that do not
        /// overflow the 16 bit array indices for vertices.
        /// </summary>
        /// <param name="sortMode">The type of depth sorting desired for the rendering.</param>
        /// <param name="effect">The custom effect to apply to the drawn geometry</param>
        public unsafe void DrawBatch(SpriteSortMode sortMode, Effect effect)
        {
            if (effect != null && effect.IsDisposed)
                throw new ObjectDisposedException("effect");

            // nothing to do
            if (_batchItemCount == 0)
                return;

            // sort the batch items
            switch (sortMode)
            {
                case SpriteSortMode.Texture:
                case SpriteSortMode.FrontToBack:
                case SpriteSortMode.BackToFront:
                    Array.Sort(_batchItemList, 0, _batchItemCount);
                    break;
            }

            // Determine how many iterations through the drawing code we need to make
            int batchIndex = 0;
            int batchCount = _batchItemCount;

            unchecked { _device._graphicsMetrics._spriteCount += batchCount; }

            // Iterate through the batches, doing short.MaxValue sets of vertices only.
            while (batchCount > 0)
            {
                int baseQuad = 0;
                int spriteCount = 0;
                Texture2D tex = null;


                int numBatchesToProcess = batchCount;
                if (numBatchesToProcess > MaxBatchSize)
                {
                    numBatchesToProcess = MaxBatchSize;
                }

                // Avoid the array checking overhead by using pointer indexing!
                fixed (VertexPositionColorTexture* vertexArrayFixedPtr = _vertexArray)
                {
                    var vertexArrayPtr = vertexArrayFixedPtr;

                    // create batch
                    for (int i = 0; i < numBatchesToProcess; i++, vertexArrayPtr += 4)
                    {
                        SpriteBatchItem item = _batchItemList[batchIndex + i];

                        // store the SpriteBatchItem data in our vertexArray
                        *(vertexArrayPtr + 0) = item.vertexTL;
                        *(vertexArrayPtr + 1) = item.vertexTR;
                        *(vertexArrayPtr + 2) = item.vertexBL;
                        *(vertexArrayPtr + 3) = item.vertexBR;
                    }
                }

                // draw batch
                for (int i = 0; i < numBatchesToProcess; i++, spriteCount++)
                {
                    SpriteBatchItem item = _batchItemList[batchIndex++];

                    // if the texture changed, we need to flush and bind the new texture
                    var shouldFlush = !ReferenceEquals(item.Texture, tex);
                    if (shouldFlush)
                    {
                        if (spriteCount > 0)
                            FlushVertexArray(baseQuad, spriteCount, effect, tex);

                        baseQuad += spriteCount;
                        spriteCount = 0;
                        tex = item.Texture;
                    }

                    // Release the texture.
                    item.Texture = null;
                }
                // flush the remaining vertexArray data
                if (spriteCount > 0)
                    FlushVertexArray(baseQuad, spriteCount, effect, tex);

                // Update our batch count to continue the process of culling down
                // large batches
                batchCount -= numBatchesToProcess;
            }
            // return items to the pool.
            _batchItemCount = 0;
        }

        /// <summary>
        /// Sends the triangle list to the graphics device. Here is where the actual drawing starts.
        /// </summary>
        /// <param name="spriteCount">The number of sprites to draw.</param>
        /// <param name="effect">The custom effect to apply to the geometry.</param>
        /// <param name="texture">The texture to draw.</param>
        private void FlushVertexArray(int baseQuad, int spriteCount, Effect effect, Texture texture)
        {
            int baseVertex = baseQuad * 4;
            int numVertices = spriteCount * 4;
            int primitiveCount = spriteCount * 2;

            if (effect == null) // If no custom effect is defined, then simply render.
            {
                _device.Textures[0] = texture;

                _device.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    _vertexArray, baseVertex, numVertices,
                    _index, 0, primitiveCount,
                    VertexPositionColorTexture.VertexDeclaration);
            }
            else // If the effect is not null, then apply each pass and render the geometry
            {
                var passes = effect.CurrentTechnique.Passes;
                foreach (var pass in passes)
                {
                    pass.Apply();

                    // We have to set the texture again on each pass,
                    // because pass.Apply() might have set a texture from the effect.
                    _device.Textures[0] = texture;

                    _device.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        _vertexArray, baseVertex, numVertices,
                        _index, 0, primitiveCount,
                        VertexPositionColorTexture.VertexDeclaration);
                }
            }
        }

        #region IDisposable Members

        ~SpriteBatcher()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected bool isDisposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    _batchItemList = null;
                    _vertexArray = null;
                    _index = null;
                }
                isDisposed = true;
            }
        }

        #endregion
    }
}

