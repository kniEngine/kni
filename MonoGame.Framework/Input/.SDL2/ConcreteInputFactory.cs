// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework;
using Microsoft.Xna.Platform.Input.Touch;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteInputFactory : InputFactory
    {

        internal override GamePadStrategy CreateGamePadStrategy()
        {
            return new ConcreteGamePad();
        }

        public override JoystickStrategy CreateJoystickStrategy()
        {
            return new ConcreteJoystick();
        }

        public override KeyboardStrategy CreateKeyboardStrategy()
        {
            return new ConcreteKeyboard();
        }

        public override MouseStrategy CreateMouseStrategy()
        {
            return new ConcreteMouse();
        }

        public override MouseCursorStrategy CreateMouseCursorStrategy(MouseCursorStrategy.MouseCursorType cursorType)
        {
            return new ConcreteMouseCursor(cursorType);
        }

        public override MouseCursorStrategy CreateMouseCursorStrategy(byte[] data, int w, int h, int originx, int originy)
        {
            return new ConcreteMouseCursor(data, w, h, originx, originy);
        }

        internal override TouchPanelStrategy CreateTouchPanelStrategy()
        {
            return new ConcreteTouchPanel();
        }

        internal override KeyboardInputStrategy CreateKeyboardInputStrategy()
        {
            return new ConcreteKeyboardInput();
        }

        internal override MessageBoxStrategy CreateMessageBoxStrategy()
        {
            return new ConcreteMessageBox();
        }
    }
}