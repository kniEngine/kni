// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.Devices.Input;
using Windows.System.Threading;
using Windows.UI.Core;
using Microsoft.Xna.Platform.Input;
using Microsoft.Xna.Platform.Input.Touch;


#if UAP
using Windows.Graphics.Display;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif

#if WINUI
using Microsoft.Graphics.Display;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Windowing;
#endif

namespace Microsoft.Xna.Framework
{
    internal class InputEvents
    {
        internal struct KeyChar
        {
            public char Character;

            public Keys Key;
        }

        public readonly ConcurrentQueue<KeyChar> TextQueue = new ConcurrentQueue<KeyChar>();
        private KeyChar _lastEnqueuedKeyChar;

        // To convert from DIPs (device independent pixels) to actual screen resolution pixels.
        private static float _currentDipFactor;

        private CoreIndependentInputSource _coreIndependentInputSource;


        public InputEvents(CoreWindow window, UIElement inputElement)
        {
            // The key events are always tied to the window as those will
            // only arrive here if some other control hasn't gotten it.
            window.KeyDown += CoreWindow_KeyDown;
            window.KeyUp += CoreWindow_KeyUp;
            window.CharacterReceived += Window_CharacterReceived;
            window.VisibilityChanged += CoreWindow_VisibilityChanged;
            window.Activated += CoreWindow_Activated;
            window.SizeChanged += CoreWindow_SizeChanged;

            DisplayInformation.GetForCurrentView().DpiChanged += InputEvents_DpiChanged;
            _currentDipFactor = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0f;

            if (inputElement is SwapChainPanel)
            {
                // Create a thread to process input events.
                WorkItemHandler workItemHandler = new WorkItemHandler( (action) =>
                {
                    CoreInputDeviceTypes inputDevices = CoreInputDeviceTypes.Mouse | CoreInputDeviceTypes.Touch | CoreInputDeviceTypes.Pen;

                    // We subscribe only to Pen. Touch & Mouse is still processed from inputElement.
                    // Currently PointerVisualizationSettings don't apply to CreateCoreIndependentInputSource 
                    // and ContactFeedback is always visible when we create a coreIndependentInputSource for Touch.
                    // If this bug get fixed we can add the Touch InputDeviceTypes.
                    // Mouse still has a race contition.
                    inputDevices &= ~CoreInputDeviceTypes.Touch;
                    inputDevices &= ~CoreInputDeviceTypes.Mouse;

                    _coreIndependentInputSource = ((SwapChainPanel)inputElement).CreateCoreIndependentInputSource(inputDevices);

                    _coreIndependentInputSource.PointerPressed += CoreWindow_PointerPressed;
                    _coreIndependentInputSource.PointerMoved += CoreWindow_PointerMoved;
                    _coreIndependentInputSource.PointerReleased += CoreWindow_PointerReleased;
                    _coreIndependentInputSource.PointerWheelChanged += CoreWindow_PointerWheelChanged;

                    _coreIndependentInputSource.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
                });
                var inputWorker = ThreadPool.RunAsync(workItemHandler, WorkItemPriority.High, WorkItemOptions.TimeSliced);
            }

            if (inputElement != null)
            {
                // If we have an input UIElement then we bind input events
                // to it else we'll get events for overlapping XAML controls.
                inputElement.PointerPressed += UIElement_PointerPressed;
                inputElement.PointerReleased += UIElement_PointerReleased;
                inputElement.PointerCanceled += UIElement_PointerReleased;
                inputElement.PointerMoved += UIElement_PointerMoved;
                inputElement.PointerWheelChanged += UIElement_PointerWheelChanged;
            }
            else // (inputElement == null)
            {
                // If we only have a CoreWindow then use it for input events.
                window.PointerPressed += CoreWindow_PointerPressed;
                window.PointerReleased += CoreWindow_PointerReleased;
                window.PointerMoved += CoreWindow_PointerMoved;
                window.PointerWheelChanged += CoreWindow_PointerWheelChanged;
            }
        }

        private void InputEvents_DpiChanged(DisplayInformation sender, object args)
        {
            _currentDipFactor = DisplayInformation.GetForCurrentView().LogicalDpi / 96.0f;
        }

        #region UIElement Events

