// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
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

                lock (typeof(InputFactory))
                {
                    if (_current != null)
                        return _current;

                    Console.WriteLine("InputFactory not found.");
                    Console.WriteLine("Initialize input with 'InputFactory.RegisterInputFactory(new ConcreteInputFactory());'.");
                    InputFactory inputFactory = CreateInputFactory();
                    InputFactory.RegisterInputFactory(inputFactory);
                }

                return _current;
            }
        }

        private static InputFactory CreateInputFactory()
        {
            Console.WriteLine("Registering ConcreteInputFactoryStrategy through reflection.");

            Type type = Type.GetType("Microsoft.Xna.Platform.Input.ConcreteInputFactory, Xna.Platform", false);
            if (type != null)
                if (type.IsSubclassOf(typeof(InputFactory)) && !type.IsAbstract)
                    return (InputFactory)Activator.CreateInstance(type);

            return null;
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

        public abstract TouchPanelStrategy CreateTouchPanelStrategy();
        public abstract GamePadStrategy CreateGamePadStrategy();
        public abstract JoystickStrategy CreateJoystickStrategy();
        public abstract KeyboardStrategy CreateKeyboardStrategy();
        public abstract MouseStrategy CreateMouseStrategy();
        public abstract MouseCursorStrategy CreateMouseCursorStrategy(MouseCursorStrategy.MouseCursorType cursorType);
        public abstract MouseCursorStrategy CreateMouseCursorStrategy(byte[] data, int w, int h, int originx, int originy);
        public abstract KeyboardInputStrategy CreateKeyboardInputStrategy();
        public abstract MessageBoxStrategy CreateMessageBoxStrategy();

    }

}
