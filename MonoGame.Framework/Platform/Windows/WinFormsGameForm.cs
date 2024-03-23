// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Framework;


namespace Microsoft.Xna.Framework.Windows
{
    internal static class MessageExtensions
    {     
        public static int GetPointerId(this Message msg)
        {
            return (short)msg.WParam;
        }

        public static System.Drawing.Point GetPointerLocation(this Message msg)
        {
            var lowword = msg.LParam.ToInt32();

            return new System.Drawing.Point()
                       {
                           X = (short)(lowword),
                           Y = (short)(lowword >> 16),
                       };
        }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    internal class WinFormsGameForm : Form
        , IMessageFilter
    {
        private readonly WinFormsGameWindow _window;

        public const int WM_QUIT       = 0x0012;
        public const int WM_ERASEBKGND = 0x0014;

        public const int WM_KEYDOWN    = 0x0100;
        public const int WM_KEYUP      = 0x0101;
        public const int WM_CHAR       = 0x0102;
        public const int WM_SYSKEYDOWN = 0x0104;
        public const int WM_SYSKEYUP   = 0x0105;
        public const int WM_UNICHAR    = 0x0109;
        public const int WM_SYSCOMMAND = 0x0112;

        public const int WM_MOUSEMOVE   = 0x0200;
        public const int WM_MOUSEWHEEL  = 0x020A;
        public const int WM_MOUSEHWHEEL = 0x020E;

        public const int WM_ENTERSIZEMOVE = 0x0231;
        public const int WM_EXITSIZEMOVE  = 0x0232;
        public const int WM_DROPFILES = 0x0233;
        public const int WM_POINTERUPDATE = 0x0245;
        public const int WM_POINTERDOWN   = 0x0246;
        public const int WM_POINTERUP     = 0x0247;
        public const int WM_TABLET_QUERYSYSTEMGESTURESTA = (0x02C0 + 12);


        internal bool IsResizing { get; set; }

        public WinFormsGameForm(WinFormsGameWindow window)
        {
            _window = window;
            Application.AddMessageFilter(this);
            base.Disposed += WinFormsGameForm_Disposed;
        }

        private void WinFormsGameForm_Disposed(object sender, EventArgs e)
        {
            base.Disposed -= WinFormsGameForm_Disposed;
            Application.RemoveMessageFilter(this);
        }

        public void CenterOnPrimaryMonitor()
        {
             Location = new System.Drawing.Point(
                 (Screen.PrimaryScreen.WorkingArea.Width  - Width ) / 2,
                 (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2);
        }

        // TNC: handle keyboard messages internally to avoid garbage from EventArgs
        // we need to handle those early in a IMessageFilter to skip OnPreviewKeyDown(PreviewKeyDownEventArgs)
        bool IMessageFilter.PreFilterMessage(ref Message m)
        {            
            if (m.HWnd != this.Handle)
                return false;
            
            switch (m.Msg)
            {
                case WM_KEYDOWN:
                case WM_KEYUP:
                case WM_CHAR:
                case WM_UNICHAR:
                    var c = m.WParam.ToInt32();
                    if (c == 0x5B && c == 0x5C) return false; // let Left/Right Windows Key through
                    if (_window.Platform_IsTextInputAttached())  return false; // let keys through if user subscribed to TextInput
                    if (_window.Platform_IsKeyUpDownAttached())  return false; // let keys through if user subscribed to KeyUp/KeyDown
                    return true; // skip message
                default:
                    return false;
            }
        }

        [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        protected override void WndProc(ref Message m)
        {
            var state = TouchLocationState.Invalid;
           
            // TNC: handle those messages internally to avoid garbage from EventArgs
            switch (m.Msg)
            {
                case WM_ERASEBKGND:
                    return; // skip repaint of the control under the swapchain (PaintEventArgs)
                case WM_MOUSEWHEEL:
                    var delta = (short)(((ulong)m.WParam >> 16) & 0xffff);
                    _window.MouseState.ScrollWheelValue += delta;
                    return;
                case WM_MOUSEHWHEEL:
                    var deltaH = (short)(((ulong)m.WParam >> 16) & 0xffff);
                    _window.MouseState.HorizontalScrollWheelValue += deltaH;
                    return;
                case WM_MOUSEMOVE:
                    return;
            }

            switch (m.Msg)
            {
                case WM_TABLET_QUERYSYSTEMGESTURESTA:
                    {
                        // This disables the windows touch helpers, popups, and 
                        // guides that get in the way of touch gameplay.
                        const int flags = 0x00000001 | // TABLET_DISABLE_PRESSANDHOLD
                                          0x00000008 | // TABLET_DISABLE_PENTAPFEEDBACK
                                          0x00000010 | // TABLET_DISABLE_PENBARRELFEEDBACK
                                          0x00000100 | // TABLET_DISABLE_TOUCHUIFORCEON
                                          0x00000200 | // TABLET_DISABLE_TOUCHUIFORCEOFF
                                          0x00008000 | // TABLET_DISABLE_TOUCHSWITCH
                                          0x00010000 | // TABLET_DISABLE_FLICKS
                                          0x00080000 | // TABLET_DISABLE_SMOOTHSCROLLING 
                                          0x00100000; // TABLET_DISABLE_FLICKFALLBACKKEYS
                        m.Result = new IntPtr(flags);
                        return;
                    }

                case WM_KEYDOWN:
                    HandleKeyMessage(ref m);
                    switch (m.WParam.ToInt32())
                    {
                        case 0x5B:  // Left Windows Key
                        case 0x5C: // Right Windows Key

                            if (_window.IsFullScreen && _window.HardwareModeSwitch)
                                this.WindowState = FormWindowState.Minimized;

                            break;
                    }
                    break;
                case WM_SYSKEYDOWN:
                    HandleKeyMessage(ref m);
                    break;
                case WM_KEYUP:
                case WM_SYSKEYUP:
                    HandleKeyMessage(ref m);
                    break;

                case WM_DROPFILES:
                    HandleDropMessage(ref m);
                    break;

                case WM_SYSCOMMAND:

                    int wParam = m.WParam.ToInt32();

                    if (wParam == 0xF060) // SC_CLOSE
                    {
                        bool disableAltF4 = !_window.AllowAltF4;
                        if (!disableAltF4 && m.LParam.ToInt32() == 0 && Focused)
                        {
                            m.Result = IntPtr.Zero;
                            return;
                        }
                    }

                    // Disable the system menu from being toggled by
                    // keyboard input so we can own the ALT key.
                    if (wParam == 0xF100) // SC_KEYMENU
                    {
                        m.Result = IntPtr.Zero;
                        return;
                    }
                    break;

                case WM_POINTERUP:
                    state = TouchLocationState.Released;
                    break;
                case WM_POINTERDOWN:
                    state = TouchLocationState.Pressed;
                    break;
                case WM_POINTERUPDATE:
                    state = TouchLocationState.Moved;
                    break;
                case WM_ENTERSIZEMOVE:
                    IsResizing = true;
                    break;
                case WM_EXITSIZEMOVE:
                    IsResizing = false;
                    break;
            }

            if (state != TouchLocationState.Invalid)
            {
                var id = m.GetPointerId();

                var position = m.GetPointerLocation();
                position = PointToClient(position);
                var vec = new Vector2(position.X, position.Y);

                _window.TouchPanelState.AddEvent(id, state, vec);
            }

            base.WndProc(ref m);
        }

        void HandleKeyMessage(ref Message m)
        {
            long virtualKeyCode = m.WParam.ToInt64();
            bool extended = (m.LParam.ToInt64() & 0x01000000) != 0;
            long scancode = (m.LParam.ToInt64() & 0x00ff0000) >> 16;
            var key = KeyCodeTranslate(
                (System.Windows.Forms.Keys)virtualKeyCode,
                extended,
                scancode);
            if (Input.KeysHelper.IsKey((int)key))
            {
                switch (m.Msg)
                {
                    case WM_KEYDOWN:
                    case WM_SYSKEYDOWN:
                        _window.Platform_OnKeyDown(key);
                        break;
                    case WM_KEYUP:
                    case WM_SYSKEYUP:
                        _window.Platform_OnKeyUp(key);
                        break;
                    default:
                        break;
                }
            }

        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern uint DragQueryFile(IntPtr hDrop, uint iFile,
            [Out] StringBuilder lpszFile, uint cch);

        void HandleDropMessage(ref Message m)
        {
            IntPtr hdrop = m.WParam;

            uint count = DragQueryFile(hdrop, uint.MaxValue, null, 0);

            string[] files = new string[count];
            for (uint i = 0; i < count; i++)
            {
                uint buffSize = DragQueryFile(hdrop, i, null, int.MaxValue);
                StringBuilder builder = new StringBuilder((int)buffSize);
                DragQueryFile(hdrop, i, builder, buffSize);
                files[i] = builder.ToString();
            }

            _window.OnFileDrop(new FileDropEventArgs(files));
            m.Result = IntPtr.Zero;
        }

        private static Microsoft.Xna.Framework.Input.Keys KeyCodeTranslate(
            System.Windows.Forms.Keys keyCode, bool extended, long scancode)
        {
            switch (keyCode)
            {
                // WinForms does not distinguish between left/right keys
                // We have to check for special keys such as control/shift/alt/ etc
                case System.Windows.Forms.Keys.ControlKey:
                    return extended
                        ? Microsoft.Xna.Framework.Input.Keys.RightControl
                        : Microsoft.Xna.Framework.Input.Keys.LeftControl;
                case System.Windows.Forms.Keys.ShiftKey:
                    // left shift is 0x2A, right shift is 0x36. IsExtendedKey is always false.
                    return ((scancode & 0x1FF) == 0x36)
                               ? Microsoft.Xna.Framework.Input.Keys.RightShift
                                : Microsoft.Xna.Framework.Input.Keys.LeftShift;
                // Note that the Alt key is now referred to as Menu.
                case System.Windows.Forms.Keys.Menu:
                case System.Windows.Forms.Keys.Alt:
                    return extended
                        ? Microsoft.Xna.Framework.Input.Keys.RightAlt
                        : Microsoft.Xna.Framework.Input.Keys.LeftAlt;

                default:
                    return (Microsoft.Xna.Framework.Input.Keys)keyCode;
            }
        }
    }
}
