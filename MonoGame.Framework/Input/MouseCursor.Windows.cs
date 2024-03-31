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
        private readonly bool _isBuildInMouseCursor;
        private IntPtr _handle;
        
        WinFormsCursor _winFormsCursor;
        
        private IntPtr PlatformGetHandle()
        {
            return _handle;
        }

        private bool PlatformIsBuildInMouseCursor
        {
            get { return _isBuildInMouseCursor; }
        }

        internal WinFormsCursor WinFormsCursor { get { return _winFormsCursor; } }


        private MouseCursor(bool isBuildInMouseCursor, IntPtr handle)
        {
            _isBuildInMouseCursor = isBuildInMouseCursor;
            _handle = handle;
            _winFormsCursor = new WinFormsCursor(handle);
        }

        private MouseCursor(bool isBuildInMouseCursor, WinFormsCursor cursor)
        {
            _isBuildInMouseCursor = isBuildInMouseCursor;
            _winFormsCursor = cursor;
        }

        private static void PlatformInitalize()
        {
            Arrow = new MouseCursor(true, WinFormsCursors.Arrow);
            IBeam = new MouseCursor(true, WinFormsCursors.IBeam);
            Wait = new MouseCursor(true, WinFormsCursors.WaitCursor);
            Crosshair = new MouseCursor(true, WinFormsCursors.Cross);
            WaitArrow = new MouseCursor(true, WinFormsCursors.AppStarting);
            SizeNWSE = new MouseCursor(true, WinFormsCursors.SizeNWSE);
            SizeNESW = new MouseCursor(true, WinFormsCursors.SizeNESW);
            SizeWE = new MouseCursor(true, WinFormsCursors.SizeWE);
            SizeNS = new MouseCursor(true, WinFormsCursors.SizeNS);
            SizeAll = new MouseCursor(true, WinFormsCursors.SizeAll);
            No = new MouseCursor(true, WinFormsCursors.No);
            Hand = new MouseCursor(true, WinFormsCursors.Hand);
        }

        private static MouseCursor PlatformFromTexture2D(byte[] data, int w, int h, int originx, int originy)
        {
            // convert ABGR to ARGB
            for (int i = 0; i < data.Length; i += 4)
            {
                byte r = data[i];
                data[i] = data[i + 2];
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
                    return new MouseCursor(false, handle);
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
