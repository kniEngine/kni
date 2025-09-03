// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

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
            throw new PlatformNotSupportedException();
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