        private void UIElement_PointerPressed(object sender, PointerRoutedEventArgs args)
        {
            //Capture this pointer so we continue getting events even if it is dragged off us
            ((UIElement)sender).CapturePointer(args.Pointer);

            var pointerPoint = args.GetCurrentPoint(null);
            PointerPressed(pointerPoint, sender as UIElement, args.Pointer);
            args.Handled = true;
        }

        private void UIElement_PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(null);
            PointerMoved(pointerPoint);
            args.Handled = true;
        }

        private void UIElement_PointerReleased(object sender, PointerRoutedEventArgs args)
        {
            ((UIElement)sender).ReleasePointerCapture(args.Pointer);

            var pointerPoint = args.GetCurrentPoint(null);
            PointerReleased(pointerPoint, sender as UIElement, args.Pointer);
            args.Handled = true;
        }

        private void UIElement_PointerWheelChanged(object sender, PointerRoutedEventArgs args)
        {
            var pointerPoint = args.GetCurrentPoint(null);
            UpdateMouse(pointerPoint);
            args.Handled = true;
        }

        #endregion // UIElement Events

        #region CoreWindow Events

        private void CoreWindow_PointerPressed(object sender, PointerEventArgs args)
        {
            PointerPressed(args.CurrentPoint, null, null);
            args.Handled = true;
        }

        private void CoreWindow_PointerMoved(object sender, PointerEventArgs args)
        {
            PointerMoved(args.CurrentPoint);
            args.Handled = true;
        }

        private void CoreWindow_PointerReleased(object sender, PointerEventArgs args)
        {
            PointerReleased(args.CurrentPoint, null, null);
            args.Handled = true;
        }

        private void CoreWindow_PointerWheelChanged(object sender, PointerEventArgs args)
        {
            UpdateMouse(args.CurrentPoint);
            args.Handled = true;
        }

        #endregion // CoreWindow Events

        private void PointerPressed(PointerPoint pointerPoint, UIElement target, Pointer pointer)
        {
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * _currentDipFactor;

            var isTouch = pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch;

            if (isTouch)
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent((int)pointerPoint.PointerId, TouchLocationState.Pressed, pos);

            if (!isTouch)
            {
                // Mouse or stylus event.
                UpdateMouse(pointerPoint);

                // Capture future pointer events until a release.		
                if (target != null)
                    target.CapturePointer(pointer);
            }
        }

        private void PointerMoved(PointerPoint pointerPoint)
        {
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * _currentDipFactor;

            var isTouch = pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch;
            var touchIsDown = pointerPoint.IsInContact;

            if (isTouch && touchIsDown)
            {
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent((int)pointerPoint.PointerId, TouchLocationState.Moved, pos);
            }

            if (!isTouch)
            {
                // Mouse or stylus event.
                UpdateMouse(pointerPoint);
            }
        }

        private void PointerReleased(PointerPoint pointerPoint, UIElement target, Pointer pointer)
        {
            var pos = new Vector2((float)pointerPoint.Position.X, (float)pointerPoint.Position.Y) * _currentDipFactor;

            var isTouch = pointerPoint.PointerDevice.PointerDeviceType == PointerDeviceType.Touch;

            if (isTouch) 
                ((IPlatformTouchPanel)TouchPanel.Current).GetStrategy<TouchPanelStrategy>().AddEvent((int)pointerPoint.PointerId, TouchLocationState.Released, pos);

            if (!isTouch)
            {
                // Mouse or stylus event.
                UpdateMouse(pointerPoint);

                // Release the captured pointer.
                if (target != null)
                    target.ReleasePointerCapture(pointer);
            }
        }

