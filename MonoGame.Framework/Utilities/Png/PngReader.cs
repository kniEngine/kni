// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Utilities.Deflate;

namespace Microsoft.Xna.Platform.Graphics.Utilities.Png
{
    public class PngReader
    {
        private int _width;
        private int _height;
        private int _bitsPerSample;
        private int _bytesPerSample;
        private int _bytesPerPixel;
        private int _bytesPerScanline;
        private IList<PngChunk> _chunks;
        private IList<PngChunk> _dataChunks;
        private ColorType _colorType;
        private Palette _palette;
        private Texture2D _texture;
        private Color[] _data;
        
        public PngReader()
        {
            _chunks = new List<PngChunk>();
            _dataChunks = new List<PngChunk>();
        }

        public Texture2D Read(Stream inputStream, GraphicsDevice graphicsDevice)
        {
            if (IsPngImage(inputStream) == false)
            {
                throw new Exception("File does not have PNG signature.");
            }

            inputStream.Position = 8;

            while (inputStream.Position != inputStream.Length)
            {
                byte[] chunkDataLengthBytes = new byte[4];
                inputStream.Read(chunkDataLengthBytes, 0, 4);
                uint chunkDataLength = chunkDataLengthBytes.ToUInt();

                inputStream.Position -= 4;

                byte[] chunkBytes = new byte[12 + chunkDataLength];
                inputStream.Read(chunkBytes, 0, (int)(12 + chunkDataLength));

                ProcessChunk(chunkBytes);
            }

            UnpackDataChunks();

            _texture = new Texture2D(graphicsDevice, _width, _height, false, SurfaceFormat.Color);
            _texture.SetData<Color>(_data);

            return _texture;
        }

        public static bool IsPngImage(Stream stream)
        {
            stream.Position = 0;
            
            byte[] signature = new byte[8];
            stream.Read(signature, 0, 8);

            bool result = signature.SequenceEqual(HeaderChunk.PngSignature);

            stream.Position = 0;

            return result;
        }

        private void ProcessChunk(byte[] chunkBytes)
        {
            string chunkType = PngChunk.GetChunkTypeString(chunkBytes.Skip(4).Take(4).ToArray());

            switch (chunkType)
            {
                case "IHDR":

                    var headerChunk = new HeaderChunk();
                    headerChunk.Decode(chunkBytes);
                    _width = (int)headerChunk.Width;
                    _height = (int)headerChunk.Height;
                    _bitsPerSample = (int)headerChunk.BitDepth;
                    _colorType = headerChunk.ColorType;
                    _chunks.Add(headerChunk);

                    break;

                case "PLTE":

                    var paletteChunk = new PaletteChunk();
                    paletteChunk.Decode(chunkBytes);
                    _palette = paletteChunk.Palette;
                    _chunks.Add(paletteChunk);

                    break;

                case "tRNS":

                    var transparencyChunk = new TransparencyChunk();
                    transparencyChunk.Decode(chunkBytes);
                    _palette.AddAlphaToColors(transparencyChunk.PaletteTransparencies);
                    break;

                case "IDAT":

                    var dataChunk = new DataChunk();
                    dataChunk.Decode(chunkBytes);
                    _dataChunks.Add(dataChunk);

                    break;

                default:
                    break;
            }
        }

