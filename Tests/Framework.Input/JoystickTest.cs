// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using Microsoft.Xna.Framework.Input;
using NUnit.Framework;

namespace Kni.Tests.Input
{
    public class JoystickTest
    {
        [TestCase(new [] { 12345, -12345 }, new [] { ButtonState.Pressed, ButtonState.Released }, true)]
        [TestCase(new [] { -7324, -32000 }, new [] { ButtonState.Pressed, ButtonState.Pressed }, false)]
        public void TestState(int[] axes, ButtonState[] buttons, bool isConnected)
        {
            JoystickHat[] hats = new[]
            {
                new JoystickHat(Buttons.DPadLeft | Buttons.DPadDown),
                new JoystickHat(Buttons.DPadLeft | Buttons.DPadUp | Buttons.DPadRight)
            };

            JoystickState state = new JoystickState();
            state.Axes = axes;
            state.Buttons = buttons;
            state.Hats = hats;
            state.IsConnected = isConnected;

            Assert.AreEqual(axes, state.Axes);
            Assert.AreEqual(buttons, state.Buttons);
            Assert.AreEqual(hats, state.Hats);
            Assert.AreEqual(isConnected, state.IsConnected);
        }

        [Test]
        public void JoyStickHatTest(
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState left, 
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState right, 
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState up, 
            [Values(ButtonState.Pressed, ButtonState.Released)] ButtonState down)
        {
            Buttons dPadButtons = (Buttons)0;
            dPadButtons |= (left  == ButtonState.Pressed) ? Buttons.DPadLeft : (Buttons)0;
            dPadButtons |= (right == ButtonState.Pressed) ? Buttons.DPadRight: (Buttons)0;
            dPadButtons |= (up    == ButtonState.Pressed) ? Buttons.DPadUp   : (Buttons)0;
            dPadButtons |= (down  == ButtonState.Pressed) ? Buttons.DPadDown : (Buttons)0;

            JoystickHat hat = new JoystickHat(dPadButtons);

            Assert.AreEqual(left, hat.Left);
            Assert.AreEqual(right, hat.Right);
            Assert.AreEqual(up, hat.Up);
            Assert.AreEqual(down, hat.Down);
        }
    }
}