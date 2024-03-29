// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2023 Nick Kastellanos

using System;
using Android.Views;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;

namespace Microsoft.Xna.Platform.Input.Touch
{
    class TouchEventListener : Java.Lang.Object
        , View.IOnTouchListener
    {
        public void SetTouchListener(GameWindow window)
        {
            GameWindow gameWindow = window;

            AndroidGameWindow androidGameWindow = (AndroidGameWindow)gameWindow;
            androidGameWindow.GameView.SetOnTouchListener(this);
        }

        bool View.IOnTouchListener.OnTouch(View v, MotionEvent e)
        {
            Vector2 position = Vector2.Zero;
            position.X = e.GetX(e.ActionIndex);
            position.Y = e.GetY(e.ActionIndex);
            int id = e.GetPointerId(e.ActionIndex);

            switch (e.ActionMasked)
            {
                // DOWN                
                case MotionEventActions.Down:
                case MotionEventActions.PointerDown:
                    TouchPanel.Current.AddEvent(id, TouchLocationState.Pressed, position);
                    break;

                // UP                
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    TouchPanel.Current.AddEvent(id, TouchLocationState.Released, position);
                    break;

                // MOVE                
                case MotionEventActions.Move:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        position.X = e.GetX(i);
                        position.Y = e.GetY(i);
                        TouchPanel.Current.AddEvent(id, TouchLocationState.Moved, position);
                    }
                    break;

                // CANCEL, OUTSIDE                
                case MotionEventActions.Cancel:
                case MotionEventActions.Outside:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        TouchPanel.Current.AddEvent(id, TouchLocationState.Released, position);
                    }
                    break;
            }

            return true;
        }

    }
}
