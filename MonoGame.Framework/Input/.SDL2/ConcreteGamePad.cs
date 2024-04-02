// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Platform.Utilities;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteGamePad : GamePadStrategy
    {
        private Sdl SDL { get { return Sdl.Current; } }

        private class GamePadInfo
        {
            public IntPtr Device;
            public int PacketNumber;
        }

        private readonly Dictionary<int, GamePadInfo> Gamepads = new Dictionary<int, GamePadInfo>();
        private readonly Dictionary<int, int> _translationTable = new Dictionary<int, int>();

        public ConcreteGamePad()
        {
            InitDatabase();
        }

        ~ConcreteGamePad()
        {
            foreach (KeyValuePair<int, GamePadInfo> entry in Gamepads)
                SDL.GAMECONTROLLER.Close(entry.Value.Device);

            Gamepads.Clear();            
        }


        private void InitDatabase()
        {
            using (Stream stream = ReflectionHelpers.GetAssembly(typeof(GamePad)).GetManifestResourceStream("gamecontrollerdb.txt"))
            {
                if (stream != null)
                {
                    using (BinaryReader reader = new BinaryReader(stream))
                    {
                        try
                        {
                            IntPtr src = SDL.RwFromMem(reader.ReadBytes((int)stream.Length), (int)stream.Length);
                            SDL.GAMECONTROLLER.AddMappingFromRw(src, 1);
                        }
                        catch { }
                    }
                }
            }
        }

        internal void AddDevice(int deviceId)
        {
            GamePadInfo gamepad = new GamePadInfo();
            gamepad.Device = SDL.GAMECONTROLLER.Open(deviceId);

            int id = 0;
            while (Gamepads.ContainsKey(id))
                id++;

            Gamepads.Add(id, gamepad);

            RefreshTranslationTable();
        }

        internal void RemoveDevice(int instanceid)
        {
            foreach (KeyValuePair<int, GamePadInfo> entry in Gamepads)
            {
                if (SDL.JOYSTICK.InstanceID(SDL.GAMECONTROLLER.GetJoystick(entry.Value.Device)) == instanceid)
                {
                    Gamepads.Remove(entry.Key);
                    SDL.GAMECONTROLLER.Close(entry.Value.Device);
                    break;
                }
            }

            RefreshTranslationTable();
        }

        internal void RefreshTranslationTable()
        {
            _translationTable.Clear();
            foreach (KeyValuePair<int,GamePadInfo> pair in Gamepads)
            {
                _translationTable[SDL.JOYSTICK.InstanceID(SDL.GAMECONTROLLER.GetJoystick(pair.Value.Device))] = pair.Key;
            }
        }

        internal void UpdatePacketInfo(int instanceid, uint packetNumber)
        {
            int index;
            if (_translationTable.TryGetValue(instanceid, out index))
            {
                GamePadInfo info = null;
                if (Gamepads.TryGetValue(index, out info))
                {
                    info.PacketNumber = packetNumber < int.MaxValue ? (int)packetNumber : (int)(packetNumber - (uint)int.MaxValue);
                }
            }
        }

        public override int PlatformGetMaxNumberOfGamePads()
        {
            return 16;
        }

        public override GamePadCapabilities PlatformGetCapabilities(int index)
        {
            if (!Gamepads.ContainsKey(index))
                return new GamePadCapabilities();

            IntPtr gamecontroller = Gamepads[index].Device;

            GamePadCapabilities caps = new GamePadCapabilities();
            caps.IsConnected = true;
            caps.DisplayName = SDL.GAMECONTROLLER.GetName(gamecontroller);
            caps.Identifier = SDL.JOYSTICK.GetGUID(SDL.GAMECONTROLLER.GetJoystick(gamecontroller)).ToString();
            caps.HasLeftVibrationMotor = caps.HasRightVibrationMotor = SDL.GAMECONTROLLER.HasRumble(gamecontroller) != 0;
            caps.GamePadType = GamePadType.GamePad;

            ParseCapabilities(gamecontroller, ref caps);

            return caps;
        }

        private void ParseCapabilities(IntPtr gamecontroller, ref GamePadCapabilities caps)
        {
            IntPtr pStrMappings = IntPtr.Zero;
            try
            {
                pStrMappings = SDL.GAMECONTROLLER.SDL_GameControllerMapping(gamecontroller);
                if (pStrMappings == IntPtr.Zero)
                    return;

                string mappings = InteropHelpers.Utf8ToString(pStrMappings);

                for (int idx = 0; idx < mappings.Length;)
                {
                    if (MatchKey("a", mappings, ref idx))
                        caps.HasAButton = true;
                    else
                    if (MatchKey("b", mappings, ref idx))
                        caps.HasBButton = true;
                    else
                    if (MatchKey("x", mappings, ref idx))
                        caps.HasXButton = true;
                    else
                    if (MatchKey("y", mappings, ref idx))
                        caps.HasYButton = true;
                    else
                    if (MatchKey("back", mappings, ref idx))
                        caps.HasBackButton = true;
                    else
                    if (MatchKey("guide", mappings, ref idx))
                        caps.HasBigButton = true;
                    else
                    if (MatchKey("start", mappings, ref idx))
                        caps.HasStartButton = true;
                    else
                    if (MatchKey("dpleft", mappings, ref idx))
                        caps.HasDPadLeftButton = true;
                    else
                    if (MatchKey("dpdown", mappings, ref idx))
                        caps.HasDPadDownButton = true;
                    else
                    if (MatchKey("dpright", mappings, ref idx))
                        caps.HasDPadRightButton = true;
                    else
                    if (MatchKey("dpup", mappings, ref idx))
                        caps.HasDPadUpButton = true;
                    else
                    if (MatchKey("leftshoulder", mappings, ref idx))
                        caps.HasLeftShoulderButton = true;
                    else
                    if (MatchKey("lefttrigger", mappings, ref idx))
                        caps.HasLeftTrigger = true;
                    else
                    if (MatchKey("rightshoulder", mappings, ref idx))
                        caps.HasRightShoulderButton = true;
                    else
                    if (MatchKey("righttrigger", mappings, ref idx))
                        caps.HasRightTrigger = true;
                    else
                    if (MatchKey("leftstick", mappings, ref idx))
                        caps.HasLeftStickButton = true;
                    else
                    if (MatchKey("rightstick", mappings, ref idx))
                        caps.HasRightStickButton = true;
                    else
                    if (MatchKey("leftx", mappings, ref idx))
                        caps.HasLeftXThumbStick = true;
                    else
                    if (MatchKey("lefty", mappings, ref idx))
                        caps.HasLeftYThumbStick = true;
                    else
                    if (MatchKey("rightx", mappings, ref idx))
                        caps.HasRightXThumbStick = true;
                    else
                    if (MatchKey("righty", mappings, ref idx))
                        caps.HasRightYThumbStick = true;

                    if (idx < mappings.Length)
                    {
                        int nidx = mappings.IndexOf(',', idx);
                        if (nidx != -1)
                        {
                            idx = nidx + 1;
                            continue;
                        }
                    }
                    break;
                }
            }
            finally
            {
                if (pStrMappings != IntPtr.Zero)
                    SDL.SDL_Free(pStrMappings);
            }
        }

        private bool MatchKey(string match, string input, ref int startIndex)
        {
            int nIndex = startIndex;
            if (!Match(match, input, ref nIndex))
                return false;
            if (!Match(":", input, ref nIndex))
                return false;

            startIndex = nIndex;
            return true;
        }

        private bool Match(string match, string input, ref int startIndex)
        {
            if (input.Length - startIndex < match.Length)
                return false;

            int matchIndex = input.IndexOf(match, startIndex, match.Length);
            if (matchIndex != startIndex)
                return false;

            startIndex += match.Length;
            return true;
        }

        private float GetFromSdlAxis(int axis)
        {
            // SDL Axis ranges from -32768 to 32767, so we need to divide with different numbers depending on if it's positive
            if (axis < 0)
                return axis / 32768f;

            return axis / 32767f;
        }

        public override GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
        {
            if (!Gamepads.ContainsKey(index))
                return GamePadState.Default;

            GamePadInfo gamepadInfo = Gamepads[index];
            IntPtr gdevice = gamepadInfo.Device;

            // Y gamepad axis is rotate between SDL and XNA
            GamePadThumbSticks thumbSticks =
                new GamePadThumbSticks(
                    new Vector2(
                        GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(gdevice, Sdl.GameController.Axis.LeftX)),
                        GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(gdevice, Sdl.GameController.Axis.LeftY)) * -1f
                    ),
                    new Vector2(
                        GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(gdevice, Sdl.GameController.Axis.RightX)),
                        GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(gdevice, Sdl.GameController.Axis.RightY)) * -1f
                    ),
                    leftDeadZoneMode,
                    rightDeadZoneMode
                );

            GamePadTriggers triggers = new GamePadTriggers(
                GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(gdevice, Sdl.GameController.Axis.TriggerLeft)),
                GetFromSdlAxis(SDL.GAMECONTROLLER.GetAxis(gdevice, Sdl.GameController.Axis.TriggerRight))
            );

            GamePadButtons buttons =
                new GamePadButtons(
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.A) == 1) ? Buttons.A : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.B) == 1) ? Buttons.B : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.Back) == 1) ? Buttons.Back : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.Guide) == 1) ? Buttons.BigButton : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.LeftShoulder) == 1) ? Buttons.LeftShoulder : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.RightShoulder) == 1) ? Buttons.RightShoulder : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.LeftStick) == 1) ? Buttons.LeftStick : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.RightStick) == 1) ? Buttons.RightStick : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.Start) == 1) ? Buttons.Start : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.X) == 1) ? Buttons.X : 0) |
                    ((SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.Y) == 1) ? Buttons.Y : 0) |
                    ((triggers.Left > 0f) ? Buttons.LeftTrigger : 0) |
                    ((triggers.Right > 0f) ? Buttons.RightTrigger : 0)
                );

            GamePadDPad dPad =
                new GamePadDPad(
                    (SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.DpadUp) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.DpadDown) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.DpadLeft) == 1) ? ButtonState.Pressed : ButtonState.Released,
                    (SDL.GAMECONTROLLER.GetButton(gdevice, Sdl.GameController.Button.DpadRight) == 1) ? ButtonState.Pressed : ButtonState.Released
                );

            GamePadState ret = new GamePadState(thumbSticks, triggers, buttons, dPad);
            ret.PacketNumber = gamepadInfo.PacketNumber;
            return ret;
        }

        public override bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
        {
            if (!Gamepads.ContainsKey(index))
                return false;

            GamePadInfo gamepad = Gamepads[index];

            return SDL.GAMECONTROLLER.Rumble(gamepad.Device, (ushort)(65535f * leftMotor),
                       (ushort)(65535f * rightMotor), uint.MaxValue) == 0 &&
                   SDL.GAMECONTROLLER.RumbleTriggers(gamepad.Device, (ushort)(65535f * leftTrigger),
                       (ushort)(65535f * rightTrigger), uint.MaxValue) == 0;
        }
    }
}
