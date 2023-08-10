#region License
/*
MIT License
Copyright © 2006 The Mono.Xna Team

All rights reserved.

Authors:
 * Rob Loach

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
#endregion License

using System;
using System.Globalization;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics
{
    [DataContract]
    public class DisplayMode
    {
        #region Fields

        private int _width;
        private int _height;
        private SurfaceFormat _format;

        #endregion Fields

        #region Properties
        
        public int Width { get { return _width; } }
        public int Height { get { return _height; } }
        public SurfaceFormat Format { get { return _format; } }

        public float AspectRatio
        {
            get { return (float)_width / (float)_height; }
        }
        
        public Rectangle TitleSafeArea
        {
            get { return GraphicsDevice.GetTitleSafeArea(0, 0, _width, _height); }
        }

        #endregion Properties

        #region Constructors
        
        internal DisplayMode(int width, int height, SurfaceFormat format)
        {
            this._width  = width;
            this._height = height;
            this._format = format;
        }

        #endregion Constructors

        #region Operators

        public static bool operator !=(DisplayMode left, DisplayMode right)
        {
            return !(left == right);
        }

        public static bool operator ==(DisplayMode left, DisplayMode right)
        {
            if (ReferenceEquals(left, right)) //Same object or both are null
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return (left._width  == right._width ) &&
                   (left._height == right._height) &&
                   (left._format == right._format);
        }

        #endregion Operators

        #region Public Methods

        public override bool Equals(object obj)
        {
            return obj is DisplayMode && this == (DisplayMode)obj;
        }

        public override int GetHashCode()
        {
            return (this._width.GetHashCode() ^ this._height.GetHashCode() ^ this._format.GetHashCode());
        }

        public override string ToString()
        {
            return String.Format("{Width:{0}, Height:{1}, Format:{2}, AspectRatio:{3}}",
                _width, _height, _format, AspectRatio);
        }

        #endregion Public Methods
    }
}
