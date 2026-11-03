// Copyright (C)2021-2024 Nick Kastellanos

using System;
using System.IO;
using Microsoft.Xna.Framework.Input;
using StbImageWriteSharp;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouseCursor : MouseCursorStrategy
    {
        string _cursorCSSPropertyValue;

        internal string CursorCSSPropertyValue { get { return _cursorCSSPropertyValue; } }

        public ConcreteMouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            this._cursorType = cursorType;
            this._handle = IntPtr.Zero;

            _cursorCSSPropertyValue = CursorTypeToCSSPropertyValue(cursorType);
        }

        private string CursorTypeToCSSPropertyValue(MouseCursorStrategy.MouseCursorType cursorType)
        {
            switch (cursorType)
            {
                case MouseCursorStrategy.MouseCursorType.Arrow:
                    return "default";
                case MouseCursorStrategy.MouseCursorType.IBeam:
                    return "text";
                case MouseCursorStrategy.MouseCursorType.Wait:
                    return "wait";
                case MouseCursorStrategy.MouseCursorType.Crosshair:
                    return "crosshair";
                case MouseCursorStrategy.MouseCursorType.WaitArrow:
                    return "progress";
                case MouseCursorStrategy.MouseCursorType.SizeNWSE:
                    return "nwse-resize";
                case MouseCursorStrategy.MouseCursorType.SizeNESW:
                    return "nesw-resize";
                case MouseCursorStrategy.MouseCursorType.SizeWE:
                    return "ew-resize";
                case MouseCursorStrategy.MouseCursorType.SizeNS:
                    return "ns-resize";
                case MouseCursorStrategy.MouseCursorType.SizeAll:
                    return "move";
                case MouseCursorStrategy.MouseCursorType.No:
                    return "not-allowed";
                case MouseCursorStrategy.MouseCursorType.Hand:
                    return "pointer";

                default:
                    throw new InvalidOperationException("cursorType");
            }
        }

        public ConcreteMouseCursor(byte[] data, int w, int h, int originx, int originy)
        {
            this._cursorType = MouseCursorStrategy.MouseCursorType.User;
            this._handle = IntPtr.Zero;

            byte[] pngBytes = ConvertToPngByteArray(data, w, h);
            string pngBase64String = Convert.ToBase64String(pngBytes);
            _cursorCSSPropertyValue = $"url(data:image/png;base64,{pngBase64String}) {originx} {originy}, auto";
        }

        private unsafe byte[] ConvertToPngByteArray(byte[] data, int width, int height)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                fixed (byte* pData = data)
                {
                    ImageWriter writer = new ImageWriter();
                    writer.WritePng(pData, width, height, ColorComponents.RedGreenBlueAlpha, stream);
                }
                return stream.ToArray();
            }
        }

        protected override void Dispose(bool dispose)
        {
            if (dispose)
            {
            }

            base.Dispose(dispose);
        }

    }
}