        private static void UpdateMouse(PointerPoint point)
        {
            var x = (int)(point.Position.X * _currentDipFactor);
            var y = (int)(point.Position.Y * _currentDipFactor);

            var state = point.Properties;

            int verticalScrollDelta = 0;
            int horizontalScrollDelta = 0;

            if (state.IsHorizontalMouseWheel)
                horizontalScrollDelta = state.MouseWheelDelta;
            else
                verticalScrollDelta = state.MouseWheelDelta;

            ((IPlatformMouse)Mouse.Current).GetStrategy<ConcreteMouse>().PrimaryWindow.MouseState = new MouseState(x, y,
                ((IPlatformMouse)Mouse.Current).GetStrategy<ConcreteMouse>().PrimaryWindow.MouseState.ScrollWheelValue + verticalScrollDelta,
                ((IPlatformMouse)Mouse.Current).GetStrategy<ConcreteMouse>().PrimaryWindow.MouseState.HorizontalScrollWheelValue + horizontalScrollDelta,
                0, 0,
                state.IsLeftButtonPressed ? ButtonState.Pressed : ButtonState.Released,
                state.IsMiddleButtonPressed ? ButtonState.Pressed : ButtonState.Released,
                state.IsRightButtonPressed ? ButtonState.Pressed : ButtonState.Released,
                state.IsXButton1Pressed ? ButtonState.Pressed : ButtonState.Released,
                state.IsXButton2Pressed ? ButtonState.Pressed : ButtonState.Released);
        }

        public void UpdateState()
        {
            // Update the keyboard state.
            ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().UpdateState();
        }

        internal static Keys KeyTranslate(Windows.System.VirtualKey inkey, CorePhysicalKeyStatus keyStatus)
        {
            switch (inkey)
            {
                // WinRT does not distinguish between left/right keys
                // We have to check for special keys such as control/shift/alt/ etc
                case Windows.System.VirtualKey.Control:
                    // we can detect right Control by checking the IsExtendedKey value.
                    return (keyStatus.IsExtendedKey) ? Keys.RightControl : Keys.LeftControl;
                case Windows.System.VirtualKey.Shift:
                    // we can detect right shift by checking the scancode value.
                    // left shift is 0x2A, right shift is 0x36. IsExtendedKey is always false.
                    return (keyStatus.ScanCode == 0x36) ? Keys.RightShift : Keys.LeftShift;
                // Note that the Alt key is now referred to as Menu.
                // ALT key doesn't get fired by KeyUp/KeyDown events.
                // One solution could be to check CoreWindow.GetKeyState(...) on every tick.
                case Windows.System.VirtualKey.Menu:
                    return Keys.LeftAlt;

                default:
                    return (Keys)inkey;
            }
        }

        private void CoreWindow_KeyUp(object sender, KeyEventArgs args)
        {
            var xnaKey = KeyTranslate(args.VirtualKey, args.KeyStatus);

            ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().ClearKey(xnaKey);
        }

        private void CoreWindow_KeyDown(object sender, KeyEventArgs args)
        {
            var xnaKey = KeyTranslate(args.VirtualKey, args.KeyStatus);

            ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().SetKey(xnaKey);

            _lastEnqueuedKeyChar = new KeyChar();
            _lastEnqueuedKeyChar.Key = xnaKey;
        }

        private void Window_CharacterReceived(CoreWindow sender, CharacterReceivedEventArgs args)
        {
            _lastEnqueuedKeyChar.Character = (char)args.KeyCode;
            TextQueue.Enqueue(_lastEnqueuedKeyChar);
        }

        private void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            // If the window is resized then also 
            // drop any current key states.
            ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().Clear();

            // required of input can stop working if we change focus
            WakeupKeyboardInput();
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            // Forget about the held keys when we lose focus as we don't
            // receive key events for them while we are in the background
            if (args.WindowActivationState == CoreWindowActivationState.Deactivated)
                ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().Clear();

            // required of input can stop working if we change focus
            WakeupKeyboardInput();
        }

        private void CoreWindow_VisibilityChanged(CoreWindow sender, VisibilityChangedEventArgs args)
        {
            // Forget about the held keys when we disappear as we don't
            // receive key events for them while we are in the background
            if (!args.Visible)
                ((IPlatformKeyboard)Keyboard.Current).GetStrategy<ConcreteKeyboard>().Clear();

            // required of input can stop working if we change focus
            WakeupKeyboardInput();
        }

        private static void WakeupKeyboardInput()
        {
#if UAP
            Window window = Window.Current;
#elif WINUI
            Window window = App.Window;
#endif
            if (window != null)
                window.CoreWindow.IsInputEnabled = true;

#if UAP
            CoreWindow coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.IsInputEnabled = true;
#elif WINUI
            AppWindow appWindow = AppWindow.Create();
            appWindow.IsInputEnabled = true;
#endif
        }
    }
}
