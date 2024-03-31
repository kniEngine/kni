// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Graphics;
using WinFormsCursor = System.Windows.Forms.Cursor;
using WinFormsCursors = System.Windows.Forms.Cursors;

namespace Microsoft.Xna.Framework.Input
{
    public partial class MouseCursor
    {
        private readonly MouseCursorType _cursorType;
        private IntPtr _handle;
        
        WinFormsCursor _winFormsCursor;

        private IntPtr PlatformGetHandle()
        {
            return _handle;
        }

        private bool PlatformIsBuildInMouseCursor
        {
            get { return _cursorType != MouseCursorType.User; }
        }

        internal WinFormsCursor WinFormsCursor { get { return _winFormsCursor; } }


        private MouseCursor(MouseCursorType cursorType)
        {
            _cursorType = cursorType;
            _handle = IntPtr.Zero;

            _winFormsCursor = CursorTypeToWinFormsCursor(cursorType);
        }

        private WinFormsCursor CursorTypeToWinFormsCursor(MouseCursorType cursorType)
        {
            switch (cursorType)
            {
                case MouseCursorType.Arrow:
                    return WinFormsCursors.Arrow;
                case MouseCursorType.IBeam:
                    return WinFormsCursors.IBeam;
                case MouseCursorType.Wait:
                    return WinFormsCursors.WaitCursor;
                case MouseCursorType.Crosshair:
                    return WinFormsCursors.Cross;
                case MouseCursorType.WaitArrow:
                    return WinFormsCursors.AppStarting;
                case MouseCursorType.SizeNWSE:
                    return WinFormsCursors.SizeNWSE;
                case MouseCursorType.SizeNESW:
                    return WinFormsCursors.SizeNESW;
                case MouseCursorType.SizeWE:
                    return WinFormsCursors.SizeWE;
                case MouseCursorType.SizeNS:
                    return WinFormsCursors.SizeNS;
                case MouseCursorType.SizeAll:
                    return WinFormsCursors.SizeAll;
                case MouseCursorType.No:
                    return WinFormsCursors.No;
                case MouseCursorType.Hand:
                    return WinFormsCursors.Hand;

                default:
                    throw new InvalidOperationException("cursorType");
            }
        }


        public MouseCursor(byte[] data, int w, int h, int originx, int originy)
        {
            // convert ABGR to ARGB
            for (int i = 0; i < data.Length; i += 4)
            {
                byte r = data[i];
                data[i + 0] = data[i + 2];
                data[i + 2] = r;
            }

            GCHandle gcHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                IntPtr dataPtr = gcHandle.AddrOfPinnedObject();
                using (Bitmap bitmap = new Bitmap(w, h, h * 4, PixelFormat.Format32bppArgb, dataPtr))
                {
                    IconInfo iconInfo = default(IconInfo);
                    IntPtr hIcon = bitmap.GetHicon();
                    GetIconInfo(hIcon, out iconInfo);
                    iconInfo.xHotspot = originx;
                    iconInfo.yHotspot = originy;
                    iconInfo.fIcon = false;
                    IntPtr handle = CreateIconIndirect(ref iconInfo);
                    DeleteObject(iconInfo.ColorBitmap);
                    DeleteObject(iconInfo.MaskBitmap);
                    DestroyIcon(hIcon);

                    _cursorType = MouseCursorType.User;
                    _handle = handle;
                    _winFormsCursor = new WinFormsCursor(handle);
                }
            }
            finally
            {
                gcHandle.Free();
            }
        }

        private void PlatformDispose(bool dispose)
        {
            if (dispose)
            {
                if (_winFormsCursor != null)
                    _winFormsCursor.Dispose();
            }

            if (_handle != IntPtr.Zero)
                DestroyIcon(_handle);

            _winFormsCursor = null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IconInfo
        {
            public bool fIcon;
            public int xHotspot;
            public int yHotspot;
            public IntPtr MaskBitmap;
            public IntPtr ColorBitmap;
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetIconInfo(IntPtr hIcon, out IconInfo pIconInfo);

        [DllImport("gdi32.dll")]
        static extern bool DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr CreateIconIndirect([In] ref IconInfo iconInfo);

        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr handle);
    }
}
