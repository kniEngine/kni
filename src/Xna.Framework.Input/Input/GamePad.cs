// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Input;

namespace Microsoft.Xna.Platform.Input
{
    public interface IGamePad
    {
        int MaximumGamePadCount { get; }

        GamePadCapabilities GetCapabilities(PlayerIndex playerIndex);
        GamePadCapabilities GetCapabilities(int index);
        GamePadState GetState(PlayerIndex playerIndex);
        GamePadState GetState(int index);
        GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode);
        GamePadState GetState(int index, GamePadDeadZone deadZoneMode);
        GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode);
        GamePadState GetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode);
        bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor);
        bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger);
        bool SetVibration(int index, float leftMotor, float rightMotor);
        bool SetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger);
    }

    public interface IPlatformGamePad
    {
        T GetStrategy<T>() where T : GamePadStrategy;
    }
}

namespace Microsoft.Xna.Framework.Input
{
    /// <summary> 
    /// Supports querying the game controllers and setting the vibration motors.
    /// </summary>
    public sealed class GamePad : IGamePad
        , IPlatformGamePad
    {
        private static GamePad _current;

        /// <summary>
        /// Returns the current GamePad instance.
        /// </summary> 
        public static GamePad Current
        {
            get
            {
                if (_current != null)
                    return _current;

                lock (typeof(GamePad))
                {
                    if (_current == null)
                        _current = new GamePad();

                    return _current;
                }
            }
        }

        /// <summary>
        /// The maximum number of game pads supported on this system.
        /// </summary>
        public static int MaximumGamePadCount
        {
            get { return ((IGamePad)GamePad.Current).MaximumGamePadCount; }
        }

        /// <summary>
        /// Returns the capabilities of the connected controller.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <returns>The capabilities of the controller.</returns>
        public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
        {
            return ((IGamePad)GamePad.Current).GetCapabilities(playerIndex);
        }

        /// <summary>
        /// Returns the capabilities of the connected controller.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <returns>The capabilities of the controller.</returns>
        public static GamePadCapabilities GetCapabilities(int index)
        {
            return ((IGamePad)GamePad.Current).GetCapabilities(index);
        }

        /// <summary>
        /// Gets the current state of a game pad controller with an independent axes dead zone.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex)
        {
            return ((IGamePad)GamePad.Current).GetState(playerIndex);
        }

        /// <summary>
        /// Gets the current state of a game pad controller with an independent axes dead zone.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(int index)
        {
            return ((IGamePad)GamePad.Current).GetState(index);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <param name="deadZoneMode">Enumerated value that specifies what dead zone type to use.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode)
        {
            return ((IGamePad)GamePad.Current).GetState(playerIndex, deadZoneMode);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="deadZoneMode">Enumerated value that specifies what dead zone type to use.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(int index, GamePadDeadZone deadZoneMode)
        {           
            return ((IGamePad)GamePad.Current).GetState(index, deadZoneMode);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="playerIndex">Player index for the controller you want to query.</param>
        /// <param name="leftDeadZoneMode">Enumerated value that specifies what dead zone type to use for the left stick.</param>
        /// <param name="rightDeadZoneMode">Enumerated value that specifies what dead zone type to use for the right stick.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            return ((IGamePad)GamePad.Current).GetState(playerIndex, leftDeadZoneMode, rightDeadZoneMode);
        }

        /// <summary>
        /// Gets the current state of a game pad controller, using a specified dead zone
        /// on analog stick positions.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="leftDeadZoneMode">Enumerated value that specifies what dead zone type to use for the left stick.</param>
        /// <param name="rightDeadZoneMode">Enumerated value that specifies what dead zone type to use for the right stick.</param>
        /// <returns>The state of the controller.</returns>
        public static GamePadState GetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            return ((IGamePad)GamePad.Current).GetState(index, leftDeadZoneMode, rightDeadZoneMode);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="playerIndex">Player index that identifies the controller to set.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            return ((IGamePad)GamePad.Current).SetVibration(playerIndex, leftMotor, rightMotor);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="playerIndex">Player index that identifies the controller to set.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="leftTrigger">(Xbox One controller only) The speed of the left trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="rightTrigger">(Xbox One controller only) The speed of the right trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return ((IGamePad)GamePad.Current).SetVibration(playerIndex, leftMotor, rightMotor, leftTrigger, rightTrigger);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(int index, float leftMotor, float rightMotor)
        {           
            return ((IGamePad)GamePad.Current).SetVibration(index, leftMotor, rightMotor);
        }

        /// <summary>
        /// Sets the vibration motor speeds on the controller device if supported.
        /// </summary>
        /// <param name="index">Index for the controller you want to query.</param>
        /// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
        /// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="leftTrigger">(Xbox One controller only) The speed of the left trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <param name="rightTrigger">(Xbox One controller only) The speed of the right trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
        /// <returns>Returns true if the vibration motors were set.</returns>
        public static bool SetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return ((IGamePad)GamePad.Current).SetVibration(index, leftMotor, rightMotor, leftTrigger, rightTrigger);
        }

        private GamePadStrategy _strategy;

        T IPlatformGamePad.GetStrategy<T>()
        {
            return (T)_strategy;
        }

        private GamePad()
        {
            _strategy = InputFactory.Current.CreateGamePadStrategy();
        }

        #region IGamePad

        int IGamePad.MaximumGamePadCount
        {
            get { return _strategy.PlatformGetMaxNumberOfGamePads(); }
        }

        GamePadCapabilities IGamePad.GetCapabilities(PlayerIndex playerIndex)
        {
            return GetCapabilities((int)playerIndex);
        }

        GamePadCapabilities IGamePad.GetCapabilities(int index)
        {
            if (index < 0 || index >= _strategy.PlatformGetMaxNumberOfGamePads())
            {
                return new GamePadCapabilities(
                    gamePadType: GamePadType.Unknown,
                    displayName: null,
                    identifier: null,
                    isConnected: false,
                    buttons: (Buttons)0,
                    hasLeftVibrationMotor: false,
                    hasRightVibrationMotor: false,
                    hasVoiceSupport: false
                );
            }

            return _strategy.PlatformGetCapabilities(index);
        }

        GamePadState IGamePad.GetState(PlayerIndex playerIndex)
        {
            return GetState((int)playerIndex, GamePadDeadZone.IndependentAxes);
        }

        GamePadState IGamePad.GetState(int index)
        {
            return GetState(index, GamePadDeadZone.IndependentAxes);
        }

        GamePadState IGamePad.GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode)
        {
            return GetState((int)playerIndex, deadZoneMode);
        }

