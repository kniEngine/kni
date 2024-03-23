// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// Describes the view bounds for render-target surface.
    /// </summary>
    [DataContract]
    public struct Viewport
    {
        private int x;
        private int y;
        private int w;
        private int h;
        private float minDepth;
        private float maxDepth;

        #region Properties

        /// <summary>
        /// The height of the bounds in pixels.
        /// </summary>
        [DataMember]
        public int Height
        {
            get { return this.h; }
            set { h = value; }
        }

        /// <summary>
        /// The upper limit of depth of this viewport.
        /// </summary>
        [DataMember]
        public float MaxDepth
        {
            get { return this.maxDepth; }
            set { maxDepth = value; }
        }

        /// <summary>
        /// The lower limit of depth of this viewport.
        /// </summary>
        [DataMember]
        public float MinDepth
        {
            get { return this.minDepth; }
            set { minDepth = value; }
        }

        /// <summary>
        /// The width of the bounds in pixels.
        /// </summary>
        [DataMember]
        public int Width
        {
            get { return this.w; }
            set { w = value; }
        }

        /// <summary>
        /// The y coordinate of the beginning of this viewport.
        /// </summary>
        [DataMember]
        public int Y
        {
            get { return this.y; }
            set { y = value; }
        }

        /// <summary>
        /// The x coordinate of the beginning of this viewport.
        /// </summary>
        [DataMember]
        public int X 
        {
            get{ return x;}
            set{ x = value;}
        }

        #endregion
        
        /// <summary>
        /// Gets the aspect ratio of this <see cref="Viewport"/>, which is width / height. 
        /// </summary>
        public float AspectRatio 
        {
            get
            {
                if ((h != 0) && (w != 0))
                {
                    return ((float)w / (float)h);
                }
                return 0f;
            }
        }
        
        /// <summary>
        /// Gets or sets a boundary of this <see cref="Viewport"/>.
        /// </summary>
        public Rectangle Bounds 
        {
            get { return new Rectangle(x, y, w, h); }
            set
            {
                x = value.X;
                y = value.Y;
                w = value.Width;
                h = value.Height;
            }
        }

        /// <summary>
        /// Returns the subset of the viewport that is guaranteed to be visible on a lower quality display.
        /// </summary>
        public Rectangle TitleSafeArea 
        {
            get { return GraphicsDevice.GetTitleSafeArea(x, y, w, h); }
        }


        /// <summary>
        /// Constructs a viewport from the given values.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="width">The width of the view bounds in pixels.</param>
        /// <param name="height">The height of the view bounds in pixels.</param>
        /// <param name="minDepth">The lower limit of depth.</param>
        /// <param name="maxDepth">The upper limit of depth.</param>
        public Viewport(int x, int y, int width, int height,float minDepth,float maxDepth)
        {
            this.x = x;
            this.y = y;
            this.w = width;
            this.h = height;
            this.minDepth = minDepth;
            this.maxDepth = maxDepth;
        }

        /// <summary>
        /// Constructs a viewport from the given values. The <see cref="MinDepth"/> will be 0.0 and <see cref="MaxDepth"/> will be 1.0.
        /// </summary>
        /// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
        /// <param name="width">The width of the view bounds in pixels.</param>
        /// <param name="height">The height of the view bounds in pixels.</param>
        public Viewport(int x, int y, int width, int height) : this(x, y, width, height, 0.0f,  1.0f)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Viewport"/> struct.
        /// </summary>
        /// <param name="bounds">A <see cref="Rectangle"/> that defines the location and size of the <see cref="Viewport"/> in a render target.</param>
        public Viewport(Rectangle bounds) : this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
        {
        }

        /// <summary>
        /// Projects a <see cref="Vector3"/> from model space into screen space.
        /// The source point is transformed from model space to world space by the world matrix,
        /// then from world space to view space by the view matrix, and
        /// finally from view space to screen space by the projection matrix.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to project.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix.Multiply(ref world, ref view, out view);
            Matrix.Multiply(ref view, ref projection, out projection);
            float w = (source.X * projection.M14) + (source.Y * projection.M24) + (source.Z * projection.M34) + projection.M44;
            Vector3.Transform(ref source, ref projection, out source);

            float invW = 1 / w;
            source.X = source.X * invW;
            source.Y = source.Y * invW;
            source.Z = source.Z * invW;

            source.X = (((source.X + 1f) * 0.5f) * this.w) + this.x;
            source.Y = (((-source.Y + 1f) * 0.5f) * this.h) + this.y;
            source.Z = (source.Z * (this.maxDepth - this.minDepth)) + this.minDepth;
            return source;
        }

        /// <summary>
        /// Unprojects a <see cref="Vector3"/> from screen space into model space.
        /// The source point is transformed from screen space to view space by the inverse of the projection matrix,
        /// then from view space to world space by the inverse of the view matrix, and
        /// finally from world space to model space by the inverse of the world matrix.
        /// Note source.Z must be less than or equal to MaxDepth.
        /// </summary>
        /// <param name="source">The <see cref="Vector3"/> to unproject.</param>
        /// <param name="projection">The projection <see cref="Matrix"/>.</param>
        /// <param name="view">The view <see cref="Matrix"/>.</param>
        /// <param name="world">The world <see cref="Matrix"/>.</param>
        /// <returns></returns>
        public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
        {
            Matrix.Multiply(ref world, ref view, out view);
            Matrix.Multiply(ref view, ref projection, out projection);
            Matrix.Invert(ref projection, out projection);
            source.X = (((source.X - this.x) / ((float) this.w)) * 2f) - 1f;
            source.Y = -((((source.Y - this.y) / ((float) this.h)) * 2f) - 1f);
            source.Z = (source.Z - this.minDepth) / (this.maxDepth - this.minDepth);
            float w = (source.X * projection.M14) + (source.Y * projection.M24) + (source.Z * projection.M34) + projection.M44;
            Vector3.Transform(ref source, ref projection, out source);
            
            float invW = 1 / w;
            source.X = source.X * invW;
            source.Y = source.Y * invW;
            source.Z = source.Z * invW;

            return source;
        }		

        /// <summary>
        /// Returns a <see cref="String"/> representation of this <see cref="Viewport"/> in the format:
        /// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>] MinDepth:[<see cref="MinDepth"/>] MaxDepth:[<see cref="MaxDepth"/>]}
        /// </summary>
        /// <returns>A <see cref="String"/> representation of this <see cref="Viewport"/>.</returns>
        public override string ToString()
        {
            return "{X:" + x + ", Y:" + y + ", Width:" + w + ", Height:" + h + ", MinDepth:" + minDepth + ", MaxDepth:" + maxDepth + "}";
        }
    }
}

