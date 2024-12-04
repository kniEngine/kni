// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.XR
{
    public class XRDevice: IDisposable
    {
        internal static GameWindow GameWindow;

        public static HeadsetState GetState()
        {
            HeadsetState state;

            var window = GameWindow as AndroidGameWindow;
            window.UpdateHeadsetState(out state);

            return state;
        }


        #region IDisposable
        ~XRDevice()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

        }
        #endregion
    }
}

