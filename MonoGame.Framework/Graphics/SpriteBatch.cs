// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2022-2023 Nick Kastellanos

using System;
using System.Text;


namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Helper class for drawing text strings and sprites in one or more optimized batches.
    /// </summary>
    public class SpriteBatch : GraphicsResource
    {
        #region Private Fields
        readonly SpriteBatcher _batcher;

        SpriteSortMode _sortMode;
        BlendState _blendState;
        SamplerState _samplerState;
        DepthStencilState _depthStencilState;
        RasterizerState _rasterizerState;
        Effect _effect;
        bool _beginCalled;

        SpriteEffect _spriteEffect;
        readonly EffectPass _spritePass;

        Rectangle _tempRect = new Rectangle(0, 0, 0, 0);
        Vector2 _texCoordTL = new Vector2(0, 0);
        Vector2 _texCoordBR = new Vector2(0, 0);
        #endregion

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>        
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice) : this(graphicsDevice, 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SpriteBatch"/>.
        /// </summary>
        /// <param name="graphicsDevice">The <see cref="GraphicsDevice"/>, which will be used for sprite rendering.</param>
        /// <param name="capacity">The initial capacity of the internal array holding batch items (the value will be rounded to the next multiple of 64).</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="graphicsDevice"/> is null.</exception>
        public SpriteBatch(GraphicsDevice graphicsDevice, int capacity)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            SetGraphicsDevice(graphicsDevice);

            _spriteEffect = new SpriteEffect(graphicsDevice);
            _spritePass = _spriteEffect.CurrentTechnique.Passes[0];

            _batcher = new SpriteBatcher(graphicsDevice, capacity);

            _beginCalled = false;
        }

        /// <summary>
        /// Begins a new sprite and text batch with the specified render state.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="SpriteSortMode.Deferred"/> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</param>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> is called next time without previous <see cref="End"/>.</exception>
        /// <remarks>This method uses optional parameters.</remarks>
        /// <remarks>The <see cref="Begin"/> Begin should be called before drawing commands, and you cannot call it again before subsequent <see cref="End"/>.</remarks>
        public void Begin
        (
             SpriteSortMode sortMode = SpriteSortMode.Deferred,
             BlendState blendState = null,
             SamplerState samplerState = null,
             DepthStencilState depthStencilState = null,
             RasterizerState rasterizerState = null,
             Effect effect = null,
             Matrix? transformMatrix = null
        )
        {
            if (_beginCalled)
                throw new InvalidOperationException("Begin cannot be called again until End has been successfully called.");

            // defaults
            _sortMode = sortMode;
            _blendState = blendState ?? BlendState.AlphaBlend;
            _samplerState = samplerState ?? SamplerState.LinearClamp;
            _depthStencilState = depthStencilState ?? DepthStencilState.None;
            _rasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            _effect = effect;
            _spriteEffect.TransformMatrix = transformMatrix;

            // Setup things now so a user can change them.
            if (sortMode == SpriteSortMode.Immediate)
            {
                Setup();
            }

            _beginCalled = true;
        }

        /// <summary>
        /// Flushes all batched text and sprites to the screen.
        /// </summary>
        /// <remarks>This command should be called after <see cref="Begin"/> and drawing commands.</remarks>
        public void End()
        {
            if (!_beginCalled)
                throw new InvalidOperationException("Begin must be called before calling End.");

            _beginCalled = false;

            if (_sortMode != SpriteSortMode.Immediate)
            {
                Setup();

                if (_batcher.BatchItemCount > 0)
                {
                    _batcher.SortBatch(_sortMode);
                    _batcher.DrawBatch(_effect);
                }
            }
        }

        void Setup()
        {
            var gd = GraphicsDevice;
            gd.BlendState = _blendState;
            gd.DepthStencilState = _depthStencilState;
            gd.RasterizerState = _rasterizerState;
            gd.SamplerStates[0] = _samplerState;

            _spritePass.Apply();
        }

        void CheckValid(Texture2D texture)
        {
            if (texture == null)
                throw new ArgumentNullException("texture");
            if (!_beginCalled)
                throw new InvalidOperationException("Draw was called, but Begin has not yet been called. Begin must be called successfully before you can call Draw.");
        }

        void CheckValid(SpriteFont spriteFont, string text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        void CheckValid(SpriteFont spriteFont, StringBuilder text)
        {
            if (spriteFont == null)
                throw new ArgumentNullException("spriteFont");
            if (text == null)
                throw new ArgumentNullException("text");
            if (!_beginCalled)
                throw new InvalidOperationException("DrawString was called, but Begin has not yet been called. Begin must be called successfully before you can call DrawString.");
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public void Draw(Texture2D texture,
                Vector2 position,
                Rectangle? sourceRectangle,
                Color color,
                float rotation,
                Vector2 origin,
                Vector2 scale,
                SpriteEffects effects,
                float layerDepth)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            Draw(texture, position, sourceRectangle, color, rotation2, origin, scale, effects, layerDepth);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public void Draw(Texture2D texture,
                Vector2 position,
                Rectangle? sourceRectangle,
                Color color,
                Complex rotation,
                Vector2 origin,
                Vector2 scale,
                SpriteEffects effects,
                float layerDepth)
        {
            CheckValid(texture);            

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }

            origin = origin * scale;

            float w, h;
            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                w = srcRect.Width * scale.X;
                h = srcRect.Height * scale.Y;
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                w = texture.Width * scale.X;
                h = texture.Height * scale.Y;
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            if ((effects & SpriteEffects.FlipVertically) != 0)
            {
                var temp = _texCoordBR.Y;
                _texCoordBR.Y = _texCoordTL.Y;
                _texCoordTL.Y = temp;
            }
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
                _texCoordBR.X = _texCoordTL.X;
                _texCoordTL.X = temp;
            }

            if (rotation.i == 0f)
            {
                item.Set(position.X - origin.X,
                         position.Y - origin.Y,
                         w,
                         h,
                         layerDepth,
                         color,
                         _texCoordTL,
                         _texCoordBR);
            }
            else
            {
                item.Set(position.X,
                         position.Y,
                         -origin.X,
                         -origin.Y,
                         w,
                         h,
                         ref rotation,
                         layerDepth,
                         color,
                         _texCoordTL,
                         _texCoordBR);
            }

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public void Draw(Texture2D texture,
                Vector2 position,
                Rectangle? sourceRectangle,
                Color color,
                float rotation,
                Vector2 origin,
                float scale,
                SpriteEffects effects,
                float layerDepth)
        {
            var scaleVec = new Vector2(scale, scale);
            Draw(texture, position, sourceRectangle, color, rotation, origin, scaleVec, effects, layerDepth);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public void Draw(Texture2D texture,
            Rectangle destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,
            float rotation,
            Vector2 origin,
            SpriteEffects effects,
            float layerDepth)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            Draw(texture, destinationRectangle, sourceRectangle, color, rotation2, origin, effects, layerDepth);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        public void Draw(Texture2D texture,
            Rectangle destinationRectangle,
            Rectangle? sourceRectangle,
            Color color,
            Complex rotation,
            Vector2 origin,
            SpriteEffects effects,
            float layerDepth)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    item.SortKey = texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    item.SortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    item.SortKey = -layerDepth;
                    break;
            }

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;

                if (srcRect.Width != 0)
                    origin.X = origin.X * (float)destinationRectangle.Width / (float)srcRect.Width;
                else
                    origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                if (srcRect.Height != 0)
                    origin.Y = origin.Y * (float)destinationRectangle.Height / (float)srcRect.Height;
                else
                    origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;

                origin.X = origin.X * (float)destinationRectangle.Width * texture.TexelWidth;
                origin.Y = origin.Y * (float)destinationRectangle.Height * texture.TexelHeight;
            }

            if ((effects & SpriteEffects.FlipVertically) != 0)
            {
                var temp = _texCoordBR.Y;
                _texCoordBR.Y = _texCoordTL.Y;
                _texCoordTL.Y = temp;
            }
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
            {
                var temp = _texCoordBR.X;
                _texCoordBR.X = _texCoordTL.X;
                _texCoordTL.X = temp;
            }

            if (rotation.i == 0f)
            {
                item.Set(destinationRectangle.X - origin.X,
                         destinationRectangle.Y - origin.Y,
                         destinationRectangle.Width,
                         destinationRectangle.Height,
                         layerDepth,
                         color,
                         _texCoordTL,
                         _texCoordBR);
            }
            else
            {
                item.Set(destinationRectangle.X,
                         destinationRectangle.Y,
                         -origin.X,
                         -origin.Y,
                         destinationRectangle.Width,
                         destinationRectangle.Height,
                         ref rotation,
                         layerDepth,
                         color,
                         _texCoordTL,
                         _texCoordBR);
            }

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            Vector2 size;

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                size = new Vector2(srcRect.Width, srcRect.Height);
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                size = new Vector2(texture.Width, texture.Height);
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            item.Set(position.X,
                     position.Y,
                     size.X,
                     size.Y,
                     0,
                     color,
                     _texCoordTL,
                     _texCoordBR);

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            if (sourceRectangle.HasValue)
            {
                var srcRect = sourceRectangle.GetValueOrDefault();
                _texCoordTL.X = srcRect.X * texture.TexelWidth;
                _texCoordTL.Y = srcRect.Y * texture.TexelHeight;
                _texCoordBR.X = (srcRect.X + srcRect.Width) * texture.TexelWidth;
                _texCoordBR.Y = (srcRect.Y + srcRect.Height) * texture.TexelHeight;
            }
            else
            {
                _texCoordTL = Vector2.Zero;
                _texCoordBR = Vector2.One;
            }

            item.Set(destinationRectangle.X,
                     destinationRectangle.Y,
                     destinationRectangle.Width,
                     destinationRectangle.Height,
                     0,
                     color,
                     _texCoordTL,
                     _texCoordBR);

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Vector2 position, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            item.Set(position.X,
                     position.Y,
                     texture.Width,
                     texture.Height,
                     0,
                     color,
                     Vector2.Zero,
                     Vector2.One);

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a sprite for drawing in the current batch.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Draw(Texture2D texture, Rectangle destinationRectangle, Color color)
        {
            CheckValid(texture);

            var item = _batcher.CreateBatchItem();
            item.Texture = texture;

            // set SortKey based on SpriteSortMode.
            item.SortKey = _sortMode == SpriteSortMode.Texture ? texture.SortingKey : 0;

            item.Set(destinationRectangle.X,
                     destinationRectangle.Y,
                     destinationRectangle.Width,
                     destinationRectangle.Height,
                     0,
                     color,
                     Vector2.Zero,
                     Vector2.One);

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public unsafe void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public unsafe void DrawString(SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color)
        {
            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color);
        }

        private unsafe void DrawString(SpriteFont spriteFont, char* pChars, int* pGlyphIndices, int charsCount, Vector2 position, Color color)
        {
            // set SortKey based on SpriteSortMode.
            float sortKey = (_sortMode == SpriteSortMode.Texture) ? spriteFont.Texture.SortingKey : 0;

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.InternalGlyphs)
            {
                for (var i = 0; i < charsCount; i++)
                {
                    var c = pChars[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    int currentGlyphIndex = pGlyphIndices[i];
                    if (currentGlyphIndex == -1)
                        throw new ArgumentException(SpriteFont.UnresolvableCharacterErrorMessage(c), "text");
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                    }

                    var p = offset;
                    p.X += pCurrentGlyph->Cropping.X;
                    p.Y += pCurrentGlyph->Cropping.Y;
                    p += position;

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    item.Set(p.X,
                             p.Y,
                             pCurrentGlyph->BoundsInTexture.Width,
                             pCurrentGlyph->BoundsInTexture.Height,
                             0,
                             color,
                             pCurrentGlyph->TexCoordTL,
                             pCurrentGlyph->TexCoordBR
                             );

                    offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
                }
            }

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            Vector2 scale2;
            scale2.X = scale;
            scale2.Y = scale;

            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation2, origin, scale2, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation2, origin, scale, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            Complex rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation, origin, scale, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            Vector2 scale2;
            scale2.X = scale;
            scale2.Y = scale;

            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation2, origin, scale2, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation2, origin, scale, effects, layerDepth);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            Complex rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation, origin, scale, effects, layerDepth);
        }

        private unsafe void DrawString(
            SpriteFont spriteFont, char* pChars, int* pGlyphIndices, int charsCount, Vector2 position, Color color,
            ref Complex rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
        {
            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    sortKey = spriteFont.Texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    sortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    sortKey = -layerDepth;
                    break;
            }

            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = (effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally;

            if (flippedVert || flippedHorz)
            {
                var size = spriteFont.MeasureString(pChars, pGlyphIndices, charsCount);

                if (flippedHorz)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }

                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }

            Matrix transformation = Matrix.Identity;
            if (rotation.i == 0)
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
                transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + position.X;
                transformation.M42 = (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
            }
            else
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) *  rotation.R;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) *  rotation.i;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * -rotation.i;
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) *  rotation.R;
                transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + (flipAdjustment.Y - origin.Y) * transformation.M21 + position.X;
                transformation.M42 = (flipAdjustment.X - origin.X) * transformation.M12 + (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
            }

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.InternalGlyphs)
            {
                for (var i = 0; i < charsCount; i++)
                {
                    var c = pChars[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    int currentGlyphIndex = pGlyphIndices[i];
                    if (currentGlyphIndex == -1)
                        throw new ArgumentException(SpriteFont.UnresolvableCharacterErrorMessage(c), "text");
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + pCurrentGlyph->LeftSideBearing;
                    }

                    var p = offset;

                    if (flippedHorz)
                        p.X += pCurrentGlyph->BoundsInTexture.Width;
                    p.X += pCurrentGlyph->Cropping.X;

                    if (flippedVert)
                        p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                    p.Y += pCurrentGlyph->Cropping.Y;

                    //Opt: Vector2.Transform(ref p, ref transformation, out p);
                    float tpx = (p.X * transformation.M11);
                    float tpy = (p.X * transformation.M12);
                    tpx += (p.Y * transformation.M21) + transformation.M41;
                    tpy += (p.Y * transformation.M22) + transformation.M42;

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    _texCoordTL = pCurrentGlyph->TexCoordTL;
                    _texCoordBR = pCurrentGlyph->TexCoordBR;

                    if ((effects & SpriteEffects.FlipVertically) != 0)
                    {
                        var temp = _texCoordBR.Y;
                        _texCoordBR.Y = _texCoordTL.Y;
                        _texCoordTL.Y = temp;
                    }
                    if ((effects & SpriteEffects.FlipHorizontally) != 0)
                    {
                        var temp = _texCoordBR.X;
                        _texCoordBR.X = _texCoordTL.X;
                        _texCoordTL.X = temp;
                    }

                    if (rotation.i == 0f)
                    {
                        item.Set(tpx,
                                 tpy,
                                 pCurrentGlyph->BoundsInTexture.Width * scale.X,
                                 pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                 layerDepth,
                                 color,
                                 _texCoordTL,
                                 _texCoordBR);
                    }
                    else
                    {
                        item.Set(tpx,
                                 tpy,
                                 0,
                                 0,
                                 pCurrentGlyph->BoundsInTexture.Width * scale.X,
                                 pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                 ref rotation,
                                 layerDepth,
                                 color,
                                 _texCoordTL,
                                 _texCoordBR);
                    }

                    offset.X += pCurrentGlyph->Width + pCurrentGlyph->RightSideBearing;
                }
            }

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <param name="rtl">Text is Right to Left.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, string text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool rtl)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                       ref rotation2, origin, scale, effects, layerDepth, rtl);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <param name="rtl">Text is Right to Left.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool rtl)
        {
            Complex rotation2;
            if (rotation == 0f)
            {
                rotation2.R = 1f;
                rotation2.i = 0f;
            }
            else
            {
                rotation2.R = (float)Math.Cos(rotation);
                rotation2.i = (float)Math.Sin(rotation);
            }

            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                ref rotation2, origin, scale, effects, layerDepth, rtl);
        }

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="spriteFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        /// <param name="rtl">Text is Right to Left.</param>
        public unsafe void DrawString(
            SpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
            Complex rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool rtl)
        {
            CheckValid(spriteFont, text);

            char* pChars = stackalloc char[text.Length];
            int* pGlyphIndices = stackalloc int[text.Length];
            spriteFont.GetGlyphIndexes(text, pChars, pGlyphIndices, text.Length);
            DrawString(spriteFont, pChars, pGlyphIndices, text.Length, position, color,
                ref rotation, origin, scale, effects, layerDepth, rtl);
        }

        private unsafe void DrawString(
            SpriteFont spriteFont, char* pChars, int* pGlyphIndices, int charsCount, Vector2 position, Color color,
            ref Complex rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth, bool rtl)
        {
            float sortKey = 0;
            // set SortKey based on SpriteSortMode.
            switch (_sortMode)
            {
                // Comparison of Texture objects.
                case SpriteSortMode.Texture:
                    sortKey = spriteFont.Texture.SortingKey;
                    break;
                // Comparison of Depth
                case SpriteSortMode.FrontToBack:
                    sortKey = layerDepth;
                    break;
                // Comparison of Depth in reverse
                case SpriteSortMode.BackToFront:
                    sortKey = -layerDepth;
                    break;
            }

            var flipAdjustment = Vector2.Zero;

            var flippedVert = (effects & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically;
            var flippedHorz = ((effects & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally) ^ rtl;

            if (flippedVert || flippedHorz || rtl)
            {
                var size = spriteFont.MeasureString(pChars, pGlyphIndices, charsCount);

                if (flippedHorz ^ rtl)
                {
                    origin.X *= -1;
                    flipAdjustment.X = -size.X;
                }
                if (flippedVert)
                {
                    origin.Y *= -1;
                    flipAdjustment.Y = spriteFont.LineSpacing - size.Y;
                }
            }

            Matrix transformation = Matrix.Identity;
            if (rotation.i == 0)
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X);
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y);
                transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + position.X;
                transformation.M42 = (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
            }
            else
            {
                transformation.M11 = (flippedHorz ? -scale.X : scale.X) *  rotation.R;
                transformation.M12 = (flippedHorz ? -scale.X : scale.X) *  rotation.i;
                transformation.M21 = (flippedVert ? -scale.Y : scale.Y) * -rotation.i;
                transformation.M22 = (flippedVert ? -scale.Y : scale.Y) *  rotation.R;
                transformation.M41 = (flipAdjustment.X - origin.X) * transformation.M11 + (flipAdjustment.Y - origin.Y) * transformation.M21 + position.X;
                transformation.M42 = (flipAdjustment.X - origin.X) * transformation.M12 + (flipAdjustment.Y - origin.Y) * transformation.M22 + position.Y;
            }

            var offset = Vector2.Zero;
            var firstGlyphOfLine = true;

            fixed (SpriteFont.Glyph* pGlyphs = spriteFont.InternalGlyphs)
            {
                for (var i = 0; i < charsCount; i++)
                {
                    var c = pChars[i];

                    if (c == '\r')
                        continue;

                    if (c == '\n')
                    {
                        offset.X = 0;
                        offset.Y += spriteFont.LineSpacing;
                        firstGlyphOfLine = true;
                        continue;
                    }

                    int currentGlyphIndex = pGlyphIndices[i];
                    if (currentGlyphIndex == -1)
                        throw new ArgumentException(SpriteFont.UnresolvableCharacterErrorMessage(c), "text");
                    var pCurrentGlyph = pGlyphs + currentGlyphIndex;

                    // The first character on a line might have a negative left side bearing.
                    // In this scenario, SpriteBatch/SpriteFont normally offset the text to the right,
                    //  so that text does not hang off the left side of its rectangle.
                    if (firstGlyphOfLine)
                    {
                        offset.X = Math.Max(rtl ? pCurrentGlyph->RightSideBearing : pCurrentGlyph->LeftSideBearing, 0);
                        firstGlyphOfLine = false;
                    }
                    else
                    {
                        offset.X += spriteFont.Spacing + (rtl ? pCurrentGlyph->RightSideBearing : pCurrentGlyph->LeftSideBearing);
                    }

                    var p = offset;

                    if (flippedHorz)
                        p.X += pCurrentGlyph->BoundsInTexture.Width;
                    p.X += pCurrentGlyph->Cropping.X;

                    if (flippedVert)
                        p.Y += pCurrentGlyph->BoundsInTexture.Height - spriteFont.LineSpacing;
                    p.Y += pCurrentGlyph->Cropping.Y;

                    //Opt: Vector2.Transform(ref p, ref transformation, out p);
                    float tpx = (p.X * transformation.M11);
                    float tpy = (p.X * transformation.M12);
                    tpx += (p.Y * transformation.M21) + transformation.M41;
                    tpy += (p.Y * transformation.M22) + transformation.M42;

                    var item = _batcher.CreateBatchItem();
                    item.Texture = spriteFont.Texture;
                    item.SortKey = sortKey;

                    _texCoordTL = pCurrentGlyph->TexCoordTL;
                    _texCoordBR = pCurrentGlyph->TexCoordBR;

                    if ((effects & SpriteEffects.FlipVertically) != 0)
                    {
                        var temp = _texCoordBR.Y;
                        _texCoordBR.Y = _texCoordTL.Y;
                        _texCoordTL.Y = temp;
                    }
                    if ((effects & SpriteEffects.FlipHorizontally) != 0)
                    {
                        var temp = _texCoordBR.X;
                        _texCoordBR.X = _texCoordTL.X;
                        _texCoordTL.X = temp;
                    }

                    if (rotation.i == 0f)
                    {
                        item.Set(tpx,
                                 tpy,
                                 pCurrentGlyph->BoundsInTexture.Width  * scale.X,
                                 pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                 layerDepth,
                                 color,
                                 _texCoordTL, _texCoordBR);
                    }
                    else
                    {
                        item.Set(tpx,
                                 tpy,
                                 0,
                                 0,
                                 pCurrentGlyph->BoundsInTexture.Width  * scale.X,
                                 pCurrentGlyph->BoundsInTexture.Height * scale.Y,
                                 ref rotation,
                                 layerDepth,
                                 color,
                                 _texCoordTL,
                                 _texCoordBR);
                    }

                    offset.X += pCurrentGlyph->Width + (rtl ? pCurrentGlyph->LeftSideBearing : pCurrentGlyph->RightSideBearing);
                }
            }

            if (_sortMode == SpriteSortMode.Immediate)
                _batcher.DrawBatch(_effect);
        }

        /// <summary>
        /// Immediately releases the unmanaged resources used by this object.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            System.Diagnostics.Debug.Assert(!IsDisposed);

            if (disposing)
            {
                if (_spriteEffect != null)
                {
                    _spriteEffect.Dispose();
                    _spriteEffect = null;
                }
                if (_batcher != null)
                {
                    _batcher.Dispose();
                }
            }

            base.Dispose(disposing);
        }
    }
}

