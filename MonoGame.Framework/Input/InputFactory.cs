// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Input.Touch;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class InputFactory
    {
        private volatile static InputFactory _current;

        internal static InputFactory Current
        {
            get
            {
                InputFactory current = _current;
                if (current != null)
                    return current;

                InputFactory inputFactory = CreateInputFactory();
                InputFactory.RegisterInputFactory(inputFactory);

                return _current;
            }
        }

        private static InputFactory CreateInputFactory()
        {
            return new ConcreteInputFactory();
        }

        public static void RegisterInputFactory(InputFactory inputFactory)
        {
            if (inputFactory == null)
                throw new NullReferenceException("inputFactory");

            lock (typeof(InputFactory))
            {
                if (_current == null)
                    _current = inputFactory;
                else
                    throw new InvalidOperationException("inputFactory allready registered.");
            }
        }

        internal abstract TouchPanelStrategy CreateTouchPanelStrategy();
        internal abstract GamePadStrategy CreateGamePadStrategy();
        public abstract JoystickStrategy CreateJoystickStrategy();
        public abstract KeyboardStrategy CreateKeyboardStrategy();
        public abstract MouseStrategy CreateMouseStrategy();
        public abstract MouseCursorStrategy CreateMouseCursorStrategy(MouseCursorStrategy.MouseCursorType cursorType);
        public abstract MouseCursorStrategy CreateMouseCursorStrategy(byte[] data, int w, int h, int originx, int originy);
        internal abstract KeyboardInputStrategy CreateKeyboardInputStrategy();
        internal abstract MessageBoxStrategy CreateMessageBoxStrategy();

    }

}
