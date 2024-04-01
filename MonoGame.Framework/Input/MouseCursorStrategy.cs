// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public class MouseCursorStrategy : IDisposable
    {
        internal MouseCursorStrategy.MouseCursorType _cursorType;
        internal IntPtr _handle;

        public bool IsBuildInMouseCursor
        {
            get { return _cursorType != MouseCursorStrategy.MouseCursorType.User; }
        }

        public IntPtr Handle
        {
            get { return _handle; }
        }


        #region IDisposable

        ~MouseCursorStrategy()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool dispose)
        {
            if (dispose)
            {
               
            }

            _cursorType = (MouseCursorStrategy.MouseCursorType)0;
            _handle = IntPtr.Zero;
        }

        #endregion IDisposable

        public enum MouseCursorType
        {
            Arrow = 1,
            IBeam,
            Wait,
            Crosshair,
            WaitArrow,
            SizeNWSE,
            SizeNESW,
            SizeWE,
            SizeNS,
            SizeAll,
            No,
            Hand,

            User = 0x80
        }
    }
}