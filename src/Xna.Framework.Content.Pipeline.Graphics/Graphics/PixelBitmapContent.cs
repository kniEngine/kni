// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content.Pipeline.Utilities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Microsoft.Xna.Framework.Content.Pipeline.Graphics
{
    public class PixelBitmapContent<T> : BitmapContent where T : struct, IEquatable<T>
    {
        private T[][] _pixelData;

        internal SurfaceFormat _format;

        public PixelBitmapContent(int width, int height)
        {
            if (!TryGetFormat(out _format))
                throw new InvalidOperationException(String.Format("Color format \"{0}\" is not supported",typeof(T).ToString()));
            Height = height;
            Width = width;

            _pixelData = new T[height][];

            Parallel.For(0, Height, (y) =>
            {
                _pixelData[y] = new T[Width];
            });

        }

        public override byte[] GetPixelData()
        {
            int formatSize = _format.GetSize();
            int dataSize = Width * Height * formatSize;
            byte[] outputData = new byte[dataSize];

            Parallel.For(0, Height, (y) =>
            {
                T[] row = _pixelData[y];
                GCHandle dataHandle = GCHandle.Alloc(row, GCHandleType.Pinned);
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                Marshal.Copy(dataPtr, outputData, (formatSize * y * Width), (Width * formatSize));

                dataHandle.Free();
            });

            return outputData;
        }

        public override void SetPixelData(byte[] sourceData)
        {
            int formatSize = _format.GetSize();
            int rowSize = Width * formatSize;

            Parallel.For(0, Height, (y) =>
            {
                T[] row = _pixelData[y];
                GCHandle dataHandle = GCHandle.Alloc(row, GCHandleType.Pinned);
                IntPtr dataPtr = dataHandle.AddrOfPinnedObject();

                Marshal.Copy(sourceData, (y * rowSize), dataPtr, rowSize);

                dataHandle.Free();
            });
        }

        public T[] GetRow(int y)
        {
            if (y < 0 || y >= Height)
                throw new ArgumentOutOfRangeException("y");

            return _pixelData[y];
        }

        /// <summary>
        /// Gets the corresponding GPU texture format for the specified bitmap type.
        /// </summary>
        /// <param name="format">Format being retrieved.</param>
        /// <returns>The GPU texture format of the bitmap type.</returns>
        public override bool TryGetFormat(out SurfaceFormat format)
        {
            if (typeof(T) == typeof(Color))
                format = SurfaceFormat.Color;
            else if (typeof(T) == typeof(Vector2))
                format = SurfaceFormat.Vector2;
            else if (typeof(T) == typeof(Vector4))
                format = SurfaceFormat.Vector4;

            else if (typeof(T) == typeof(byte))
                format = SurfaceFormat.Alpha8;
            else if (typeof(T) == typeof(Single))
                format = SurfaceFormat.Single;

            else if (typeof(T) == typeof(Alpha8))
                format = SurfaceFormat.Alpha8;
            else if (typeof(T) == typeof(Bgr565))
                format = SurfaceFormat.Bgr565;
            else if (typeof(T) == typeof(Bgra4444))
                format = SurfaceFormat.Bgra4444;
            else if (typeof(T) == typeof(Bgra5551))
                format = SurfaceFormat.Bgra5551;
            else if (typeof(T) == typeof(Byte4))
                format = SurfaceFormat.Color;

            else if (typeof(T) == typeof(HalfSingle))
                format = SurfaceFormat.HalfSingle;
            else if (typeof(T) == typeof(HalfVector2))
                format = SurfaceFormat.HalfVector2;
            else if (typeof(T) == typeof(HalfVector4))
                format = SurfaceFormat.HalfVector4;

            else if (typeof(T) == typeof(NormalizedByte2))
                format = SurfaceFormat.NormalizedByte2;
            else if (typeof(T) == typeof(NormalizedByte4))
                format = SurfaceFormat.NormalizedByte4;
            else if (typeof(T) == typeof(NormalizedShort2))
                format = SurfaceFormat.NormalizedByte2;
            else if (typeof(T) == typeof(NormalizedShort4))
                format = SurfaceFormat.NormalizedByte4;

            else if (typeof(T) == typeof(Rg32))
                format = SurfaceFormat.Rg32;
            else if (typeof(T) == typeof(Rgba1010102))
                format = SurfaceFormat.Rgba1010102;
            else if (typeof(T) == typeof(Rgba64))
                format = SurfaceFormat.Rgba64;

            else
            {
                format = (SurfaceFormat)(int)-1;
                return false;
            }

            return true;
        }

        public T GetPixel(int x, int y)
        {
            return _pixelData[y][x];
        }

        public void SetPixel(int x, int y, T value)
        {
            _pixelData[y][x] = value;
        }

        public void ReplaceColor(T originalColor, T newColor)
        {
            Parallel.For(0, Height, (y) =>
            {
                T[] row = _pixelData[y];
                for (int x = 0; x < Width; x++)
                {
                    if (row[x].Equals(originalColor))
                        row[x] = newColor;
                }
            });
        }

        protected override bool TryCopyFrom(BitmapContent sourceBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat sourceFormat;
            if (!sourceBitmap.TryGetFormat(out sourceFormat))
                return false;

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (_format == sourceFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                SetPixelData(sourceBitmap.GetPixelData());
                return true;
            }

            // If the source is not Vector4 or requires resizing, send it through BitmapContent.Copy
            if (!(sourceBitmap is PixelBitmapContent<Vector4>) || sourceRegion.Width != destinationRegion.Width || sourceRegion.Height != destinationRegion.Height)
            {
                try
                {
                    BitmapContent.Copy(sourceBitmap, sourceRegion, this, destinationRegion);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            // Convert from a Vector4 format
            PixelBitmapContent<Vector4> src = sourceBitmap as PixelBitmapContent<Vector4>;
            if (default(T) is IPackedVector)
            {
                Parallel.For(0, sourceRegion.Height, (y) =>
                {
                    T pixel = default(T);
                    IPackedVector p = (IPackedVector)pixel;
                    Vector4[] row = src.GetRow(sourceRegion.Top + y);
                    for (int x = 0; x < sourceRegion.Width; x++)
                    {
                        p.PackFromVector4(row[sourceRegion.Left + x]);
                        pixel = (T)p;

                        SetPixel(destinationRegion.Left + x, destinationRegion.Top + y,
                                 pixel);
                    }
                });
            }
            else
            {
                VectorConverterEx<T> converter = new VectorConverterEx() as VectorConverterEx<T>;
                // If no converter could be created, converting from this format is not supported
                if (converter == null)
                    return false;

                Parallel.For(0, sourceRegion.Height, (y) =>
                {
                    Vector4[] row = src.GetRow(sourceRegion.Top + y);
                    for (int x = 0; x < sourceRegion.Width; x++)
                    {
                        SetPixel(destinationRegion.Left + x, destinationRegion.Top + y,
                                 converter.FromVector4(row[sourceRegion.Left + x]));
                    }
                });
            }

            return true;
        }

        protected override bool TryCopyTo(BitmapContent destinationBitmap, Rectangle sourceRegion, Rectangle destinationRegion)
        {
            SurfaceFormat destinationFormat;
            if (!destinationBitmap.TryGetFormat(out destinationFormat))
                return false;

            // A shortcut for copying the entire bitmap to another bitmap of the same type and format
            if (_format == destinationFormat && (sourceRegion == new Rectangle(0, 0, Width, Height)) && sourceRegion == destinationRegion)
            {
                destinationBitmap.SetPixelData(GetPixelData());
                return true;
            }

            // If the destination is not Vector4 or requires resizing, send it through BitmapContent.Copy
            if (!(destinationBitmap is PixelBitmapContent<Vector4>) || sourceRegion.Width != destinationRegion.Width || sourceRegion.Height != destinationRegion.Height)
            {
                try
                {
                    BitmapContent.Copy(this, sourceRegion, destinationBitmap, destinationRegion);
                    return true;
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            // Convert to a Vector4 format
            PixelBitmapContent<Vector4> dest = destinationBitmap as PixelBitmapContent<Vector4>;
            if (default(T) is IPackedVector)
            {
                Parallel.For(0, sourceRegion.Height, (y) =>
                {
                    T[] row = GetRow(sourceRegion.Top + y);
                    for (int x = 0; x < sourceRegion.Width; x++)
                    {
                        dest.SetPixel(destinationRegion.Left + x, destinationRegion.Top + y,
                                      ((IPackedVector)row[sourceRegion.Left + x]).ToVector4());
                    }
                });
            }
            else
            {
                VectorConverterEx<T> converter = new VectorConverterEx() as VectorConverterEx<T>;
                // If no converter could be created, converting from this format is not supported
                if (converter == null)
                    return false;

                Parallel.For(0, sourceRegion.Height, (y) =>
                {
                    T[] row = GetRow(sourceRegion.Top + y);
                    for (int x = 0; x < sourceRegion.Width; x++)
                    {
                        dest.SetPixel(destinationRegion.Left + x, destinationRegion.Top + y,
                                      converter.ToVector4(row[sourceRegion.Left + x]));
                    }
                });
            }

            return true;
        }
    }
}
