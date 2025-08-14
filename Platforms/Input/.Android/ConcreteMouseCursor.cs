// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;
using Android.Content;
using Android.Views;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouseCursor : MouseCursorStrategy
    {
        PointerIconType _androidCursor;
        PointerIcon _pointerIcon;

        internal PointerIcon PointerIcon
        {
            get
            {
                if (_pointerIcon == null)
                {
                    if (_cursorType != MouseCursorType.User)
                    {
                        if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
                        {
                            Context appContext = Android.App.Application.Context;
                            _pointerIcon = PointerIcon.GetSystemIcon(appContext, _androidCursor);
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                return _pointerIcon;
            }
        }

        public ConcreteMouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            this._cursorType = cursorType;
            this._handle = IntPtr.Zero;

            _androidCursor = CursorTypeToAndroidCursor(cursorType);
        }

        private PointerIconType CursorTypeToAndroidCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
            {
                switch (cursorType)
                {
                    case MouseCursorStrategy.MouseCursorType.Arrow:
                        return PointerIconType.Arrow;
                    case MouseCursorStrategy.MouseCursorType.IBeam:
                        return PointerIconType.Text;
                    case MouseCursorStrategy.MouseCursorType.Wait:
                        return PointerIconType.Wait;
                    case MouseCursorStrategy.MouseCursorType.Crosshair:
                        return PointerIconType.Crosshair;
                    case MouseCursorStrategy.MouseCursorType.WaitArrow:
                        return PointerIconType.Arrow;
                    case MouseCursorStrategy.MouseCursorType.SizeNWSE:
                        return PointerIconType.TopRightDiagonalDoubleArrow;
                    case MouseCursorStrategy.MouseCursorType.SizeNESW:
                        return PointerIconType.TopLeftDiagonalDoubleArrow;
                    case MouseCursorStrategy.MouseCursorType.SizeWE:
                        return PointerIconType.HorizontalDoubleArrow;
                    case MouseCursorStrategy.MouseCursorType.SizeNS:
                        return PointerIconType.VerticalDoubleArrow;
                    case MouseCursorStrategy.MouseCursorType.SizeAll:
                        return PointerIconType.AllScroll;
                    case MouseCursorStrategy.MouseCursorType.No:
                        return PointerIconType.NoDrop;
                    case MouseCursorStrategy.MouseCursorType.Hand:
                        return PointerIconType.Hand;

                    default:
                        throw new InvalidOperationException("cursorType");
                }
            }
            else
            {
                return (PointerIconType)0;
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
