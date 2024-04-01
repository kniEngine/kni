// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IPlatformMouseCursor
    {
        T GetStrategy<T>() where T : MouseCursorStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input
{
    /// <summary>
    /// Describes a mouse cursor.
    /// </summary>
    public class MouseCursor : IDisposable
        , IPlatformMouseCursor
    {
        private MouseCursorStrategy _strategy;

        /// <summary>
        /// Gets the default arrow cursor.
        /// </summary>
        public static MouseCursor Arrow { get; private set; }

        /// <summary>
        /// Gets the cursor that appears when the mouse is over text editing regions.
        /// </summary>
        public static MouseCursor IBeam { get; private set; }

        /// <summary>
        /// Gets the waiting cursor that appears while the application/system is busy.
        /// </summary>
        public static MouseCursor Wait { get; private set; }

        /// <summary>
        /// Gets the crosshair ("+") cursor.
        /// </summary>
        public static MouseCursor Crosshair { get; private set; }

        /// <summary>
        /// Gets the cross between Arrow and Wait cursors.
        /// </summary>
        public static MouseCursor WaitArrow { get; private set; }

        /// <summary>
        /// Gets the northwest/southeast ("\") cursor.
        /// </summary>
        public static MouseCursor SizeNWSE { get; private set; }

        /// <summary>
        /// Gets the northeast/southwest ("/") cursor.
        /// </summary>
        public static MouseCursor SizeNESW { get; private set; }

        /// <summary>
        /// Gets the horizontal west/east ("-") cursor.
        /// </summary>
        public static MouseCursor SizeWE { get; private set; }

        /// <summary>
        /// Gets the vertical north/south ("|") cursor.
        /// </summary>
        public static MouseCursor SizeNS { get; private set; }

        /// <summary>
        /// Gets the size all cursor which points in all directions.
        /// </summary>
        public static MouseCursor SizeAll { get; private set; }

        /// <summary>
        /// Gets the cursor that points that something is invalid, usually a cross.
        /// </summary>
        public static MouseCursor No { get; private set; }

        /// <summary>
        /// Gets the hand cursor, usually used for web links.
        /// </summary>
        public static MouseCursor Hand { get; private set; }


        static MouseCursor()
        {
            Arrow     = new MouseCursor(MouseCursorStrategy.MouseCursorType.Arrow);
            IBeam     = new MouseCursor(MouseCursorStrategy.MouseCursorType.IBeam);
            Wait      = new MouseCursor(MouseCursorStrategy.MouseCursorType.Wait);
            Crosshair = new MouseCursor(MouseCursorStrategy.MouseCursorType.Crosshair);
            WaitArrow = new MouseCursor(MouseCursorStrategy.MouseCursorType.WaitArrow);
            SizeNWSE  = new MouseCursor(MouseCursorStrategy.MouseCursorType.SizeNWSE);
            SizeNESW  = new MouseCursor(MouseCursorStrategy.MouseCursorType.SizeNESW);
            SizeWE    = new MouseCursor(MouseCursorStrategy.MouseCursorType.SizeWE);
            SizeNS    = new MouseCursor(MouseCursorStrategy.MouseCursorType.SizeNS);
            SizeAll   = new MouseCursor(MouseCursorStrategy.MouseCursorType.SizeAll);
            No        = new MouseCursor(MouseCursorStrategy.MouseCursorType.No);
            Hand      = new MouseCursor(MouseCursorStrategy.MouseCursorType.Hand);
        }

        T IPlatformMouseCursor.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private MouseCursor(MouseCursorStrategy.MouseCursorType cursorType)
        {
            _strategy = new ConcreteMouseCursor(cursorType);
        }

        public MouseCursor(byte[] data, int w, int h, int originx, int originy)
        {
            _strategy = new ConcreteMouseCursor(data, w, h, originx, originy);
        }

        /// <summary>
        /// Creates a mouse cursor from the specified texture.
        /// </summary>
        /// <param name="texture">Texture to use as the cursor image.</param>
        /// <param name="originx">X cordinate of the image that will be used for mouse position.</param>
        /// <param name="originy">Y cordinate of the image that will be used for mouse position.</param>
        public static MouseCursor FromTexture2D(Texture2D texture, int originx, int originy)
        {
            if (texture.Format != SurfaceFormat.Color && texture.Format != SurfaceFormat.ColorSRgb)
                throw new ArgumentException("Only Color or ColorSrgb textures are accepted for mouse cursors", "texture");

            int w = texture.Width;
            int h = texture.Height;
            byte[] data = new byte[w * h * 4];
            texture.GetData(data);

            return new MouseCursor(data, w, h, originx, originy);
        }

        public IntPtr Handle { get { return _strategy.Handle; } }


        #region IDisposable

        ~MouseCursor()
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
                if (_strategy.IsBuildInMouseCursor)
                throw new InvalidOperationException("Disposing Stock MouseCursors is not allowed.");

                if (_strategy != null)
                {
                    _strategy.Dispose();
                    _strategy = null;
                }
            }

        }

        #endregion IDisposable

    }
}
