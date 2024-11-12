// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.ApplicationModel.Activation;

namespace Microsoft.Xna.Platform
{
    class UAPFrameworkView<T> : IFrameworkView
        where T : Game, new()
    {
        private CoreApplicationView _applicationView;
        private T _game;

        public UAPFrameworkView()
        {
        }

        void IFrameworkView.Initialize(CoreApplicationView applicationView)
        {
            _applicationView = applicationView;

            _applicationView.Activated += ApplicationView_Activated;
        }

        private void ApplicationView_Activated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Launch)
            {
                // Save any launch parameters to be parsed by the platform.
                ConcreteGame.LaunchParameters = ((LaunchActivatedEventArgs)args).Arguments;
                ConcreteGame.PreviousExecutionState = ((LaunchActivatedEventArgs)args).PreviousExecutionState;

                // Construct the game.
                _game = new T();
            }
            else if (args.Kind == ActivationKind.Protocol)
            {
                // Save any protocol launch parameters to be parsed by the platform.
                var protocolArgs = args as ProtocolActivatedEventArgs;
                ConcreteGame.LaunchParameters = protocolArgs.Uri.AbsoluteUri;
                ConcreteGame.PreviousExecutionState = protocolArgs.PreviousExecutionState;

                // Construct the game if it does not exist
                // Protocol can be used to reactivate a suspended game
                if (_game == null)
                {
                    _game = new T();
                }
            }
        }

        void IFrameworkView.Load(string entryPoint)
        {
        }

        void IFrameworkView.Run()
        {
            // Initialize and run the game.
            _game.Run();
        }

        void IFrameworkView.SetWindow(CoreWindow window)
        {
            // Initialize the singleton window.
            UAPGameWindow.Instance.Initialize(window, null);
        }

        void IFrameworkView.Uninitialize()
        {
            // TODO: I have no idea when and if this is
            // called... as of Win8 build 8250 this seems 
            // like its never called.
        }
    }
}