        GamePadState IGamePad.GetState(int index, GamePadDeadZone deadZoneMode)
        {
            return GetState(index, deadZoneMode, deadZoneMode);
        }

        GamePadState IGamePad.GetState(PlayerIndex playerIndex, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            return GetState((int)playerIndex, leftDeadZoneMode, rightDeadZoneMode);
        }

        GamePadState IGamePad.GetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            if (index < 0 || index >= _strategy.PlatformGetMaxNumberOfGamePads())
                return new GamePadState();

            return _strategy.PlatformGetState(index, leftDeadZoneMode, rightDeadZoneMode);
        }

        bool IGamePad.SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
        {
            return SetVibration((int)playerIndex, leftMotor, rightMotor, 0.0f, 0.0f);
        }

        bool IGamePad.SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            return SetVibration((int)playerIndex, leftMotor, rightMotor, leftTrigger, rightTrigger);
        }

        bool IGamePad.SetVibration(int index, float leftMotor, float rightMotor)
        {
            return SetVibration(index, leftMotor, rightMotor, 0.0f, 0.0f);
        }

        bool IGamePad.SetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (index < 0 || index >= _strategy.PlatformGetMaxNumberOfGamePads())
                return false;

            return _strategy.PlatformSetVibration(index, MathHelper.Clamp(leftMotor, 0.0f, 1.0f), MathHelper.Clamp(rightMotor, 0.0f, 1.0f), MathHelper.Clamp(leftTrigger, 0.0f, 1.0f), MathHelper.Clamp(rightTrigger, 0.0f, 1.0f));
        }


        #endregion IGamePad
    }
}
