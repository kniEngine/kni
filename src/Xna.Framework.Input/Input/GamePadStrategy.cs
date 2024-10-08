﻿// Copyright (C)2024 Nick Kastellanos

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Platform.Input
{
    public abstract class GamePadStrategy
    {
        public abstract float LeftThumbDeadZone { get; }
        public abstract float RightThumbDeadZone { get; }
        public abstract int PlatformGetMaxNumberOfGamePads();
        public abstract GamePadCapabilities PlatformGetCapabilities(int index);
        public abstract GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode);
        public abstract bool PlatformSetVibration(int index, float v1, float v2, float v3, float v4);

        protected GamePadCapabilities CreateGamePadCapabilities(
            GamePadType gamePadType, string displayName, string identifier, bool isConnected,
            Buttons buttons,
            bool hasLeftVibrationMotor, bool hasRightVibrationMotor,
            bool hasVoiceSupport)
        {
            return new GamePadCapabilities(
                        gamePadType: gamePadType,
                        displayName: displayName,
                        identifier: identifier,
                        isConnected: isConnected,
                        buttons: buttons,
                        hasLeftVibrationMotor: hasLeftVibrationMotor,
                        hasRightVibrationMotor: hasRightVibrationMotor,
                        hasVoiceSupport: hasVoiceSupport
                    );
        }

        protected GamePadState CreateGamePadState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadButtons buttons, GamePadDPad dPad)
        {
            return new GamePadState(thumbSticks, triggers, buttons, dPad);
        }
        protected GamePadState CreateGamePadState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadButtons buttons, GamePadDPad dPad,
            int packetNumber)
        {
            GamePadState state = new GamePadState(thumbSticks, triggers, buttons, dPad);
            state.PacketNumber = packetNumber;
            return state;
        }
        protected GamePadState CreateGamePadState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadButtons buttons, GamePadDPad dPad,
            bool isConnected)
        {
            GamePadState state = new GamePadState(thumbSticks, triggers, buttons, dPad);
            state.IsConnected = isConnected;
            return state;
        }

        protected GamePadThumbSticks CreateGamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            return new GamePadThumbSticks(leftPosition, rightPosition, leftDeadZoneMode, rightDeadZoneMode);
        }

        protected GamePadDPad CreateGamePadDPad(Buttons buttons)
        {
            return new GamePadDPad(buttons);
        }
    }
}