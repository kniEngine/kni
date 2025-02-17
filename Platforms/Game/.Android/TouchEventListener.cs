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
                    ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddPressedEvent(id, position);
                    break;

                // MOVE
                case MotionEventActions.Move:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        position.X = e.GetX(i);
                        position.Y = e.GetY(i);
                        ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddMovedEvent(id, position);
                    }
                    break;

                // UP
                case MotionEventActions.Up:
                case MotionEventActions.PointerUp:
                    ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddReleasedEvent(id, position);
                    break;

                // OUTSIDE
                case MotionEventActions.Outside:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddReleasedEvent(id, position);
                    }
                    break;

                // CANCEL
                case MotionEventActions.Cancel:
                    for (int i = 0; i < e.PointerCount; i++)
                    {
                        id = e.GetPointerId(i);
                        ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<ConcreteTouchPanel>().AddCanceledEvent(id, position);
                    }
                    break;
            }

            return true;
        }

    }
}
