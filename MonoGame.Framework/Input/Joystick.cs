// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IJoystick
    {
        bool IsSupported { get; }
        int LastConnectedIndex { get; }

        JoystickCapabilities GetCapabilities(int index);
        JoystickState GetState(int index);
        void GetState(int index, ref JoystickState joystickState);
    }

    public interface IPlatformJoystick
    {
        T GetStrategy<T>() where T : JoystickStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input
{
    /// <summary> 
    /// Allows interaction with joysticks. Unlike <see cref="GamePad"/> the number of Buttons/Axes/DPads is not limited.
    /// </summary>
    public sealed class Joystick : IJoystick
        , IPlatformJoystick
    {
        private static Joystick _current;

        /// <summary>
        /// Returns the current Keyboard instance.
        /// </summary> 
        public static Joystick Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(Joystick))
                {
                    if (_current == null)
                        _current = new Joystick();

                    return _current;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the current platform supports reading raw joystick data.
        /// </summary>
        /// <value><c>true</c> if the current platform supports reading raw joystick data; otherwise, <c>false</c>.</value>
        public static bool IsSupported
        {
            get { return ((IJoystick)Joystick.Current).IsSupported; }
        }

        /// <summary>
        /// Gets a value indicating the last joystick index connected to the system. If this value is less than 0, no joysticks are connected.
        /// <para>The order joysticks are connected and disconnected determines their index.
        /// As such, this value may be larger than 0 even if only one joystick is connected.
        /// </para>
        /// </summary>
        public static int LastConnectedIndex
        {
            get { return ((IJoystick)Joystick.Current).LastConnectedIndex; }
        }

        /// <summary>
        /// Gets the capabilities of the joystick.
        /// </summary>
        /// <param name="index">Index of the joystick you want to access.</param>
        /// <returns>The capabilities of the joystick.</returns>
        public static JoystickCapabilities GetCapabilities(int index)
        {
            return ((IJoystick)Joystick.Current).GetCapabilities(index);
        }

        /// <summary>
        /// Gets the current state of the joystick.
        /// </summary>
        /// <param name="index">Index of the joystick you want to access.</param>
        /// <returns>The state of the joystick.</returns>
        public static JoystickState GetState(int index)
        {
            return ((IJoystick)Joystick.Current).GetState(index);
        }

        /// <summary>
        /// Gets the current state of the joystick by updating an existing <see cref="JoystickState"/>.
        /// </summary>
        /// <param name="joystickState">The <see cref="JoystickState"/> to update.</param>
        /// <param name="index">Index of the joystick you want to access.</param>
        public static void GetState(ref JoystickState joystickState, int index)
        {
            ((IJoystick)Joystick.Current).GetState(index, ref joystickState);
        }

        private JoystickStrategy _strategy;

        T IPlatformJoystick.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private Joystick()
        {
            _strategy = new ConcreteJoystick();
        }

        #region IJoystick

        bool IJoystick.IsSupported
        {
            get { return _strategy.PlatformIsSupported; }
        }

        /// <summary>
        /// Gets a value indicating the last joystick index connected to the system. If this value is less than 0, no joysticks are connected.
        /// <para>The order joysticks are connected and disconnected determines their index.
        /// As such, this value may be larger than 0 even if only one joystick is connected.
        /// </para>
        /// </summary>
        int IJoystick.LastConnectedIndex
        {
            get { return _strategy.PlatformLastConnectedIndex; }
        }

        /// <summary>
        /// Gets the capabilities of the joystick.
        /// </summary>
        /// <param name="index">Index of the joystick you want to access.</param>
        /// <returns>The capabilities of the joystick.</returns>
        JoystickCapabilities IJoystick.GetCapabilities(int index)
        {
            return _strategy.PlatformGetCapabilities(index);
        }

        /// <summary>
        /// Gets the current state of the joystick.
        /// </summary>
        /// <param name="index">Index of the joystick you want to access.</param>
        /// <returns>The state of the joystick.</returns>
        JoystickState IJoystick.GetState(int index)
        {
            return _strategy.PlatformGetState(index);
        }

        /// <summary>
        /// Gets the current state of the joystick by updating an existing <see cref="JoystickState"/>.
        /// </summary>
        /// <param name="joystickState">The <see cref="JoystickState"/> to update.</param>
        /// <param name="index">Index of the joystick you want to access.</param>
        void IJoystick.GetState(int index, ref JoystickState joystickState)
        {
            _strategy.PlatformGetState(index, ref joystickState);
        }

        #endregion

    }
}
