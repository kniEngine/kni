// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021-2024 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using WinForms = System.Windows.Forms;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteMouse : MouseStrategy
    {
        internal GameWindow PrimaryWindow;

        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTSTRUCT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        [return: MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(out POINTSTRUCT pt);
        
        [DllImport("user32.dll", ExactSpelling=true, CharSet=CharSet.Auto)]
        internal static extern int MapWindowPoints(HandleRef hWndFrom, HandleRef hWndTo, out POINTSTRUCT pt, int cPoints);

        private static WinForms.Control _window;
        private static MouseInputWnd _mouseInputWnd = new MouseInputWnd();

        public override IntPtr PlatformGetWindowHandle()
        {
            return (_window == null) ? IntPtr.Zero : _window.Handle;
        }

        public override void PlatformSetWindowHandle(IntPtr windowHandle)
        {
            // Unregister old window
            if (_mouseInputWnd.Handle != IntPtr.Zero)
                _mouseInputWnd.ReleaseHandle();

            _window = WinForms.Control.FromHandle(windowHandle);
            _mouseInputWnd.AssignHandle(windowHandle);
        }

        public override bool PlatformIsRawInputAvailable()
        {
            return _mouseInputWnd.IsRawInputAvailable;
        }

        public override MouseState PlatformGetState()
        {
            POINTSTRUCT pos;
            GetCursorPos(out pos);

            // map screen position to window position. If no window is set return the screen position.
            if (_window != null)
                MapWindowPoints(new HandleRef(null, IntPtr.Zero), new HandleRef(_window, _window.Handle), out pos, 1);

            Point clientPos = new Point(pos.X, pos.Y);
            WinForms.MouseButtons buttons = WinForms.Control.MouseButtons;
            
            return new MouseState(
                clientPos.X,
                clientPos.Y,
                _mouseInputWnd.ScrollWheelValue,
                _mouseInputWnd.HorizontalScrollWheelValue,
                _mouseInputWnd.RawX,
                _mouseInputWnd.RawY,
                (buttons & WinForms.MouseButtons.Left)   == WinForms.MouseButtons.Left ? ButtonState.Pressed : ButtonState.Released,
                (buttons & WinForms.MouseButtons.Middle) == WinForms.MouseButtons.Middle ? ButtonState.Pressed : ButtonState.Released,
                (buttons & WinForms.MouseButtons.Right)  == WinForms.MouseButtons.Right ? ButtonState.Pressed : ButtonState.Released,
                (buttons & WinForms.MouseButtons.XButton1) == WinForms.MouseButtons.XButton1 ? ButtonState.Pressed : ButtonState.Released,
                (buttons & WinForms.MouseButtons.XButton2) == WinForms.MouseButtons.XButton2 ? ButtonState.Pressed : ButtonState.Released
                );
        }

        public override void PlatformSetPosition(int x, int y)
        {
            System.Drawing.Point pt = new System.Drawing.Point(x, y);

            // map window position to screen position. If no window is set assume input was in screen position.
            if (_window != null)
                pt = _window.PointToScreen(pt);

            SetCursorPos(pt.X, pt.Y);
        }

        public override void PlatformSetCursor(MouseCursor cursor)
        {
            if (_window != null)
                _window.Cursor = cursor.WinFormsCursor;
        }

        #region Nested class MouseInputWnd
        /// <remarks>
        /// Subclass WindowHandle to read WM_MOUSEHWHEEL messages
        /// </remarks>
        class MouseInputWnd : System.Windows.Forms.NativeWindow
        {
            const int WM_MOUSEMOVE   = 0x0200;
            const int WM_LBUTTONDOWN = 0x0201;
            const int WM_LBUTTONUP   = 0x0202;
            const int WM_RBUTTONDOWN = 0x0204;
            const int WM_RBUTTONUP   = 0x0205;
            const int WM_MBUTTONDOWN = 0x0207;
            const int WM_MBUTTONUP   = 0x0208;
            const int WM_MOUSEWHEEL  = 0x020A;
            const int WM_MOUSEHWHEEL = 0x020E;
            const int WM_INPUT       = 0x00FF;
            
            public int ScrollWheelValue = 0;
            public int HorizontalScrollWheelValue = 0;
            public int RawX = 0;
            public int RawY = 0;

            protected override void WndProc(ref WinForms.Message m)
            {
                switch (m.Msg)
                {
                    case WM_MOUSEWHEEL:
                        short delta = (short)(((ulong)m.WParam >> 16) & 0xffff);
                        ScrollWheelValue += delta;
                        break;
                    case WM_MOUSEHWHEEL:
                        short deltaH = (short)(((ulong)m.WParam >> 16) & 0xffff);
                        HorizontalScrollWheelValue += deltaH;
                        break;
                    case WM_INPUT:
                        HandleRawInput(ref m);
                        break;
                }

                base.WndProc(ref m);
            }

            internal bool IsRawInputAvailable { get; private set; }

            protected override void OnHandleChange()
            {                
                base.OnHandleChange();

                // Register RawInput
                if (Handle != IntPtr.Zero)
                {
                    RAWINPUTDEVICE rid;
                    rid.UsagePage = 0x01;
                    rid.Usage = 0x02;
                    rid.Flags = RIDEV_INPUTSINK;
                    rid.hwndTarget = Handle;
                    bool hr = RegisterRawInputDevices(ref rid, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
                    IsRawInputAvailable = hr;
                }
            }

            public override void ReleaseHandle()
            {
                // Unregister RawInput
                RAWINPUTDEVICE rid;
                rid.UsagePage = 0x01;
                rid.Usage = 0x02;
                rid.Flags = RIDEV_REMOVE;
                rid.hwndTarget = IntPtr.Zero;
                bool hr = RegisterRawInputDevices(ref rid, 1, (uint)Marshal.SizeOf(typeof(RAWINPUTDEVICE)));
                IsRawInputAvailable = false;

                base.ReleaseHandle();
            }
            
            
            [StructLayout(LayoutKind.Sequential)]
            internal struct RAWINPUTDEVICE
            {
                internal ushort UsagePage;
                internal ushort Usage;
                internal int Flags;
                internal IntPtr hwndTarget;
            }

            [DllImport("user32.dll", SetLastError = true)]
            static extern bool RegisterRawInputDevices(ref RAWINPUTDEVICE pRawInputDevice, uint numberDevices, uint size);

            private const int RIDEV_REMOVE    = 0x00000001;
            private const int RIDEV_NOLEGACY  = 0x00000030;
            private const int RIDEV_INPUTSINK = 0x00000100;
            

            [StructLayout(LayoutKind.Sequential)]
            internal struct RAWMOUSE
            {
                public ushort Flags;
                public ushort ButtonFlags;
                public ushort ButtonData;

                public uint RawButtons;
                public int LastX;
                public int LastY;
                public uint ExtraInformation;
            }
            
            [StructLayout(LayoutKind.Explicit)]
            public struct RAWINPUTDATA
            {
                [FieldOffset(0)]
                public RAWMOUSE mouse;
                //[FieldOffset(0)]
                //internal Rawkeyboard keyboard;
                //[FieldOffset(0)]
                //internal Rawhid hid;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RAWINPUTHEADER
            {
                public int Type;                     // Type of raw input (RIM_TYPEHID 2, RIM_TYPEKEYBOARD 1, RIM_TYPEMOUSE 0)
                public int Size;                     // Size in bytes of the entire input packet of data. This includes RAWINPUT plus possible extra input reports in the RAWHID variable length array. 
                public IntPtr hDevice;               // A handle to the device generating the raw input data. 
                public IntPtr wParam;                // RIM_INPUT 0 if input occurred while application was in the foreground else RIM_INPUTSINK 1 if it was not.
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct RAWINPUT
            {
                public RAWINPUTHEADER header;           // 64 bit header size is 24  32 bit the header size is 16
                public RAWINPUTDATA data;               // Creating the rest in a struct allows the header size to align correctly for 32 or 64 bit
            }

            [DllImport("User32.dll")]
            internal static extern int GetRawInputData( IntPtr hRawInput, uint command, 
                                                        IntPtr pData, ref uint size, int sizeHeader);
            
            private const uint RID_INPUT  = 0x10000003;
            private const uint RID_HEADER = 0x10000005;

            private const uint RIM_TYPEMOUSE    = 0x00;
            private const uint RIM_TYPEKEYBOARD = 0x01;
            private const uint RIM_TYPEHID      = 0x02;            

            private const uint MOUSE_MOVE_RELATIVE      = 0x00;
            private const uint MOUSE_MOVE_ABSOLUTE      = 0x01;
            private const uint MOUSE_VIRTUAL_DESKTOP    = 0x02;
            private const uint MOUSE_ATTRIBUTES_CHANGED = 0x04;

            private const uint RI_MOUSE_WHEEL = 0x0400;

            private unsafe void HandleRawInput(ref WinForms.Message message)
            {
                int hr;
                uint dataSize = 0;
                hr = GetRawInputData(message.LParam, RID_INPUT, IntPtr.Zero, ref dataSize, sizeof(RAWINPUTHEADER));

                // the RAWINPUT struct we pass in as &ri should be the same size as the available data size
                if (dataSize != sizeof(RAWINPUT))
                    return;

                RAWINPUT ri;
                hr = GetRawInputData(message.LParam, RID_INPUT, new IntPtr(&ri), ref dataSize, sizeof(RAWINPUTHEADER));

                if (ri.header.Type == RIM_TYPEMOUSE)
                {
                    RAWMOUSE* mi = &ri.data.mouse;

                    if ((mi->Flags & MOUSE_MOVE_ABSOLUTE) != 0)
                    {
                        // handle absolute position here
                    }
                    else // MOUSE_MOVE_RELATIVE
                    {
                        unchecked
                        {
                            RawX += mi->LastX;
                            RawY += mi->LastY;
                        }
                    }
                    //if ((mi->ButtonFlags & RI_MOUSE_WHEEL) != 0)
                    //{
                    //    // handle wheel here.
                    //    // Wheel delta is in mi->ButtonData
                    //}
                }

                return;
            }
        }
        #endregion Nested class MouseInputWnd
    }
}
