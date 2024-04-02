// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using DroidKeycode = Android.Views.Keycode;

namespace Microsoft.Xna.Platform.Input
{
    public sealed class ConcreteKeyboard : KeyboardStrategy
    {
        private List<Keys> _keys;
        private readonly IDictionary<DroidKeycode, Keys> _keyMap;

        public ConcreteKeyboard()
        {
            _keyMap = LoadKeyMap();
        }

        public override KeyboardState PlatformGetState()
        {
            return new KeyboardState(_keys);
        }

        internal void SetKeys(List<Keys> keys)
        {
            _keys = keys;
        }

        internal bool KeyDown(DroidKeycode keyCode)
        {
            Keys key;
            if (_keyMap.TryGetValue(keyCode, out key) && key != Keys.None)
            {
                if (!_keys.Contains(key))
                    _keys.Add(key);
                return true;
            }
            return false;
        }

        internal bool KeyUp(DroidKeycode keyCode)
        {
            Keys key;
            if (_keyMap.TryGetValue(keyCode, out key) && key != Keys.None)
            {
                if (_keys.Contains(key))
                    _keys.Remove(key);
                return true;
            }
            return false;
        }

        private IDictionary<DroidKeycode, Keys> LoadKeyMap()
        {
            // create a map for every Keycode and default it to none so that every possible key is mapped
            Dictionary<DroidKeycode, Keys> maps = Enum.GetValues(typeof(DroidKeycode))
                .Cast<DroidKeycode>()
                .ToDictionary(key => key, key => Keys.None);

            // then update it with the actual mappings
            maps[DroidKeycode.DpadLeft] = Keys.Left;
            maps[DroidKeycode.DpadRight] = Keys.Right;
            maps[DroidKeycode.DpadUp] = Keys.Up;
            maps[DroidKeycode.DpadDown] = Keys.Down;
            maps[DroidKeycode.DpadCenter] = Keys.Enter;
            maps[DroidKeycode.Num0] = Keys.D0;
            maps[DroidKeycode.Num1] = Keys.D1;
            maps[DroidKeycode.Num2] = Keys.D2;
            maps[DroidKeycode.Num3] = Keys.D3;
            maps[DroidKeycode.Num4] = Keys.D4;
            maps[DroidKeycode.Num5] = Keys.D5;
            maps[DroidKeycode.Num6] = Keys.D6;
            maps[DroidKeycode.Num7] = Keys.D7;
            maps[DroidKeycode.Num8] = Keys.D8;
            maps[DroidKeycode.Num9] = Keys.D9;
            maps[DroidKeycode.A] = Keys.A;
            maps[DroidKeycode.B] = Keys.B;
            maps[DroidKeycode.C] = Keys.C;
            maps[DroidKeycode.D] = Keys.D;
            maps[DroidKeycode.E] = Keys.E;
            maps[DroidKeycode.F] = Keys.F;
            maps[DroidKeycode.G] = Keys.G;
            maps[DroidKeycode.H] = Keys.H;
            maps[DroidKeycode.I] = Keys.I;
            maps[DroidKeycode.J] = Keys.J;
            maps[DroidKeycode.K] = Keys.K;
            maps[DroidKeycode.L] = Keys.L;
            maps[DroidKeycode.M] = Keys.M;
            maps[DroidKeycode.N] = Keys.N;
            maps[DroidKeycode.O] = Keys.O;
            maps[DroidKeycode.P] = Keys.P;
            maps[DroidKeycode.Q] = Keys.Q;
            maps[DroidKeycode.R] = Keys.R;
            maps[DroidKeycode.S] = Keys.S;
            maps[DroidKeycode.T] = Keys.T;
            maps[DroidKeycode.U] = Keys.U;
            maps[DroidKeycode.V] = Keys.V;
            maps[DroidKeycode.W] = Keys.W;
            maps[DroidKeycode.X] = Keys.X;
            maps[DroidKeycode.Y] = Keys.Y;
            maps[DroidKeycode.Z] = Keys.Z;
            maps[DroidKeycode.Space] = Keys.Space;
            maps[DroidKeycode.Escape] = Keys.Escape;
            maps[DroidKeycode.Back] = Keys.Back;
            maps[DroidKeycode.Home] = Keys.Home;
            maps[DroidKeycode.Enter] = Keys.Enter;
            maps[DroidKeycode.Period] = Keys.OemPeriod;
            maps[DroidKeycode.Comma] = Keys.OemComma;
            maps[DroidKeycode.Menu] = Keys.Help;
            maps[DroidKeycode.Search] = Keys.BrowserSearch;
            maps[DroidKeycode.VolumeUp] = Keys.VolumeUp;
            maps[DroidKeycode.VolumeDown] = Keys.VolumeDown;
            maps[DroidKeycode.MediaPause] = Keys.Pause;
            maps[DroidKeycode.MediaPlayPause] = Keys.MediaPlayPause;
            maps[DroidKeycode.MediaStop] = Keys.MediaStop;
            maps[DroidKeycode.MediaNext] = Keys.MediaNextTrack;
            maps[DroidKeycode.MediaPrevious] = Keys.MediaPreviousTrack;
            maps[DroidKeycode.Mute] = Keys.VolumeMute;
            maps[DroidKeycode.AltLeft] = Keys.LeftAlt;
            maps[DroidKeycode.AltRight] = Keys.RightAlt;
            maps[DroidKeycode.ShiftLeft] = Keys.LeftShift;
            maps[DroidKeycode.ShiftRight] = Keys.RightShift;
            maps[DroidKeycode.Tab] = Keys.Tab;
            maps[DroidKeycode.Del] = Keys.Delete;
            maps[DroidKeycode.Minus] = Keys.OemMinus;
            maps[DroidKeycode.LeftBracket] = Keys.OemOpenBrackets;
            maps[DroidKeycode.RightBracket] = Keys.OemCloseBrackets;
            maps[DroidKeycode.Backslash] = Keys.OemBackslash;
            maps[DroidKeycode.Semicolon] = Keys.OemSemicolon;
            maps[DroidKeycode.PageUp] = Keys.PageUp;
            maps[DroidKeycode.PageDown] = Keys.PageDown;
            maps[DroidKeycode.CtrlLeft] = Keys.LeftControl;
            maps[DroidKeycode.CtrlRight] = Keys.RightControl;
            maps[DroidKeycode.CapsLock] = Keys.CapsLock;
            maps[DroidKeycode.ScrollLock] = Keys.Scroll;
            maps[DroidKeycode.NumLock] = Keys.NumLock;
            maps[DroidKeycode.Insert] = Keys.Insert;
            maps[DroidKeycode.F1] = Keys.F1;
            maps[DroidKeycode.F2] = Keys.F2;
            maps[DroidKeycode.F3] = Keys.F3;
            maps[DroidKeycode.F4] = Keys.F4;
            maps[DroidKeycode.F5] = Keys.F5;
            maps[DroidKeycode.F6] = Keys.F6;
            maps[DroidKeycode.F7] = Keys.F7;
            maps[DroidKeycode.F8] = Keys.F8;
            maps[DroidKeycode.F9] = Keys.F9;
            maps[DroidKeycode.F10] = Keys.F10;
            maps[DroidKeycode.F11] = Keys.F11;
            maps[DroidKeycode.F12] = Keys.F12;
            maps[DroidKeycode.NumpadDivide] = Keys.Divide;
            maps[DroidKeycode.NumpadMultiply] = Keys.Multiply;
            maps[DroidKeycode.NumpadSubtract] = Keys.Subtract;
            maps[DroidKeycode.NumpadAdd] = Keys.Add;

            return maps;
        }
    }
}
