// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2021 Nick Kastellanos

using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Platform
{
    sealed class ConcreteGame : GameStrategy
    {
        //internal static string LaunchParameters;

        private WinFormsGameWindow _gameWindow;

        public ConcreteGame(Game game) : base(game)
        {
            _gameWindow = new WinFormsGameWindow(this);
            base.Window = _gameWindow;
            base.SetWindowListeners();
        }

        protected override void RunGameLoop()
        {
            _gameWindow.RunGameLoop();
        }

        public override bool IsMouseVisible
        {
            get { return base.IsMouseVisible; }
            set
            {
                if (base.IsMouseVisible != value)
                {
                    base.IsMouseVisible = value;
                    _gameWindow.MouseVisibleToggled();
                }
            }
        }
        
        public override void TickExiting()
        {
            if (_gameWindow != null)
                _gameWindow.Dispose();

            _gameWindow = null;
            Window = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_gameWindow != null)
                {
                    _gameWindow.Dispose();
                    _gameWindow = null;
                    Window = null;
                }
                
            }

            base.Dispose(disposing);
        }
    }
}
