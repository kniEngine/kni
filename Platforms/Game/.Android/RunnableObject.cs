// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Platform
{
    internal class RunnableObject : Java.Lang.Object
        , Java.Lang.IRunnable
    {
        private Android.OS.Handler _handler;
        private int _frameRequests = 0;

        public event EventHandler Tick;

        public RunnableObject() : base()
        {
        }

        public void InitLoopHandler()
        {
            Android.OS.Looper looper = Android.OS.Looper.MainLooper;
            _handler = new Android.OS.Handler(looper);
        }

        public void RequestFrame()
        {
            if (_frameRequests == 0)
            {
                _handler.Post((Java.Lang.IRunnable)this);
                _frameRequests++;
            }
        }

        void Java.Lang.IRunnable.Run()
        {
            _frameRequests--;

            var handler = Tick;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }
}