        private void UnpackDataChunks()
        {
            var dataByteList = new List<byte>();

            foreach (var dataChunk in _dataChunks)
            {
                if (dataChunk.Type == "IDAT")
                {
                    dataByteList.AddRange(dataChunk.Data);
                }
            }

            var compressedStream = new MemoryStream(dataByteList.ToArray());
            var decompressedStream = new MemoryStream();

            try
            {
                using (var deflateStream = new ZlibStream(compressedStream, CompressionMode.Decompress))
                {
                    deflateStream.CopyTo(decompressedStream);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("An error occurred during DEFLATE decompression.", exception);
            }

            var decompressedBytes = decompressedStream.ToArray();
            var pixelData = DeserializePixelData(decompressedBytes);

            DecodePixelData(pixelData);
        }

        private byte[][] DeserializePixelData(byte[] pixelData)
        {
            _bytesPerPixel = CalculateBytesPerPixel();
            _bytesPerSample = _bitsPerSample / 8;
            _bytesPerScanline = (_bytesPerPixel * _width) + 1;
            int scanlineCount = pixelData.Length / _bytesPerScanline;

            if (pixelData.Length % _bytesPerScanline != 0)
            {
                throw new Exception("Malformed pixel data - total length of pixel data not multiple of ((bytesPerPixel * width) + 1)");
            }

            var result = new byte[scanlineCount][];

            for (int y = 0; y < scanlineCount; y++)
            {
                result[y] = new byte[_bytesPerScanline];
                
                for (int x = 0; x < _bytesPerScanline; x++)
                {
                    result[y][x] = pixelData[y * _bytesPerScanline + x];
                }
            }
            
            return result;
        }

        private void DecodePixelData(byte[][] pixelData)
        {
            _data = new Color[_width * _height];
            
            byte[] previousScanline = new byte[_bytesPerScanline];

            for (int y = 0; y < _height; y++)
            {
                var scanline = pixelData[y];

                FilterType filterType = (FilterType)scanline[0];
                byte[] defilteredScanline;

                switch (filterType)
                {
                    case FilterType.None:

                        defilteredScanline = NoneFilter.Decode(scanline);

                        break;

                    case FilterType.Sub:

                        defilteredScanline = SubFilter.Decode(scanline, _bytesPerPixel);

                        break;

                    case FilterType.Up:

                        defilteredScanline = UpFilter.Decode(scanline, previousScanline);

                        break;

                    case FilterType.Average:

                        defilteredScanline = AverageFilter.Decode(scanline, previousScanline, _bytesPerPixel);

                        break;

                    case FilterType.Paeth:

                        defilteredScanline = PaethFilter.Decode(scanline, previousScanline, _bytesPerPixel);

                        break;

                    default:
                        throw new Exception("Unknown filter type.");
                }

                previousScanline = defilteredScanline;
                ProcessDefilteredScanline(defilteredScanline, y);
            }
        }

        private void ProcessDefilteredScanline(byte[] defilteredScanline, int y)
        {
            switch (_colorType)
            {
                case ColorType.Grayscale:

                    for (int x = 0; x < _width; x++)
                    {
                        int offset = 1 + (x * _bytesPerPixel);

                        byte intensity = defilteredScanline[offset];

                        _data[(y * _width) + x] = new Color(intensity, intensity, intensity);
                    }

                    break;

                case ColorType.GrayscaleWithAlpha:

                    for (int x = 0; x < _width; x++)
                    {
                        int offset = 1 + (x * _bytesPerPixel);

                        byte intensity = defilteredScanline[offset];
                        byte alpha = defilteredScanline[offset + _bytesPerSample];

                        _data[(y * _width) + x] = new Color(intensity, intensity, intensity, alpha);
                    }

                    break;

                case ColorType.Palette:

                    for (int x = 0; x < _width; x++)
                    {
                        var pixelColor = _palette[defilteredScanline[x + 1]];

                        _data[(y * _width) + x] = pixelColor;
                    }

                    break;

                case ColorType.Rgb:

                    for (int x = 0; x < _width; x++)
                    {
                        int offset = 1 + (x * _bytesPerPixel);
                        
                        int red = defilteredScanline[offset];
                        int green = defilteredScanline[offset + _bytesPerSample];
                        int blue = defilteredScanline[offset + 2 * _bytesPerSample];

                        _data[(y * _width) + x] = new Color(red, green, blue);
                    }

                    break;

                case ColorType.RgbWithAlpha:

                    for (int x = 0; x < _width; x++)
                    {
                        int offset = 1 + (x * _bytesPerPixel);

                        int red = defilteredScanline[offset];
                        int green = defilteredScanline[offset + _bytesPerSample];
                        int blue = defilteredScanline[offset + 2 * _bytesPerSample];
                        int alpha = defilteredScanline[offset + 3 * _bytesPerSample];

                        _data[(y * _width) + x] = new Color(red, green, blue, alpha);
                    }

                    break;

                default:
                    break;
            }
        }

        private int CalculateBytesPerPixel()
        {
            switch (_colorType)
            {
                case ColorType.Grayscale:
                    return _bitsPerSample / 8;

                case ColorType.GrayscaleWithAlpha:
                    return (2 * _bitsPerSample) / 8;

                case ColorType.Palette:
                    return _bitsPerSample / 8;

                case ColorType.Rgb:
                    return (3 * _bitsPerSample) / 8;

                case ColorType.RgbWithAlpha:
                    return (4 * _bitsPerSample) / 8;

                default:
                    throw new Exception("Unknown color type.");
            }
        }
    }
}
